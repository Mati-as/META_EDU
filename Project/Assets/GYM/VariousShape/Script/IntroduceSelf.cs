using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;

public class IntroduceSelf : MonoBehaviour
{
    public enum ShapeType
    {
        Square, Flower, Star, Circle, Triangle
    }

    [SerializeField] private ShapeType myShape;

    Sequence introduceSeq;

    private Vector3 oriPosition;
    private Vector3 oriScale;

    private Vector3 targetVector;
    private Vector3 targetScale;

    public VariousShape_GameManager gameManager;
    public GameStage gameStage;

    Vector3 shakeStrength = new Vector3(0.2f, 0f, 0f);

    private Tween currentAni;

    private void OnEnable()
    {
        DOVirtual.DelayedCall(0.7f, () => RandomAni());
    }

    public void RandomAni()
    {
        if (currentAni != null && currentAni.IsActive())
        {
            currentAni.Kill();
            currentAni = null;
        }

        int ranvalue = Random.Range(0, 3);
        switch (ranvalue)
        {
            case 0:
                currentAni = transform
                    .DOScale(transform.localScale * 1.2f, 1f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
                break;

            case 1:
                currentAni = DOTween.Sequence()
                    .Append(
                        transform
                            .DOLocalJump(
                                endValue: transform.localPosition,
                                jumpPower: 0.5f,
                                numJumps: 1,
                                duration: 0.5f
                            )
                            .SetEase(Ease.Linear)
                    )
                    .AppendInterval(2f)
                    .SetLoops(-1, LoopType.Restart);
                break;

            case 2:
                currentAni = DOTween.Sequence()
                    .Append(
                        transform
                            .DOShakePosition(
                                duration: 1f,
                                strength: shakeStrength,
                                vibrato: 5,
                                randomness: 90f,
                                snapping: false,
                                fadeOut: true
                            )
                    )
                    .AppendInterval(2f)
                    .SetLoops(-1, LoopType.Restart);
                break;
        }
    
    }


    public void Introduce(ShapeType shape)
    {
        if (!gameStage.endStageStart) return;

        if (currentAni != null && currentAni.IsActive())
        {
            currentAni.Kill();
            currentAni = null;
        }

        introduceSeq = DOTween.Sequence();
        oriPosition = this.gameObject.transform.position;
        oriScale = this.gameObject.transform.localScale;
        float currentZ = transform.position.z;
        targetVector = new Vector3(0, 1.2f, currentZ);
        targetScale = this.gameObject.transform.localScale * 3f;

        char randomLetter = (char)('A' + Random.Range(0, 6));
        Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/Click_{randomLetter}");

        switch (shape)
        {
            case ShapeType.Square:
                introduceSeq
                    .AppendCallback(() => gameManager.isintroducing = true)
                    .Append(gameObject.transform.DOMove(targetVector, 1.5f).SetEase(Ease.OutBack))
                    .Join(gameObject.transform.DOScale(targetScale, 1.5f).SetEase(Ease.Linear))
                    .AppendCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("네모", "audio_2_네모_"));
                    })
                    .AppendInterval(2.5F)
                    .Append(gameObject.transform.DOMove(oriPosition, 1.5f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 1.5f).SetEase(Ease.Linear))
                    .OnComplete(() =>
                    {
                        gameManager.isintroducing = false;
                        RandomAni();
                    });
                break;
            case ShapeType.Flower:
                introduceSeq
                    .AppendCallback(() => gameManager.isintroducing = true)
                    .Append(gameObject.transform.DOMove(targetVector, 1.5f).SetEase(Ease.OutBack))
                    .Join(gameObject.transform.DOScale(targetScale, 1.5f).SetEase(Ease.Linear))
                    .AppendCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("꽃", "audio_4_꽃_"));
                    })
                    .AppendInterval(2.5F)
                    .Append(gameObject.transform.DOMove(oriPosition, 1.5f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 1.5f).SetEase(Ease.Linear))
                    .OnComplete(() =>
                    {
                        gameManager.isintroducing = false;
                        RandomAni();
                    });
                break;
            case ShapeType.Star:
                introduceSeq
                    .AppendCallback(() => gameManager.isintroducing = true)
                    .Append(gameObject.transform.DOMove(targetVector, 1.5f).SetEase(Ease.OutBack))
                    .Join(gameObject.transform.DOScale(targetScale, 1.5f).SetEase(Ease.Linear))
                    .AppendCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("별", "audio_30_별"));
                    })
                    .AppendInterval(2.5F)
                    .Append(gameObject.transform.DOMove(oriPosition, 1.5f).SetEase(Ease.OutBack))
                    .Join(gameObject.transform.DOScale(oriScale, 1.5f).SetEase(Ease.Linear))
                    .OnComplete(() =>
                    {
                        gameManager.isintroducing = false;
                        RandomAni();
                    });
                break;
            case ShapeType.Circle:
                introduceSeq
                    .AppendCallback(() => gameManager.isintroducing = true)
                    .Append(gameObject.transform.DOMove(targetVector, 1.5f).SetEase(Ease.OutBack))
                    .Join(gameObject.transform.DOScale(targetScale, 1.5f).SetEase(Ease.Linear))
                    .AppendCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("동그라미", "audio_1_동그라미_"));
                    })
                    .AppendInterval(2.5F)
                    .Append(gameObject.transform.DOMove(oriPosition, 1.5f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 1.5f).SetEase(Ease.Linear))
                    .OnComplete(() =>
                    {
                        gameManager.isintroducing = false;
                        RandomAni();
                    });
                break;
            case ShapeType.Triangle:
                introduceSeq
                    .AppendCallback(() => gameManager.isintroducing = true)
                    .Append(gameObject.transform.DOMove(targetVector, 1.5f).SetEase(Ease.OutBack))
                    .Join(gameObject.transform.DOScale(targetScale, 1.5f).SetEase(Ease.Linear))
                    .AppendCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("세모", "audio_5_세모_"));
                    })
                    .AppendInterval(2.5F)
                    .Append(gameObject.transform.DOMove(oriPosition, 1.5f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 1.5f).SetEase(Ease.Linear))
                    .OnComplete(() =>
                    {
                        gameManager.isintroducing = false;
                        RandomAni();
                    });
                break;
        }
    }

    public void IntroduceSelfShape()
    {
        Introduce(myShape);
    }


}
