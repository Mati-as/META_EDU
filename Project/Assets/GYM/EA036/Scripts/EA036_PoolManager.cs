using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EA036_PoolManager : MonoBehaviour
{
    private EA036_GameManager gameManager;
    
    [SerializeField] private GameObject cellParent;    // 셀 부모
    [SerializeField] private GameObject prefabToSpawn; // 스폰할 프리팹
    [SerializeField] private float spawnInterval = 1f; // 시퀀스 간격

    private const int ROW_COUNT = 5;
    private const int COL_COUNT = 4;

    private Dictionary<(int r, int c), Vector3> spawnPos;
    private Dictionary<(int r, int c), bool> isOccupied;

    public Sequence spawnSeq;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<EA036_GameManager>();
        
        // 행 열 초기화
        spawnPos    = new Dictionary<(int, int), Vector3>();
        isOccupied  = new Dictionary<(int, int), bool>();

    }

    private void Start()
    {
        cellParent = gameManager.ObjectAppearPositions;

        for (int r = 0; r < ROW_COUNT; r++)
        {
            Transform rowTransform = cellParent.transform.GetChild(r);
            for (int c = 0; c < COL_COUNT; c++)
            {
                var positionGameObject = rowTransform.GetChild(c).gameObject;
                spawnPos[(r, c)]   = positionGameObject.transform.position;
                isOccupied[(r, c)] = false;
            }
        }

        isOccupied[(0, 1)] = true;
        isOccupied[(1, 1)] = true;
        isOccupied[(2, 1)] = true;
        isOccupied[(4, 1)] = true;
        isOccupied[(0, 3)] = true;
        isOccupied[(1, 3)] = true;
        isOccupied[(4, 3)] = true;

        spawnSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Logger.Log("행,열 생성 시퀀스 시작");
                // for (int i = 0; i < 2; i++) //두개씩 생성 가능
                // {
                    List<(int r, int c)> emptyCellList = GetEmptyCells();
                    if (emptyCellList.Count > 0)
                    {
                        int randomIndex = Random.Range(0, emptyCellList.Count);
                        (int randomR, int randomC) = emptyCellList[randomIndex];
                        SpawnToyAt(randomR, randomC);
                    }
               // }
            })
            .AppendInterval(spawnInterval)
            .SetLoops(-1, LoopType.Restart)
            .Pause();
            ;
    }

    public List<(int r, int c)> GetEmptyCells()  // 빈 칸 목록 반환
    {
        List<(int, int)> emptyCells = new List<(int, int)>();

        foreach (KeyValuePair<(int r, int c), bool> keyValuePair in isOccupied)
        {
            if (keyValuePair.Value == false)
            {
                Logger.Log("빈공간 발견");
                emptyCells.Add(keyValuePair.Key);
            }
        }

        return emptyCells;
    }

    private void SpawnToyAt(int r, int c)
    {
        Logger.Log("빈공간에 생성중");

        Vector3 spawnPosition = spawnPos[(r, c)];

        GameObject instance = poolManager.GetFromPool(prefabToSpawn, spawnPosition); //생성하는 기능 
        //instance.transform.position = spawnPosition;
    
        CellInfo info = instance.GetComponent<CellInfo>();
        if (info == null)
        {
            Debug.LogError($"CellInfo 컴포넌트를 찾을 수 없습니다.");
        }
        else
        {
            info.row     = r;
            info.col     = c;
            info.poolManager = this;
        }

        isOccupied[(r, c)] = true;
    }

    // 외부에서 해당 칸을 해제할 때 호출
    public void ReleaseCell(int r, int c)
    {
        isOccupied[(r, c)] = false;
    }
    
}
