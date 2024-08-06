using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroUIController : UI_PopUp
{
    private XmlDocument _doc;
    private TextAsset _xmlAsset;
    private enum UI
    {
        IntroText_Bottom    
    }
    
    public override bool Init()
    {
        BindObject(typeof(UI));
        var sceneName = SceneManager.GetActiveScene().name;
    
        // load the font asset
        var asset = Resources.Load<TMP_FontAsset>("TMP_UI_IntroText/" + sceneName);
        if (asset == null)
        {
            Debug.LogWarning("Failed to load TMP_FontAsset for scene: " + sceneName);
            TurnOff();
            return false; // early return if font asset is not found
        }
        GetObject((int)UI.IntroText_Bottom).GetComponent<TextMeshProUGUI>().font = asset;
    
        // load the xml file
      
            Utils.ReadXML(ref _doc, "Assets/Resources/TMP_UI_IntroText/IntroTexts.xml");
            if (_doc == null)
            {
                Debug.LogWarning("Failed to load XML document.");
                TurnOff();
                return false;
            }
        
            XmlNode root = _doc.DocumentElement;
            if (root == null)
            {
                Debug.LogWarning("Root element is missing in the XML document.");
                TurnOff();
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
                if (node.Attributes["sceneid"].Value == sceneName)
                {
                    GetObject((int)UI.IntroText_Bottom).GetComponent<TextMeshProUGUI>().text = node.Attributes["text"].Value;
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
        


        return true;
    }

    private void TurnOff()
    {
        gameObject.SetActive(false);
    }
    
    
    
}
