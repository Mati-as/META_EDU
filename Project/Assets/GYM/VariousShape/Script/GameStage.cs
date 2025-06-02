using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    private enum Stage
    {
        None, Square, Flower, Heart, Circle, Done
    }
    private Stage _stage = Stage.None;

    [SerializeField] private GameObject MainSquare;
    [SerializeField] private List<GameObject> square = new List<GameObject>(7);

    [SerializeField] private GameObject MainFlower;
    [SerializeField] private List<GameObject> flower = new List<GameObject>(7);

    [SerializeField] private GameObject MainHeart;
    [SerializeField] private List<GameObject> heart = new List<GameObject>(7);

    [SerializeField] private GameObject MainCircle;
    [SerializeField] private List<GameObject> circle = new List<GameObject>(7);

    //[SerializeField] private GameObject MainTriangle;
    //[SerializeField] private List<GameObject> triangle = new List<GameObject>(7);

    [SerializeField] private GameObject EndScene;

    Vector3 MainSquareScale = new Vector3(5.44f, 5.619f, 6.117f);
    Vector3 MainFlowerScale = new Vector3(2.4f, 2.48f, 2.7f);
    Vector3 MainHeartScale = new Vector3(1.3f, 1.3f, 1.3f);
    Vector3 MainCircleScale = new Vector3(6.22f, 6.43f, 7f);
    //Vector3 MainTriangleScale = new Vector3(6.22f, 6.43f, 7f);

    Vector3 shakeX = new Vector3(0, 0, 15f);

    public bool flowerStageClear = false;
    public bool heartStageClear = false;

    public bool twiceissue1 = false;
    public bool twiceissue2 = false;

    public bool isStageStart = false;

    [SerializeField] private List<ParticleSystem> victoryParticles;
    public AudioClip victoryAudioClip;


    private void Start()
    {
        victoryAudioClip = Resources.Load<AudioClip>("VariousShape/Audio/Victory");

    }

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
                        NextStage(Stage.Heart);
                        isStageStart = false;
                    }
                    break;

                case Stage.Heart:
                    if (!heart.Any(o => o.activeInHierarchy))
                    {
                        NextStage(Stage.Circle);
                        isStageStart = false;
                    }
                    break;

                case Stage.Circle:
                    if (!circle.Any(o => o.activeInHierarchy))
                    {
                        NextStage(Stage.Done);
                        Debug.Log(_stage);
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
            case Stage.Heart: StartHeartStage(); break;
            case Stage.Circle: StartCircleStage(); break;
            case Stage.Done: StartEndScene(); break;
        }
    }

    public void StartSquareStage()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("친구들 네모 안에 모여요!", "audio_10_친구들_네모_안에_모여요_"));
                MainSquare.SetActive(true);
                MainSquare.transform.DOScale(MainSquareScale, 1)
                    .From(0.01f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { MainSquare.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
            })
            .AppendInterval(10f)
            .AppendCallback(() =>
            {
                MainSquare.SetActive(false);
                Messenger.Default.Publish(new NarrationMessage("작은 네모들이 생겼어", "audio_11_작은_네모들이_생겼어_"));

                for (int i = 0; i < square.Count; i++)
                {
                    square[i].gameObject.SetActive(true);
                    square[i].transform.DOScale(Vector3.one * 1.1f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                }
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("작은 네모들을 터치해 주세요", "audio_12_작은_네모들을_터치해_주세요_"));
                isStageStart = true;
            });
    }

    public void StartFlowerStage()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() => 
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, victoryAudioClip);
                victoryParticles[0].Play();
                victoryParticles[1].Play();
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Debug.Log("꽃모양 스테이지 시퀀스 시작");
                Messenger.Default.Publish(new NarrationMessage("친구들 꽃 안에 모여요!", "audio_13_친구들_꽃_안에_모여요_"));
                MainFlower.SetActive(true);
                MainFlower.transform.DOScale(MainFlowerScale, 1)
                    .From(0.01f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { MainFlower.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
            })
            .AppendInterval(10f)

            .AppendCallback(() =>
            {
                MainFlower.SetActive(false);
                Messenger.Default.Publish(new NarrationMessage("작은 꽃들이 생겼어!", "audio_14_작은_꽃들이_생겼어_"));

                for (int i = 0; i < flower.Count; i++)
                {
                    flower[i].gameObject.SetActive(true);
                    flower[i].transform.DOScale(Vector3.one * 0.6f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                }
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("작은 꽃들을 터치해주세요!", "audio_15_작은_꽃들을_터치해주세요_"));
               
                flowerStageClear = true;
                isStageStart = true;
            });

    }
    public void StartHeartStage()
    {
        Sequence stageSeq = DOTween.Sequence()
           .AppendCallback(() =>
           {
               Managers.Sound.Play(SoundManager.Sound.Effect, victoryAudioClip);
               victoryParticles[0].Play();
               victoryParticles[1].Play();
           })
           .AppendInterval(3f)
           .AppendCallback(() =>
           {
               Debug.Log("하트모양 스테이지 시퀀스 시작");
               Messenger.Default.Publish(new NarrationMessage("친구들 하트 안에 모여요!", "audio_16_친구들_하트_안에_모여요_"));
               MainHeart.SetActive(true);
               MainHeart.transform.DOScale(MainHeartScale, 1)
                   .From(0.01f)
                   .SetEase(Ease.OutBack)
                   .OnComplete(() => { MainHeart.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
           })
           .AppendInterval(10f)

           .AppendCallback(() =>
           {
               MainHeart.SetActive(false);
               Messenger.Default.Publish(new NarrationMessage("작은 하트들이 생겼어!", "audio_17_작은_하트들이_생겼어_"));

               for (int i = 0; i < heart.Count; i++)
               {
                   heart[i].gameObject.SetActive(true);
                   heart[i].transform.DOScale(Vector3.one * 0.25f * 1.2f, 0.5f)
                                   .SetEase(Ease.InOutSine)
                                   .SetLoops(-1, LoopType.Yoyo);
               }
           })
           .AppendInterval(5f)
           .AppendCallback(() =>
           {
               Messenger.Default.Publish(new NarrationMessage("작은 하트들을 터치해주세요!", "audio_18_작은_하트들을_터치해주세요_"));

               heartStageClear = true;
               isStageStart = true;
           });
    }
    public void StartCircleStage()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, victoryAudioClip);
                victoryParticles[0].Play();
                victoryParticles[1].Play();
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Debug.Log("원 모양 스테이지 시퀀스 시작");
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
                isStageStart = true;
            });
    }

    public void StartEndScene()
    {
        Sequence stageSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, victoryAudioClip);
                victoryParticles[0].Play();
                victoryParticles[1].Play();
            })
            .AppendInterval(3f)
           .AppendCallback(() =>
           {
               EndScene.SetActive(true);
               EndScene.transform.DOScale(Vector3.one, 0.5f)
                                    .SetEase(Ease.InOutSine);
           });
    }
}
