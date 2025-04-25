using System;
using UnityEngine;
using System.Xml;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Hopscotch_UIManager : UI_Base
{
    public float fadeOutDelay;
    private CanvasGroup _canvasGroup;
    private CanvasGroup _InplayCanvasGroup;

    public static event Action OnFadeOutFinish ;
    enum UI_Type
    {
        Head,
        Body
    }

    enum UI_Object
    {
        Frame,
        InPlayTexts
    }

    //private XmlNode soundNode;
    private TextAsset _xmlAsset;
    private XmlDocument _xmlDoc;
    private string _path = "Data/UI_TextData";

    void Start()
    {
        Init();
        _canvasGroup = GetComponentInChildren<CanvasGroup>();
        _canvasGroup.alpha = 1;
        _InplayCanvasGroup.alpha = 0;
        
        _canvasGroup.DOFade(0, 1f).SetDelay(fadeOutDelay)
            .OnComplete(() =>
            {
                OnFadeOutFinish?.Invoke();
                
                _InplayCanvasGroup.DOFade(1.3f, 1f).SetDelay(fadeOutDelay)
                    .OnComplete(() =>
                    {
                        OnFadeOutFinish?.Invoke();
                    })
                    .SetDelay(2.8f);
            });
    
        
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindTMP(typeof(UI_Type));
        
        Utils.LoadXML(ref _xmlAsset,ref _xmlDoc, _path,ref _path);
        var headNode = _xmlDoc.SelectSingleNode($"//StringData[@ID='{SceneManager.GetActiveScene().name + "_Head"}']");
        var headMessage = headNode.Attributes["string"].Value;
        GetTMP((int)UI_Type.Head).text = headMessage;
        
        var bodyNode = _xmlDoc.SelectSingleNode($"//StringData[@ID='{SceneManager.GetActiveScene().name + "_Body"}']");
        var bodyMessage = bodyNode.Attributes["string"].Value;
        GetTMP((int)UI_Type.Body).text = bodyMessage;

        BindObject(typeof(UI_Object));

        GetObject((int)UI_Object.InPlayTexts).TryGetComponent(out _InplayCanvasGroup);
        
        return true;
    }

}
