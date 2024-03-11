using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadInitialScene : MonoBehaviour
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
        //StartCoroutine(LoadScene());
        StartCoroutine(RotateIcon());

        loadingCompleted = false;
        nextScene = 0;
    }

    IEnumerator LoadScene()
    {
        //yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;
        //while (!op.isDone)
        while(true)
        {
            //yield return null;

            timer += Time.deltaTime;

            if (op.progress >= 0.9f)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);
                loadingPercent.text = "progressBar.value";

                if (progressBar.value == 1.0f)
                    op.allowSceneActivation = true;
            }
            else
            {
                progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
                if (progressBar.value >= op.progress)
                {
                    timer = 0f;

                    //End of scene index
                    if (nextScene == 2 && loadingCompleted)
                    {
                        StopAllCoroutines();
                    }
                }
            }
        }
    }

    IEnumerator RotateIcon()
    {
        float timer = 0f;
        while (true)
        {

            yield return new WaitForSeconds(0.01f);
            timer += Time.deltaTime;

            if (timer <= 6.0f)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, 100f, timer/6);
                loadingIcon.rectTransform.Rotate(new Vector3(0, 0, 100 * Time.deltaTime));
                loadingPercent.text = Mathf.RoundToInt(progressBar.value).ToString();
            }
            else
            {
                StopAllCoroutines();
                onInitialLoadComplete?.Invoke();
                Debug.Log("loadComplete");
                gameObject.SetActive(false);
            }
            
        }
    }
}


