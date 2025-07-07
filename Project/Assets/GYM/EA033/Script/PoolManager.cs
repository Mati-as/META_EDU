using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public enum PoolType
{
    Bell,
    Bulb,
    Candy,
    Star
}

public class PoolManager : MonoBehaviour
{
    [SerializeField] private Transform[] poolParents = new Transform[4];

    [Header("스폰 설정")]
    [SerializeField] private Transform stageObjectSpawnStartPosition;
    [SerializeField] private Transform midPoint;
    
    [SerializeField] private int totalSpawnCount = 20;
    [SerializeField] private int otherCount = 3;
    [SerializeField] private float interval = 1.5f;
    
    private EA033_GameManager _gameManager;

    private Dictionary<PoolType, List<GameObject>> pools = new Dictionary<PoolType, List<GameObject>>();
    private Sequence spawnSequence;
    
    private List<PoolType> spawnTypes;
    
    private const int ROW_COUNT = 4;
    private const int COL_COUNT = 5;

    private readonly Vector3[][] _StageObjectsPosArray = new Vector3[ROW_COUNT][];
    [SerializeField] private Transform StageObjectAppearPosition;
    private List<Vector3> allPositions;
    
    private void Start()
    {
        for (int i = 0; i < poolParents.Length; i++)
        {
            var type = (PoolType)i;
            var list = new List<GameObject>();
            for (int c = 0; c < poolParents[i].childCount; c++)
            {
                var go = poolParents[i].GetChild(c).gameObject;
                go.SetActive(false);
                list.Add(go);
            }
            pools[type] = list;
        }

        SaveStageObjectPosArray();
    }

    public GameObject GetFromPool(PoolType type)
    {
        foreach (var go in pools[type])
            if (!go.activeSelf)
            {
                Logger.Log($"{type}풀에서 꺼내오는중");
                go.SetActive(true);
                return go;
            }
        Debug.LogWarning($"[{type}] 풀에 남은 오브젝트가 없습니다!");
        return null;
    }

    // public void ReturnToPool(PoolType type, GameObject go)
    // {
    //     if (pools[type].Contains(go)) go.SetActive(false);
    //     else Debug.LogError($"[{type}] 이 오브젝트는 해당 풀에 속해 있지 않습니다: {go.name}");
    // }

    public void StopSpawnPool()
    {
        spawnSequence.Kill();
            
        for (int i = 0; i < poolParents.Length; i++)
        {
            var type = (PoolType)i;
            var list = new List<GameObject>();
            for (int c = 0; c < poolParents[i].childCount; c++)
            {
                var go = poolParents[i].GetChild(c).gameObject;
                go.SetActive(false);
                list.Add(go);
            }
            pools[type] = list;
        }
    }
    
    public void SpawnRandomPools(PoolType poolType)
    {
        // 매 호출마다 새 리스트 생성
        var spawnTypes = new List<PoolType>();

        // 위치 리스트 생성
        var allPositions = _StageObjectsPosArray
            .SelectMany(posRow => posRow)
            .OrderBy(_ => Random.value)
            .ToList();

        // 개수 계산
        int typeCount    = Enum.GetValues(typeof(PoolType)).Length;
        int primaryCount = totalSpawnCount - otherCount * (typeCount - 1);
        if (primaryCount < 0) primaryCount = totalSpawnCount;

        // 리스트 채우기
        spawnTypes.AddRange(Enumerable.Repeat(poolType, primaryCount));
        foreach (PoolType t in Enum.GetValues(typeof(PoolType)))
            if (t != poolType)
                spawnTypes.AddRange(Enumerable.Repeat(t, otherCount));

        // 셔플
        spawnTypes = spawnTypes.OrderBy(_ => Random.value).ToList();

        // 순차 스폰
        spawnSequence = DOTween.Sequence();
        for (int i = 0; i < spawnTypes.Count; i++)
        {
            int idx = i;
            var t = spawnTypes[idx];
            spawnSequence.AppendCallback(() =>
                {
                    var obj = GetFromPool(t);

                    var col = obj.GetComponent<Collider>();
                    
                    int lane = Random.Range(0, stageObjectSpawnStartPosition.childCount);
                    var startPos = stageObjectSpawnStartPosition.GetChild(lane).position;
                    var endPos = allPositions[idx];

                    obj.transform.position = startPos;

                    obj.transform
                        .DOPath(new[] { midPoint.position, endPos }, 2f)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => {
                            col.enabled = true;
                        });
                })
                .AppendInterval(interval);
        }
        spawnSequence.Play();
    }

    
    private void SaveStageObjectPosArray()
    {
        for (int i = 0; i < ROW_COUNT; i++)
        {
            _StageObjectsPosArray[i] = new Vector3[COL_COUNT];
            for (int k = 0; k < COL_COUNT; k++)
            {
                _StageObjectsPosArray[i][k] =
                    StageObjectAppearPosition.GetChild(i).GetChild(k).transform.position;
                Logger.ContentTestLog("StageObjectAppearPosition : " + _StageObjectsPosArray[i][k]);
            }
        }
    }
}
