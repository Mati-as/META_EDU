using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorImageManager : MonoBehaviour
{

    private enum CursorImage
    {
        Click,
        Hover,
        Max
    }
    public static CursorImageManager instance = null;   
    
    public Texture2D[] Sprites_mouse;

    public void Init()
    {
        Sprites_mouse = new Texture2D[(int)CursorImage.Max];
        var cur = Resources.Load<Texture2D>("UI/Cursor/Cursor");
        Sprites_mouse[(int)CursorImage.Click]= cur;
        var hov = Resources.Load<Texture2D>("UI/Cursor/Hand");
        Sprites_mouse[(int)CursorImage.Hover] =hov;
    }
    private void Awake()
    {
      
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
                Destroy(this.gameObject);
        }

    }
    
    public Texture2D Get_arrow_image()
    {
        return Sprites_mouse[(int)CursorImage.Click];
    }

    public Texture2D Get_hand_image()
    {
        return Sprites_mouse[(int)CursorImage.Hover];
    }
}