using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class ClickableRandomMover : MonoBehaviour
{
    [SerializeField] private Transform[] targetPoints;

    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private Ease moveEase = Ease.InOutQuad;
    [SerializeField] private float vanishDuration = 0.5f;
    [SerializeField] private Ease vanishEase = Ease.InBack;

    private Vector3  startPos;
    private Quaternion startRot;
    private Vector3  startScale;

    
    private int clickCount;
    [SerializeField] private bool isMoving;

    [SerializeField] private SideWalk_GameManager manager;

    private void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<SideWalk_GameManager>();
        }
        
        startPos   = transform.position;
        startRot   = transform.rotation;
        startScale = transform.localScale;
    }

   
    public void OnMove()
    {
        if (!isMoving)
        {
            clickCount++;
            manager.puzzleCounter++;

            isMoving = true;
            if (clickCount < 2)
            {
                DOTween.Sequence()
                    .Append(transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, vibrato: 1, elasticity: 0.5f))
                    .AppendCallback(MoveToRandomPoint)
                    .OnComplete(() =>
                    {
                        transform
                            .DOScale(startScale * 1.08f, 0.5f)
                            .SetEase(Ease.Linear)
                            .SetLoops(-1, LoopType.Yoyo);
                    });
                
            }
            else
            {
                Vanish();
            }
        }
    }

    private void MoveToRandomPoint()
    {
        if (targetPoints == null || targetPoints.Length == 0) return;

        int index = Random.Range(0, targetPoints.Length);
        var targetPos = targetPoints[index].position;
        transform.DOMove(targetPos, moveDuration).SetEase(moveEase).OnComplete(() => isMoving = false);
    }

    private void Vanish()
    {
        transform.DOScale(Vector3.zero, vanishDuration)
            .SetEase(vanishEase)
            .OnComplete(() => gameObject.SetActive(false));
        
        DOVirtual.DelayedCall(1f, () =>
        {
            transform.position       = startPos;
            transform.rotation       = startRot;
            transform.localScale     = startScale;
            clickCount = 0;
            isMoving = false;
        });
    }
}