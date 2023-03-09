﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

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

        //the raycast used by XR Interaction Toolkit  for hand interactions
        public XRRayInteractor leftRaycast, rightRaycast;

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

        private static UnityEngine.Object _lock = new UnityEngine.Object();
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
            int temp;
            bool validTarget;
/*            RaycastHit raycastHit;
            //left
            //if (leftRaycast.TryGetHitInfo(out lCollision.worldPosition, out lCollision.worldNormal, out temp, out validTarget))
            if (leftRaycast.TryGetCurrent3DRaycastHit(out raycastHit))
            {
                // lCollision.collidedWith = leftRaycast.ge

                if (raycastHit.transform.gameObject != null) Debug.Log("Object:" + raycastHit.transform.gameObject.name);
                else Debug.Log("Nuthin");
               
               // leftRaycast.GetCurrentRaycastHit(out raycastHit);
               // raycastHit.
            }
            else Debug.Log("Nuthin");*/

            //right
            //rightRaycast.TryGetHitInfo(out rCollision.worldPosition, out rCollision.worldNormal, out temp, out validTarget);


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
/*
        public void OnSelect(InputAction.CallbackContext context)
       // public void Gripped()
        {
            if(context.canceled) return; //button input was released; ignore pls

            RaycastHit raycastHit;
            //left
            if (leftRaycast.TryGetCurrent3DRaycastHit(out raycastHit))
            {
                if (raycastHit.transform.gameObject != null) Debug.Log("Object: " + raycastHit.transform.gameObject.name);
                else Debug.Log("Nuthin");

            }
            else Debug.Log("No raycast hit");

           

            *//*            //find which hand selected
                        Hand hand = DetermineHandController(context);


                        //
                        // Check for collisions of pointer and UI / scene objects.
                        //
                        Core.Common.PointerCollisionInfo collision = (hand == Hand.Left) ? lCollision : rCollision;

                        var obj = collision.collidedWith;
                        OVALObject collidedWithOO = Core.OVALObject.GetOwner(obj);
                        if (!measureTarget && collidedWithOO) measureTarget = collidedWithOO;


                        //
                        // At this point, we have all the information we need to manipulate the points etc
                        //

                        var controllerTransform = (tracking == Tracking.Left) ? (common.lTransform) : (common.rTransform);
                        var pos = (measureOnSurface) ? (collidedAt) : (controllerTransform.position);*//*


        }*/

        //move this over to a util class later
        //both hands?
        public enum Hand { Left, Right, Neither };
        public Hand DetermineHandController(InputAction.CallbackContext context)
        {
            var device = context.control.device;

            if (device.usages.Contains(CommonUsages.LeftHand))
                return Hand.Left;
            if (device.usages.Contains(CommonUsages.RightHand))
                return Hand.Right;

            return Hand.Neither;
        }
    }
}
