using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class WristUIToggle : MonoBehaviour
{
    /// <summary>
    /// The parent game object that contain all of the UI menus, attached to the left wrist.
    /// </summary>
    public GameObject WristUI;
    public InputActionReference MenuPressed;
    public XRInteractorLineVisual leftHandLine;

    private void Start()
    {
        //if the UI is disable before the scene starts, Start() is never called and 
        //OnMenuPressed is never subscribed. Subscribe first, then hide the UI.
        MenuPressed.action.started += OnMenuPressed;
        WristUI.SetActive(true);
        leftHandLine.enabled = false;
    }

    private void OnMenuPressed(InputAction.CallbackContext obj)
    {
        //toggle the menu off/on with the menu button
        bool enable = !WristUI.activeInHierarchy;
        WristUI.SetActive(enable);
        leftHandLine.enabled = !enable;
    }
}
