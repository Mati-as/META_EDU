using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectMove: MonoBehaviour
{
    public SelectColorManager manager;

    public List<Transform> targets;

    public float timer;

    private bool isSequencePlaying = false;


    void Start()
    {
        manager = FindObjectOfType<SelectColorManager>();
    }

    private void Update()
    {
        if (!isSequencePlaying && manager.time >= timer)
        {
            sequence();
            isSequencePlaying = true;
        }

    }


    void sequence()
    {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < targets.Count; i++)
            {
                sequence.AppendInterval(2f)
                        .Append(this.gameObject.transform.DOMove(targets[i].position, 1f));

                //if (i == targets.Count - 1)
                //{
                //    sequence.OnComplete(() => { sequence.Kill();});
                //}
                //스퀀스 종료 후 삭제
            }

    }    




}
