using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Message_anim_controller : MonoBehaviour
{
    private List<string> Animation_clip = new List<string>();
    private Animation Message_anim;
    
    [HideInInspector]public Text Message_text;
    
    [HideInInspector] public Text Message_text_sub;
    
    public static event Action onIntroUIOff; 
    //0 : On, 1 : Off
    /*
     * 
     *  1. Message Tool , 클릭에 따라 애니메이션 재생
     *  2. Message Intro , OnOff 애니메이션 재생
     *  3. Message Content_func,  OnOff 애니메이션 재생
     *  
     */
    void Start()
    {
        Message_anim = this.GetComponent<Animation>();
        Init_Animation();
        Animation_On();
#if UNITY_EDITOR
        //Debug.Log("UI Animation On");
#endif

        UI_Scene_Button.onBtnShut -= DeactivateUI;
        UI_Scene_Button.onBtnShut += DeactivateUI;
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= DeactivateUI;
    }

    public void Animation_On()
    {
        Message_anim.Play(Animation_clip[0]);
    }
    public void Animation_Off()
    {
        var isAnimPlayed = Message_anim.Play(Animation_clip[1]);
        if(isAnimPlayed) StartCoroutine(Active_false());
#if UNITY_EDITOR
       
#endif
    }

    public void Animation_On_Off()
    {
        Message_anim.Play(Animation_clip[2]);
        //StartCoroutine(Active_false_time(5f,1f));
    }

    public void Change_text(string Field)
    {
        Message_text.text = Field;
    }
    public void Change_text_sub(string Field)
    {
        Message_text_sub.text = Field;
    }

    void Init_Animation()
    {
        foreach(AnimationState state in Message_anim)
        {
            Animation_clip.Add(state.name);
        }
    }

    IEnumerator Active_false()
    {
        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
    }
    
    private void DeactivateUI()
    {
        Animation_Off();
        
    }

}
