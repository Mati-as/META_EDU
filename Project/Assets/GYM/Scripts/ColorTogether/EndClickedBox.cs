using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class EndClickedBox : MonoBehaviour
{
    [SerializeField] private Vector3 startTransform;
    [SerializeField] private Vector3 startScale = new Vector3(2.8f, 5.5f, 2);
    [SerializeField] private Vector3 targetScale = new Vector3(5, 8, 4);
    public Transform showPosition;
    ColorTogether_Manager manager;
    public int narrationPathNum;

    private void Start()
    {
        manager = FindObjectOfType<ColorTogether_Manager>();
        startTransform = transform.position;
    }

    public void Clicked()
    {
        manager.endCanClicked = false;

        var sequence = DOTween.Sequence();

        sequence.AppendCallback(() =>
        {
            manager.endCanClicked = false;
            transform.DOScale(targetScale, 2f);
            transform.DOJump(showPosition.position, 1.5f, 1, 2f).SetEase(Ease.OutQuad);
            transform.DOShakePosition(0.3f, 0.4f);
        });
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() => transform.DOShakeRotation(1.5f, 30f, 10, 90f).SetEase(Ease.OutQuad));
        sequence.AppendCallback(() => EndClickedBoxNarrationAll(40 + narrationPathNum, 40 + narrationPathNum));
        sequence.AppendInterval(2);
        sequence.AppendCallback(() =>
        {
            transform.DOJump(startTransform, 1.5f, 1, 2f).SetEase(Ease.OutQuad);
            transform.DOShakePosition(0.3f, 0.2f, 10, 90, false);
            transform.DOScale(startScale, 2f).SetEase(Ease.Linear);
            manager.endCanClicked = true;
        });

    }
    void EndClickedBoxNarrationAll(int audioPath, int narrationImgPath)
    {
        string AudioPath = "Audio/ColorTogether/audio_";
        AudioClip clip = Resources.Load<AudioClip>(AudioPath + audioPath);
        float cliplength = clip.length;
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);

        manager.narrationBG.sizeDelta = new Vector2(440, 143);

        manager.narrationImgGameObject.SetActive(true);

        manager.TakeTextImg("Image/ColorTogether/Img_" + narrationImgPath);
        DOVirtual.DelayedCall(cliplength + 2f, () =>
        {
            manager.narrationImgGameObject.transform.localScale = Vector3.one;
            manager.narrationImgGameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 10, 1);
            manager.narrationImgGameObject.SetActive(false);
        });
    }

}