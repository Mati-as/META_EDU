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
    public Text Message_text;
    public Text Message_text_sub;

    // underground UI에서 구독
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
        Debug.Log("UI Animation On");
#endif
    }

    public void Animation_On()
    {
        Message_anim.Play(Animation_clip[0]);
        StartCoroutine(DeactivateAfterDelay());
    }
    public void Animation_Off()
    {
        Message_anim.Play(Animation_clip[1]);
        StartCoroutine(Active_false());
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
        this.gameObject.SetActive(false);
    }

    public float _interval = 10f;
    IEnumerator DeactivateAfterDelay()
    {
#if UNITY_EDITOR
        Debug.Log("UI Animation Off");
#endif
        yield return new WaitForSeconds(_interval);
        onIntroUIOff?.Invoke();
        Animation_Off();
    }


    //IEnumerator Active_false_time(float timer_1, float timer_2)
    //{
    //    yield return new WaitForSeconds(timer_1);
    //    Message_anim.Play(Animation_clip[1]);
    //    yield return new WaitForSeconds(timer_2);
    //    this.gameObject.SetActive(false);
    //}
}
