using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Tag OVALObjects with this component.
 * 
 * Provides a way to resolve game objects in the scene to an OVALObject
 * "owner", which is very useful for knowing where to store annotations
 * and how to deal with drag events etc.
 * 
 * An OVALObject periodically examines its position/scale/rotation, and 
 * if a change is detected then a PoseObject event is raised. The Update()
 * method will interpolate between a specified "old" and "target" pose if
 * needed, and so OVALObject poses should remain consistent across network
 * scenes.
 * 
 * IMPORTANT NOTE:
 * 
 * There's a risk of "feedback cascade" from pose events: receipt of a new
 * pose target induces pose changes via Update(). These changes are then
 * detected in MonitorPose(), triggering new pose events from *every
 * connected peer*; these events are then detected by *every peer*, and
 * trigger new pose events from *every peer* and so on. Bad News!
 * 
 * To prevent this, pose events are only raised by the "owner" of the
 * OVALObject (typically the master client) and the object monitor
 * ignores incoming pose events that were actually raised by the local
 * client itself (preventing single-client feedback). For playback of
 * events, we therefore assign a special Guid (System.Guid.Empty).
*/

namespace OU.OVAL.Core
{
    using EventType = Events.Type;
    using EventArgs = Events.Args;

    public class OVALObject : MonoBehaviour
    {
        //
        // Static variables & methods
        //

        // Map GameObject to OVALObject "owner" in scene etc.
        private static Dictionary<GameObject, OVALObject> objectToOVAL = new Dictionary<GameObject, OVALObject>();

        public static void RegisterOwner(GameObject go, OVALObject owner)
        {
            objectToOVAL[go] = owner;
        }
        public static OVALObject GetOwner(GameObject go)
        {
            OVALObject oo;
            return (objectToOVAL.TryGetValue(go, out oo)) ? (oo) : (null);
        }

        //
        // Instance variables & methods
        //

        public Dictionary<string, string> textMetadata = new Dictionary<string, string>();
        public GameObject annotationContainer = null; // activate/deactivate to show/hide annotations
        public bool draggable = true;
        public bool recentreOnStart = true;

        public float poseMonitorIntervalSeconds = 0.5f; // how often pose changes examined
        public float poseSnapDurationSeconds    = 0.1f; // how fast object is "snapped" onto last pose update

        PoseUpdate poseUpdate = new PoseUpdate();
        Coroutine monitorPose;

