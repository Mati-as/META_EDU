using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Image_mouse : MonoBehaviour
{
    public Texture2D[] Sprites_mouse;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    //Update is called once per frame
    //void Update()
    //{

    //}

    public Texture2D Arrow_image()
    {
        return Sprites_mouse[0];
    }

    public Texture2D Hand_image()
    {
        return Sprites_mouse[1];
    }
}
