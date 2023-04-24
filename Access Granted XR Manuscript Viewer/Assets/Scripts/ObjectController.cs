using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Rotates a XRGrabInteractable object while it is in the user's hand.
/// Holding an additional button will instead translate the object's position
/// instead of rotating.
/// </summary>
public class ObjectController : MonoBehaviour
{
    public XRGrabInteractable obj;
    private float xAngle, yAngle, zAngle;

    // Start is called before the first frame update
    void Start()
    {
        xAngle = 0.0f;
        yAngle = 0.0f;
        zAngle = 0.0f;
    }

    void FixedUpdate()
    {
        
    }

    void OnRotate(InputValue inputValue)
    {
        //only rotate while grabbed
        if (obj.isSelected)
        {
            Vector2 rotateVector = inputValue.Get<Vector2>();
            xAngle = rotateVector.x;
            yAngle = rotateVector.y;

            obj.transform.Rotate(yAngle, xAngle, zAngle, Space.World);
            //if (xAngle < 0.5f)
            //{
            //    Debug.Log("xAngle: " + xAngle);
            //}
        }


    }

    void OnTranslate(InputValue inputValue)
    {
        Debug.Log("translating...");
    }


}
