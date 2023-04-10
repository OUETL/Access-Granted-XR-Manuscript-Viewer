using OU.OVAL.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;

public class TextureToggle : MonoBehaviour
{
    public Toggle textureToggle;
    public TMP_Dropdown dropdown;

    public PageLoader pageLoader;

    /// <summary>
    /// Enable/Disable two book view.
    /// Will hide the second book and disable the second dropdown if set to false.
    /// </summary>
    /// <param name="toggle"></param>
    public void SetTwoBookView(bool toggle)
    {
        pageLoader.gameObject.SetActive(toggle);
        textureToggle.interactable = toggle;
        dropdown.interactable = toggle;
    }



}
