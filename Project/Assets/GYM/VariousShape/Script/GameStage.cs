using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameStage : MonoBehaviour
{
    private enum Stage
    {
        None, Square, Flower, Star, Circle, Triangle, Done
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

    [SerializeField] private GameObject MainTriangle;
    [SerializeField] private List<GameObject> triangle = new List<GameObject>(7);

    [SerializeField] private GameObject EndScene;

    Vector3 MainSquareScale = Vector3.one * 1.1f;
    Vector3 MainFlowerScale = Vector3.one * 1.05f;
    Vector3 MainStarScale = Vector3.one * 1.1f;
    Vector3 MainCircleScale = Vector3.one * 0.95f;
    Vector3 MainTriangleScale = Vector3.one * 1.15f;

    Vector3 TargetSquareScale = Vector3.one * 0.2f;
    Vector3 TargetFlowerScale = Vector3.one * 0.55f;
    Vector3 TargetStarScale = Vector3.one * 0.25f;
    Vector3 TargetCircleScale = Vector3.one * 0.25f;
    Vector3 TargetTriangleScale = Vector3.one * 0.25f;

    Vector3 shakeX = new Vector3(0, 0, 15f);

    public bool flowerStageClear = false;
    public bool StarStageClear = false;

    public bool twiceissue1 = false;
    public bool twiceissue2 = false;

    public bool isStageStart = false;

    [SerializeField] private List<ParticleSystem> victoryParticles;
    public AudioClip victoryAudioClip;

    [SerializeField]
    private VariousShape_GameManager gameManager;

    [SerializeField]
    private ParticleSystem explosionParticle;


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
                        NextStage(Stage.Triangle);
                        Debug.Log(_stage);
                        isStageStart = false;
                    }
                    break;

                case Stage.Triangle:
                    if (!triangle.Any(o => o.activeInHierarchy))
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
        gameManager.isStageStart = false;
        switch (next)
        {
            case Stage.Square: StartSquareStage(); break;
            case Stage.Flower: StartFlowerStage(); break;
            case Stage.Star: StartStarStage(); break;
            case Stage.Circle: StartCircleStage(); break;
            case Stage.Triangle: StartTriangleStage(); break;
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
            .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                MainSquare.SetActive(false);
                Messenger.Default.Publish(new NarrationMessage("작은 네모들이 생겼어", "audio_11_작은_네모들이_생겼어_"));

                for (int i = 0; i < square.Count; i++)
                {
                    //square[i]를 로컬 변수에 담아야됨
                    var shape = square[i].gameObject;
                    float randomValue = Random.Range(0f, 0.5f);

                    shape.SetActive(true);
                    shape.transform.localScale = Vector3.one * 0.1f;

                    shape.transform
                        .DOScale(TargetSquareScale, 0.5f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            DOVirtual.DelayedCall(randomValue, () =>
                                 {
                                     shape.transform
                                    .DOScale(TargetSquareScale * 1.2f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                     .SetLoops(-1, LoopType.Yoyo);
                                 });
                        });
                }
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("작은 네모들을 터치해 주세요", "audio_12_작은_네모들을_터치해_주세요_"));
                isStageStart = true;
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                gameManager.isStageStart = true;
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
            .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                explosionParticle.Play();
                MainFlower.SetActive(false);
                Messenger.Default.Publish(new NarrationMessage("작은 꽃들이 생겼어!", "audio_14_작은_꽃들이_생겼어_"));

                for (int i = 0; i < flower.Count; i++)
                {
                    var shape = flower[i].gameObject;
                    var randomValue = Random.Range(0, 0.5f);

                    shape.SetActive(true);
                    shape.transform.localScale = Vector3.one * 0.1f;

                    shape.transform
                        .DOScale(TargetFlowerScale, 0.5f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            DOVirtual.DelayedCall(randomValue, () =>
                            {
                                shape.transform
                                .DOScale(TargetFlowerScale * 1.2f, 0.5f)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(-1, LoopType.Yoyo);
                            });
                        });
                }
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("작은 꽃들을 터치해주세요!", "audio_15_작은_꽃들을_터치해주세요_"));
               
                flowerStageClear = true;
                isStageStart = true;
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                gameManager.isStageStart = true;
            });

    }
    public void StartStarStage()
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
               Debug.Log("별모양 스테이지 시퀀스 시작");
               Messenger.Default.Publish(new NarrationMessage("친구들 별모양 안에 모여요!", "audio_31_친구들_별모양_안에_모여요_"));
               MainStar.SetActive(true);
               MainStar.transform.DOScale(MainStarScale, 1)
                   .From(0.01f)
                   .SetEase(Ease.OutBack)
                   .OnComplete(() => { MainStar.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
           })
           .AppendInterval(10f)
           .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(1f)
           .AppendCallback(() =>
           {
               explosionParticle.Play();
               MainStar.SetActive(false);
               Messenger.Default.Publish(new NarrationMessage("작은 별들이 생겼어!", "audio_32_작은_별들이_생겼어_"));

               for (int i = 0; i < star.Count; i++)
               {
                   var shape = star[i].gameObject;
                   float randomValue = Random.Range(0, 0.5f);

                   shape.SetActive(true);
                   shape.transform.localScale = Vector3.one * 0.1f;

                   shape.transform
                       .DOScale(TargetStarScale, 0.5f)
                       .SetEase(Ease.OutBack)
                       .OnComplete(() =>
                       {
                           DOVirtual.DelayedCall(randomValue, () =>
                           {
                               shape.transform
                               .DOScale(TargetStarScale * 1.2f, 0.5f)
                               .SetEase(Ease.InOutSine)
                               .SetLoops(-1, LoopType.Yoyo);
                           });
                       });
               }
           })
           .AppendInterval(3f)
           .AppendCallback(() =>
           {
               Messenger.Default.Publish(new NarrationMessage("작은 별들을 터치해주세요!", "audio_33_작은_별들을_터치해주세요_"));

               StarStageClear = true;
               isStageStart = true;
           })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                gameManager.isStageStart = true;
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
              Debug.Log("원모양 스테이지 시퀀스 시작");
              Messenger.Default.Publish(new NarrationMessage("친구들 동그라미 안에 모여요!", "audio_7_친구들_동그라미_안에_모여요_"));
              MainCircle.SetActive(true);
              MainCircle.transform.DOScale(MainCircleScale, 1)
                  .From(0.01f)
                  .SetEase(Ease.OutBack)
                  .OnComplete(() => { MainCircle.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
          })
          .AppendInterval(10f)
          .AppendCallback(() => explosionParticle.Play())
           .AppendInterval(1f)
          .AppendCallback(() =>
          {
              explosionParticle.Play();
              MainCircle.SetActive(false);
              Messenger.Default.Publish(new NarrationMessage("작은 동그라미들이 생겼어!", "audio_8_작은_동그라미들이_생겻어_"));

              for (int i = 0; i < circle.Count; i++)
              {
                  var shape = circle[i].gameObject;
                  float randomValue = Random.Range(0.0f, 0.5f);

                  shape.SetActive(true);
                  shape.transform.localScale = Vector3.one * 0.1f;

                  shape.transform
                      .DOScale(TargetCircleScale, 0.5f)
                      .SetEase(Ease.OutBack)
                      .OnComplete(() =>
                      {
                          DOVirtual.DelayedCall(randomValue, () =>
                          {
                              shape.transform
                              .DOScale(TargetCircleScale * 1.2f, 0.5f)
                              .SetEase(Ease.InOutSine)
                              .SetLoops(-1, LoopType.Yoyo);
                          });
                      });
              }
          })
          .AppendInterval(3f)
          .AppendCallback(() =>
          {
              Messenger.Default.Publish(new NarrationMessage("작은 동그라미들을 터치해주세요!", "audio_9_작은_동그라미를_터치해_주세요_"));

              isStageStart = true;
          })
           .AppendInterval(2f)
           .AppendCallback(() =>
           {
               gameManager.isStageStart = true;
           });
    }

    public void StartTriangleStage()
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
              Debug.Log("세모모양 스테이지 시퀀스 시작");
              Messenger.Default.Publish(new NarrationMessage("친구들 세모 안에 모여요!", "audio_19_친구들_세모_안에_모여요_"));
              MainTriangle.SetActive(true);
              MainTriangle.transform.DOScale(MainTriangleScale, 1)
                  .From(0.01f)
                  .SetEase(Ease.OutBack)
                  .OnComplete(() => { MainTriangle.transform.DOShakeRotation(duration: 0.5f, strength: shakeX); });
          })
          .AppendInterval(10f)
          .AppendCallback(() => explosionParticle.Play())
           .AppendInterval(1f)
          .AppendCallback(() =>
          {
              explosionParticle.Play();
              MainTriangle.SetActive(false);
              Messenger.Default.Publish(new NarrationMessage("작은 세모들이 생겼어!", "audio_20_작은_세모들이_생겼어_"));

              for (int i = 0; i < triangle.Count; i++)
              {
                  var shape = triangle[i].gameObject;
                  float randomValue = Random.Range(0.0f, 0.5f);

                  shape.SetActive(true);
                  shape.transform.localScale = Vector3.one * 0.1f;

                  shape.transform
                      .DOScale(TargetTriangleScale, 0.5f)
                      .SetEase(Ease.OutBack)
                      .OnComplete(() =>
                      {
                          DOVirtual.DelayedCall(randomValue, () =>
                          {
                              shape.transform
                              .DOScale(TargetTriangleScale * 1.2f, 0.5f)
                              .SetEase(Ease.InOutSine)
                              .SetLoops(-1, LoopType.Yoyo);
                          });
                      });
              }
          })
          .AppendInterval(3f)
          .AppendCallback(() =>
          {
              Messenger.Default.Publish(new NarrationMessage("작은 세모들을 터치해주세요!", "audio_21_작은_세모들을_터치해주세요_"));

              isStageStart = true;
          })
           .AppendInterval(2f)
           .AppendCallback(() =>
           {
               gameManager.isStageStart = true;
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
               Messenger.Default.Publish(new NarrationMessage("마지막으로 모양을 다시 알아볼까!", "audio_22_마지막으로_모양을_다시_알아볼까_"));

               EndScene.SetActive(true);
               EndScene.transform.DOScale(Vector3.one, 0.5f)
                                    .SetEase(Ease.InOutSine);
           });
    }
}
