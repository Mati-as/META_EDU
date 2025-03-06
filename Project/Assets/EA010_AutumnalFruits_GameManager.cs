using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA010_AutumnalFruits_GameManager : Ex_BaseGameManager
{
    public enum Fruits
    {
        Chestnut,
        Acorn,
        Apple,
        Gingko,
        Persimmon,

        Max
        //Jujube,
    }

    public enum SeqName
    {
        Default,
        OnPuzzleClick,
        OnTreeScene_A,
        OnTreeScene_B,
        OnTreeScene_C,
        OnTreeScene_D,
        OnTreeScene_E,
        OnFinish
    }

    public enum MessageSequence
    {
        Intro,
        OnPuzzleStart,
        Chestnut,
        Acorn,
        Apple,
        Gingko,
        Persimmon,
        
    }

    private enum Obj
    {
        WoodBlocks,
        FallRelatedPictureSprite,
        Chestnut,
        Acorn,
        Apple,
        Gingko,
        Persimmon,
        Max
    }

    private const int FALL_IMAGE_COUNT = 5;
    private SpriteRenderer FallRelatedPictureSprite;

    private const int ROUND_COUNT = 1; // 5; 
    private int _currentRoundCount;

    private const int WOODBLOCK_COUNT_TO_GET_RID_OF = 10;
    private int _currentRemovedWoodBlockCount;

    private const int COLUMN_COUNT = 13;
    private const int ROW_COUNT = 7;
    private readonly Transform[][] allWoodblocks = new Transform[COLUMN_COUNT][];

    private readonly Dictionary<int, Transform> _woodBlockMap = new();
    private readonly Dictionary<int, Sequence> _woodBlockSeqMap = new(); //
    private readonly Dictionary<int, bool> _isWoodBlockClickedMap = new(); //
    private bool _isClickable = true;
    private bool _isRoundFinished;
    private Vector3 _defaultScale = Vector3.zero;
    private readonly Ease _ease = Ease.InOutBack;

    private Animator _mainAnimator;
    private readonly int SEQ_NUM = Animator.StringToHash("_seqNum");
    private readonly int DROP = Animator.StringToHash("_drop");
    private int _currentSeqNum;

    public static event Action<string> SequnceMessage;

    private readonly Dictionary<int, Animator> _fruitAnimatorMap = new();

    public int currentSeqNum
    {
        get => _currentSeqNum;
        set
        {
            _currentSeqNum = value;
            SetSeqNum(_currentSeqNum);

            switch ((SeqName)currentSeqNum)
            {
                case SeqName.Default:
                    DOVirtual.DelayedCall(2.5f, OnInitialStart);
                    break;
                case SeqName.OnPuzzleClick:
                    DOVirtual.DelayedCall(2.5f, OnPuzzleGameStart);
                    break;
                case SeqName.OnTreeScene_A:
                    DOVirtual.DelayedCall(2.5f, ()=>
                    {
                        SequnceMessage?.Invoke("OnTreeScene_A");
                    });
                    break;
                case SeqName.OnTreeScene_B:
                    DOVirtual.DelayedCall(2.5f, ()=>
                    {
                        SequnceMessage?.Invoke("OnTreeScene_A");
                    });
                    break;
                case SeqName.OnTreeScene_C:
                    DOVirtual.DelayedCall(2.5f, ()=>
                    {
                        SequnceMessage?.Invoke("OnTreeScene_A");
                    });
                    break;
                case SeqName.OnTreeScene_D:
                    DOVirtual.DelayedCall(2.5f, ()=>
                    {
                        SequnceMessage?.Invoke("OnTreeScene_A");
                    });
                    break;
                case SeqName.OnTreeScene_E:
                    DOVirtual.DelayedCall(2.5f, ()=>
                    {
                        SequnceMessage?.Invoke("OnTreeScene_A");
                    });
                    break;
                case SeqName.OnFinish:
                    DOVirtual.DelayedCall(2.5f, ()=>
                    {
                        SequnceMessage?.Invoke("OnTreeScene_A");
                    });
                    break;
            }

            ;
        }
    }


    protected override void Init()
    {
        base.Init();

        _mainAnimator = GetComponent<Animator>();

        BindObject(typeof(Obj));

        FallRelatedPictureSprite = GetObject((int)Obj.FallRelatedPictureSprite).GetComponent<SpriteRenderer>();
        _isClickable = false;

        SetAllWoodBlocks();

        currentSeqNum = 0;


        _fruitAnimatorMap.Add((int)Fruits.Chestnut, GameObject.Find("Chestnut").GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Acorn, GameObject.Find("Acorn").GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Apple, GameObject.Find("Apple").GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Gingko, GameObject.Find("Gingko").GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Persimmon, GameObject.Find("Persimmon").GetComponent<Animator>());
        InitFruitOnTrees();
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        DOVirtual.DelayedCall(1.5f, () =>
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "EA010/Narration01");

            DOVirtual.DelayedCall(1.5f, () => { currentSeqNum = 1; });
        });
    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (currentSeqNum == (int)SeqName.Default || currentSeqNum == (int)SeqName.OnFinish) return;


        if (currentSeqNum == (int)SeqName.OnPuzzleClick)
        {
            if (_isRoundFinished || !_isClickable) return;
            OnRaysyncOnPuzzeGame();
        }
        else
        {
            OnRaySyncOnFruitOnTree();
        }
    }


    private void SetSeqNum(int currentSeq)
    {
        _mainAnimator.SetInteger(SEQ_NUM, currentSeq);
    }

    private void OnPuzzleGameStart()
    {
        _isClickable = true;
        _currentRoundCount = 1;
    }

    private void OnRaysyncOnPuzzeGame()
    {
        foreach (var hit in GameManager_Hits)
        {
            var id = hit.transform.GetInstanceID();

            Logger.Log($"hit.transform.name : {hit.transform.name}");
            if (hit.transform.name.Contains("WoodBlock"))
            {
                _woodBlockMap.TryAdd(id, hit.transform);

                _woodBlockSeqMap.TryAdd(id, DOTween.Sequence());
                _woodBlockSeqMap[id]?.Kill();

                _isWoodBlockClickedMap.TryAdd(id, false);

                if (!_isWoodBlockClickedMap[id])
                {
                    OnWoodBlockClicked(id);
                    _isWoodBlockClickedMap[id] = true;
                    _isClickable = false;
                }
            }


            if (_currentRemovedWoodBlockCount >= WOODBLOCK_COUNT_TO_GET_RID_OF)
                DOVirtual.DelayedCall(3f, OnRoundFinished);
        }
    }

    #region PuzzleGamePart

    private void OnWoodBlockClicked(int id)
    {
        var randomChar = (char)Random.Range('A', 'D');
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/EA010/Click_" + randomChar);

        _currentRemovedWoodBlockCount++;
        var randomEase = (Ease)Random.Range((int)Ease.InOutBack, (int)Ease.OutBounce);
        var duration = Random.Range(0.5f, 1.5f);
        _woodBlockSeqMap[id].Append(_woodBlockMap[id].DOScale(Vector3.zero, duration).SetEase(randomEase))
            .OnComplete(() => { _woodBlockMap[id].gameObject.SetActive(false); });

        DOVirtual.DelayedCall(0.2f, () => { _isClickable = true; });
    }

    private void OnInitialStart()
    {
        _isClickable = true;
    }

    private void OnRoundFinished()
    {
        _isClickable = false;

        foreach (var oneColumn in allWoodblocks)
        foreach (var block in oneColumn)
            block.DOScale(Vector3.zero, 0.5f)
                .SetEase(_ease)
                .OnComplete(() => { block.gameObject.SetActive(false); });


        DOVirtual.DelayedCall(3f, ResetForNextRound);

        if (!_isRoundFinished && ROUND_COUNT <= _currentRoundCount)
        {
            _isRoundFinished = true;
            DOVirtual.DelayedCall(2f, () =>
            {
                Logger.Log($"Next Round Start {_currentRoundCount}");
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA010/OnCorrectA");
                currentSeqNum = 2;
            });
        }
    }


    private void ResetForNextRound()
    {
        foreach (var key in _woodBlockMap.Keys.ToArray())
        {
            KillWoodScaleSeq(key);
            _woodBlockMap[key].gameObject.SetActive(true);
            _isWoodBlockClickedMap[key] = false;
        }

        DOVirtual.DelayedCall(3f, OnNextRoundStart);
    }


    private void OnNextRoundStart()
    {
        _isRoundFinished = false;
        _isClickable = true;
        _currentRemovedWoodBlockCount = 0;
        _currentRoundCount++;

        foreach (var key in _woodBlockMap.Keys.ToArray())
        {
            _woodBlockMap[key].gameObject.SetActive(true);
            _isWoodBlockClickedMap[key] = false;
            _isClickable = true;
        }

        var ranDuration = Random.Range(0.5f, 1.5f);
        foreach (var oneColumn in allWoodblocks)
        foreach (var block in oneColumn)
            block.DOScale(_defaultScale, ranDuration).SetEase(
                _ease).OnStart(() => { block.gameObject.SetActive(true); }).OnComplete(() => { });


        DOVirtual.DelayedCall(3f, () =>
        {
            _isClickable = true;
            foreach (var key in _woodBlockMap.Keys.ToArray()) KillWoodScaleSeq(key);
        });
    }


    private void KillWoodScaleSeq(int key)
    {
        _woodBlockSeqMap[key]?.Kill();
        _woodBlockSeqMap[key] = DOTween.Sequence();
    }

    private void SetAllWoodBlocks()
    {
        for (var col = 0; col < COLUMN_COUNT; col++)
        {
            GetObject((int)Obj.WoodBlocks).transform.GetChild(col);
            allWoodblocks[col] = new Transform[ROW_COUNT];

            for (var row = 0; row < ROW_COUNT; row++)
                allWoodblocks[col][row] = GetObject((int)Obj.WoodBlocks).transform.GetChild(col).GetChild(row);
        }

        _defaultScale = allWoodblocks[0][0].localScale;
    }

    #endregion

    #region TreeSelectionpart

    private void OnTreeSelectionStart()
    {
        _isClickable = true;
    }

    private void OnRaySyncOnFruitOnTree()
    {
        foreach (var hit in GameManager_Hits)
        {
     

            Logger.Log($"hit.transform.name : {hit.transform.name}");
            if (currentSeqNum == (int)SeqName.OnTreeScene_A && hit.transform.name.Contains("Chestnut"))
            {
                DropFruitOnTrees(Fruits.Chestnut);
                SequnceMessage?.Invoke("chestnut");
                DOVirtual.DelayedCall(3f, () => { currentSeqNum = (int)SeqName.OnTreeScene_B; });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_B && hit.transform.name.Contains("Acorn"))
            {
                DropFruitOnTrees(Fruits.Acorn);
                SequnceMessage?.Invoke("Acorn");
                DOVirtual.DelayedCall(3f, () => { currentSeqNum = (int)SeqName.OnTreeScene_C; });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_C && hit.transform.name.Contains("Apple"))
            {
                DropFruitOnTrees(Fruits.Apple);
                SequnceMessage?.Invoke("Apple");
                DOVirtual.DelayedCall(3f, () => { currentSeqNum = (int)SeqName.OnTreeScene_D; });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_D && hit.transform.name.Contains("Gingko"))
            {
                DropFruitOnTrees(Fruits.Gingko);
                SequnceMessage?.Invoke("Gingko");
                DOVirtual.DelayedCall(3f, () => { currentSeqNum = (int)SeqName.OnTreeScene_E; });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_E && hit.transform.name.Contains("Persimmon"))
            {
                DropFruitOnTrees(Fruits.Persimmon);
                SequnceMessage?.Invoke("Persimmon");
                DOVirtual.DelayedCall(3f, () => { currentSeqNum = (int)SeqName.OnFinish; });
                return;
            }
        }
    }

    private void InitFruitOnTrees()
    {
        foreach (var key in _fruitAnimatorMap.Keys.ToArray()) _fruitAnimatorMap[key].SetBool(DROP, false);
    }

    private void DropFruitOnTrees(Fruits fruit)
    {
        _fruitAnimatorMap[(int)fruit].SetBool(DROP, true);
    }

    #endregion
}