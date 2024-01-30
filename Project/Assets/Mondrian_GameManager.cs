using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class Mondrian_GameManager : IGameManager
{

  private float _cubeMoveCurrentTime; 
  enum RayCasterMovePosition
  {
    Start,
    Arrival,
    Max
  }
  
  //자식객체에서 중복값을 피해서 가져오기 위한 HashSet설정
  private HashSet<Color> mondrian_CubeColors;
  
  private GameObject _cubes;
  private Color[] _colors;

  
  // 파도타기방식(형태) RayCasting 및 색상변환로직 구현용 RayCaster. 
  private GameObject _rayCasterParent;
  private Transform[] _rayCasters;
 

  private Dictionary<int, MeshRenderer> _meshRendererMap;
  private Dictionary<int, Sequence> _sequences;
  private MeshRenderer[] _meshRenderers;


  [Header("RayCaster Setting")] 
  [SerializeField] private Transform[] movePath;
  private Vector3[] _pathPos;
  [Range(0,60)]
  public float interval;
  private bool _isRayCasterMoving;
  private float rayMoveCurrentTime; 


  private void Update()
  {
    _cubeMoveCurrentTime += Time.deltaTime;
    rayMoveCurrentTime += Time.deltaTime;

    if (rayMoveCurrentTime > interval)
    {
      RayCasterMovePlay();
      rayMoveCurrentTime = 0;
    }
  }

  protected override void Init()
  {
    base.Init();
    _meshRendererMap = new Dictionary<int, MeshRenderer>();
    _sequences = new Dictionary<int, Sequence>();
    mondrian_CubeColors = new HashSet<Color>();
    _pathPos = new Vector3[(int)RayCasterMovePosition.Max];
    
    _pathPos[0] = movePath[(int)RayCasterMovePosition.Start].position;
    _pathPos[1] = movePath[(int)RayCasterMovePosition.Arrival].position;

    
    //cube settings.
    _cubes = GameObject.Find("Cubes");
    _meshRenderers = _cubes.GetComponentsInChildren<MeshRenderer>();

    foreach (var meshRenderer in _meshRenderers) AddColor(meshRenderer.material.color);

    SetColorArray(mondrian_CubeColors);
    foreach (var meshRenderer in _meshRenderers)
    {
      var instanceID = meshRenderer.GetInstanceID();
      _meshRendererMap.Add(instanceID, meshRenderer);
    }

    //raycaster settings.
    _rayCasterParent = GameObject.Find("RayCaster");
    _rayCasters = _rayCasterParent.GetComponentsInChildren<Transform>();
    _wait = new WaitForSeconds(0.2f);
  }

  public void AddColor(Color newColor)
  {
    bool added = mondrian_CubeColors.Add(newColor);

// #if UNITY_EDITOR
// if (added)
// {
//   Debug.Log("색상추가 성공: " + newColor);
// }
// else
// {
//   Debug.Log("중복X " + newColor);
// }
// #endif

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

  Color GetRandomColor(Color currentColor)
  {
    int randomIndex = UnityEngine.Random.Range(0, _colors.Length);
    while(currentColor == _colors[randomIndex])
    {
       randomIndex = UnityEngine.Random.Range(0, _colors.Length);
    }
    return _colors[randomIndex];
  }

  protected override void OnRaySynced()
  {

    RandomlyChangeColor(GameManager_Ray);

  }

  private void RandomlyChangeColor(Ray ray)
  {
  
    GameManager_Hits = Physics.RaycastAll(ray);

    foreach (var hit in GameManager_Hits)
    {
      int currentInstance = hit.transform.gameObject.GetComponent<MeshRenderer>().GetInstanceID();
      if (_meshRendererMap.ContainsKey(currentInstance))
      {
#if UNITY_EDITOR
    
#endif
        Sequence sequence = DOTween.Sequence();

        if (_sequences.ContainsKey(currentInstance))
        {
          if (!_sequences[currentInstance].IsActive())
          {
            sequence
              .Append(_meshRendererMap[currentInstance].material
              .DOColor(GetRandomColor(_meshRendererMap[currentInstance].material.color),Random.Range(0.55f,0.75f)));
            _sequences.TryAdd(currentInstance, sequence);
            sequence.Play();
          }
        }
        else
        {
          sequence.Append(_meshRendererMap[currentInstance].material
          .DOColor(GetRandomColor(_meshRendererMap[currentInstance].material.color),Random.Range(0.55f,0.75f)));
          _sequences.TryAdd(currentInstance, sequence);
          sequence.Play();
        }
     

      }
    }
  }

  private void RayCasterMovePlay()
  {
    _rayCasterParent.transform.position = _pathPos[(int)RayCasterMovePosition.Start];
    _rayCasterParent.transform
      .DOMove(_pathPos[(int)RayCasterMovePosition.Arrival], 3.2f)
      .OnStart(() =>
      {
        _rayCastCoroutine = StartCoroutine(RayCasterMoveCoroutine());
      })
      .OnComplete(() =>
      {
        StopCoroutine(_rayCastCoroutine);
      })
      .SetEase(Ease.Linear);
  }

  private void RayCasterMove()
  {
    // 각 자식 객체의 위치에서 아래 방향으로 레이를 발사합니다.
    foreach (var childTransform in _rayCasters)
    {
      
      if (childTransform != transform)
      {
        Ray raycasterMoveRay = new Ray(childTransform.position, Vector3.down);
        RaycastHit hit;

        // 레이캐스트 발사 (예: 100 유닛 거리까지)
        if (Physics.Raycast(raycasterMoveRay, out hit, 1000f))
        {
          RandomlyChangeColor(raycasterMoveRay);
          Debug.Log("Raycastermove hit: " + hit.transform.name);
        }
      }
    }
  }

  private Coroutine _rayCastCoroutine;
  private WaitForSeconds _wait;
  
  private IEnumerator RayCasterMoveCoroutine()
  {
    while (true)
    {
      Debug.Log("Move And RayCastring");
      RayCasterMove();
      yield return _wait; // 0.2초 간격으로 대기
    }
  }
}
