using System.Collections.Generic;
using UnityEngine;

namespace OU.OVAL.Core
{
    using EventType = Events.Type;
    using EventArgs = Events.Args;

    //
    // Underlying data structure
    //
    public class Annotation
    {
        public string name = "";
        public Color color = Color.black;
        public float width = 1.0f;
        public List<List<Vector3>> sections = new List<List<Vector3>>();

        public void Clear()
        {
            sections.Clear();
        }
        public void AddSection()
        {
            int N = sections.Count;

            if ((N < 1) || (sections[N - 1].Count > 0))
            {
                sections.Add(new List<Vector3>());
            }
        }
        public void AddPoint(float x, float y, float z)
        {
            AddPoint(new Vector3(x, y, z));
        }
        public void AddPoint(Vector3 v)
        {
            int N = sections.Count;
            if (N < 1) AddSection();
            var l = sections[sections.Count - 1];
            l.Add(v);
        }
    }

    //
    // Renderer for underlying structure
    //
    public class AnnotationRender
    {
        public List<LineRenderer> sections = new List<LineRenderer>();
        public Color color = Color.black;

        public void Clear()
        {
            foreach (var lr in sections)
            {
                GameObject.Destroy(lr);
                GameObject.Destroy(lr.gameObject);
            }
            sections.Clear();
            color = Color.black;
        }

        public void Update(Annotation a)
        {
            bool setColor = false;

            if (color != a.color) setColor = true;

            for (int i = 0; i < a.sections.Count; i++)
            {
                var s = a.sections[i];
                LineRenderer lr;

                // Do we need to add a section?
                if (i >= sections.Count)
                {
                    var parent = new GameObject("Doodle");
                    lr = parent.AddComponent<LineRenderer>();
                    sections.Add(lr);

                    if (lr == null)
                    {
                        Debug.LogWarning("lr is null");
                        return;
                    }

                    lr.useWorldSpace = false;
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    setColor = true;
                }
                else
                {
                    lr = sections[i];
                }

                if (setColor)
                {
                    color = a.color;

                    Gradient gradient = new Gradient();
                    gradient.SetKeys(
                        new GradientColorKey[] {
                                new GradientColorKey(color, 0.0f),
                                new GradientColorKey(color, 1.0f)
                        },
                        new GradientAlphaKey[] {
                                new GradientAlphaKey(color.a, 0.0f),
                                new GradientAlphaKey(color.a, 1.0f)
                        });

                    lr.colorGradient = gradient;
                }

                lr.startWidth = a.width;

                lr.positionCount = s.Count;
                lr.SetPositions(s.ToArray());
            }

            //
            // Should we trim the number of line renderers to
            // match the input annotation data?
            //
            int delta = sections.Count - a.sections.Count;
            if (delta > 0)
            {
                for (int i = a.sections.Count; i < sections.Count; i++)
                {
                    GameObject.Destroy(sections[i]);
                }
                sections.RemoveRange(a.sections.Count, delta);
            }
        }
    }

    //
    // Responds to annotation events by creating a new annotation for the specified parent object.
    //
    public class AnnotationMonitor : MonoBehaviour
    {
        void Awake()
        {
            Events.AddEventHandler(EventType.AnnotationRequest, OnAnnotationEvent);
        }

        void OnAnnotationEvent(object raisedBy, EventArgs args)
        {
            var a = MiscUtil.DeserializeFromString<Core.Annotation>(args.data);

            GameObject go = new GameObject(a.name);
            GameObject modifiedObject = go; // assume "free-standing" annotation, until we know otherwise

            if (string.IsNullOrEmpty(args.path)) { }
            else
            {
                var parent = ObjectUtil.FindModel(args.path);
                if (!parent)
                {
                    Debug.LogWarning($"Unable to find parent {args.path} for object {a.name}");
                    Destroy(go);
                    return;
                }

                var oo = OVALObject.GetOwner(parent);
                if (oo != null)
                {
                    modifiedObject = oo.gameObject; // overall "master" object that annotation was added to
                    parent = oo.annotationContainer;
                }
                go.transform.SetParent(parent.transform);
            }

            //
            // Generate the annotation renderer, and add child objects to
            // container.
            //
            var render = new Core.AnnotationRender();
            render.Update(a);
            foreach (var lr in render.sections)
            {
                lr.gameObject.transform.SetParent(go.transform);
            }

            // Trigger refresh of model selector layout etc to reflect new annotation
            Events.RaiseLocal(this, new EventArgs(EventType.SceneModified));
        }
    }
}
