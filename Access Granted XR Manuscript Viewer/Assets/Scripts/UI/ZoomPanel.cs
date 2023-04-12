using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomPanel : MonoBehaviour
{
    [SerializeField] private Material zoomShaderMaterial;
    public GameObject magnifyingGlass;
    public Slider zoomSlider;

    //public GameObject page;

    /// <summary>
    /// Hide/Show the magnifying glass. 
    /// </summary>
    /// <param name="change"></param>
    public void ToggleMagnifyingGlass(bool zoom)
    {
        magnifyingGlass.SetActive(zoom);
    }
    /// <summary>
    /// Adjust zoom level according to the UI slider value
    /// </summary>
    public void OnSliderValueChanged()
    {
        //Debug.Log(zoomSlider.value);
        zoomShaderMaterial.SetFloat("_ZoomAmount", zoomSlider.value);
        //gameObject.transform.localScale = zoomSlider.value;
    }
}
