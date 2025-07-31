using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public enum MainSeq
{
    StartSequence,
    SelectAgeStageSequence,
    CardGameStageSequence,
    ObjectGameStageSequence,
    ChangeStageSequence,
    EndSequence
}

public class EA038_GameManager : Ex_BaseGameManager
{
     

    private MainSeq _currentSequence;

    private enum Cameras
    {
        Camera1,
        Camera2,
    }

    private enum Objects
    {
        CorrectCardPositions,
        SetCardPositions,
        CardPool,
        
        
    }

    private enum Particle
    {
        Victory1,
        Victory2
    }

    public int gamePlayAge;
    [SerializeField] private int wrongCardClickedCount = 0;
    [SerializeField] private int correctCardClickedCount = 0;
    
    private EA038_UIManager _uiManager;
    private Vector3 clickEffectPos;
    private List<int> numbers;
    
    public List<EA038_Card> _cards;
    private Dictionary<Collider, EA038_Card> _cardByCollider;
    
    private Vector3 correctCardtargetPos;

    private Sequence cardShakeSeq;
    private Vector3 originalCardScale;
    protected override void Init()
    {
        //Bind<CinemachineVirtualCamera>(typeof(Cameras));
        BindObject(typeof(Objects));
        Bind<ParticleSystem>(typeof(Particle));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA038_UIManager>();
        _currentSequence = MainSeq.StartSequence;

        psResourcePath = "EA038/Asset/Fx_Click";
        SetPool(); //클릭 이펙트 용 풀

        gamePlayAge = 3; //컨텐츠 기본 설정 나이 (3세)

        numbers = Enumerable.Range(2, 6).ToList();
        
        _cardByCollider = FindObjectsOfType<EA038_Card>()
            .ToDictionary(card => card.GetComponent<Collider>(), card => card);

        cardShakeSeq = DOTween.Sequence();

        originalCardScale = new Vector3(0.04224154f, 0.004107093f, 0.03364548f);

        // Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 12; //카메라들 우선순위 초기화
        // for (int i = (int)Cameras.Camera2; i <= (int)Cameras.Camera4; i++)
        //     Get<CinemachineVirtualCamera>(i).Priority = 10;
        //
        // Managers.Sound.Play(SoundManager.Sound.Bgm, "EA033/Audio/BGM");
        //
        // var stageParents = new[]
        // {
        //     GetObject((int)Objects.BellStageTreeGroup).transform,
        //     GetObject((int)Objects.BulbStageTreeGroup).transform,
        //     GetObject((int)Objects.CandyStageTreeGroup).transform,
        //     GetObject((int)Objects.StarStageTreeGroup).transform
        // };

        // GetObject((int)Objs.Intro_Triangles).gameObject.SetActive(false);
        // GetObject((int)Objs.Intro_Stars).gameObject.SetActive(false);

    }

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();

