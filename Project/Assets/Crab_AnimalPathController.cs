using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class Crab_AnimalPathController : MonoBehaviour
{
    enum StartDirection      // 1     2
                         // 4     3 
    {
        Upper_Left,
        Upper_Right,
        Bottom_Right,
        Bottom_Left
    }

    enum AwayPath
    {
        start,
        arrival
    }
    [SerializeField]
    private Crab_EffectController c_efc;

    private Vector3 _pivotPoint; 
    
    private string _crabPrefabPath = "게임별분류/비디오컨텐츠/Crab/";

    
    private Stack<GameObject> crabPool;
    [Header("Start Points")] 
    public Transform[] startPoints;

    [Header("Offsets")] 
    
    public float startOffset;
    public float midOffset;
    public float endOffset;
    
    
    private Vector3 main; 
    private Vector3 start;
    private Vector3 mid;
    private Vector3 end;
    
    private Vector3[] path;
    private Vector3[] awayPath;


    private void Awake()
    {
        crabPool = new Stack<GameObject>();
        
        path = new Vector3[3];
        awayPath = new Vector3[2];
        _distancesFromStartPoint = new float[4];
    }

    private void Start()
    {
        DOTween.Init();
        Init();
    }
    

    void Init()
    {
        Crab_EffectController.OnClickForEachClick -= OnClicked;
        Crab_EffectController.OnClickForEachClick += OnClicked;
      
        
        SetPool(crabPool,"CrabA");
        SetPool(crabPool,"CrabC");
        SetPool(crabPool,"CrabB");
    }


    private void OnDestroy()
    {
        Crab_EffectController.OnClickForEachClick -= OnClicked;
    }


    private void OnClicked()
    {
        UpdateDistanceFromStartPointToClickedPoint();
        
        DoPathToClickPoint();
#if UNITY_EDITOR
        Debug.Log($"Crab_AnimalPathOnclick..");
#endif
    }


    private int _closestStartPointIndex =0;
    private int _farthestStartPointIndex = 0; 
    private float[] _distancesFromStartPoint;

    /// <summary>
    /// 4개의 Start 포인트에서 가장 가까운 생성지점이 어딘지 매 클릭시 업데이트 합니다.
    /// </summary>
    private void UpdateDistanceFromStartPointToClickedPoint()
    {
        for (int i = (int)StartDirection.Upper_Left; i < startPoints.Length ; ++i)
        {
            _distancesFromStartPoint[i] = Vector2.Distance(c_efc.hitPoint, startPoints[i].position);
        }

        _closestStartPointIndex = (int)(StartDirection.Upper_Left); 
        
        for (int section = (int)(StartDirection.Upper_Right); section <  (int)(StartDirection.Bottom_Left); section++)
        {
            if (_distancesFromStartPoint[section] > _distancesFromStartPoint[_closestStartPointIndex])
            {
                _closestStartPointIndex = section; 
            }

            if (_distancesFromStartPoint[section] < _distancesFromStartPoint[_closestStartPointIndex])
            {
                _farthestStartPointIndex = section;
                
                awayPath[(int)AwayPath.arrival] = startPoints[_farthestStartPointIndex].position;
            }
                
        }

    }

    private void DoPathToClickPoint()
    {
        GameObject obj = GetFromPool();
        
        if (obj != null)
        {
#if UNITY_EDITOR
            Debug.Log($"crab Is Moving");
#endif
            obj.transform.position = startPoints[_closestStartPointIndex].position;

            MoveAndSetPath();

            obj.transform.DOPath(path, 3f, PathType.CatmullRom)
                .OnComplete(() =>
                {
                    awayPath[(int)AwayPath.start] = obj.transform.position;
                    obj.transform.DOPath(awayPath, 3f, PathType.CatmullRom)
                        .OnComplete(() =>
                        {
                            obj.SetActive(false);
                            crabPool.Push(obj);
                        });
                });
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"object from pool is null");
#endif
        }
    }

    /// <summary>
    /// 클릭시 게가 이동할 경로를 클릭하는 곳으로 이동시키는 함수 입니다.
    /// </summary>
    private void MoveAndSetPath()
    {
        main = c_efc.hitPoint;
        
        start = main + Vector3.up * startOffset;
        mid = main + Vector3.right * midOffset;
        end = main + Vector3.down * endOffset;
        
        path[0] = start;
        path[1] = mid;
        path[2] = end;
    }
    
    private void SetPool(Stack<GameObject> pool, string name)
    {

        /*
         * 프리팹은 로컬에서 Active/Inactive 대상이 아니므로 반드시 가져오기/비활성화 하기 분리해서
         * 로직을 실행해야 함
         */ 
        
        GameObject prefab = Resources.Load<GameObject>(_crabPrefabPath + name);

        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.transform.position = startPoints[0].position;
        
            instance.SetActive(false);
#if UNITY_EDITOR
            Debug.Log($"gameObjIsActive: {gameObject.activeSelf}");
#endif
            pool.Push(instance);
           
        }
        else
        {
          
#if UNITY_EDITOR
            Debug.LogError($"this gameObj is null.. pathinfo: {_crabPrefabPath+name}");
#endif
        }
       
    }

    /// <summary>
    /// 사실상 GetFromPool
    /// </summary>
    /// <returns></returns>
    private GameObject GetFromPool()
    {
        if (crabPool.Count > 0)
        {
            var obj = crabPool.Pop();
            obj.SetActive(true);
            return obj;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError($"there's no object in pool");
#endif
            return null;
        }
        return null;
    }

}
