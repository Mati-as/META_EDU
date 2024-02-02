using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_anim_controller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private List<string> Animation_clip = new List<string>();
    private Animation Button_anim;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Button_anim.Play(Animation_clip[0]);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Button_anim.Play(Animation_clip[1]);
    }

    // Start is called before the first frame update
    void Start()
    {
        Button_anim = this.GetComponent<Animation>();
        Init_Animation();
    }
    void Init_Animation()
    {
        foreach (AnimationState state in Button_anim)
        {
            Animation_clip.Add(state.name);
        }
    }
}
