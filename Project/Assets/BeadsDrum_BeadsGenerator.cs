using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BeadsDrum_BeadsGenerator : MonoBehaviour
{
    private Queue<GameObject> _beadsContainer;
    private Vector3 _spawnPosA;
    private Vector3 _spawnPosB;
    private Vector3[] _spawnPositions;
    public int beadsCount;

    private void Init()
    {
        SetPool(_beadsContainer, "게임별분류/기본컨텐츠/ColorBeadsDrum/Prefabs/Bead", beadsCount);

        _spawnPosA = transform.GetChild(0).transform.Find("GeneratePositionRight").position;
        _spawnPosB = transform.GetChild(0).transform.Find("GeneratePositionLeft").position;
        var posCount = 2;
        _spawnPositions = new Vector3[posCount];
        _spawnPositions[0] = _spawnPosA;
        _spawnPositions[1] = _spawnPosB;
    }

    private void Start()
    {
        _beadsContainer = new Queue<GameObject>();

        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        UI_Scene_Button.onBtnShut += OnStartButtonClicked;

        Init();
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
    }

    private void SetPool(Queue<GameObject> pool, string path, int poolCount = 50)
    {
        for (var poolSize = 0; poolSize < poolCount; poolSize++)
        {
            var randomChar = (char)Random.Range('A', 'E' + 1);
            var randomPath = path + randomChar;
            var prefab = Resources.Load<GameObject>(randomPath);

            if (prefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("this gameObj to pool is null.");
#endif
                return;
            }

            var bead = Instantiate(prefab, transform);
            bead.SetActive(false);
            pool.Enqueue(bead);
        }
    }

    private void OnStartButtonClicked()
    {
        GenerateBeads();
    }

    private GameObject GetFromPool(Queue<GameObject> pool)
    {
        if (pool.Count < 0) return null;

        var bead = pool.Dequeue();
        return bead;
    }

    private IEnumerator GenerateBeadsCoroutine()
    {
        var i = 0;

        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/비즈드럼/FallingBeads");
        while (_beadsContainer.Count > 0)
        {
            var bead = GetFromPool(_beadsContainer);
            var targetPosition = _spawnPositions[i % 2] + Vector3.up * (i * 0.05f) +
                                 Vector3.forward * Random.Range(0, 3.5f) + Vector3.right * Random.Range(0, 3.5f);
            ;


            bead.transform.position = _spawnPositions[i % 2];
            bead.SetActive(true);
            bead.transform.DOMove(targetPosition, 0.085f);


            yield return new WaitForSeconds(0.015f);

#if UNITY_EDITOR
            Debug.Log($"Beads generating....{_beadsContainer.Count}");
#endif

            i++;
        }
    }

    private void GenerateBeads()
    {
        // Start the coroutine
        StartCoroutine(GenerateBeadsCoroutine());
    }
}