using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Serialization;


public class VideoContents_UIManager : UI_Base
{
     public float fadeOutDelay;
    private CanvasGroup _canvasGroup;
    enum UI_Type
    {
        Head,
        Body
    }

    //private XmlNode soundNode;
    private TextAsset _xmlAsset;
    private XmlDocument _xmlDoc;
    private string _path = "VideoContents/Data/UI_TextData";

    void Start()
    {
        Init();
        _canvasGroup = GetComponentInChildren<CanvasGroup>();
        _canvasGroup.DOFade(0, 1f).SetDelay(fadeOutDelay);

    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindText(typeof(UI_Type));
        
        Utils.LoadXML(ref _xmlAsset,ref _xmlDoc, _path);


        var headNode = _xmlDoc.SelectSingleNode($"//StringData[@ID='{SceneManager.GetActiveScene().name + "_Head"}']");
        if (headNode != null)
        {
            var headMessage = headNode.Attributes["string"].Value;
            GetText((int)UI_Type.Head).text = headMessage;
        }

        var bodyNode = _xmlDoc.SelectSingleNode($"//StringData[@ID='{SceneManager.GetActiveScene().name + "_Body"}']");

        var bodyMessage = bodyNode.Attributes["string"].Value;
        GetText((int)UI_Type.Body).text = bodyMessage;


        return true;
    }

}
