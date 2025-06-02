using DG.Tweening;
using UnityEngine;

public class IntroduceSelf : MonoBehaviour
{
    public enum ShapeType
    {
        Square, Flower, Star, Circle
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
                        Debug.Log("사각형 나레이션");
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
                        Debug.Log("꽃 나레이션");
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
                        Debug.Log("별 나레이션");
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
                        Debug.Log("원 나레이션");
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
