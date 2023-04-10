using OU.OVAL.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SetTwoBookView : MonoBehaviour
{
    //maybe later the settings from previous session are loaded in
    public bool TwoBookView = false;
    public Toggle twoBookViewCheckbox;
    public GameObject rightToggle;
    public GameObject rightBook;


    //enable/disable right book
    public void SetView(bool view)
    {
        rightToggle.SetActive(view);
        rightBook.SetActive(view);
    }
}
