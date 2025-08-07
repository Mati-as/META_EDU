using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_LoadingOnSceneChange : MonoBehaviour
{
    [SerializeField]
    public Slider progressBar;
    public Text loadingPercent;
    public Image loadingIcon;
    
    private bool loadingCompleted;
    private int nextScene;


    public static event Action onInitialLoadComplete;


    private void Start()
    {
        Cursor.visible = false; // 커서 숨김
     
        StartCoroutine(LoadSceneWithLoading(UI_Master.GameNameWaitingForConfirmation));
    }

    IEnumerator LoadSceneWithLoading(string sceneName)
    {
        gameObject.SetActive(true);
        float fakeTimer = 0f;

        // 1. 비동기로 씬 로딩 시작 (자동 전환은 막아둠)
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        // 2. 로딩 애니메이션 + 가짜 진행바
        while (fakeTimer < 1.5f)
        {
            fakeTimer += Time.deltaTime;

            progressBar.value = Mathf.Lerp(progressBar.value, 100f, fakeTimer/5);
            loadingIcon.rectTransform.Rotate(new Vector3(0, 0, 100 * Time.deltaTime));
            loadingPercent.text = Mathf.RoundToInt(progressBar.value).ToString();

            yield return null;
        }

        // 3. 실제 로딩이 완료됐는지 기다림
        while (async.progress < 0.89f)
        {
            yield return null; // 아직 로딩 중
        }
  
        // 4. 1.5초도 지났고, 로딩도 완료 → 씬 전환 허용
        onInitialLoadComplete?.Invoke();
        async.allowSceneActivation = true;
        
    }

    
    public void LoadScene(string sceneName)
    {
        // string originalName = sceneName;
        // string modifiedName = originalName.Substring("SceneName_".Length);
       
        gameObject.SetActive(false);
        SceneManager.LoadSceneAsync(sceneName);
    }
}
