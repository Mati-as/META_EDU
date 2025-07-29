using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public enum EA038_MainSeq
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
    private EA038_MainSeq _currentSequence;

    private enum Cameras
    {
        Camera1,
        Camera2,
    }

    private enum Objects
    {
        CorrectObjectPositions,
        SetObjectPositions,
        CardPool,
        CarPool,
        
        
    }
    
    private enum Cars
    {
        Ban,
        Truck,
        Taxi,
        FireTruck,
        Ambulance,
        PoliceCar,
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
    
    public List<EA038_Card> ea038_Cards;
    public List<EA038_Card> ea038_Cars;
    public List<EA038_Card> ea038_Fruits;
    public List<EA038_Card> ea038_Block;
    private Dictionary<Collider, EA038_Card> _cardByCollider;
    
    private Vector3 correctObjtargetPos;

    private Sequence cardShakeSeq;
    
    private Vector3 originalCardScale;
    private Vector3 originalCarScale;
    private Vector3 originalFruitScale;
    private Vector3 originalBlockScale;

    [SerializeField] private int totalTargetClickCount = 15;
    
    protected override void Init()
    {
        //Bind<CinemachineVirtualCamera>(typeof(Cameras));
        BindObject(typeof(Objects));
        Bind<ParticleSystem>(typeof(Particle));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA038_UIManager>();
        _currentSequence = EA038_MainSeq.StartSequence;

        psResourcePath = "EA038/Asset/Fx_Click";
        SetPool(); //클릭 이펙트 용 풀

        gamePlayAge = 5; //컨텐츠 기본 설정 나이 (3세)

        numbers = Enumerable.Range(2, 6).ToList();

        _cardByCollider = FindObjectsOfType<EA038_Card>()
            .ToDictionary(card => card.GetComponent<Collider>(), card => card);
        
        cardShakeSeq = DOTween.Sequence();

        originalCardScale = new Vector3(0.04224154f, 0.004107093f, 0.03364548f);
        originalCarScale = Vector3.one * 0.0936f;

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
            ChangeStage(EA038_MainSeq.StartSequence);
        //ChangeStage(EA038_MainSeq.CardGameStageSequence);
    }

    public void ChangeStage(EA038_MainSeq next)
    {
        _currentSequence = next;
        switch (next)
        {
            case EA038_MainSeq.StartSequence: OnStartStage(); break;
            case EA038_MainSeq.SelectAgeStageSequence: OnSelectAgeStage(); break;
            case EA038_MainSeq.CardGameStageSequence: OnCardGameStage(); break;
            case EA038_MainSeq.ObjectGameStageSequence: OnObjectGameStage(); break;
            case EA038_MainSeq.ChangeStageSequence: OnChangeStage(); break;
            case EA038_MainSeq.EndSequence: OnEndStage(); break;
            
        }
        
        Logger.Log($"{next}스테이지로 변경");
    }
    
    private int cardGamePlayCount = 0;
    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_currentSequence == EA038_MainSeq.CardGameStageSequence)
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
                                correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                    .GetChild(0).GetChild(correctCardClickedCount - 1).transform.position;
                                break;
                            case 4:
                                correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                    .GetChild(1).GetChild(correctCardClickedCount - 1).transform.position;
                                break;
                            case 5:
                                correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                    .GetChild(2).GetChild(correctCardClickedCount - 1).transform.position;
                                break;
                        }

                        Vector3 targetScale = originalCardScale * 0.8f;

                        card.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                        card.transform.DOScale(targetScale, 0.5f);
                        card.transform.DORotate(new Vector3(0, 38, 0), 1f);

                        if (correctCardClickedCount == gamePlayAge) //게임 종료 
                        {
                            cardGamePlayCount++;
                                
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

                            foreach (var cards in ea038_Cards)
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
                                    if (cardGamePlayCount == 2)
                                    {
                                        ChangeStage(EA038_MainSeq.ObjectGameStageSequence);
                                    }
                                    else
                                    {
                                        SettingCardGame();

                                        for (int i = 0; i < GetObject((int)Objects.CardPool).transform.childCount; i++)
                                        {
                                            GetObject((int)Objects.CardPool).transform.GetChild(i).gameObject
                                                    .transform.localPosition
                                                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).gameObject
                                                    .transform.localPosition;

                                        }
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
        else if (_currentSequence == EA038_MainSeq.ObjectGameStageSequence)
            foreach (var hit in GameManager_Hits)
            {
                var clickedObj = hit.collider.gameObject;
                //clickedObj.transform.DOKill();

                // clickEffectPos = hit.point;
                // //clickEffectPos.y += 0.2f;
                // PlayParticleEffect(clickEffectPos);
                // PlayClickSound();

                var obj = clickedObj.GetComponent<EA038_Car>();
                
                if (obj.carValue == gamePlayAge && obj.canClicked)
                {
                    correctCardClickedCount++;
                    Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");

                    obj.canClicked = false;
                    obj.KillShake();

                    switch (gamePlayAge)
                    {
                        case 3:
                            correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                .GetChild(0).GetChild(correctCardClickedCount - 1).transform.position;
                            break;
                        case 4:
                            correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                .GetChild(1).GetChild(correctCardClickedCount - 1).transform.position;
                            break;
                        case 5:
                            correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                .GetChild(2).GetChild(correctCardClickedCount - 1).transform.position;
                            break;
                    }

                    Vector3 targetScale = originalCarScale * 0.9f;

                    obj.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                    obj.transform.DOScale(targetScale, 0.5f);
                    obj.transform.DORotate(new Vector3(0, 0, 0), 1f);

                    if (correctCardClickedCount == gamePlayAge) //게임 종료 
                    {
                        
                        cardGamePlayCount++;
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

                        Transform carPool = GetObject((int)Objects.CarPool).transform;

                        for (int i = 0; i < carPool.childCount; i++)
                        {
                            var typeParent = carPool.GetChild(i);

                            for (int j = 0; j < typeParent.childCount; j++)
                            {
                                typeParent.GetChild(j).gameObject.GetComponent<EA038_Car>().canClicked = false;
                            }
                        }

                        //자동차 게임 초기화
                        DOVirtual.DelayedCall(4f, () =>
                        {
                            wrongCardClickedCount = 0;
                            correctCardClickedCount = 0;

                            for (int i = 0; i < GetObject((int)Objects.CarPool).transform.childCount; i++)
                            {
                                var typeParent = GetObject((int)Objects.CarPool).transform.GetChild(i);
                                for (int j = 0; j < typeParent.childCount; j++)
                                {
                                    typeParent.GetChild(j).gameObject.transform.DOScale(Vector3.zero, 1f)
                                        .SetEase(Ease.OutCubic).OnComplete(() =>
                                            typeParent.GetChild(j).gameObject.SetActive(false));
                                }
                            }

                            DOVirtual.DelayedCall(2f, () =>
                            {
                                SettingCarObject();
                            });
                        });
                    }
                }
                else if (obj.carValue != gamePlayAge && obj.canClicked)
                {
                    wrongCardClickedCount++;
                    if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                    {
                        Logger.Log("아니야! 잘 생각해봐!");
                    }

                    obj.canClicked = false;
                    clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);

                }
            }

    }

    private void OnStartStage()
    {
        DOVirtual.DelayedCall(2f, () => ChangeStage(EA038_MainSeq.SelectAgeStageSequence));

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
        //나레이션 추가 예정
        SettingCardGame();
        
    }

    private void OnObjectGameStage()
    {
        SettingCarObject();
    }
    
    private void OnChangeStage()
    {
        
    }

    private void OnEndStage()
    {
        
    }

    #region 카드게임 기능
    private void SettingCardGame()
    {
        int total = totalTargetClickCount;

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
            ea038_Cards[i].SetValue(values[i]);
            ea038_Cards[i].ChangeValueTMP(values[i]);
        }

        foreach (var card in ea038_Cards)
            card.gameObject.transform.DOScale(originalCardScale, 1f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    card.canClicked = true;
                    if (card.cardValue == gamePlayAge)
                        card.Shake();
                });

    }
    
    
    #endregion
    
    #region 자동차 게임 기능
    
    Dictionary<int, Cars> _CarMapping = new Dictionary<int, Cars>();
    private void SettingCarObject()
    {
        for (int i = 0; i < GetObject((int)Objects.CarPool).transform.childCount; i++)
        {
            var typeParent = GetObject((int)Objects.CarPool).transform.GetChild(i);
            for (int j = 0; j < typeParent.childCount; j++)
            {
                typeParent.GetChild(j).gameObject.SetActive(false);
            }
        }
        
        Cars[] carArray = (Cars[])Enum.GetValues(typeof(Cars));

        // 셔플
        for (int i = carArray.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);  // 0 <= j <= i
            Cars tmp       = carArray[i];
            carArray[i]    = carArray[j];
            carArray[j]    = tmp;
        }

        _CarMapping.Clear();

        for (int i = 0; i < carArray.Length; i++)
        {
            int key = i + 2;
            _CarMapping.Add(key, carArray[i]);
        }
        

        int total = totalTargetClickCount;

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

        Dictionary<Cars,int> reverseMap = _CarMapping
            .ToDictionary(pair => pair.Value, pair => pair.Key);

        Transform carPool = GetObject((int)Objects.CarPool).transform;

        for (int i = 0; i < carPool.childCount; i++)
        {
            var typeParent = carPool.GetChild(i);
            Cars carType = (Cars)i; 

            int key = reverseMap[carType];

            for (int j = 0; j < typeParent.childCount; j++)
            {
                var car = typeParent.GetChild(j)
                    .GetComponent<EA038_Car>();
                car.SetValue(key);
                car.ChangeValueTMP(key);
            }
        }
        
        for (int i = 0; i < total; i++)             //values[] 대로 딕셔너리에서 뽑아와서 해당 자동차 생성하는 로직
        {
            GetDeactiveChild(GetObject((int)Objects.CarPool).transform.GetChild((int)_CarMapping[values[i]]).transform).transform.position
                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).transform.position;

            EA038_Car car = GetDeactiveChild(GetObject((int)Objects.CarPool).transform.GetChild((int)_CarMapping[values[i]]).transform)
                .transform.gameObject.GetComponent<EA038_Car>();

            GameObject carObj = GetDeactiveChild(GetObject((int)Objects.CarPool).transform
                .GetChild((int)_CarMapping[values[i]]).transform);

            carObj.SetActive(true);
            carObj.transform.DOScale(originalCarScale, 1f).SetEase(Ease.OutBack)
                .From(Vector3.zero)
                .OnComplete(() =>
                {
                    car.canClicked = true;
                    if (car.carValue == gamePlayAge)
                        car.Shake();
                });
        }
    }

    GameObject GetDeactiveChild(Transform parent)
    {
        foreach (Transform child in parent)
            if (!child.gameObject.activeSelf)
                return child.gameObject;
        return null;
    }
    
    #endregion
    
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
