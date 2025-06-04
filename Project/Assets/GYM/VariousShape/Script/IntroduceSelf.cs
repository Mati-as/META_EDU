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

    public void Introduce(ShapeType shape)
    {
        introduceSeq = DOTween.Sequence();
        oriPosition = this.gameObject.transform.position;
        oriScale = this.gameObject.transform.localScale;
        float currentZ = transform.position.z;
        targetVector = new Vector3(0, 1.5f, currentZ);
        targetScale = this.gameObject.transform.localScale * 2f;

        switch (shape)
        {
            case ShapeType.Square:
                introduceSeq
                    .Append(gameObject.transform.DOMove(targetVector, 2f).SetEase(Ease.InSine))
                    .Join(gameObject.transform.DOScale(targetScale,2f).SetEase(Ease.Linear))
                    .JoinCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("네모", "audio_2_네모_"));
                        gameManager.isintroducing = true;
                    })
                    .AppendInterval(2F)
                    .Append(gameObject.transform.DOMove(oriPosition, 2f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 2f).SetEase(Ease.Linear))
                    .OnComplete(()=> gameManager.isintroducing = false);

                break;
            case ShapeType.Flower:
                introduceSeq
                    .Append(gameObject.transform.DOMove(targetVector, 2f).SetEase(Ease.InSine))
                    .Join(gameObject.transform.DOScale(targetScale, 2f).SetEase(Ease.Linear))
                    .JoinCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("꽃", "audio_4_꽃_"));
                        gameManager.isintroducing = true;
                    })
                    .AppendInterval(2F)
                    .Append(gameObject.transform.DOMove(oriPosition, 2f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 2f).SetEase(Ease.Linear))
                    .OnComplete(() => gameManager.isintroducing = false);
                break;
            case ShapeType.Star:
                introduceSeq
                    .Append(gameObject.transform.DOMove(targetVector, 2f).SetEase(Ease.InSine))
                    .Join(gameObject.transform.DOScale(targetScale, 2f).SetEase(Ease.Linear))
                    .JoinCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("별", "audio_30_별"));
                        gameManager.isintroducing = true;
                    })
                    .AppendInterval(2F)
                    .Append(gameObject.transform.DOMove(oriPosition, 2f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 2f).SetEase(Ease.Linear))
                    .OnComplete(() => gameManager.isintroducing = false);
                break;
            case ShapeType.Circle:
                introduceSeq
                    .Append(gameObject.transform.DOMove(targetVector, 2f).SetEase(Ease.InSine))
                    .Join(gameObject.transform.DOScale(targetScale, 2f).SetEase(Ease.Linear))
                    .JoinCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("동그라미", "audio_1_동그라미_"));
                        gameManager.isintroducing = true;
                    })
                    .AppendInterval(2F)
                    .Append(gameObject.transform.DOMove(oriPosition, 2f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 2f).SetEase(Ease.Linear))
                    .OnComplete(() => gameManager.isintroducing = false);
                break;
            case ShapeType.Triangle:
                introduceSeq
                    .Append(gameObject.transform.DOMove(targetVector, 2f).SetEase(Ease.InSine))
                    .Join(gameObject.transform.DOScale(targetScale, 2f).SetEase(Ease.Linear))
                    .JoinCallback(() =>
                    {
                        Messenger.Default.Publish(new NarrationMessage("세모", "audio_5_세모_"));
                        gameManager.isintroducing = true;
                    })
                    .AppendInterval(2F)
                    .Append(gameObject.transform.DOMove(oriPosition, 2f).SetEase(Ease.OutSine))
                    .Join(gameObject.transform.DOScale(oriScale, 2f).SetEase(Ease.Linear))
                    .OnComplete(() => gameManager.isintroducing = false);
                break;
        }
    }

    public void IntroduceSelfShape()
    {
        Introduce(myShape);
    }


}