        GameStart();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UI_InScene_StartBtn.onGameStartBtnShut -= GameStart;
    }
    
    private void GameStart()
    {
        //if (_stage == MainSeq.OnStart)
        ChangeStage(MainSeq.StartSequence);
        //NextStage(MainSeq.OnFinish);
    }

    public void ChangeStage(MainSeq next)
    {
        _currentSequence = next;
        switch (next)
        {
            case MainSeq.StartSequence: OnStartStage(); break;
            case MainSeq.SelectAgeStageSequence: OnSelectAgeStage(); break;
            case MainSeq.CardGameStageSequence: OnCardGameStage(); break;
            case MainSeq.ObjectGameStageSequence: OnObjectGameStage(); break;
            case MainSeq.ChangeStageSequence: OnChangeStage(); break;
            case MainSeq.EndSequence: OnEndStage(); break;
            
        }
        
        Logger.Log($"{next}스테이지로 변경");
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_currentSequence == MainSeq.CardGameStageSequence)
            foreach (var hit in GameManager_Hits)
            {
                var clickedObj = hit.collider.gameObject;
                //clickedObj.transform.DOKill();

                // clickEffectPos = hit.point;
                // //clickEffectPos.y += 0.2f;
                // PlayParticleEffect(clickEffectPos);
                // PlayClickSound();

                if (_cardByCollider.TryGetValue(hit.collider, out var card))
                {
                    if (card.cardValue == gamePlayAge && card.canClicked)
                    {
                        correctCardClickedCount++;
                        Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");

                        card.canClicked = false;
                        card.KillShake();

                        //정답 나레이션 순서대로 재생 (하나 - 둘 - 셋 - ...)

                        switch (gamePlayAge)
                        {
                            case 3:
                                correctCardtargetPos = GetObject((int)Objects.CorrectCardPositions).transform
                                    .GetChild(0).GetChild(correctCardClickedCount - 1).transform.position;
                                break;
                            case 4:
                                correctCardtargetPos = GetObject((int)Objects.CorrectCardPositions).transform
                                    .GetChild(1).GetChild(correctCardClickedCount - 1).transform.position;
                                break;
                            case 5:
                                correctCardtargetPos = GetObject((int)Objects.CorrectCardPositions).transform
                                    .GetChild(2).GetChild(correctCardClickedCount - 1).transform.position;
                                break;
                        }

                        Vector3 targetScale = originalCardScale * 0.8f;

                        card.transform.DOJump(correctCardtargetPos, 0.5f, 1, 1f);
                        card.transform.DOScale(targetScale, 0.5f);
                        card.transform.DORotate(new Vector3(0, 38, 0), 1f);

                        if (correctCardClickedCount == gamePlayAge) //게임 종료 
                        {
                            switch (gamePlayAge)
                            {
                                case 3:
                                    Logger.Log("다 찾았어요! 세살!");
                                    break;
                                case 4:
                                    Logger.Log("다 찾았어요! 네살!");
                                    break;
                                case 5:
                                    Logger.Log("다 찾았어요! 다섯살!");
                                    break;
                            }

                            foreach (var cards in _cards)
                            {
                                cards.canClicked = false;
                            }

                            //카드게임 초기화
                            DOVirtual.DelayedCall(4f, () =>
                            {
                                wrongCardClickedCount = 0;
                                correctCardClickedCount = 0;

                                foreach (Transform child in GetObject((int)Objects.CardPool).transform)
                                {
                                    child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
                                }

                                DOVirtual.DelayedCall(2f, () =>
                                {
                                    SettingCardGame();
                                    
                                    for (int i = 0; i < GetObject((int)Objects.CardPool).transform.childCount; i++)
                                    {
                                        GetObject((int)Objects.CardPool).transform.GetChild(i).gameObject
                                                .transform.localPosition
                                            = GetObject((int)Objects.SetCardPositions).transform.GetChild(i).gameObject
                                                .transform.localPosition;

                                    }
                                });
                            });
                        }
                    }
                    else if (card.cardValue != gamePlayAge && card.canClicked)
                    {
                        wrongCardClickedCount++;
                        if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                        {
                            Logger.Log("아니야! 잘 생각해봐!");
                        }
                        
                        card.canClicked = false;
                        clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
                        
                    }
                }
            }
    }

    private void OnStartStage()
    {
        DOVirtual.DelayedCall(2f, () => ChangeStage(MainSeq.SelectAgeStageSequence));

    }
    
    private void OnSelectAgeStage()
    {
        //먼저 나이를 설정해주세요!
        
        // 화면 중앙에 3,4,5세 버튼이 있고 해당 버튼을 터치하면 해당 나이로 설정된 게임 진행
        // _gamePlayAge
        //터치하면 해당 버튼이 화면 중앙으로 크게 이동한 뒤 n살 나레이션 재생
        
        //테스트용
        _uiManager.ShowSelectAgeBtn();
    }

    private void OnCardGameStage()
    {
        SettingCardGame();
    }

    private void SettingCardGame()
    {
        int total = _cards.Count;

        List<int> values = new List<int>(total);

        numbers.Remove(gamePlayAge);

        for (int i = 0; i < gamePlayAge; i++)
            values.Add(gamePlayAge);

        values.AddRange(numbers);

        int leftValueCount = gamePlayAge + numbers.Count;
        for (int i = leftValueCount; i < total; i++)
            values.Add(numbers[Random.Range(0, numbers.Count)]);

        // 셔플
        for (int i = 0; i < values.Count; i++)
        {
            int j = Random.Range(0, total);
            int tmp = values[i];
            values[i] = values[j];
            values[j] = tmp;
        }

        for (int i = 0; i < total; i++)
        {
            _cards[i].SetValue(values[i]);
            _cards[i].ChangeCardValueTMP(values[i]);
        }

        foreach (var card in _cards)
            card.gameObject.transform.DOScale(originalCardScale, 1f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    card.canClicked = true;
                    if (card.cardValue == gamePlayAge)
                        card.Shake();
                });

    }

    private void OnObjectGameStage()
    {
        
    }

    private void OnChangeStage()
    {
        
    }

    private void OnEndStage()
    {
        
    }
    
    
    
    private AudioClip[] _clickClips;
    
    private void PlayClickSound() 
    {
        int idx = Random.Range(0, _clickClips.Length);
        
        if (_clickClips[idx] == null)
            Logger.Log("사운드 경로에 없음");
        
        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);
        
    }
    
    private void PlayVictorySoundAndEffect()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA038/Audio/audio_Victory");

        Get<ParticleSystem>((int)Particle.Victory1).Play();
        Get<ParticleSystem>((int)Particle.Victory2).Play();
    }

}
