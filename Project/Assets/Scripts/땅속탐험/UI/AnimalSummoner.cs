using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using MyCustomizedEditor;
#endif

public class AnimalSummoner : MonoBehaviour
{
    private Vector3 targetScale;
    public float maximizedScale; // 목표 크기
    public float maximizingDuration;
    public float minimizingDuration;
    
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "개미", "지렁이", "두더지", "땅거미", "쥐며느리", "달팽이", "뱀", "쇠똥구리", "개구리", "다람쥐", "토끼", "여우"
    })]
#endif
    
    public static GameObject[] allAnimals = new GameObject[20];

    public Dictionary<string, Transform> messageTransformByName = new();

    private void Start()
    {
        targetScale = new Vector3(maximizedScale, maximizedScale, maximizedScale);
        SetTransforms();
    }

    private void SetTransforms()
    {
        foreach (var obj in allAnimals) messageTransformByName[obj.name] = obj.transform;
    }

    public static void ActivateAllAnimals()
    {
        foreach (var obj in allAnimals)
        {
            obj.SetActive(true);
        }
    }


    private bool isUIPlaying;

    public void Move()
    {
        isUIPlaying = false;

        if (!isUIPlaying)
        {
            isUIPlaying = true;

            transform.DOMove(messageTransformByName[FootstepManager.currentlyClickedObjectName].transform.position, 1f)
                .OnComplete(() => isUIPlaying = false);
        }
    }

    public void ScaleUp()
    {

        transform.DOScale(targetScale, maximizingDuration).SetEase(Ease.OutBounce);
    }

    public void ScaleDown()
    {
       
        transform.DOScale(targetScale, minimizingDuration).SetEase(Ease.OutBounce);
    }
}