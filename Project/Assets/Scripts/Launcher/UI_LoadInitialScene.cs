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
              

                Managers.UI.CloseAllPopupUI();
                Managers.UI.ShowPopupUI<UI_Home>();
            }
            
        }
    }
}


