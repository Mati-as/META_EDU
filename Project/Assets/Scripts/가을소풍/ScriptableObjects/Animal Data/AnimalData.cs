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

}
