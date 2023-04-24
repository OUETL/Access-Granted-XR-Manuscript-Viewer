using OU.OVAL.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Empty GameObject that contains all page GameObjects.
/// The parent object also has an attach child object and potentially other 
/// children in the future, so for easy searching, all pages are children of
/// the PageContainer object.
/// </summary>
public class PageLoader : MonoBehaviour
{
    /// <summary>
    /// GameObjects that have the mesh and material for each page. Pre-loaded into
    /// the scene as child objects because it was the easiest way to deal with the
    /// imported meshes.
    /// 
    /// Also it seems the meshes aren't positioned/rotated the same way at (0, 0, 0),
    /// so doing it this way means the pages can be adjusted so they don't 'jump'
    /// when switching to a different page.
    /// </summary>
    public List<Page> Pages;
    /// <summary>
    /// Dropdown that requires the list of pages
    /// </summary>
    public TMP_Dropdown dropdown;
    public Toggle AlternateTextureCheckbox;

    public Page CurrentPage = null;

    // Start is called before the first frame update
    void Start()
    {
        //get a list of all pages, including inactive objects
        Pages = new List<Page>(GetComponentsInChildren<Page>(true));

        FillDropdownLists();
        CurrentPage = Pages[0];
        CurrentPage.gameObject.SetActive(true);
    }

    private void FillDropdownLists()
    {
        //get the name of each page to instatiate the dropdown lists with
        var pageOptionsList = new List<TMP_Dropdown.OptionData>();
        foreach (var page in Pages)
        {
            pageOptionsList.Add(new TMP_Dropdown.OptionData(page.name));
        }

        //replace old list with new list
        dropdown.ClearOptions();
        dropdown.options.AddRange(pageOptionsList);

        
    }

    public Page GetPage(int value)
    {
        if (value < 0 || value >= Pages.Count) return null;

        return Pages[value];
    }

    /// <summary>
    /// Swaps out the page GameObject for another page, chosen from the dropdown list.
    /// </summary>
    public void ChangePage(int index)
    {
        Page newPage = GetPage(index);
        if (newPage != null)
        {
            newPage.gameObject.SetActive(true);
            CurrentPage.gameObject.SetActive(false);

            CurrentPage = newPage;

            //reset the checkbox value to match default texture
            AlternateTextureCheckbox.isOn = true;
        }
    }
    /// <summary>
    /// Sets the texture of the current active page object.
    /// </summary>
    /// <param name="alternate"></param>
    public void AlternateTexture(bool alternate)
    {
        CurrentPage.SetAlternateTexture(alternate);
    }
}
