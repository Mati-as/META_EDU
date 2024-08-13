
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuButtonController : MonoBehaviour
{
    private Button _button;
    private IGameManager _gm;
    
  
    private void Awake()
    {
        _button = GetComponent<Button>();
        _gm = GameObject.FindWithTag("GameManager").GetComponent<IGameManager>();
    }
    
    private void Start()
    {
        _button.onClick.AddListener(ButtonClicked);
    }

    
    public void ButtonClicked()
    {
        MetaEduLauncher.isBackButton =
            gameObject.name.Contains("Back") ? true : false;

        StartCoroutine(ChangeScene());

    }

    private WaitForSeconds _wait =new WaitForSeconds(1.0f); 
    private IEnumerator ChangeScene()
    {
        Managers.isGameStopped = true;
        yield return _wait;
        TerminateProcess();
        SceneManager.LoadScene("METAEDU_LAUNCHER");

       
      
    }

    /// <summary>
    /// 씬이동 초기화 수행 전, 다양한 초기화를 진행합니다.
    /// </summary>
    private void TerminateProcess()
    {
        DOTween.KillAll();
    }
}
