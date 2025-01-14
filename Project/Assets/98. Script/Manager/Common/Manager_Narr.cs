using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Narr : MonoBehaviour
{
    private AudioSource Audio;

    [Header("[ COMPONENT CHECK ]")]
    //Common
    public int Content_Seq = 0;

    public AudioClip[] Audio_seq_narration;

    // Start is called before the first frame update
    void Start()
    {
        Audio = GetComponent<AudioSource>();

    }

    public void Change_Audio_narr(int Number_seq)
    {

        Content_Seq = Number_seq;
        //if (Content_Seq != 0)
        //{

        //}

        //0110 기존 기능 비활성화
        //Audio.clip = Audio_seq_narration[Content_Seq];
        //Audio.Play();
        //동작하는거 확인함
        //지금은 Sound 함수를 오버라이딩해서 내꺼 전용으로 하나 더 만들었고 씬내의 스크립트에서 로딩한 파일들을 제공하고 그걸 전달해서 실행시키는 방식임
        //근데 효과음, BGM은 기존에 있는 시스템 활용해도 될 것 같은데
        Managers.soundManager.Play(SoundManager.Sound.Narration, Audio_seq_narration[Content_Seq],1f);
    }

    public void Set_Audio_seq_narration(AudioClip[] audio)
    {
        Audio_seq_narration = audio;
    }
}