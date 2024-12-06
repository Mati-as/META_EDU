using System.IO;
using System.Xml;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_IntroTutorialUIController : UI_PopUp
{
    private XmlDocument _doc;
    private TextAsset _xmlAsset;
    private string _currentSceneName;
    private enum Intro_UI
    {
        IntroText_Top,
        IntroText_Bottom, 
        Tutorial_Image,
        Tutorial_Animation,
        ScreenDim
    }
    
    
    
    public override bool Init()
    {
        BindObject(typeof(Intro_UI));
        UI_Scene_StartBtn.onBtnShut -= FadeOutScreen;
        UI_Scene_StartBtn.onBtnShut += FadeOutScreen;
        
        
        
        _currentSceneName = SceneManager.GetActiveScene().name;
        GetObject((int)Intro_UI.ScreenDim).GetComponent<Image>().DOFade(231/255f, 0.5f);

        var isUISucessfullyLoaded = LoadUITextInfo();
        if(!isUISucessfullyLoaded)TurnOff();
        
        #region Loading Image and Prefab(Animation Part)

        var tutorialBgSprite = Resources.Load<Sprite>("UI/Tutorial_Bg/" + SceneManager.GetActiveScene().name);
        GetObject((int)Intro_UI.Tutorial_Image).GetComponent<Image>().sprite = tutorialBgSprite;

        #endregion

        return true;
    }
    

    private void OnDestroy()
    {
        UI_Scene_StartBtn.onBtnShut -= FadeOutScreen;

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
        var asset = Resources.Load<TMP_FontAsset>("TMP_UI_IntroText/IntroTextAsset");
        if (asset == null)
        {
            Debug.LogWarning("Failed to load TMP_FontAsset for scene: " + _currentSceneName);
            
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
      
            if (node.Attributes["sceneid"].Value == _currentSceneName +"_B")
            {
                GetObject((int)Intro_UI.IntroText_Bottom).GetComponent<TextMeshProUGUI>().text = node.Attributes["text"].Value;
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
