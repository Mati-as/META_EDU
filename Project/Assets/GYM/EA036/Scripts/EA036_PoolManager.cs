using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using DG.Tweening;
using UniRx.Triggers;
using UnityEditor;

public class EA036_PoolManager : MonoBehaviour
{
    private EA036_GameManager gameManager;
    
    [SerializeField] private GameObject cellParent;    // 셀 부모
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
        cellParent = gameManager.objectAppearPositions;

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

        spawnSeq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                Logger.Log("행,열 생성 시퀀스 시작");
                StartSpawning();
                DOVirtual.DelayedCall(0.5f, StartSpawning);
            })
            .AppendInterval(spawnInterval)
            .SetLoops(-1, LoopType.Restart)
            .Pause();
            ;
    }

    private void StartSpawning()
    {
        List<(int r, int c)> emptyCellList = GetEmptyCells();
        if (emptyCellList.Count > 0)
        {
            int randomIndex = Random.Range(0, emptyCellList.Count);
            (int randomR, int randomC) = emptyCellList[randomIndex];
            SpawnToyAt(randomR, randomC);
        }
    }
    
    private List<(int r, int c)> GetEmptyCells()
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
    }   // 빈 칸 목록 반환



    private void SpawnToyAt(int r, int c)
    {
        Logger.Log("빈공간에 생성중");

        Vector3 spawnPosition = spawnPos[(r, c)];

        Transform poolParent = gameManager.toysPoolParent.transform;
        
        foreach (Transform child in poolParent)
            if (!child.gameObject.activeSelf)
            {
                var obj = child.gameObject;

                CellInfo info = obj.GetComponent<CellInfo>();
                info.row = r;
                info.col = c;
                
                isOccupied[(r, c)] = true;
                
                int i = Random.Range(0, gameManager.toySpawnPositions.Length);
                obj.SetActive(true);
                obj.transform.position = gameManager.toySpawnPositions[i].transform.position;
                obj.transform.DOJump(spawnPosition, 1f, 1, 1f).SetEase(Ease.InOutBack)
                    .OnComplete(() => info.canClicked = true);
                
                return;
            }

    }

    public void ReleaseCell(int r, int c)
    {
        isOccupied[(r, c)] = false;
    }       // 해당 위치 bool값 변경
}
