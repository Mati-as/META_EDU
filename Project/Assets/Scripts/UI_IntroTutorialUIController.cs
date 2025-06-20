using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_IntroTutorialUIController : UI_PopUp
{
    private XmlDocument _doc;
   // private TextAsset _xmlAsset;
    private string _currentSceneName;
    private enum Intro_UI
    {
        IntroText_Top,
        //   IntroText_Bottom,
        //    Tutorial_Animation,
        ScreenDim,
        
        TutorialImageA,
        TutorialImageB,
    }

    private enum TMPs
    {
        TutorialTextA,
        TutorialTextB
    }
    
    
    
    public override bool InitEssentialUI()
    {
        BindObject(typeof(Intro_UI));
        BindTMP(typeof(TMPs));
        UI_InScene_StartBtn.onGameStartBtnShut -= FadeOutScreen;
        UI_InScene_StartBtn.onGameStartBtnShut += FadeOutScreen;
        
        
        
        _currentSceneName = SceneManager.GetActiveScene().name;
        GetObject((int)Intro_UI.ScreenDim).GetComponent<Image>().DOFade(231/255f, 0.5f);

        var isUISucessfullyLoaded = LoadUITextInfo();
        if(!isUISucessfullyLoaded)TurnOff();
        
        #region Loading Image and Prefab(Animation Part)

        GetIntroImage();
        
    
        // var imageA = Resources.Load<Sprite>("UI/Tutorial_Bg/" + SceneManager.GetActiveScene().name+"_1");
        // GetObject((int)Intro_UI.TutorialImageA).GetComponent<Image>().sprite = imageA;
        
        //2025.05.16 이미지 분할방식으로 업데이트 주석해제 필요. 현재는 통합형 이미지 사용중 입니다.
 //       var imageB = Resources.Load<Sprite>("UI/Tutorial_Bg/" + SceneManager.GetActiveScene().name +"_2");
//        GetObject((int)Intro_UI.TutorialImageB).GetComponent<Image>().sprite = imageB;

        #endregion

        return true;
    }

    private void GetIntroImage()
    {
        StartCoroutine(LoadTutorialImage());
    }
    public IEnumerator LoadTutorialImage()
    {
        string imagePath = Path.Combine(Application.streamingAssetsPath, $"Tutorial_Bg/{SceneManager.GetActiveScene().name}_1.png");
        string uri = "file://" + imagePath;

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("이미지를 불러올 수 없습니다: " + uwr.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
                GetObject((int)Intro_UI.TutorialImageA).GetComponent<Image>().sprite = sprite;
            }
        }
    }
    private void OnDestroy()
    {
        UI_InScene_StartBtn.onGameStartBtnShut -= FadeOutScreen;

    }
    private void TurnOff()
    {
        gameObject.SetActive(false);
    }

    private void FadeOutScreen()
    {
        GetObject((int)Intro_UI.ScreenDim).GetComponent<Image>().DOFade(0, 0.5f);
    }

    #region Lodaing UI Text Information Part
    /// <summary>
    /// 1. UI 문구에 표시할 내용을 XML에서 찾아 문구에 표시합니다.
    /// 2. TextMeshPro 에셋을 찾아 해당 컴포넌트 폰트 에셋에 할당합니다.
    /// 3. XML Document에 해당 텍스트작성, 인트로용 이미지가 필요합니다.
    /// 4. 인트로용 애니메이션은 씬에서 별도로 구현합니다. 
    /// </summary>
    private bool LoadUITextInfo()
    {
    
        var asset = Resources.Load<TMP_FontAsset>("TMP_UI_IntroText/Archive/UI_Heading_Dynamic");
        if (asset == null)
        {
            Debug.LogWarning("Failed to load TMP_FontAsset for scene: " + _currentSceneName +"\n Loadaed Dynamic Asset");
            
            asset = Resources.Load<TMP_FontAsset>("TMP_UI_IntroText/DynamicTMP");
            return false; // early return if font asset is not found
        }

        GetObject((int)Intro_UI.IntroText_Top).GetComponent<TextMeshProUGUI>().font = asset;
    
        // load the xml file
        var path =  Path.Combine(Application.streamingAssetsPath, "IntroTexts.xml");
        Utils.ReadXML(ref _doc, path);
        if (_doc == null)
        {
            Debug.LogWarning("Failed to load XML document.");
          
            return false;
        }
        
        XmlNode root = _doc.DocumentElement;
        if (root == null)
        {
            Debug.LogWarning("Root element is missing in the XML document.");
            return false;
        }
        
        var nodes = root.SelectNodes("StringData");
        if (nodes == null)
        {
            Debug.LogWarning("StringData nodes are missing in the XML document.");
            TurnOff();
            return false;
        }
            

        int isUiInfoOnText = 0;
        foreach (XmlNode node in nodes)
        {
            if (node.Attributes["sceneid"].Value == _currentSceneName +"_T")
            {
                GetObject((int)Intro_UI.IntroText_Top).GetComponent<TextMeshProUGUI>().text = node.Attributes["text"].Value;
                isUiInfoOnText = 1;
                break;
            }
        }
        
        foreach (XmlNode node in nodes)
        {
      
            if (node.Attributes["sceneid"].Value == _currentSceneName +"_1")
            {
                GetTMP((int)TMPs.TutorialTextA).text = node.Attributes["text"].Value;
                isUiInfoOnText = 1;
                break;
            }
        }
        
        foreach (XmlNode node in nodes)
        {
      
            if (node.Attributes["sceneid"].Value == _currentSceneName +"_2")
            {
                GetTMP((int)TMPs.TutorialTextB).text = node.Attributes["text"].Value;
                isUiInfoOnText = 1;
                break;
            }
        }

        if (isUiInfoOnText == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Current UI is old version or not set in XML file.");
#endif
        }


        #endregion

        return true;
    }
    
}
