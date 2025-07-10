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
        None, Square, Flower, Star, Circle, Triangle, Done
    }
    private Stage _stage = Stage.None;

    public bool endStageStart = false;

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

    public bool flowerStageClear = false;
    public bool starStageClear = false;

    public bool twiceIssue1 = false;
    public bool twiceIssue2 = false;

    public bool isStageStart = false;

    [SerializeField] private List<ParticleSystem> victoryParticles;
    public AudioClip victoryAudioClip;

    [SerializeField]
    private VariousShape_GameManager gameManager;

    [SerializeField]
    private ParticleSystem explosionParticle;

    public ParticleSystem centerMagicParticle;

    private void Start()
    {
        victoryAudioClip = Resources.Load<AudioClip>("VariousShape/Audio/Victory");

    }
    
    private float _elapsedTime = 0f;
    private int   _secondsCount = 0;

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= 1f)
        {
            _secondsCount++;

            _elapsedTime -= 1f;

            Debug.Log($"경과한 초: {_secondsCount}");
        }
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
                    else if (_secondsCount == 31)
                    {
                        foreach (var t in square)
                        {
                            t.SetActive(false);
                        }
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
                    else if (_secondsCount == 31)
                    {
                        foreach (var t in flower)
                        {
                            t.SetActive(false);
                        }
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
                    else if (_secondsCount == 31)
                    {
                        foreach (var t in star)
                        {
                            t.SetActive(false);
                        }
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
                    else if (_secondsCount == 31)
                    {
                        foreach (var t in circle)
                        {
                            t.SetActive(false);
                        }
                        NextStage(Stage.Triangle);
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
                    else if (_secondsCount == 31)
                    {
                        foreach (var t in triangle)
                        {
                            t.SetActive(false);
                        }
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
                Managers.Sound.Play(SoundManager.Sound.Effect, "VariousShape/Audio/OnBallInPipe");
            })
            .Append(MainSquare.transform.DOScale(MainSquareScale, 1f).From(0.01f).SetEase(Ease.InOutBack))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                MainSquare.transform
                    .DOScale(MainSquareScale * 0.95f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                centerMagicParticle.Play();
            })
            .AppendInterval(9f)
            .AppendCallback(() => centerMagicParticle.Stop())
            .AppendInterval(0.5f)
            .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(
                    new NarrationMessage("작은 네모들을 터치해 주세요", "audio_1_작은_네모들이_생겼어요_작은_네모들을_터치해_주세요_"));
                
                MainSquare.SetActive(false);
                
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
                                    .DOScale(TargetSquareScale * 1.1f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                            });
                        });
                }
            })
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                isStageStart = true;
                _secondsCount = 0;
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
                Messenger.Default.Publish(new NarrationMessage("친구들 꽃 안에 모여요!", "audio_13_친구들_꽃_안에_모여요_"));
                MainFlower.SetActive(true);
                Managers.Sound.Play(SoundManager.Sound.Effect, "VariousShape/Audio/OnBallInPipe");
            })
            .Append(MainFlower.transform.DOScale(MainFlowerScale, 1f).From(0.01f).SetEase(Ease.InOutBack))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                MainFlower.transform
                    .DOScale(MainFlowerScale * 0.95f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                centerMagicParticle.Play();
            })
            .AppendInterval(9f)
            .AppendCallback(() => centerMagicParticle.Stop())
            .AppendInterval(0.5f)
            .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                explosionParticle.Play();
                MainFlower.SetActive(false);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
               Messenger.Default.Publish(new NarrationMessage("작은 꽃들을 터치해주세요!", "audio_4_작은_꽃들이_생겼어요_작은_꽃들을_터치해_주세요_"));
                
                flowerStageClear = true;
                
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
                                    .DOScale(TargetFlowerScale * 1.1f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                            });
                        });
                }
            })
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                isStageStart = true;
                _secondsCount = 0;
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
                Messenger.Default.Publish(new NarrationMessage("친구들 별모양 안에 모여요!", "audio_31_친구들_별모양_안에_모여요_"));
                MainStar.SetActive(true);
                Managers.Sound.Play(SoundManager.Sound.Effect, "VariousShape/Audio/OnBallInPipe");
            })
            .Append(MainStar.transform.DOScale(MainStarScale, 1f).From(0.01f).SetEase(Ease.InOutBack))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                MainStar.transform
                    .DOScale(MainStarScale * 0.95f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                centerMagicParticle.Play();
            })
            .AppendInterval(9f)
            .AppendCallback(() => centerMagicParticle.Stop())
            .AppendInterval(0.5f)
            .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                explosionParticle.Play();
                MainStar.SetActive(false);
            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("작은 별들을 터치해주세요!", "audio_2_작은_별들이_생겼어요_작은_별들을_터치해_주세요_"));

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
                                    .DOScale(TargetStarScale * 1.1f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                            });
                        });
                }

                starStageClear = true;

            })
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                isStageStart = true;
                _secondsCount = 0;
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
                Messenger.Default.Publish(new NarrationMessage("친구들 동그라미 안에 모여요!", "audio_7_친구들_동그라미_안에_모여요_"));
                MainCircle.SetActive(true);
                Managers.Sound.Play(SoundManager.Sound.Effect, "VariousShape/Audio/OnBallInPipe");
            })
            .Append(MainCircle.transform.DOScale(MainCircleScale, 1f).From(0.01f).SetEase(Ease.InOutBack))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                MainCircle.transform
                    .DOScale(MainCircleScale * 0.95f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                centerMagicParticle.Play();
            })
            .AppendInterval(9f)
            .AppendCallback(() => centerMagicParticle.Stop())
            .AppendInterval(0.5f)
            .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                explosionParticle.Play();
                MainCircle.SetActive(false);

            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("작은 동그라미들을 터치해주세요!",
                        "audio_3_작은_동그라미들이_생겼어요_작은_동그라미들을_터치해_주세요_"));

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
                                    .DOScale(TargetCircleScale * 1.1f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                            });
                        });
                }

            })
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                _secondsCount = 0;
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
                Messenger.Default.Publish(new NarrationMessage("친구들 세모 안에 모여요!", "audio_19_친구들_세모_안에_모여요_"));
                MainTriangle.SetActive(true);
                Managers.Sound.Play(SoundManager.Sound.Effect, "VariousShape/Audio/OnBallInPipe");
            })
            .Append(MainTriangle.transform.DOScale(MainTriangleScale, 1f).From(0.01f).SetEase(Ease.InOutBack))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                MainTriangle.transform
                    .DOScale(MainTriangleScale * 0.95f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                centerMagicParticle.Play();
            })
            .AppendInterval(9f)
            .AppendCallback(() => centerMagicParticle.Stop())
            .AppendInterval(0.5f)
            .AppendCallback(() => explosionParticle.Play())
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                explosionParticle.Play();
                MainTriangle.SetActive(false);

            })
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("작은 세모들을 터치해주세요!",
                    "audio_0_작은_세모들이_생겼어요_작은_세모들을_터치해_주세요_"));
                
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
                                    .DOScale(TargetTriangleScale * 1.1f, 0.5f)
                                    .SetEase(Ease.InOutSine)
                                    .SetLoops(-1, LoopType.Yoyo);
                            });
                        });
                }

                

            })
            .AppendInterval(0.1f)
            .AppendCallback(() =>
            {
                isStageStart = true;
                _secondsCount = 0;
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
           })
           .AppendInterval(3f)
           .AppendCallback(() =>
           {
               EndScene.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);
               endStageStart = true;
           });
    }
}
