using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    private enum Stage
    {
        None, Square, Flower, Star, Circle, Done
    }
    private Stage _stage = Stage.None;

    [SerializeField] private GameObject MainSquare;
    [SerializeField] private List<GameObject> square = new List<GameObject>(7);

    [SerializeField] private GameObject MainFlower;
    [SerializeField] private List<GameObject> flower = new List<GameObject>(7);

    [SerializeField] private GameObject MainStar;
    [SerializeField] private List<GameObject> star = new List<GameObject>(7);

    [SerializeField] private GameObject MainCircle;
    [SerializeField] private List<GameObject> circle = new List<GameObject>(7);

    Vector3 MainSquareScale = new Vector3(5.44f, 5.619f, 6.117f);
    Vector3 MainFlowerScale = new Vector3(2.4f, 2.48f, 2.7f);
    Vector3 MainStarScale = new Vector3(4.44f, 4.6f, 5f);
    Vector3 MainCircleScale = new Vector3(6.22f, 6.43f, 7f);

    Vector3 shakeX = new Vector3(0, 0, 15f);

    public bool flowerStageClear = false;
    public bool starStageClear = false;

    public bool twiceissue1 = false;
    public bool twiceissue2 = false;

    public bool isStageStart = false;

    void Update()
    {
        if (isStageStart)
        {
            switch (_stage)
            {
                case Stage.Square:
                    if (!square.Any(o => o.activeInHierarchy))
                    {
                        NextStage(Stage.Flower);
                        isStageStart = false;
                    }
                    break;

                case Stage.Flower:
                    if (!flower.Any(o => o.activeInHierarchy))
                    {
                        NextStage(Stage.Star);
                        isStageStart = false;
                    }
                    break;

                case Stage.Star:
                    if (!star.Any(o => o.activeInHierarchy))
                    {
                        NextStage(Stage.Circle);
                        isStageStart = false;
                    }
                    break;

                case Stage.Circle:
                    if (!circle.Any(o => o.activeInHierarchy))
                    {
                        NextStage(Stage.Done);
                        isStageStart = false;
                    }
                    break;

                case Stage.Done:
                    break;
            }
        }
    }

    public void OnGameStart()
    {
        if (_stage == Stage.None)
            NextStage(Stage.Square);
    }

    private void NextStage(Stage next)
    {
        _stage = next;
        switch (next)
        {
            case Stage.Square: StartSquareStage(); break;
            case Stage.Flower: StartFlowerStage(); break;
            case Stage.Star: StartStarStage(); break;
            case Stage.Circle: StartCircleStage(); break;
            case Stage.Done:
                Debug.Log("All stages complete!");
                break;
        }
    }

    public void StartSquareStage()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Debug.Log("사각형 스테이지 시퀀스 시작");
                MainSquare.SetActive(true);
                MainSquare.transform.DOScale(MainSquareScale, 1)
                    .From(0.01f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { MainSquare.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
            })
            //꽃 모양 위로 올라가야해요!
            .AppendInterval(10f)  //10.9.8 1초까지 대기 
                                  //화면 중앙에 펑 터지는 이펙트 생성
            .AppendCallback(() =>
            {
                MainSquare.SetActive(false);
                for (int i = 0; i < square.Count; i++)
                {
                    square[i].gameObject.SetActive(true);
                    square[i].transform.DOScale(Vector3.one * 1.1f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                }
                //클릭시 랜덤 위치로 이동 
                //3번 클릭시 비활성화 
                //카운트해서 7개가 비활성화되면 성공 효과음, 이펙트 재생
                //다음 사각형 시퀀스 시작
                //모양마다 반복
                isStageStart = true;
            });
    }

    public void StartFlowerStage()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Debug.Log("꽃모양 스테이지 시퀀스 시작");
                MainFlower.SetActive(true);
                MainFlower.transform.DOScale(MainFlowerScale, 1)
                    .From(0.01f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { MainFlower.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
            })
            //꽃 모양 위로 올라가야해요!
            .AppendInterval(10f)  //10.9.8 1초까지 대기 
                                  //화면 중앙에 펑 터지는 이펙트 생성
            .AppendCallback(() =>
            {
                MainFlower.SetActive(false);
                for (int i = 0; i < flower.Count; i++)
                {
                    flower[i].gameObject.SetActive(true);
                    flower[i].transform.DOScale(Vector3.one * 0.6f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                }
                //클릭시 랜덤 위치로 이동 
                //3번 클릭시 비활성화 
                //카운트해서 7개가 비활성화되면 성공 효과음, 이펙트 재생
                //다음 사각형 시퀀스 시작
                //모양마다 반복
                flowerStageClear = true;
                isStageStart = true;
            });
    }
    public void StartStarStage()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Debug.Log("별모양 스테이지 시퀀스 시작");
                MainStar.SetActive(true);
                MainStar.transform.DOScale(MainStarScale, 1)
                    .From(0.01f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { MainStar.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
            })
            //꽃 모양 위로 올라가야해요!
            .AppendInterval(10f)  //10.9.8 1초까지 대기 
                                  //화면 중앙에 펑 터지는 이펙트 생성
            .AppendCallback(() =>
            {
                MainStar.SetActive(false);
                for (int i = 0; i < star.Count; i++)
                {
                    star[i].gameObject.SetActive(true);
                    star[i].transform.DOScale(Vector3.one * 1.2f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                }
                //클릭시 랜덤 위치로 이동 
                //3번 클릭시 비활성화 
                //카운트해서 7개가 비활성화되면 성공 효과음, 이펙트 재생
                //다음 사각형 시퀀스 시작
                //모양마다 반복
                starStageClear = true;
                isStageStart = true;
            });
    }
    public void StartCircleStage()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Debug.Log("꽃모양 스테이지 시퀀스 시작");
                MainCircle.SetActive(true);
                MainCircle.transform.DOScale(MainCircleScale, 1)
                    .From(0.01f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { MainCircle.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
            })
            //꽃 모양 위로 올라가야해요!
            .AppendInterval(10f)  //10.9.8 1초까지 대기 
                                  //화면 중앙에 펑 터지는 이펙트 생성
            .AppendCallback(() =>
            {
                MainCircle.SetActive(false);
                for (int i = 0; i < circle.Count; i++)
                {
                    circle[i].gameObject.SetActive(true);
                    circle[i].transform.DOScale(Vector3.one * 1.6f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                }
                //클릭시 랜덤 위치로 이동 
                //3번 클릭시 비활성화 
                //카운트해서 7개가 비활성화되면 성공 효과음, 이펙트 재생
                //다음 사각형 시퀀스 시작
                //모양마다 반복
            });
    }

}
