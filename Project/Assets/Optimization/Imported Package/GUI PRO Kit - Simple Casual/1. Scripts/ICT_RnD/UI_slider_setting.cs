using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_slider_setting : MonoBehaviour
{
    // Start is called before the first frame update
    //설정 메뉴에서 전체 소리 조절하기 위한 함수
    //해당 하는 오브젝트 기능 인스펙터 창에서 체크하고 해당하는 함수 호출
    //각 슬라이더가 값이 바뀔경우 볼륨 조절 함수 호출

    private Slider volume_slider;
    public bool All_sound = false;
    public bool Effect = false;
    public bool BGM = false;
    public bool Narration = false;

    private float tmp_volume;
    private float volume;

    void Start()
    {
        volume_slider = this.gameObject.GetComponent<Slider>();
        volume = volume_slider.value;
        if (tmp_volume != volume)
        {
            if (All_sound == true)
            {
                //volume_slider.value = Manager_audio.instance.Get_all_volume();
            }
            else if (Effect == true)
            {
                volume_slider.value = Manager_audio.instance.Get_Effect_volume();
            }
            else if (BGM == true)
            {
                volume_slider.value = Manager_audio.instance.Get_BGM_volume();
            }
            else if (Narration == true)
            {
                volume_slider.value = Manager_audio.instance.Get_Narration_volume();
            }
        }
        //초기 값을 가져오는게 필요할 듯?
    }
   
    // Update is called once per frame
    void Update()
    {
        volume = volume_slider.value;
        if (tmp_volume != volume)
        {
            if (All_sound == true)
            {
                Manager_audio.instance.Set_all_sound_volume(volume_slider.value);
            }
            else if (Effect == true)
            {
                Manager_audio.instance.Set_effect_sound_volume(volume_slider.value);
            }
            else if (BGM == true)
            {
                Manager_audio.instance.Set_BGM_volume(volume_slider.value);
            }
            else if (Narration == true)
            {
                Manager_audio.instance.Set_Narration_volume(volume_slider.value);
            }
        }
        tmp_volume = volume;
    }
}
