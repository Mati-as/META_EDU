using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    public GameObject Loading;
    public GameObject Main;
    public GameObject Menu;

    // Start is called before the first frame update
    [SerializeField]
    public Slider progressBar;
    public Text loadingPercent;
    public Image loadingIcon;


    public string[] gameSceneNames;
    private bool loadingCompleted;
    private int nextScene;

    private Coroutine _progressBarCoroutine;
    

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(LoadScene());
        
        _progressBarCoroutine = StartCoroutine(RotateIcon());

        loadingCompleted = false;
        nextScene = 0;
    }

    IEnumerator LoadScene()
    {
        if (_progressBarCoroutine != null)
        {
            StopCoroutine(_progressBarCoroutine);
        }
        //yield return null;

     
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        float timer = 0.0f;
        //while (!op.isDone)
        while (true)
        {
            yield return null;
            if (progressBar != null)
            {
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
    }

    IEnumerator RotateIcon()
    {
        float timer = 0f;
        while (true)
        {
            if (progressBar == null)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.01f);
                timer += Time.deltaTime;

                Debug.Log(progressBar.value);
                //Debug.Log("check");
                if (progressBar.value < 100f)
                {
                    progressBar.value = Mathf.RoundToInt(Mathf.Lerp(progressBar.value, 100f, timer / 4));
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
    }

    public void Button_Gamestart()
    {
        Main.SetActive(false);
        Menu.SetActive(true);
    }
    enum SceneName
    {
        Launcher,
        fallenLeaves,
        animalTrip,
        undergroundAdventure
    }
    public void Button_Contents(int contentname)
    {
        switch (contentname)
        {
            case -1:
                Debug.Log("ERROR: Current Scene is Null");
                break;

            case 0:
                SceneManager.LoadSceneAsync(gameSceneNames[(int)SceneName.Launcher]);
                break;

            case 1:
                SceneManager.LoadSceneAsync(gameSceneNames[(int)SceneName.fallenLeaves]);
                break;
            
            case 2:
                SceneManager.LoadSceneAsync(gameSceneNames[(int)SceneName.animalTrip]);
                break;
            
            case 3:
                SceneManager.LoadSceneAsync(gameSceneNames[(int)SceneName.undergroundAdventure]);
                break;

            default: Debug.Log("ERROR: Current Scene is Null");
                break;

        }
    }
}
