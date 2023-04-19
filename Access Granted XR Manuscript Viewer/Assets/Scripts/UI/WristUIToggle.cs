using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristUIToggle : MonoBehaviour
{
    /// <summary>
    /// The parent game object that contain all of the UI menus, attached to the left wrist.
    /// </summary>
    public GameObject WristUI;

    public void OnMenu()
    {
        //toggle the menu off/on with the menu button
        WristUI.SetActive(!WristUI.activeInHierarchy);
    }
}
