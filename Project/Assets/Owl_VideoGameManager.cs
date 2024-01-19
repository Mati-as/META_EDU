using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Owl_VideoGameManager : Base_Interactable_VideoGameManager
{  private GameObject Owl_SpeechBubble;
    private float _defaultScale;
        
    protected override void Init()
    {
        base.Init();

        Managers.Sound.Play(SoundManager.Sound.Bgm,
            "Audio/Gamemaster Audio - Fun Casual Sounds/Ω_Bonus_Music/music_candyland",volume:0.125f);
        
        rewindParticlePrefabPath = "게임별분류/비디오컨텐츠/Owl/CFX/CFX_OnRewind";
        rewindParticleAudioPath =  "Audio/비디오 컨텐츠/Owl/Leaves";
            
        
        GameObject prefab = Resources.Load<GameObject>(rewindParticlePrefabPath);
     
        if(prefab ==null) Debug.LogError($"Particle is null. Resource Path : {rewindParticlePrefabPath}");
        else
        {
            GameObject rewindPsPosition = GameObject.Find("Position_Rewind_Particle");
            _particleOnRewind = Instantiate(prefab, rewindPsPosition.transform.position,
                rewindPsPosition.transform.rotation).GetComponent<ParticleSystem>();
        }
       
        //UI 관련 로직
        Owl_SpeechBubble = GameObject.Find(nameof(Owl_SpeechBubble));
        Debug.Assert(Owl_SpeechBubble!=null);


        _defaultScale = Owl_SpeechBubble.transform.localScale.x;
        Owl_SpeechBubble.transform.localScale = Vector3.zero;
        
        OnReplayStart += UI_OnReplayStart;
        OnReplayStart -= UI_OnReplayStart;
    }
    
    
    
    private void UI_OnReplayStart()
    {
        Owl_SpeechBubble.transform
            .DOScale(Vector3.one * _defaultScale, 3f)
            .SetEase(Ease.OutBounce)
            .SetDelay(2f);
        
#if UNITY_EDITOR
        Debug.Log("UI Replaying");
#endif

    }

    private void OnDestroy()
    {
        OnReplayStart -= UI_OnReplayStart;
    }
  




}
