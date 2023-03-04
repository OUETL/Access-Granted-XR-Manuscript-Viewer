using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OU.OVAL.Core
{
    //
    // System-wide access to commonly used data and objects via static Instance.
    // Monobehaviour-derived class, as designed to be actually added to an object in the
    // system, which then exposes the public members for modification through Unity's editor
    // for basic system setup.
    //
    public sealed class Common : MonoBehaviour
    {
        //
        // Class not struct, so can copy/pass references.
        //
        public class PointerCollisionInfo
        {
            public GameObject collidedWith = null;
            public Vector3 worldPosition, worldNormal;
            public float distance;
        }

        //
        // Assume these set in the Unity editor
        //

        // Left and right controller transforms
        public Transform lTransform;
        public Transform rTransform;

        // FIX THIS - VIVE DEPENDENT! Is there a suitable base class this derives from that is built-in to Unity?
/*        public Pointer3DRaycaster lRaycaster;
        public Pointer3DRaycaster rRaycaster;*/
        public RaycastHit lRaycaster;
        public RaycastHit rRaycaster;

        // We commonly need collsions between the controller pointers and various objects in the scene
        public PointerCollisionInfo lCollision = new PointerCollisionInfo();
        public PointerCollisionInfo rCollision = new PointerCollisionInfo();

        //public InputWrapperBase inputWrapper;

        // Handy system container GameObjects
        public GameObject sceneContainer, playerContainer, modelContainer;
        public GameObject uiPanels;

        //
        // Misc.
        //

        public Config config = new Config();
        public System.Guid guid = System.Guid.NewGuid(); // tag events; unique to this local session
        public UserInfoMap userInfoMap = new UserInfoMap();
        public ScalableVisualElements scalableVisualElements = new ScalableVisualElements();
        //
        // Singleton implementation; we assume there's an object in the scene with a
        // Common MonoBehaviour assigned to it.
        //

        private static Object _lock = new Object();
        private static Common _instance;
        public static Common Instance
        {
            get
            {
                if (_instance) return _instance; // avoid lock contention if valid instance
                lock (_lock)
                {
                    if (!_instance) _instance = FindObjectOfType<Common>();
                }
                return _instance;
            }
        }

        //
        // Get pointer collisions, so they are commonly available. FixedUpdate() is called before
        // any Update() methods, so results should be available for use in any object's Update():
        // https://docs.unity3d.com/Manual/ExecutionOrder.html
        //
        private void FixedUpdate()
        {
            lCollision.collidedWith = null;
            rCollision.collidedWith = null;

/*            // Left
            {
                var srr = Instance.lRaycaster.SortedRaycastResults;
                if (srr.Count > 0)
                {
                    lCollision.collidedWith = srr[0].gameObject;
                    lCollision.worldPosition = srr[0].worldPosition;
                    lCollision.worldNormal = srr[0].worldNormal;
                    lCollision.distance = srr[0].distance;
                }
            }

            // Right
            {
                var srr = Instance.rRaycaster.SortedRaycastResults;
                if (srr.Count > 0)
                {
                    rCollision.collidedWith = srr[0].gameObject;
                    rCollision.worldPosition = srr[0].worldPosition;
                    rCollision.worldNormal = srr[0].worldNormal;
                    rCollision.distance = srr[0].distance;
                }
            }*/
        }

        public void Start()
        {
            // Registration of visual elements should have taken place in the appropriate
            // Awake() routines of the elements themselves.
            scalableVisualElements.SetScaleFactor( config.visualScaleFactor );
        }
    }
}
