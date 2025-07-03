using System;
using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_LoadInitialScene : UI_PopUp
{
    [SerializeField]
    public Slider progressBar;
    public Text loadingPercent;
    public Image loadingIcon;
    
    private bool loadingCompleted;
    private int nextScene;


    public static event Action onInitialLoadComplete;


    void Start()
    {
       // StartCoroutine(LoadScene());
        StartCoroutine(RotateIcon());

        loadingCompleted = false;
        nextScene = 0;
        
    }



    IEnumerator RotateIcon()
    {
        float timer = 0f;
        while (true)
        {

            yield return new WaitForSeconds(0.01f);
            timer += Time.deltaTime;

            if (timer <= 2.0f)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, 100f, timer/6);
                loadingIcon.rectTransform.Rotate(new Vector3(0, 0, 100 * Time.deltaTime));
                loadingPercent.text = Mathf.RoundToInt(progressBar.value).ToString();
            }
            else
            {
                onInitialLoadComplete?.Invoke();
                gameObject.SetActive(false);
                StopAllCoroutines();
              
                if (Managers.IsAlreadyFirstTimeHomeOpened)
                {
                    if (UIManager.UISelectionOnGameExit != null)
                    {
                        Type uISelectionToShow = UIManager.UISelectionOnGameExit.GetType();
                        Logger.CoreClassLog("뒤로가기시 Prev Popup UI Type : " + uISelectionToShow);
                        Managers.UI.ShowPopupUI(uISelectionToShow);
                    }
                    else
                    {
                        Type uISelectionToShow = UIManager.UISelectionOnGameExit.GetType();
                        Logger.CoreClassLog("뒤로가기시 Managers.UI.UISelectionOnGameExit is null");
                        Managers.UI.ShowPopupUI(uISelectionToShow);
                    }
                }
                else
                {
                    Logger.CoreClassLog("뒤로가기X Initial Load Complete, Opening Home UI");
                    Managers.UI.ShowPopupUI<UI_Home>();
                    Managers.IsAlreadyFirstTimeHomeOpened = true;
                }
            }
            
        }
    }
}


