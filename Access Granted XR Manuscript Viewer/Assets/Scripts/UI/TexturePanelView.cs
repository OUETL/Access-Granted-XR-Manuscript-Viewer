using OU.OVAL.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TexturePanelView : MonoBehaviour
{
    //maybe later the setting from previous session are loaded in
    public bool TwoBookView = false;
    public Toggle viewToggle;
    public TextureToggle rightToggle, leftToggle;
    public GameObject left, right; //testing
    public Texture alternateTexture;

    // Start is called before the first frame update
    void Start()
    {
        viewToggle.onValueChanged.AddListener(delegate {
            ViewToggleValueChanged(viewToggle);
        });

        leftToggle.alternateTexture = alternateTexture;
        rightToggle.alternateTexture = alternateTexture;

        Testing();
    }

    //enable/disable right book
    void ViewToggleValueChanged(Toggle change)
    {
        rightToggle.ToggleEnable(change.isOn);
    }

    public void Testing()
    {
        rightToggle.ChangePage(right);
        leftToggle.ChangePage(left);
    }
}
