using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;
#if UNITY_EDITOR 
using MyCustomizedEditor;
#endif


[CreateAssetMenu(fileName = "Underground_Animal_Data_Scriptable", menuName = "Underground/underground_allAnimals", order = int.MaxValue )]
public class GroundAnimalData :ScriptableObject
{

#if UNITY_EDITOR 
  [NamedArrayAttribute(new[]
  {
    "개미", "지렁이", "두더지", "땅거미", "쥐며느리", "달팽이", "뱀", "쇠똥구리", "개구리", "다람쥐", "토끼", "여우"
  })]
#endif

  public GameObject[] allAnimals = new GameObject[20];
}
