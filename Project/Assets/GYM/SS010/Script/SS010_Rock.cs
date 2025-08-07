using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class SS010_Rock : MonoBehaviour
{
    public int clickedCount = 0;

    private Vector3 downPos;
    private Vector3 originalPos;

    [SerializeField] private float movingDuration = 1.5f;
    [SerializeField] private float divingDuration = 1.5f;
    
    private bool isMoving = false;
        
    private void Start()
    {
        originalPos = gameObject.transform.position;
        downPos = gameObject.transform.position + new Vector3(0, -2f, 0);
    }


    public void ClickedRock()
    {
        clickedCount++;

        if (clickedCount % 2 == 0 && clickedCount != 0 && !isMoving)
        {
            DOTween.Sequence()
                .Append(gameObject.transform.DOMove(downPos, movingDuration).OnStart(() => isMoving = true))
                .AppendInterval(divingDuration)
                .Append(gameObject.transform.DOMove(originalPos, movingDuration).OnComplete(() => isMoving = false))
                ;
        }
        
    }
    



}
