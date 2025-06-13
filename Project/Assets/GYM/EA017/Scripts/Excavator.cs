using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class Excavator : MonoBehaviour
{
    [SerializeField] private Animator excavatorAni;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveDuration;

    [SerializeField] private GameObject soil;

    private AnimationClip Digclip;

    private bool isDigging = false;

    public SoilCount soilCountClass;

    public Construction_GameManager manager;

    private bool btnTwiceIssue = false;

    private bool audioTwiceIssue = false;

    public AudioSource audioSource;

    private void Start()
    {
        manager = FindObjectOfType<Construction_GameManager>();

        audioSource = GetComponent<AudioSource>();
        excavatorAni = GetComponent<Animator>();
        Digclip = Resources.Load<AnimationClip>("Construction/Animation/digClip_Excavator");
        soil.SetActive(false);
    }

    public void StartExcavation()
    {
        manager.ClickSound();

        //int randomindex = Random.Range(0, 2);
        if (!btnTwiceIssue)
        {
            btnTwiceIssue = true;
            DOVirtual.DelayedCall(0.1f, () => btnTwiceIssue = false);

            if (!isDigging && !audioTwiceIssue)
            {
                isDigging = true;
                audioTwiceIssue = true;
                Sequence seq = DOTween.Sequence();

                seq.AppendCallback(() =>
                {
                    //audioSource.clip = manager.audioClipMove1;
                    audioSource.clip = manager.audioClipWork1;
                    audioSource.Play();
                    excavatorAni.SetBool("Move", true);
                    Vector3 targetPos = transform.position + transform.forward * moveDistance;
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad);
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() => { 
                    excavatorAni.SetBool("Move", false);
                    //audioSource.Stop();
                });

                seq.AppendCallback(() => { excavatorAni.SetBool("Dig", true);
                    //audioSource.clip = manager.audioClipWork1;
                    //audioSource.Play();
                });
                seq.AppendInterval(Digclip.length - 1);
                seq.AppendCallback(() => soil.SetActive(true));
                seq.AppendInterval(0.5f);
                seq.AppendCallback(() =>
                {
                    excavatorAni.SetBool("Dig", false);
                    //audioSource.Stop();
                });
                seq.AppendCallback(() => soilCountClass.SoilDecreaseStep(VehicleType.Excavator));

                seq.AppendInterval(0.5f);

                seq.AppendCallback(() =>
                {
                    excavatorAni.SetBool("Move", true);
                    Vector3 targetPos = transform.position - transform.forward * moveDistance;
                    transform.DOMove(targetPos, moveDuration).SetEase(Ease.OutQuad);
                    //audioSource.clip = manager.audioClipMove1;
                    //audioSource.Play();
                });
                seq.AppendInterval(moveDuration);
                seq.AppendCallback(() => { excavatorAni.SetBool("Move", false); audioSource.Stop(); });

                seq.AppendInterval(1f);
                seq.AppendCallback(() => soil.SetActive(false));
                seq.AppendCallback(() => { isDigging = false; audioTwiceIssue = false; });
            
            }

        }
    }
}