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

    [SerializeField] private Transform stageObjectSpawnStartPosition;
    [SerializeField] private Transform StageObjectAppearPosition;
    
    [SerializeField] private int totalSpawnCount = 20;
    [SerializeField] private float spawnInterval = 1f;

    private const int ROW_COUNT = 4;
    private const int COL_COUNT = 5;
    private Vector3[][] stageObjectAppearPositions = new Vector3[ROW_COUNT][];

    private Dictionary<PoolType, List<GameObject>> pools = new Dictionary<PoolType, List<GameObject>>();
    private Sequence spawnSequence;

    [SerializeField] private GameObject SantaSacks;
    [SerializeField] private GameObject OriginalLocalScale;
    
    private void Awake()
    {
        for (int i = 0; i < poolParents.Length; i++)
        {
            var type = (PoolType)i;
            var list = new List<GameObject>();
            foreach (Transform child in poolParents[i])
            {
                child.gameObject.SetActive(false);
                list.Add(child.gameObject);
            }
            pools[type] = list;
        }

        for (int r = 0; r < ROW_COUNT; r++)
        {
            stageObjectAppearPositions[r] = new Vector3[COL_COUNT];
            for (int c = 0; c < COL_COUNT; c++)
            {
                stageObjectAppearPositions[r][c] = StageObjectAppearPosition.GetChild(r).GetChild(c).position;
            }
        }
    }

    public void StartSpawning(PoolType mainType)
    {
        spawnSequence?.Kill();

        // 무한루프 중단 없이 계속 호출
        spawnSequence = DOTween.Sequence()
            .SetLoops(-1, LoopType.Restart)
            .AppendCallback(() => TrySpawn(mainType))
            .AppendInterval(spawnInterval)
            .Play();
    }

    private void TrySpawn(PoolType mainType)
    {
        var emptyCells = GetEmptyCells();

        // 빈 칸이 없으면 스폰 시도만 건너뛴다
        if (emptyCells.Count == 0)
            return;

        SpawnOne(mainType);
    }

    private void SpawnOne(PoolType mainType)
    {
        // 빈 셀 찾기
        var emptyCells = GetEmptyCells();
        if (emptyCells.Count == 0)
        {
            spawnSequence.Kill();
            return;
        }

        // 타입 확률 선택: 55% 메인, 나머지 45%는 각 15%
        var spawnType = ChooseType(mainType);

        // 빈 셀에서 랜덤 선택
        var cell = emptyCells[Random.Range(0, emptyCells.Count)];
        var endPos = stageObjectAppearPositions[cell.r][cell.c];

        var obj = GetFromPool(spawnType);

        RestObject(obj, spawnType);
        
        int count = stageObjectSpawnStartPosition.transform.childCount;
        int i = Random.Range(0, count);
        obj.transform.position = stageObjectSpawnStartPosition.transform.GetChild(i).gameObject.transform.position;
        
        var sack = SantaSacks.transform.GetChild(i);
        sack.DOPunchScale(new Vector3(0.5f,0.5f,0),0.8f,12,1.2f)
            .SetEase(Ease.OutElastic);
        sack.DOShakeRotation(0.8f, new Vector3(0,0,30), 15, 90f);
        
        obj.transform
            .DOJump(endPos, 1f, 1, spawnInterval)
            .OnStart(()=> ToggleColliders(obj, false))
            .OnComplete(() =>
            {
                ToggleColliders(obj, true);
                if (spawnType == mainType)
                    obj.transform
                        .DOScale(obj.transform.localScale * 1.2f, 0.3f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.OutQuad); 
                
            });
    }

    
    
    private List<(int r, int c)> GetEmptyCells()
    {
        var free = new List<(int, int)>();
        foreach (var kv in pools)
        {
            foreach (var go in kv.Value)
            {
                if (go.activeSelf == false) continue;
            }
        }
        for (int r = 0; r < ROW_COUNT; r++) //중복처리 되는지 확인하는 기능
        {
            for (int c = 0; c < COL_COUNT; c++)
            {
                Vector3 target = stageObjectAppearPositions[r][c];
                bool occupied = pools.Values
                    .SelectMany(list => list)
                    .Any(go => go.activeSelf && Vector3.Distance(go.transform.position, target) < 0.1f);
                if (!occupied)
                    free.Add((r, c));
            }
        }
        return free;
    }

    private readonly PoolType[] AllPoolTypes = (PoolType[])Enum.GetValues(typeof(PoolType));

    private PoolType ChooseType(PoolType mainType)
    {
        float r = Random.value; //0~1 랜덤 실수 
        
        var others = AllPoolTypes
            .Where(t => t != mainType)
            .ToArray();
        
        if (r < 0.45f) 
            return mainType;
        
        float rr = (r - 0.45f) / 0.55f;
        if (rr < 1f/3f) 
            return others[0];
        if (rr < 2f/3f) 
            return others[1];
        return others[2];
    }

    private void RestObject(GameObject obj, PoolType spawnType) //전에 사용한 흔적들 제거(스케일 차이, 닷트윈 애니메이션 삭제)
    {
        obj.transform.DOKill(); 
        switch (spawnType)
        {
            case PoolType.Bell:
                obj.transform.localScale = OriginalLocalScale.transform.GetChild(0).gameObject.transform.localScale;
                break;
            case PoolType.Bulb:
                obj.transform.localScale = OriginalLocalScale.transform.GetChild(1).gameObject.transform.localScale;
                break;
            case PoolType.Candy:
                obj.transform.localScale = OriginalLocalScale.transform.GetChild(2).gameObject.transform.localScale;
                break;
            case PoolType.Star:
                obj.transform.localScale = OriginalLocalScale.transform.GetChild(3).gameObject.transform.localScale;
                break;
        }
    }
    
    private void ToggleColliders(GameObject obj, bool enabled)
    {
        obj.GetComponent<Collider>().enabled = enabled;
    }

    public GameObject GetFromPool(PoolType type)
    {
        foreach (var go in pools[type])
        {
            if (!go.activeSelf)
            {
                go.SetActive(true);
                return go;
            }
        }
        Debug.LogWarning($"[{type}] 풀 오브젝트 부족");
        return null;
    }

    public void StopSpawnPool()
    {
        spawnSequence?.Kill();
        foreach (var list in pools.Values)
            foreach (var go in list)
                go.SetActive(false);
    }
}
