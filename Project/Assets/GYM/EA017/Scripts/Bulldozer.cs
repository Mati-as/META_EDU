using DG.Tweening;
using UnityEngine;

public class Bulldozer : MonoBehaviour
{
    [SerializeField] private Construction_GameManager manager;
    [SerializeField] private SoilCount soilCountClass;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator bulldozerAnimator;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveDuration;
    
    [SerializeField] private GameObject soil;

    private bool _btnTwiceIssue;     //버튼 중첩 방지용 
    private bool _audioTwiceIssue;
    private bool _isWork;

    [SerializeField] private AnimationClip workClip;

    [SerializeField] private float workClipLength;

    private void Start()
    {
        manager = FindAnyObjectByType<Construction_GameManager>();
        soilCountClass = FindAnyObjectByType<SoilCount>();

        audioSource = GetComponent<AudioSource>();
        bulldozerAnimator = GetComponent<Animator>();
        workClip = Resources.Load<AnimationClip>("Construction/Animation/workClip_Bulldozer");

        workClipLength = workClip.length;

        _btnTwiceIssue = false; //버튼 중첩 방지용 
        _audioTwiceIssue = false;
        _isWork = false;

    }

    public void StartBulldozerWork()
    {
        manager.ClickSound();
        
        float move = moveDistance + soilCountClass.plusMoveDistance;
        
        if (!_btnTwiceIssue)
        {
            _btnTwiceIssue = true;
            DOVirtual.DelayedCall(0.1f, () => _btnTwiceIssue = false);
            audioSource.clip = manager.heavyMachinerySound;

            if (!_isWork && !_audioTwiceIssue)
            {
                _isWork = true;
                _audioTwiceIssue = true;

                var seq = DOTween.Sequence();

                seq.AppendCallback(() =>
                {
                    audioSource.Play();
                    bulldozerAnimator.SetBool("Move", true);
                    var targetPos = transform.position + transform.forward * move;
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad);
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() => bulldozerAnimator.SetBool("Move", false));

                seq.AppendCallback(() => bulldozerAnimator.SetBool("Work", true));
                seq.AppendInterval(workClipLength - 1);
                seq.AppendCallback(() => soil.SetActive(false));
                seq.AppendInterval(0.5f);
                seq.AppendCallback(() => bulldozerAnimator.SetBool("Work", false));
                seq.AppendCallback(() => soilCountClass.SoilDecreaseStep(VehicleType.Bulldozer));

                seq.AppendInterval(0.5f);

                seq.AppendCallback(() =>
                {
                    bulldozerAnimator.SetBool("Move", true);
                    var targetPos = transform.position - transform.forward * move;
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad);
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() =>
                {
                    bulldozerAnimator.SetBool("Move", false);
                    audioSource.Stop();
                    soil.SetActive(true);
                });

                seq.AppendInterval(1f);
                seq.AppendCallback(() => { _isWork = false; _audioTwiceIssue = false; });

            }

        }
    }
}