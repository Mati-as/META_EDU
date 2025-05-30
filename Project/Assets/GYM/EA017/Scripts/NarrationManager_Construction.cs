using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NarrationItem_Construction
{
    public AudioClip audioClip;
    public Sprite image;

    public NarrationItem_Construction(AudioClip audioClip, Sprite image)
    {
        this.audioClip = audioClip;
        this.image = image;
    }
}

public class NarrationManager_Construction : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Image narrationImage;

    private List<NarrationItem_Construction> narrationList = new List<NarrationItem_Construction>();

    private void Start()
    {
        LoadNarrations();
    }

    private void LoadNarrations()
    {
        narrationList.Add(new NarrationItem_Construction(Resources.Load<AudioClip>("Audio/Narration1"), Resources.Load<Sprite>("Images/Img1")));
        narrationList.Add(new NarrationItem_Construction(Resources.Load<AudioClip>("Audio/Narration2"), Resources.Load<Sprite>("Images/Img2")));
    }

    // 특정 타이밍에 나레이션을 실행하는 메서드
    public void PlayNarration(int index)
    {
        if (index >= 0 && index < narrationList.Count)
        {
            NarrationItem_Construction item = narrationList[index];

            audioSource.clip = item.audioClip;
            //narrationList[0, 1];
            Managers.Sound.Play(SoundManager.Sound.Narration, "adfi"); // 기존 사운드 매니저 플레이 

            narrationImage.sprite = item.image;

        }
        else
        {
            Debug.LogError("나레이션 인덱스 초과나 미만입니다");
        }
    }
}