        void applyToMeshes( List<Transform> all, System.Action<Transform,MeshFilter> action )
        {
            foreach (var t in all)
            {
                var mf = t?.gameObject?.GetComponent<MeshFilter>();
                if (mf != null) action(t,mf);
            }
        }
        void Awake()
        {
            //
            // Every OVAL object has an annotation container as standard
            //
            {
                var t = gameObject.transform.Find(Constants.AnnotationContainerName);
                if (t == null)
                {
                    annotationContainer = new GameObject(Constants.AnnotationContainerName);
                    annotationContainer.transform.SetParent(gameObject.transform);
                    // Set these AFTER parent transform set
                    annotationContainer.transform.localPosition = Vector3.zero;
                    annotationContainer.transform.localRotation = Quaternion.identity;
                    annotationContainer.transform.localScale = Vector3.one;
                }
                else
                {
                    annotationContainer = t.gameObject;
                }
            }

            List<Transform> all = new List<Transform>();
            ObjectUtil.GetTransforms(gameObject.transform, all);

            //
            // Register direct descendents of the OVALObject (ie no intermediate OVALObjects)
            //
            foreach (var t in all)
            {
                var go = t.gameObject;

                // "Self" GameObject obviously registered to this OVALObject
                if (go == gameObject)
                {
                    RegisterOwner(go, this);
                    continue;
                }

                // Child OVALObjects NOT registered to this; considered separate entities
                var oo = go.GetComponent<OVALObject>();
                if (oo != null) continue;

                // Walk "up" hierarchy; does GameObject resolve to this OVALObject?
                var t_ = t.parent;
                while (t_!=null)
                {
                    oo = t_.gameObject.GetComponent<OVALObject>();
                    if (oo != null)
                    {
                        if (oo == this) RegisterOwner(go,this); // If "this" owns "go", register
                        break; // in any case, stop here - found parent OVALObject for "go"
                    }
                    t_ = t_.parent;
                }
            }

            // Modify location / scaling, if required
            {
                float factor = 1.0f;
                var b = ObjectUtil.CalculateWorldBounds(all);

                var limit = Common.Instance.config.limitDisplayedSize;
                if (limit > 0.0)
                {
                    var L = b.size[0];
                    L = Mathf.Max(L, b.size[1]);
                    L = Mathf.Max(L, b.size[2]);

                    if (L > limit)
                    {
                        factor = (float)(limit / L);
                        transform.localScale = new Vector3(factor, factor, factor);
                        Debug.LogWarning($"OVALObject: limited object size! size={b.size} (limit={limit}): scale factor is therefore {factor}");
                    }
                }

                // Move local centre of geometry to local origin
                if (recentreOnStart)
                {
                    transform.localPosition = transform.localPosition - (b.center * factor);
                }
            }

            // Add backfaces. CAUTION: increases triangle count, can violate Unity's 65k mesh triangle limit
            // applyToMeshes(all, (t, mf) => { MeshUtil.AddBackfaces(mf.mesh); });

            // Add mesh colliders; uses current vertices, so call AFTER any vertex add/modify
            applyToMeshes(all, (t, mf) => {
                var mc = t.gameObject.GetComponent<MeshCollider>();
                if (mc == null) t.gameObject.AddComponent<MeshCollider>();
            });

            // Flip vertex normals, if needed
            // applyToMeshes(all, (t, mf) => { MeshUtil.FlipVertexNormals(mf.mesh); });

            /*
            // Recalculate vertex normals - BE CAREFUL! POTENTIALLY COSTLY!
            applyToMeshes(all, (t, mf) => {
                var mu = new MeshUtil(mf.mesh);
                mu.RecalculateVertexNormals(60f);
                mu.CopyTo(mf.mesh);
            });
            */

            poseUpdate.Reset(gameObject); // Required for correct scene restoration of initially hidden objects
        }

        private void OnEnable()
        {
            monitorPose = StartCoroutine("MonitorPose");
        }
        private void OnDisable()
        {
            StopCoroutine(monitorPose);
        }
        private void Update()
        {
            if (poseUpdate.Update(Time.time)) // pose update required?
            {
                transform.localPosition = poseUpdate.current.position;
                transform.localScale = poseUpdate.current.scale;
                transform.localRotation = poseUpdate.current.rotation;
            }
        }

        // See important note at the start of this file.
        IEnumerator MonitorPose()
        {
            var path = ObjectUtil.GetGameObjectPath(gameObject);

            Pose p0 = new Pose(gameObject); // previous pose
            Pose p  = new Pose(gameObject); // instantaneous pose

            while (true)
            {
                p.Set(gameObject);
/*                if (Network.IsMaster() && (p!=p0)) // see note re. OVALObject "owner"
                {
                    var data = MiscUtil.SerializeToString<Pose>(p);
                    Network.RaiseGlobal(this, new EventArgs(EventType.Pose, path:path, data:data) );
                    p0.Set(p);
                }*/

                yield return new WaitForSeconds(poseMonitorIntervalSeconds);
            }
        }

        // Called by e.g. ObjectMonitor in response to incoming pose events.
        public void SetPoseTarget( Pose p )
        {
            poseUpdate.Start(gameObject, p, poseSnapDurationSeconds);
        }

        public string GetTextMetadata(string key)
        {
            if (!textMetadata.ContainsKey(key)) return null;
            return textMetadata[key];
        }
        public string SetTextMetadata(string key, string value)
        {
            string previousValue = (textMetadata.ContainsKey(key)) ? (textMetadata[key]) : (null);
            textMetadata[key] = value;
            return previousValue;
        }
    }
}
