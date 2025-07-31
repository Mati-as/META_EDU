using UnityEngine;
using DG.Tweening;

public class ChildAnimatorController : MonoBehaviour
{
    [SerializeField] private GameObject leftChild;
    [SerializeField] private GameObject centerChild;
    [SerializeField] private GameObject rightChild;

    [SerializeField] private Vector3 leftChildTargetTransform = new Vector3(-269.28668212890627f, 34.73564910888672f, 113.57100677490235f);
    [SerializeField] private Vector3 rightChildTargetTransform = new Vector3(-271.13665771484377f, 34.73564910888672f, 97.71601104736328f);

    [SerializeField] private Vector3 leftStartVector3 = new Vector3(-269.28668212890627f, 34.73564910888672f, 98.74101257324219f);
    [SerializeField] private Vector3 rightStartVector3 = new Vector3(-271.13665771484377f, 34.73564910888672f, 113.40100860595703f);

    [SerializeField] private Vector3 rightEndVector3 = new Vector3(-269.836669921875f, 34.73564910888672f, 108.42100524902344f);
    [SerializeField] private Vector3 leftEndVector3 = new Vector3(-269.836669921875f, 34.73564910888672f, 103.5610122680664f);
    [SerializeField] private Vector3 centerEndVector3 = new Vector3(-270.63665771484377f, 34.73564910888672f, 106.09101104736328f);

    [SerializeField] private Animator leftChildAnimator;
    [SerializeField] private Animator centerChildAnimator;
    [SerializeField] private Animator rightChildAnimator;

    private void Start()
    {
        leftChild.transform.position = leftStartVector3;
        rightChild.transform.position = rightStartVector3;

        leftChild.SetActive(false);
        centerChild.SetActive(false);
        rightChild.SetActive(false);

    }
    
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    
    public void Walk()
    {
        leftChild.transform.position = leftStartVector3;
        rightChild.transform.position = rightStartVector3;

        leftChild.transform.rotation = Quaternion.identity;
        rightChild.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        
        //워크 애니메이션 진행
        DOVirtual.DelayedCall(2f, () =>
        {
            leftChild.SetActive(true);
            leftChildAnimator.SetBool(WalkHash, true);
            leftChild.transform.DOMove(leftChildTargetTransform, 8f).OnComplete(() =>
            {
                leftChild.SetActive(false);
                leftChildAnimator.SetBool(WalkHash, true);
            });
        });
        DOVirtual.DelayedCall(9f, () =>
        {
            rightChild.SetActive(true);
            rightChildAnimator.SetBool(WalkHash, true);
            DOVirtual.DelayedCall(0.5f, () =>
            {
                rightChild.transform.DOMove(rightChildTargetTransform, 7f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    rightChild.SetActive(false);
                    rightChildAnimator.SetBool(WalkHash, true);
                });
            });

        });
    }

    private static readonly int GoodHash = Animator.StringToHash("Good");
    
    public void End()
    {
        rightChild.transform.position = rightEndVector3;
        leftChild.transform.position = leftEndVector3;
        centerChild.transform.position = centerEndVector3;
          
        leftChild.SetActive(true);
        centerChild.SetActive(true);
        rightChild.SetActive(true);

        centerChild.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        leftChild.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        rightChild.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        //굿애니메이션 진행
        leftChildAnimator.SetBool(GoodHash, true);
        centerChildAnimator.SetBool(GoodHash, true);
        rightChildAnimator.SetBool(GoodHash, true);
    }



}
