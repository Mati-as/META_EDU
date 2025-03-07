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
        Ginkgo,
        Persimmon,

        Max
        //Jujube,
    }


    /// <summary>
    ///     SeqName에따라 Text및 각종 함수 몇경
    /// </summary>
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
        Persimmon
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
    private SpriteRenderer spriteRenderer;

    private const int ROUND_COUNT = (int)Fruits.Persimmon; // 총Max개 ; 
    private int _currentRoundCount =-1; //flag

    private int currentRoundCount
    {
        get => _currentRoundCount;
        set
        {
            if (value == _currentRoundCount)
            {
                Logger.Log($"currentRoundCount 중복할당시도... {value}");
                return;
            }

            _isRoundFinished = true;
            DOVirtual.DelayedCall(3f,()=>
            {
                _isRoundFinished = false;
            });
            
            
            _currentRoundCount = value;
            var spriteShowDelay = 1f;
            
            if (value == (int)Fruits.Chestnut)
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/ChestnutDescription");
                DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length,
                    () => { Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/WhatFruit"); });
                DOVirtual.DelayedCall(spriteShowDelay,()=>{spriteRenderer.sprite = fruitImages[(int)Fruits.Chestnut];});
                SeqMessageEvent?.Invoke("Q");
                //SeqMessageEvent?.Invoke(nameof(Fruits.Chestnut) + 'Q');
                return;
            }

            if (value == (int)Fruits.Acorn)
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/AcornDescription");
                DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length,
                    () => { Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/WhatFruit"); });
                DOVirtual.DelayedCall(spriteShowDelay,()=>{spriteRenderer.sprite = fruitImages[(int)Fruits.Acorn];});
                
                  SeqMessageEvent?.Invoke("Q");
                //SeqMessageEvent?.Invoke(nameof(Fruits.Acorn) + 'Q');
                return;
            }

            if (value == (int)Fruits.Apple)
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/AppleDescription");
                DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length,
                    () => { Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/WhatFruit"); });
                DOVirtual.DelayedCall(spriteShowDelay,()=>{spriteRenderer.sprite = fruitImages[(int)Fruits.Apple];});
                
                  SeqMessageEvent?.Invoke("Q");
                //SeqMessageEvent?.Invoke(nameof(Fruits.Apple) + 'Q');
                return;
            }

            if (value == (int)Fruits.Ginkgo)
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/GinkgoDescription");
                DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length,
                    () => { Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/WhatFruit"); });
                DOVirtual.DelayedCall(spriteShowDelay,()=>{spriteRenderer.sprite = fruitImages[(int)Fruits.Ginkgo];});
                
                  SeqMessageEvent?.Invoke("Q");
                //SeqMessageEvent?.Invoke(nameof(Fruits.Ginkgo) + 'Q');
                return;
            }

            if (value == (int)Fruits.Persimmon)
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/PersimmonDescription");
                DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length,
                    () => { Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/WhatFruit"); });
                DOVirtual.DelayedCall(spriteShowDelay,()=>{spriteRenderer.sprite = fruitImages[(int)Fruits.Persimmon];});
                
                  SeqMessageEvent?.Invoke("Q");
                //SeqMessageEvent?.Invoke(nameof(Fruits.Persimmon) + 'Q');
                return;
            }


            Logger.LogError($"해당하는 과일없음.{value}");
        }
    }

    [Range(0,50)]
    [SerializeField]
    private int WOODBLOCK_COUNT_TO_GET_RID_OF = 25;
    private int _currentRemovedWoodBlockCount;

    private const int COLUMN_COUNT = 13;
    private const int ROW_COUNT = 7;
    private readonly Transform[][] allWoodblocks = new Transform[COLUMN_COUNT][];

    private readonly Dictionary<int, Transform> _woodBlockMap = new();
    private readonly Dictionary<int, Sequence> _woodBlockSeqMap = new(); //
    private readonly Dictionary<int, bool> _isWoodBlockClickedMap = new(); //
    private bool _isWoodBlockClickable = true;
    private bool _isRoundFinished =true;
    private Vector3 _defaultScale = Vector3.zero;
    private readonly Ease _ease = Ease.InOutBack;

    private Animator _mainAnimator;
    private readonly int SEQ_NUM = Animator.StringToHash("_seqNum");
    private readonly int DROP = Animator.StringToHash("_drop");
    private int _currentSeqNum;

    public static event Action<string> SeqMessageEvent;

    private readonly Dictionary<int, Animator> _fruitAnimatorMap = new();

    public int currentSeqNum
    {
        get => _currentSeqNum;
        set
        {

         
            _currentSeqNum = value;

            SetSeqNumAnim(value);
            Logger.Log($"Default Message Invoke{(SeqName)value}");
            switch ((SeqName)value)
            {
                case SeqName.Default:

                    DOVirtual.DelayedCall(1f, OnInitialStart);
                    SeqMessageEvent?.Invoke("Default");
                    break;
                case SeqName.OnPuzzleClick:
                    DOVirtual.DelayedCall(2.5f, OnPuzzleGameStart);
                    break;
                case SeqName.OnTreeScene_A:
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        SeqMessageEvent?.Invoke("OnTreeScene_A");
                        _isFruitOnTreeAlreadyClicked = false;
                    });
                    break;
                case SeqName.OnTreeScene_B:
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        SeqMessageEvent?.Invoke("OnTreeScene_A");
                        _isFruitOnTreeAlreadyClicked = false;
                    });
                    break;
                case SeqName.OnTreeScene_C:
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        SeqMessageEvent?.Invoke("OnTreeScene_A");
                        _isFruitOnTreeAlreadyClicked = false;
                    });
                    break;
                case SeqName.OnTreeScene_D:
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        SeqMessageEvent?.Invoke("OnTreeScene_A");
                        _isFruitOnTreeAlreadyClicked = false;
                    });
                    break;
                case SeqName.OnTreeScene_E:
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        SeqMessageEvent?.Invoke("OnTreeScene_A");
                        _isFruitOnTreeAlreadyClicked = false;
                    });
                    break;
                case SeqName.OnFinish:
                    DOVirtual.DelayedCall(2.5f, () => { SeqMessageEvent?.Invoke("OnTreeScene_A"); });
                    break;
            }
            
           
        }
    }


    private readonly Sprite[] fruitImages = new Sprite[5];

    private void LoadSpriteImage()
    {
        fruitImages[(int)Fruits.Apple] = Resources.Load<Sprite>("Runtime/EA010/" + nameof(Fruits.Apple));
        fruitImages[(int)Fruits.Ginkgo] = Resources.Load<Sprite>("Runtime/EA010/" + nameof(Fruits.Ginkgo));
        fruitImages[(int)Fruits.Chestnut] = Resources.Load<Sprite>("Runtime/EA010/" + nameof(Fruits.Chestnut));
        fruitImages[(int)Fruits.Persimmon] = Resources.Load<Sprite>("Runtime/EA010/" + nameof(Fruits.Persimmon));
        fruitImages[(int)Fruits.Acorn] = Resources.Load<Sprite>("Runtime/EA010/" + nameof(Fruits.Acorn));
    }

    protected sealed override void Init()
    {
        base.Init();

        LoadSpriteImage();
        _mainAnimator = GetComponent<Animator>();
        BindObject(typeof(Obj));

        spriteRenderer = GetObject((int)Obj.FallRelatedPictureSprite).GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
        
        _isWoodBlockClickable = false;

        SetAllWoodBlocks();

        _fruitAnimatorMap.Add((int)Fruits.Chestnut, GameObject.Find(nameof(Fruits.Chestnut)).GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Acorn, GameObject.Find(nameof(Fruits.Acorn)).GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Apple, GameObject.Find(nameof(Fruits.Apple)).GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Ginkgo, GameObject.Find(nameof(Fruits.Ginkgo)).GetComponent<Animator>());
        _fruitAnimatorMap.Add((int)Fruits.Persimmon,
            GameObject.Find(nameof(Fruits.Persimmon)).GetComponent<Animator>());

        InitFruitOnTrees();
        currentSeqNum = 0;

        Logger.Log("Init--------------------------------");
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        DOVirtual.DelayedCall(1.5f, () => { DOVirtual.DelayedCall(1.5f, () => { currentSeqNum = 1; }); });
    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (currentSeqNum == (int)SeqName.Default || currentSeqNum == (int)SeqName.OnFinish) return;


        if (currentSeqNum == (int)SeqName.OnPuzzleClick)
        {
            if (_isRoundFinished || !_isWoodBlockClickable) return;
            OnRaysyncOnPuzzeGame();
        }
        else
        {
            OnRaySyncOnFruitOnTree();
        }
    }


    private void SetSeqNumAnim(int currentSeq)
    {
        _mainAnimator.SetInteger(SEQ_NUM, currentSeq);
    }

    private void OnPuzzleGameStart()
    {
        _isWoodBlockClickable = true;
        currentRoundCount = (int)Fruits.Chestnut;
    }

    private void OnRaysyncOnPuzzeGame()
    {
        foreach (var hit in GameManager_Hits)
        {
            var id = hit.transform.GetInstanceID();

//            Logger.Log($"hit.transform.name : {hit.transform.name}");
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
                    _isWoodBlockClickable = false;
                }
            }


            if (_currentRemovedWoodBlockCount >= WOODBLOCK_COUNT_TO_GET_RID_OF)
            {
                _isRoundFinished = true;
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA010/OnCorrectA");

                var answerNarDealy = 1f;
                
                Logger.Log($"{(Fruits)currentRoundCount} --------------나레이션 및 소리재생 ");
                switch ((Fruits)currentRoundCount)
                {
                    case Fruits.Chestnut:

                        DOVirtual.DelayedCall(answerNarDealy, () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Chestnut));
                            SeqMessageEvent?.Invoke(nameof(Fruits.Chestnut));
                        });
                        break;

                    case Fruits.Acorn:

                        DOVirtual.DelayedCall(answerNarDealy, () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Acorn));
                            SeqMessageEvent?.Invoke(nameof(Fruits.Acorn));
                        });
                        break;

                    case Fruits.Apple:

                        DOVirtual.DelayedCall(answerNarDealy, () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Apple));
                            SeqMessageEvent?.Invoke(nameof(Fruits.Apple));
                        });
                        break;

                    case Fruits.Ginkgo:

                        DOVirtual.DelayedCall(answerNarDealy, () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Ginkgo));
                            SeqMessageEvent?.Invoke(nameof(Fruits.Ginkgo));
                        });
                        break;

                    case Fruits.Persimmon:

                        DOVirtual.DelayedCall(answerNarDealy, () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Persimmon));
                            SeqMessageEvent?.Invoke(nameof(Fruits.Persimmon));
                        });
                        break;

                    default:

                        Logger.Log("invalid Fruit name-------------------------------");
                        break;
                }


                DOVirtual.DelayedCall(3.5f, OnRoundFinished);
            }
        }
    }

    #region PuzzleGamePart

    private void OnWoodBlockClicked(int id)
    {
        var randomChar = (char)Random.Range('A', 'D');
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/EA010/Click_" + randomChar);

        _currentRemovedWoodBlockCount++;
        var randomEase = (Ease)Random.Range((int)Ease.InOutBack, (int)Ease.OutBounce);
        var duration = Random.Range(0.15f, 0.40f);
        _woodBlockSeqMap[id].Append(_woodBlockMap[id].DOScale(Vector3.zero, duration).SetEase(randomEase))
            .OnComplete(() => { _woodBlockMap[id].gameObject.SetActive(false); });

        DOVirtual.DelayedCall(0.11f, () => { _isWoodBlockClickable = true; });
    }

    private void OnInitialStart()
    {
        _isWoodBlockClickable = true;
    }

    private void OnRoundFinished()
    {
        _isWoodBlockClickable = false;

        foreach (var oneColumn in allWoodblocks)
        foreach (var block in oneColumn)
            block.DOScale(Vector3.zero, 0.5f)
                .SetEase(_ease)
                .OnComplete(() => { block.gameObject.SetActive(false); })
                .SetDelay(Random.Range(0.01f,0.55f));
                


        DOVirtual.DelayedCall(3f, ResetForNextRound);

        if (ROUND_COUNT <= _currentRoundCount && currentSeqNum != 2)
        {
            _isRoundFinished = true;

            DOVirtual.DelayedCall(4f, () =>
            {
                Logger.Log($"Next Round Start {_currentRoundCount}");
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA010/OnCorrectA");
                currentSeqNum = (int)SeqName.OnTreeScene_A;
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

        SeqMessageEvent?.Invoke(nameof(SeqName.Default));
        DOVirtual.DelayedCall(3f, OnNextRoundStart);
    }


    private void OnNextRoundStart()
    {
       
        _isWoodBlockClickable = true;
        _currentRemovedWoodBlockCount = 0;
        currentRoundCount++;

        foreach (var key in _woodBlockMap.Keys.ToArray())
        {
            _woodBlockMap[key].gameObject.SetActive(true);
            _isWoodBlockClickedMap[key] = false;
            _isWoodBlockClickable = true;
        }

        var ranDuration = Random.Range(0.5f, 1.5f);
        foreach (var oneColumn in allWoodblocks)
        foreach (var block in oneColumn)
            block.DOScale(_defaultScale, ranDuration).SetEase(
                _ease).OnStart(() => { block.gameObject.SetActive(true); }) .SetDelay(Random.Range(0.01f,0.25f));


        DOVirtual.DelayedCall(3f, () =>
        {
            _isWoodBlockClickable = true;
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
        _isWoodBlockClickable = true;
    }


    private bool _isFruitOnTreeAlreadyClicked = false;
    private void OnRaySyncOnFruitOnTree()
    {
        
        if(_isFruitOnTreeAlreadyClicked) return;
        
        
        foreach (var hit in GameManager_Hits)
        {
            var NarrDealy = 1f;
            var nextSeqDelay = 3f;
            
            Logger.Log($"hit.transform.name : {hit.transform.name}");
            if (currentSeqNum == (int)SeqName.OnTreeScene_A && hit.transform.name.Contains(nameof(Fruits.Chestnut)))
            {
                DropFruitOnTrees(Fruits.Chestnut);
                DOVirtual.DelayedCall(NarrDealy,()=>{Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Chestnut));});
                SeqMessageEvent?.Invoke(nameof(Fruits.Chestnut));
                DOVirtual.DelayedCall(nextSeqDelay, () =>
                {
                    currentSeqNum = (int)SeqName.OnTreeScene_B;
                });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_B && hit.transform.name.Contains(nameof(Fruits.Acorn)))
            {
                DropFruitOnTrees(Fruits.Acorn);
                SeqMessageEvent?.Invoke(nameof(Fruits.Acorn));
                DOVirtual.DelayedCall(NarrDealy,()=>{Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Acorn));});
                DOVirtual.DelayedCall(nextSeqDelay, () =>
                {
                    currentSeqNum = (int)SeqName.OnTreeScene_C;
                });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_C && hit.transform.name.Contains(nameof(Fruits.Apple)))
            {
                DropFruitOnTrees(Fruits.Apple);
                SeqMessageEvent?.Invoke(nameof(Fruits.Apple));
                DOVirtual.DelayedCall(NarrDealy,()=>{Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Apple));});
                DOVirtual.DelayedCall(nextSeqDelay, () =>
                {
                    currentSeqNum = (int)SeqName.OnTreeScene_D;
                });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_D && hit.transform.name.Contains(nameof(Fruits.Ginkgo)))
            {
                DropFruitOnTrees(Fruits.Ginkgo);
                SeqMessageEvent?.Invoke(nameof(Fruits.Ginkgo));
                DOVirtual.DelayedCall(NarrDealy,()=>{Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Ginkgo));});
                DOVirtual.DelayedCall(nextSeqDelay, () =>
                {
                    currentSeqNum = (int)SeqName.OnTreeScene_E;
                });
                return;
            }

            if (currentSeqNum == (int)SeqName.OnTreeScene_E && hit.transform.name.Contains(nameof(Fruits.Persimmon)))
            {
                DropFruitOnTrees(Fruits.Persimmon);
                SeqMessageEvent?.Invoke(nameof(Fruits.Persimmon));
                DOVirtual.DelayedCall(NarrDealy,()=>{Managers.Sound.Play(SoundManager.Sound.Narration, "EA010/" + nameof(Fruits.Persimmon));});
                DOVirtual.DelayedCall(nextSeqDelay, () =>
                {
                    currentSeqNum = (int)SeqName.OnFinish;
                });
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
        _isFruitOnTreeAlreadyClicked =true;
        _fruitAnimatorMap[(int)fruit].SetBool(DROP, true);
    }

    #endregion
}