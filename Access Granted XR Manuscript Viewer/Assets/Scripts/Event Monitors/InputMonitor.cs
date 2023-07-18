using OU.OVAL;
using OU.OVAL.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static MeasurePanel;
using static OU.OVAL.Core.Common;

public class InputMonitor : MonoBehaviour
{
    public class HandInputInfo : EventArgs
    {
        public Common.PointerCollisionInfo PointerCollisionInfo;
        public Transform HandTransform;
        public Common.Hand Hand;

        public override string ToString()
        {
            return $"{Hand}, Collided with {PointerCollisionInfo.collidedWith} at {PointerCollisionInfo.worldPosition}.";
        }
    }

    //public event EventHandler<HandInputInfo> SelectPressed;
    //the raycast used by XR Interaction Toolkit  for hand interactions
    public XRRayInteractor leftRaycast, rightRaycast;

    // Left and right controller transforms
    public Transform lTransform, rTransform;

    public void ChangeInteractionLayer()
    {
        leftRaycast.interactionLayers = InteractionLayerMask.NameToLayer("OvalObject");
    }
    private void OnLeftSelect(InputValue value)
    {
        RaycastHit raycastHit;
        //left
        if (leftRaycast.TryGetCurrent3DRaycastHit(out raycastHit))
        {
            HandInputInfo leftHand = new HandInputInfo();
            leftHand.Hand = Common.Hand.Left;
            leftHand.HandTransform = lTransform;
            leftHand.PointerCollisionInfo = new Common.PointerCollisionInfo()
            {
                collidedWith = raycastHit.transform.gameObject,
                worldPosition = raycastHit.point,
                worldNormal = raycastHit.normal,
                distance = raycastHit.distance

            };

            if (raycastHit.transform.gameObject != null) Debug.Log("Object: " + raycastHit.transform.gameObject.name);
            else Debug.Log("Nuthin");

            //anything subscribed can now draw/measure
            //Draw?.Invoke(this, leftHand);
        }
        else Debug.Log("No raycast hit");

    }

    HandInputInfo LeftHand = new HandInputInfo();
    HandInputInfo RightHand = new HandInputInfo();

    private void Awake()
    {
        //left
        LeftHand.Hand = Common.Hand.Left;
        LeftHand.HandTransform = lTransform;

        RightHand.Hand = Common.Hand.Right;
        RightHand.HandTransform = rTransform;
    }

    private void UpdateRaycastCollisionsForHand(XRRayInteractor rayInteractor, HandInputInfo hand)
    {
        RaycastHit raycastHit;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out raycastHit))
        {
            GameObject collisionObject = null;
            if (raycastHit.transform.gameObject != null && raycastHit.transform.gameObject.layer == LayerMask.NameToLayer("OvalObject"))
                collisionObject = raycastHit.transform.gameObject;

            hand.PointerCollisionInfo = new Common.PointerCollisionInfo()
            {
                collidedWith = collisionObject,
                worldPosition = raycastHit.point,
                worldNormal = raycastHit.normal,
                distance = raycastHit.distance

            };
        }
        else
        {
            hand.PointerCollisionInfo.collidedWith = null;
        }
    }

    public HandInputInfo GetHandInput(Tracking tracking)
    {
        if (tracking == Tracking.Left)
        {
            UpdateRaycastCollisionsForHand(leftRaycast, LeftHand);
            return LeftHand;
        }
        else if (tracking == Tracking.Right)
        {
            UpdateRaycastCollisionsForHand(rightRaycast, RightHand);
            return RightHand;
        }
        else { return null; }
    }
}
