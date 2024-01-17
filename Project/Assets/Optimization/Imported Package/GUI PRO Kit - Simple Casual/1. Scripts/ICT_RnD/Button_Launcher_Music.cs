using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_Launcher_Music : MonoBehaviour, IPointerClickHandler
{
    private GameLauncher_ICT Launcher;

    public bool Play = false;
    public bool Replay = false;
    public bool Stop = false;
    public bool Analysis = false;
    public bool Listening = false;
    public bool TEST_1 = false;
    public bool TEST_2 = false;
    public bool TEST_3 = false;

    void Start()
    {
        Launcher = GameObject.Find("Launcher").GetComponent<GameLauncher_ICT>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {

        //Music content
        if (Play)
            Launcher.Button_Music_Play();

        if (Replay)
            Launcher.Button_Music_Replay();

        if (Stop)
            Launcher.Button_Music_Stop();

        if (Analysis)
            Launcher.Button_Music_Analysis();

        if (Listening)
            Launcher.Button_Music_Listening();

        if (TEST_1)
            Manager_ResultInDetail.instance.Add_RIDdata(Random.Range(-10,10));

        if (TEST_2)
            Manager_ResultInDetail.instance.Add_RIDdata(Random.Range(-10, 10), 0.3f);

        if (TEST_3)
            Manager_ResultInDetail.instance.Add_RIDdata(Random.Range(-10, 10), Random.Range(-10, 10), 0.3f);
    }
}
