using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.UI;

public class UI_SoundSetting : UI_PopUp
{
    private Slider[] _volumeSliders = new Slider[(int)SoundManager.Sound.Max];

    public enum UI
    {
        MainVolume,
        BGMVolume,
        EffectVolume,
        NarrationVolume,
    }
    public override bool InitEssentialUI()
    {
           _volumeSliders = new Slider[(int)SoundManager.Sound.Max];

        _volumeSliders[(int)SoundManager.Sound.Main] = GetObject((int)UI.MainVolume).GetComponent<Slider>();
        _volumeSliders[(int)SoundManager.Sound.Main].value =
            Managers.Sound.volumes[(int)SoundManager.Sound.Main];
#if UNITY_EDITOR
        Debug.Log($" 메인 볼륨 {Managers.Sound.volumes[(int)SoundManager.Sound.Main]}");
#endif

        _volumeSliders[(int)SoundManager.Sound.Bgm] = GetObject((int)UI.BGMVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Effect] = GetObject((int)UI.EffectVolume).GetComponent<Slider>();

        _volumeSliders[(int)SoundManager.Sound.Narration] =
            GetObject((int)UI.NarrationVolume).GetComponent<Slider>();

        for (int i = 0; i < (int)SoundManager.Sound.Max; i++)
        {
            _volumeSliders[i].maxValue = Managers.Sound.VOLUME_MAX[i];
            _volumeSliders[i].value = Managers.Sound.volumes[i];
        }


        // default Volume값은 SoundManager에서 관리하며, 초기화 이후, UI Slider가 이를 참조하여 표출하도록 합니다.
        // default Value는 시연 테스트에 결과에 따라 수정가능합니다. 
        _volumeSliders[(int)SoundManager.Sound.Main].onValueChanged.AddListener(_ =>
        {
            Managers.Sound.volumes[(int)SoundManager.Sound.Main] =
                _volumeSliders[(int)SoundManager.Sound.Main].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Main].volume =
                Managers.Sound.volumes[(int)SoundManager.Sound.Main];

            Managers.Sound.volumes[(int)SoundManager.Sound.Bgm] =
                _volumeSliders[(int)SoundManager.Sound.Bgm].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Bgm].value);

            Managers.Sound.volumes[(int)SoundManager.Sound.Effect] =
                _volumeSliders[(int)SoundManager.Sound.Effect].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Effect].value);

            Managers.Sound.volumes[(int)SoundManager.Sound.Narration] =
                _volumeSliders[(int)SoundManager.Sound.Narration].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Narration],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Narration].value);

            //    A_SettingManager.SaveCurrentSetting(SensorManager.height,);
        });
        _volumeSliders[(int)SoundManager.Sound.Bgm].onValueChanged.AddListener(_ =>
        {
            Managers.Sound.volumes[(int)SoundManager.Sound.Bgm] =
                _volumeSliders[(int)SoundManager.Sound.Bgm].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Bgm].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Bgm],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Bgm].value);
        });

        _volumeSliders[(int)SoundManager.Sound.Effect].onValueChanged.AddListener(_ =>
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/TestSound/Test_Effect");

            Managers.Sound.volumes[(int)SoundManager.Sound.Effect] =
                _volumeSliders[(int)SoundManager.Sound.Effect].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Effect].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Effect],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Effect].value);
        });

        _volumeSliders[(int)SoundManager.Sound.Narration].onValueChanged.AddListener(_ =>
        {
            if (!Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].isPlaying)
                Managers.Sound.Play(SoundManager.Sound.Narration, "Audio/TestSound/Test_Narration");
            Managers.Sound.volumes[(int)SoundManager.Sound.Narration] =
                _volumeSliders[(int)SoundManager.Sound.Narration].value;
            Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].volume =
                Mathf.Lerp(0, Managers.Sound.VOLUME_MAX[(int)SoundManager.Sound.Narration],
                    Managers.Sound.volumes[(int)SoundManager.Sound.Main] *
                    _volumeSliders[(int)SoundManager.Sound.Narration].value);
        });

        
        return base.InitEssentialUI();
    }
}
