using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectController : MonoBehaviour
{
    public GameObject obj;
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
