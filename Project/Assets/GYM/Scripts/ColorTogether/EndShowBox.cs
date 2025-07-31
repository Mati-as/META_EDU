using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EndShowBox : MonoBehaviour
{
    [SerializeField] private ColorTogether_Manager manager;
    [SerializeField] private List<GameObject> boxes = new List<GameObject>(6);
    [SerializeField] private Transform showPosition;
    [SerializeField] private Vector3 startScale = new Vector3(3.72f, 7.28f, 2.64f);
    [SerializeField] private Vector3 targetScale = new Vector3(4, 8, 3);
    [SerializeField] private List<Transform> startTransforms = new List<Transform>(6);

    void Start()
    {
        manager = FindObjectOfType<ColorTogether_Manager>();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("ShowPosition"))
                showPosition = child;

            for (int i = 0; i < 6; i++)
            {
                if (child.name.StartsWith($"EndBox_{i + 1}"))
                    boxes.Add(child.gameObject);

                if (child.name.StartsWith($"startPosition_{i + 1}"))
                    startTransforms.Add(child.transform);
            }
        }
    }


    public void ShowPositionBox()
    {
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < boxes.Count; i++)
        {
            int index = i;
            sequence.AppendInterval(2f);    //라운드 종료 나레이션 시간 기다림
            sequence.AppendCallback(() =>
            {
                boxes[index].SetActive(true);
                boxes[index].transform.DOScale(targetScale, 2f);
                boxes[index].transform.DOJump(showPosition.position, 1.5f, 1, 2f).SetEase(Ease.OutQuad);
                boxes[index].transform.DOShakePosition(0.3f, 0.4f);
            });

            sequence.AppendInterval(2f);

            sequence.AppendCallback(() =>
            boxes[index].transform.DOShakeRotation(1.5f, 30f, 10, 90f).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => manager.narrationBG.sizeDelta = new Vector2(840, 143));
            sequence.AppendCallback(() => manager.NarrationALL(30 + index, 30 + index));

            sequence.AppendInterval(2);

            sequence.AppendCallback(() =>
            boxes[index].transform.DOJump(startTransforms[index].position, 1.5f, 1, 2f));
            sequence.AppendCallback(() => boxes[index].transform.DOShakeRotation(1.5f, 30f, 10, 90f).SetEase(Ease.OutQuad));
            sequence.AppendCallback(() => boxes[index].transform.DOScale(startScale, 2f).SetEase(Ease.Linear));
        }
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() => manager.narrationBG.sizeDelta = new Vector2(1050, 143));
        sequence.AppendCallback(() => manager.NarrationALL(36, 36));
        sequence.AppendCallback(() => manager.endCanClicked = true);
        sequence.AppendCallback(manager.ReloadScene);
            ;
    }

}