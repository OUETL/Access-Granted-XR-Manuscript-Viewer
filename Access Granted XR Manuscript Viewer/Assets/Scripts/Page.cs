using OU.OVAL.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//will name it better later
public class Page : MonoBehaviour
{
    public Texture NormalTexture, AlternateTexture;
    public Material Material;

    // Start is called before the first frame update
    void Start()
    {
        Material = GetComponent<Renderer>().material;
        NormalTexture = Material.mainTexture;

    }

    /// <summary>
    /// Swaps between the default page texture and the alternate texture.
    /// </summary>
    /// <param name="toggle"></param>
    public void SetAlternateTexture(bool alternate)
    {
        Material.mainTexture = alternate ? NormalTexture : AlternateTexture;
    }
}
