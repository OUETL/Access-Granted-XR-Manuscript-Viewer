using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class InputMonitor : MonoBehaviour
{
    //the raycast used by XR Interaction Toolkit  for hand interactions
    public XRRayInteractor leftRaycast, rightRaycast;

    public void Update()
    {
        
    }


/*    public void OnSelect(InputAction.CallbackContext context)
    // public void Gripped()
    {
        if (context.canceled) return; //button input was released; ignore pls

        RaycastHit raycastHit;
        //left
        if (leftRaycast.TryGetCurrent3DRaycastHit(out raycastHit))
        {
            if (raycastHit.transform.gameObject != null) Debug.Log("Object: " + raycastHit.transform.gameObject.name);
            else Debug.Log("Nuthin");

        }
        else Debug.Log("No raycast hit");




    }*/
    private void OnSelect(InputValue value)
    {
       // if (context.canceled) return; //button input was released; ignore pls

        RaycastHit raycastHit;
        //left
        if (leftRaycast.TryGetCurrent3DRaycastHit(out raycastHit))
        {
            if (raycastHit.transform.gameObject != null) Debug.Log("Object: " + raycastHit.transform.gameObject.name);
            else Debug.Log("Nuthin");

        }
        else Debug.Log("No raycast hit");




    }
}
