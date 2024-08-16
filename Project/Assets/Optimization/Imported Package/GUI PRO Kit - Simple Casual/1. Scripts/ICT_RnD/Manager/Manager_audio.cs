//
// using UnityEngine;
//
// public class Manager_audio : MonoBehaviour
// {
//     public static Manager_audio instance = null;
//
//     private AudioSource Click;
//     private AudioSource BGM;
//     private AudioSource Narration;
//     private AudioSource Warm;
//     private AudioSource Carrot;
//     private AudioSource Corn;
//     private AudioSource Aloe;
//
//
//     //private float All_volume = 1f;
//     private float Effect_volume = 0.5f;
//     private float Narration_volume = 0.5f;
//     private float BGM_volume = 0.3f;
//
//
//     public GameObject Launcher;
//     private void Awake()
//     {
//         if (instance == null)
//         {
//             instance = this; 
//             DontDestroyOnLoad(gameObject); 
//         }
//         else
//         {
//             if (instance != this) 
//                 Destroy(this.gameObject); 
//         }
//     }
//
//     void Start()
//     {
//         Init_sound();
//     }
//
//     //public float Get_all_volume()
//     //{
//     //    return All_volume;
//     //}
//     public float Get_Effect_volume()
//     {
//         return Effect_volume;
//     }
//     public float Get_BGM_volume()
//     {
//         return BGM_volume;
//     }
//     public float Get_Narration_volume()
//     {
//         return Narration_volume;
//     }
//
//     public void Get_click()
//     {
//         Click.Play();
//         //Debug.Log("click");
//     }
//
//   
//     public void Get_bgm()
//     {
//         BGM.Play();
//         //Debug.Log("click");
//     }
//     public void Get_narration()
//     {
//         Narration.Play();
//         //Debug.Log("click");
//     }
//
//     public void Get_Correct_answer()
//     {
//         //Correct_answer.Play();
//         //Debug.Log("click");
//     }
//
//     public void Get_Wrong_answer()
//     {
//         //Wrong_answer.Play();
//         //Debug.Log("click");
//     }
//    
//     private void OnLevelWasLoaded(int level)
//     {
//         Init_sound();
//     }
//     private void Init_sound()
//     {
//
//         BGM = this.transform.GetChild(0).gameObject.GetComponent<AudioSource>();
//         Click = this.transform.GetChild(1).gameObject.GetComponent<AudioSource>();
//         Narration = Launcher.GetComponent<AudioSource>();
//
//         Warm = this.transform.GetChild(3).gameObject.GetComponent<AudioSource>(); 
//         Carrot = this.transform.GetChild(4).gameObject.GetComponent<AudioSource>();
//         Corn = this.transform.GetChild(5).gameObject.GetComponent<AudioSource>();
//         Aloe = this.transform.GetChild(6).gameObject.GetComponent<AudioSource>();
//
//         //Correct_answer = this.transform.GetChild(7).gameObject.GetComponent<AudioSource>();
//         //Wrong_answer = this.transform.GetChild(10).gameObject.GetComponent<AudioSource>();
//
//         Set_effect_sound_volume(Effect_volume);
//         Set_BGM_volume(BGM_volume);
//     }
//     public void Set_all_sound_volume(float volume)
//     {
//         if (volume == 0)
//         {
//             Click.mute = true;
//             BGM.mute = true;
//
//         }
//         else if (volume == 1)
//         {
//             Click.mute = false;
//             BGM.mute = false;
//
//         }
//     }
//
//     public void Set_effect_sound_volume(float volume)
//     {
//         Click.volume = volume;
//     }
//
//     public void Set_BGM_volume(float volume)
//     {
//         BGM.volume = volume;
//         Warm.volume = volume;
//         Carrot.volume = volume;
//         Corn.volume = volume;
//         Aloe.volume = volume;
//     }
//     public void Set_Narration_volume(float volume)
//     {
//         Narration.volume = volume;
//     }
//
// }
