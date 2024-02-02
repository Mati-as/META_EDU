using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_audio : MonoBehaviour
{
    public static Manager_audio instance = null;

    private AudioSource Click;
    private AudioSource BGM;
    private AudioSource Narration;


    //private float All_volume = 1f;
    private float Effect_volume = 0.5f;
    private float BGM_volume = 0.3f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            if (instance != this) 
                Destroy(this.gameObject); 
        }
    }

    void Start()
    {
        Init_sound();
    }

    //public float Get_all_volume()
    //{
    //    return All_volume;
    //}
    public float Get_Effect_volume()
    {
        return Effect_volume;
    }
    public float Get_BGM_volume()
    {
        return BGM_volume;
    }

    public void Get_click()
    {
        Click.Play();
        //Debug.Log("click");
    }

  
    public void Get_bgm()
    {
        BGM.Play();
        //Debug.Log("click");
    }
    public void Get_narration()
    {
        Narration.Play();
        //Debug.Log("click");
    }

    public void Get_Correct_answer()
    {
        //Correct_answer.Play();
        //Debug.Log("click");
    }

    public void Get_Wrong_answer()
    {
        //Wrong_answer.Play();
        //Debug.Log("click");
    }
   
    private void OnLevelWasLoaded(int level)
    {
        Init_sound();
    }
    private void Init_sound()
    {

        BGM = this.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
        Click = this.transform.GetChild(1).gameObject.GetComponent<AudioSource>();
        Narration = this.transform.GetChild(2).gameObject.GetComponent<AudioSource>();
        
        //Correct_answer = this.transform.GetChild(7).gameObject.GetComponent<AudioSource>();
        //Wrong_answer = this.transform.GetChild(10).gameObject.GetComponent<AudioSource>();

        Set_effect_sound_volume(Effect_volume);
        Set_BGM_volume(BGM_volume);
    }
    public void Set_all_sound_volume(float volume)
    {
        if (volume == 0)
        {
            Click.mute = true;
            BGM.mute = true;

            //Hover.mute = true;
            //Correct_answer.mute = true;
            //Wrong_answer.mute = true;
        }
        else if (volume == 1)
        {
            Click.mute = false;
            BGM.mute = false;

            //Hover.mute = false;
            //Correct_answer.mute = false;
            //Wrong_answer.mute = false;
        }
    }

    public void Set_effect_sound_volume(float volume)
    {
        Click.volume = volume;
        //Hover.volume = volume;
        //Correct_answer.volume = volume;
        //Wrong_answer.volume = volume;
    }

    public void Set_BGM_volume(float volume)
    {
        BGM.volume = volume;
    }

}
