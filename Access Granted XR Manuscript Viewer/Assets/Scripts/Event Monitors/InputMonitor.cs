using OU.OVAL;
using OU.OVAL.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InputMonitor : MonoBehaviour
{
    public class HandInputInfo : EventArgs
    {
        public Common.PointerCollisionInfo PointerCollisionInfo;
        public Transform HandTransform;
        public Common.Hand Hand;
    }

    public event EventHandler<HandInputInfo> SelectPressed;
    //the raycast used by XR Interaction Toolkit  for hand interactions
    public XRRayInteractor leftRaycast, rightRaycast;

    // Left and right controller transforms
    public Transform lTransform, rTransform;
    public MeasurePanel panel;
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

            //sends a message to the ui panels that need the collision info
            //SelectPressed?.Invoke(this, leftHand);
            panel.Select(leftHand);
        }
        else Debug.Log("No raycast hit");

    }

    private void OnRightSelect(InputValue value)
    {
        Debug.Log("right hand");

    }
}
