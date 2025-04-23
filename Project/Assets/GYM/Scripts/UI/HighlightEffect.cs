using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HighlightEffect : MonoBehaviour
{
    public Image Img;
    private UI_EventHandler handler;
    private Animator animator;
    //public string SelectChangeColor;

    private void Awake()
    {
        handler = GetComponent<UI_EventHandler>();
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        handler.OnPointerEnterHandler += () => OnPointerEnter();
        handler.OnPointerExitHandler += () => OnPointerExit();
        handler.OnClickHandler += () => OnPointerClick();
    }

    private void OnPointerClick()
    {
        ResetColor();
        EndSelectAni();
    }

    private void ChangeColor()
    {
        Img.color = Color.yellow;
    }

    private void ResetColor()
    {
        Img.color = Color.white;
    }

    private void OnPointerEnter()
    {
        ChangeColor();
        StartSelectAni();
    }

    private void OnPointerExit()
    {
        ResetColor();
        EndSelectAni();
    }
    private void StartSelectAni()
    {
        animator.SetBool("Select", true);
    }

    private void EndSelectAni()
    {
        animator.SetBool("Select", false);
    }

    private void OnDisable()
    {
        handler.OnPointerEnterHandler -= OnPointerEnter;
        handler.OnPointerExitHandler -= OnPointerExit;
        handler.OnClickHandler -= OnPointerClick;
    }

}