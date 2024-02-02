using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    public GameObject Loading;
    public GameObject Main;
    public GameObject Menu;
    public GameObject Setting;

    // Start is called before the first frame update
    [SerializeField]
    public Slider progressBar;
    public Text loadingPercent;
    public Image loadingIcon;

    private bool loadingCompleted;
    private int nextScene;

    //로딩 후 홈 화면 전환
    //홈화면에서 콘텐츠 전환
    //각 화면에서 씬으로 전환

    // Start is called before the first frame update
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
        while (true)
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

            Debug.Log(progressBar.value);
            //Debug.Log("check");
            if (progressBar.value < 100f)
            {
                progressBar.value = Mathf.RoundToInt(Mathf.Lerp(progressBar.value, 100f, timer / 8));
                loadingIcon.rectTransform.Rotate(new Vector3(0, 0, 100 * Time.deltaTime));
                loadingPercent.text = progressBar.value.ToString();
            }
            else
            {
                StopAllCoroutines();
                //Debug.Log("100%");

                Loading.SetActive(false);
                Main.SetActive(true);
            }
        }
    }

    public void Button_Gamestart()
    {
        Main.SetActive(false);
        Menu.SetActive(true);
    }
    public void Button_Contents(int contentname)
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void Button_Setting()
    {
        Setting.SetActive(true);
        //일시정지 기능
    }
    public void Button_Close()
    {
        Setting.SetActive(false);
        //일시정지 해제 기능 추가
    }
    public void Button_Back_ToMode()
    {
        //이전 씬으로 복귀
    }

}
