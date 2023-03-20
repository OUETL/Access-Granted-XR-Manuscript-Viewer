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

    /// <summary>
    /// OvalObject representing page(s) of the manuscript.
    /// </summary>
    private GameObject ovalObject;
    /// <summary>
    /// The material of the oval object, which we will change the texture of.
    /// </summary>
    public Material material;
    
    /// <summary>
    /// The default texture of the page.
    /// </summary>
    public Texture pageTexture;
    /// <summary>
    /// The alternate texture of the page, when the texture is 'disabled'.
    /// </summary>
    public Texture alternateTexture;

    // Start is called before the first frame update
    void Start()
    {
        textureToggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(textureToggle);
        });
    }

    //actual object swapping in scene and such
    //obj will be loaded and instantiated in main panel
    //assuming the alternate texture doesn't change on a per-page basis
    public void ChangePage(GameObject page)
    {
        ovalObject = page;

        material = ovalObject.GetNamedChild("default").GetComponent<Renderer>().material;
        pageTexture = material.mainTexture;
    }

    public void ToggleEnable(bool toggle)
    {
        //disable/enable ui, hide/show book
        ovalObject.SetActive(toggle);
        textureToggle.interactable = toggle;
        dropdown.interactable = toggle;
    }

    void ToggleValueChanged(Toggle toggle)
    {
        if (ovalObject != null) 
            material.mainTexture = toggle.isOn ? pageTexture : alternateTexture;
    }
}
