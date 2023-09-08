using UnityEngine;



[CreateAssetMenu(fileName = "AnimalData", menuName = "AnimalData/AnimalInfo", order = int.MaxValue )]
public class AnimalData : ScriptableObject
{
    [Header("Name")] [Space(10f)]
    public string animalName;
    [Space(10f)] [Header("Size")] [Space(10f)]
    public float defaultSize; //초기 사이즈
    public float increasedSize; //정답 맞췄을 때 사이즈

    [Space(10f)] [Header("prefab")] [Space(10f)]
    public GameObject animalPrefab;
    
    [Space(20)] [Header("Animal Color Default and Fresnel Settings")]
    public Color outlineColor;

    
    // 09-01-23 애니메이션 로직 중  IDEL_ANIM 미사용.
   public static readonly int IDLE_ANIM = Animator.StringToHash("idle"); 
   public static readonly int ROLL_ANIM = Animator.StringToHash("Roll");
   public static readonly int FLY_ANIM = Animator.StringToHash("Fly");
   public static readonly int SPIN_ANIM = Animator.StringToHash("Spin");
   public static readonly int RUN_ANIM = Animator.StringToHash("Run");
   public static readonly int SELECTABLE = Animator.StringToHash("Selectable");
   public static readonly int SELECTABLE_A = Animator.StringToHash("SelectableA");
   public static readonly int SELECTABLE_B= Animator.StringToHash("SelectableB");
   public static readonly int IS_GAME_FINISHED_ANIM = Animator.StringToHash(("IsGameFinished"));
}
