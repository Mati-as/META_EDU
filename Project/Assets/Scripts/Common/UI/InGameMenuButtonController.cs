
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuButtonController : MonoBehaviour
{
    private Button _button;
    
  
    private void Awake()
    {
        _button = GetComponent<Button>();
    }
    
    private void Start()
    {
        _button.onClick.AddListener(ButtonClicked);
    }

    
    public void ButtonClicked()
    {
        MetaEduLauncher.isBackButton =
            gameObject.name.Contains("Back") ? true : false;
                
        SceneManager.LoadScene("METAEDU_LAUNCHER");

    }
}
