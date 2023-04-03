using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoomPanel : MonoBehaviour
{
    [SerializeField] private Material zoomShaderMaterial;
    public GameObject magnifyingGlass;
    public Slider zoomSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ViewToggleValueChanged(Toggle change)
    {
        magnifyingGlass.SetActive(change.isOn);
    }

    public void OnValueChanged()
    {
        Debug.Log(zoomSlider.value);
        zoomShaderMaterial.SetFloat("_ZoomAmount", zoomSlider.value);
    }
}
