using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Mondrian_GameManager : IGameManager
{
  //자식객체에서 중복값을 피해서 가져오기 위한 HashSet설정
  private HashSet<Color> mondrian_CubeColors;
  
  private GameObject _cubes;
  private Color[] _colors;
 

  private Dictionary<int, MeshRenderer> _meshRendererMap;
  private MeshRenderer[] _meshRenderers; 
  protected override void Init()
  {
    base.Init();
    
    _meshRendererMap = new Dictionary<int, MeshRenderer>();
    mondrian_CubeColors = new HashSet<Color>();
    _cubes = GameObject.Find("Cubes");
 
      _meshRenderers = _cubes.GetComponentsInChildren<MeshRenderer>();
   
    foreach (var meshRenderer in _meshRenderers)
    {
      AddColor(meshRenderer.material.color);
    }
    
    SetColorArray(mondrian_CubeColors);
    foreach (var meshRenderer in _meshRenderers)
    {
      int instanceID =meshRenderer.GetInstanceID();
      _meshRendererMap.Add(instanceID, meshRenderer);
    }

  }

  public void AddColor(Color newColor)
  {
    // Try to add the new color to the set
    bool added = mondrian_CubeColors.Add(newColor);

#if UNITY_EDITOR
if (added)
{
  Debug.Log("색상추가 성공: " + newColor);
}
else
{
  Debug.Log("중복X " + newColor);
}
#endif

  }
  
  private void SetColorArray(HashSet<Color> colorSet)
  {
    if (colorSet.Count == 0)
    {
      Debug.LogError("색상 집합이 비어있습니다.");
    }

    _colors = new Color[colorSet.Count];
    colorSet.CopyTo(_colors);
  }

  Color GetRandomColor()
  {
    int randomIndex = UnityEngine.Random.Range(0, _colors.Length);
    return _colors[randomIndex];
  }

  protected override void OnRaySynced()
  {


    Color newColor = GetRandomColor();
    GameManager_Hits = Physics.RaycastAll(GameManager_Ray);
#if UNITY_EDITOR
    if (GameManager_Hits.Length > 0) 
    Debug.Log($"hit position: {GameManager_Hits[0].transform.gameObject.name} ");
#endif
    foreach (var hit in GameManager_Hits)
    {
      int currentInstance = hit.transform.gameObject.GetComponent<MeshRenderer>().GetInstanceID();
      if (_meshRendererMap.ContainsKey(currentInstance))
      {
#if UNITY_EDITOR
        Debug.Log($"Valid Click! : ColorChange() : _colors.Length{_colors.Length} ");
#endif
        _meshRendererMap[currentInstance].material.DOColor(GetRandomColor(),1f);
      }
    }
  }
}
