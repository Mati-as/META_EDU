using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "AnimalData", menuName = "AnimalData/AnimalInfo", order = int.MaxValue )]
public class AnimalData : ScriptableObject
{

    [Header("Audio")] 
    public AudioClip aninalSound;
    public AudioClip UIAnimAudioA;
    public AudioClip UIAnimAudioB;
   
    [Header("Transform")]
    [Space(10f)]
    [HideInInspector]
    public Vector3 initialPosition;
    [HideInInspector]
    public Quaternion initialRotation;
    [Space(15f)]
    public Vector3 animalPositionOffset;
    [Header("Name")] [Space(10f)] 
    
    public string englishName;
    public string koreanName;
    [Space(10f)] [Header("Size")] [Space(10f)]
    [Range(0,10)]
    public float defaultSize;//초기 사이즈
    [Range(0,10)] 
    public float increasedSize; //정답 맞췄을 때 사이즈

    [Space(10f)] [Header("prefab")] [Space(10f)]
    public GameObject animalPrefab;
    
    [Space(20)] [Header("Animal Color Default and Fresnel Settings")]
    public Color outlineColor;
    
    [Space(20)] [Header("Position Settings")]
    
    [HideInInspector]
    public Transform inPlayPosition;
    
    [Header("Per Status Settings")]
    [Space(20)]
    [Header("Initial Setting")] [Space(5f)]
    [Header("On GameStart")] [Space(5f)]
    [Range(-200f,0f)]
    public float randomRotatableRangeMin;
    [Range(0,200f)]
    public float randomRotatableRangeMax;
    [Header("On Round Is Ready")] [Space(5f)]
    [Header("On Round Started")] [Space(5f)]
    public float moveInTime;
    [Range(0,10)]
    public float animationPlayIntervalMin;
    [Range(0,10)]
    public float animationPlayIntervalMax;
    public float animationDuration;
    public float rotationSpeedInRound;
   [Header("On Correct")] [Space(5f)]
    public float correctAnimTime;
    public float movingTimeSecWhenCorrectToSpotLight;
    public float movingTimeSecWhenCorrectToTouchDownSpot;
    public float rotationTimeWhenCorrect;
    public static Transform SPOTLIGHT_POSITION_FOR_ANIMAL;
    public static Transform LOOK_AT_POSITION;
    public static Transform TOUCH_DOWN_POSITION;
    [Header("On Round Finished")] [Space(5f)]
    [Header("On GameFinished")]
    public float finishedMoveInTime;

    
    // 09-01-23 애니메이션 로직 중  IDEL_ANIM 미사용.
   public static readonly int IDLE_ANIM = Animator.StringToHash("idle"); 
   public static readonly int ROLL_ANIM = Animator.StringToHash("Roll");
   public static readonly int FLY_ANIM = Animator.StringToHash("Fly");
   public static readonly int SPIN_ANIM = Animator.StringToHash("Spin");
   public static readonly int RUN_ANIM = Animator.StringToHash("Run");
   public static readonly int SELECTABLE_A = Animator.StringToHash("SelectableA");
   public static readonly int SELECTABLE_B = Animator.StringToHash("SelectableB");
   public static readonly int SELECTABLE_C= Animator.StringToHash("SelectableC");
   public static readonly int IS_GAME_FINISHED_ANIM = Animator.StringToHash(("IsGameFinished"));
   public static readonly int JUMP_ANIM = Animator.StringToHash(("Jump"));
   public static readonly int SIT_ANIM = Animator.StringToHash(("Sit"));
   public static readonly int BOUNCE_ANIM = Animator.StringToHash(("Bounce"));
}
