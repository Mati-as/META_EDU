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

        Managers.Sound.Play(SoundManager.Sound.Narration, Audio_seq_narration[Content_Seq],1f);
    }

    public void Set_Audio_seq_narration(AudioClip[] audio)
    {
        Audio_seq_narration = audio;
    }
}