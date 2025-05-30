using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class EA019_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default,
        OnIntro,
        OnColor,
        OnShape,
        OnBalloonFind,
        OnOutro,
        OnFinish
    }

    private enum AnimSeqOnColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Pink,
    }
    
    private enum AnimSeqOnShape
    {
        Heart,
        Triangle,
        Star,
        Square,
        Balloon,
        Flower,
    }
   

    private enum Objs
    {
        Balloons,
        Balloon_RedHeart,
        Balloon_OrangeTriangle,
        Balloon_YellowStar,
        Balloon_GreenCircle,
        Balloon_BlueSquare,
        Balloon_PinkFlower,
        
        Intro_Hearts,
        Intro_Triangles,
        Intro_Stars,
        Intro_Circles,
        Intro_Squares,
        Intro_Flowers,
        
        ShapeIntro_Hearts,
        ShapeIntro_Triangles,
        ShapeIntro_Stars,
        ShapeIntro_Circles,
        ShapeIntro_Squares,
        ShapeIntro_Flowers,
        
        BalloonAppearPositions
    }
    
    public int CurrentMainSeqNum
    {
        get => CurrentMainMainSequence;
        set
        {

            CurrentMainMainSequence = value;
            ChangeThemeSeqAnim(value);
            
            switch (value)
            {
                case (int)MainSeq.Default:
                    
                    break;

                case (int)MainSeq.OnIntro:
                    PlayIntroBalloonsAnim();
   
                    DOVirtual.DelayedCall(8f,()=>
                    {
                        KillIntroBalloonsAnim();
                    });
                    DOVirtual.DelayedCall(10f,()=>
                    {
                        CurrentMainSeqNum = (int)MainSeq.OnColor;
                       PlayAnimForOnColorOrShapeSeq((int)Objs.Intro_Hearts,delay:2.5f);
                    });
                    break;

                case (int)MainSeq.OnColor:
                    //초기화
                    _uiManager.PopFromZeroInstructionUI("색깔을 알아볼까요?");
                    _currentSubSeqNum = 0;
                    curruentIntroObjNum = (int)Objs.Intro_Hearts;

                    mainAnimator.SetInteger(SEQ_NUM, (int)MainSeq.OnColor);
                    mainAnimator.SetInteger(SUB_SEQ_NUM, _currentSubSeqNum);
                 
                    
                    break;

                case (int)MainSeq.OnShape:
                    _currentSubSeqNum = 0;
                    curruentIntroObjNum = (int)Objs.Intro_Hearts;
                   
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Flowers), true);
                    _uiManager.PopFromZeroInstructionUI("모양을 알아볼까요?");
                    
                    DOVirtual.DelayedCall(2.5f,()=>
                    {
                        PlayAnimForOnColorOrShapeSeq((int)Objs.Intro_Hearts,delay:2.5f);
                    });
                    break;

                case (int)MainSeq.OnBalloonFind:
                    _currentSubSeqNum = 0;
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Flowers), true);
                      
                    DOVirtual.DelayedCall(3.5f,()=>
                    {
                        
                        _uiManager.PopFromZeroInstructionUI("제시된 풍선을 터치해서 풍선을 날려주세요");
                        DOVirtual.DelayedCall(3.5f,()=>
                        {
                            StartBalloonFindRound();
                        });
                    });

                   
                    break;

                case (int)MainSeq.OnOutro:
                    break;

                case (int)MainSeq.OnFinish:
                    break;

            }
        }
    }

    private int curruentIntroObjNum = (int)Objs.Intro_Hearts;

    #region 색깔풍선 찾기 파트 -------------------------------------------

    //private readonly Dictionary<int,bool> _isPosEmptyMap = new(); // 재생성관련 , true인경우 좋은음식은 여기서 생성
    private readonly Dictionary<int, GameObject> allObj = new();
    // 각 풍선 담아놓는 풀
    private readonly Dictionary<int, Stack<GameObject>> _balloonClonePool = new();
    private EA019_UIManager _uiManager;

    private int _currentSubSeqNum = 0; // 애니메이터 서브시퀀스 번호
    private int SUB_SEQ_NUM = Animator.StringToHash("subSeqNum");
    
    public GameObject PoolRoot
    {
        get
        {
            var root = GameObject.Find("@BalloonPoolRoot");
            if (root == null) root = new GameObject { name = "@BalloonPoolRoot" };

            
            root.gameObject.transform.localScale = Vector3.one*0.2772007f;
            return root;
        }
    }
    private int _currentRound = 1;
    private const int MAX_ROUND = 5;
    private const int COLUMN_COUNT = 7;
    private const int ROW_COUNT = 3;
    private Vector3[][] _balloonFindPosArray = new Vector3[ROW_COUNT][];
    
    private List<Objs> _remainAnswerList =new List<Objs>// 남은 정답 후보
    {
        Objs.Balloon_RedHeart,
        Objs.Balloon_OrangeTriangle,
        Objs.Balloon_YellowStar,
        Objs.Balloon_GreenCircle,
        Objs.Balloon_PinkFlower
    }; 
    private List<GameObject> _spawnedBalloons = new(); // 풀에서 꺼낸 현재 라운드의 풍선들


    private void SaveBalloonsPosArray()
    {
        for(int i = 0 ; i < _balloonFindPosArray.Length; i++)
        {
            _balloonFindPosArray[i] = new Vector3[COLUMN_COUNT];
            for(int k =0 ; k < COLUMN_COUNT ; k++)
            {
                _balloonFindPosArray[i][k] =  
                    GetObject((int)Objs.BalloonAppearPositions).transform
                        .GetChild(i).GetChild(k).transform.position;
                Logger.ContentTestLog("BalloonAppearPositions : " + _balloonFindPosArray[i][k]);
            }
        }
    }

    /// <summary>
    /// 씬 배치 객체는 enum 활용하나, instatiate로 생성되는 객체는 ID로 
    /// </summary>
    private void SetBalloonPool()
    {

        for (int objEnum = (int)Objs.Balloon_RedHeart; objEnum <= (int)Objs.Balloon_PinkFlower; objEnum++)
        {

            allObj.Add(GetObject(objEnum).transform.GetInstanceID(), GetObject(objEnum));
           
            _balloonClonePool.Add(objEnum, new Stack<GameObject>());
//            _defaultPosMap.Add(objEnum, GetObject(objEnum).transform.position);
            //_isPosEmptyMap.Add(objEnum, false);


            for (int count = 0; count < 50; count++)
            {
                var instantiatedFood = Instantiate(GetObject((int)objEnum), PoolRoot.transform, true);
                instantiatedFood.name = ((Objs)objEnum).ToString() + $"{objEnum}".ToString();
                _balloonClonePool[objEnum].Push(instantiatedFood);
                allObj.Add(instantiatedFood.transform.GetInstanceID(), instantiatedFood);
                _defaultSizeMap.TryAdd(instantiatedFood.transform.GetInstanceID(),
                    instantiatedFood.transform.localScale);
                instantiatedFood.SetActive(false);

            }
        }
    }

    private void SpawnBalloonsForCurrentRound()
    {
        if (_remainAnswerList.Count == 0)
        {
            Logger.ContentTestLog("🎉 모든 라운드 완료!");
            return;
        }
      

        // 1. 정답 풍선 하나 뽑기
        int answerIndex = Random.Range(0, _remainAnswerList.Count);
        Objs correctBalloonType = _remainAnswerList[answerIndex];
        _remainAnswerList.RemoveAt(answerIndex);
        
        Logger.ContentTestLog("🎈 풍선 찾기 라운드 시작: " + _currentRound + ", 정답 풍선: " + correctBalloonType);
        
        // 2. 위치 섞기
        List<Vector3> allPositions = _balloonFindPosArray.SelectMany(posRow => posRow).ToList();
        allPositions = allPositions.OrderBy(_ => Random.value).ToList();

        // 3. 정답 풍선 10개
        for (int i = 0; i < 10; i++)
        {
            GameObject balloon = GetBalloonFromPool((int)correctBalloonType);
            balloon.transform.position = allPositions[i];
            balloon.transform.localScale = Vector3.zero;
            balloon.SetActive(true);
            _spawnedBalloons.Add(balloon);

            // DOTween 바람 애니메이션
            balloon.transform.DOScale(_defaultSizeMap[balloon.transform.GetInstanceID()], 0.5f).SetEase(Ease.OutBack);
        }

        // 4. 오답 풍선 11개
        var wrongTypes = _remainAnswerList.ToList();
        while (wrongTypes.Count < 4) wrongTypes.Add(correctBalloonType); // 오답 풍선 부족 방지용

        for (int i = 10; i < 21; i++)
        {
            Objs wrongType;
            do
            {
                wrongType = wrongTypes[Random.Range(0, wrongTypes.Count)];
            } while (wrongType == correctBalloonType);

            GameObject balloon = GetBalloonFromPool((int)wrongType);
            balloon.transform.position = allPositions[i];
            balloon.transform.localScale = Vector3.zero;
            balloon.SetActive(true);
            _spawnedBalloons.Add(balloon);

            balloon.transform.DOScale(_defaultSizeMap[balloon.transform.GetInstanceID()], 0.5f).SetEase(Ease.OutBack);
        }
    }
    
    private GameObject GetBalloonFromPool(int objEnum)
    {
        if (_balloonClonePool[objEnum].Count == 0)
        {
            Logger.ContentTestLog($"⚠ 풀 부족! {objEnum} 인스턴스 생성");
            var newObj = Instantiate(GetObject(objEnum), PoolRoot.transform);
            allObj.Add(newObj.transform.GetInstanceID(), newObj);
            _defaultSizeMap.TryAdd(newObj.transform.GetInstanceID(), newObj.transform.localScale);
            return newObj;
        }

        return _balloonClonePool[objEnum].Pop();
    }
    
    private void StartBalloonFindRound()
    {
        
        SpawnBalloonsForCurrentRound();
    }

// 다음 라운드로 갈 때 (정답 누르면):
    private void GoToNextBalloonFindRound()
    {
        ReturnBalloonsToPool();
        SpawnBalloonsForCurrentRound();
    }
    
    private void ReturnBalloonsToPool()
    {
        foreach (var balloon in _spawnedBalloons)
        {
            int objEnum = allObj.FirstOrDefault(x => x.Value == balloon).Key;
            balloon.SetActive(false);
            _balloonClonePool[objEnum]?.Push(balloon);
        }

        _spawnedBalloons.Clear();
    }
    #endregion

   
    
    

    #region Main Init ---------------------------------------------------------------------------------------------
    protected override void Init()
    {
        BindObject(typeof(Objs));
        
        psResourcePath = "Runtime/EA019/Fx_Click";
        base.Init();
        
        _uiManager = UIManagerObj.GetComponent<EA019_UIManager>();
        InitBalloonsForIntro();
        
        EA019_UIManager.onNextButtonClicked -= OnNextButtonClicked;
        EA019_UIManager.onNextButtonClicked += OnNextButtonClicked;
        
        for(int i =(int)Objs.Intro_Hearts; i <= (int)Objs.Intro_Flowers; i++)
        {
            for (int k = 0; k <  GetObject(i).transform.childCount; k++)
            {
                Transform child = GetObject(i).transform.GetChild(k);
                _defaultSizeMap.TryAdd(child.GetInstanceID(), child.localScale);
                child.localScale = Vector3.zero;
            }
        }
        GetObject((int)Objs.Intro_Hearts).gameObject.SetActive(false);
        GetObject((int)Objs.Intro_Triangles).gameObject.SetActive(false);
        GetObject((int)Objs.Intro_Stars).gameObject.SetActive(false);
        GetObject((int)Objs.Intro_Circles).gameObject.SetActive(false);
        GetObject((int)Objs.Intro_Squares).gameObject.SetActive(false);
        GetObject((int)Objs.Intro_Flowers).gameObject.SetActive(false);
        
        SaveBalloonsPosArray();
        SetBalloonPool();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EA019_UIManager.onNextButtonClicked -= OnNextButtonClicked;
    }
    #endregion

    
    #region Main Animation Part On Shape & Color -------------------------------------------------------------------
    
    private void PlayAnimForOnColorOrShapeSeq(int index, float delay = 2.2f)
    {
    
      
        switch (index)
        {
            case (int)Objs.Intro_Hearts:

                DOVirtual.DelayedCall(delay, () =>
                {
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Hearts));
                    _uiManager.PopFromZeroInstructionUI(  CurrentMainSeqNum == (int)MainSeq.OnColor?"빨간색":"하트");
                    _uiManager.ActivateNextButton(2f);
                });
                break;
            case (int)Objs.Intro_Triangles:
                Logger.ContentTestLog("triangles(Orange) Anim Play-------");
                PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Hearts), true);
                DOVirtual.DelayedCall(delay, () =>
                {
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Triangles));

                    _uiManager.PopFromZeroInstructionUI(CurrentMainSeqNum == (int)MainSeq.OnColor?"주황색":"세모");
                    _uiManager.ActivateNextButton(2f);
                });
                break;
            case (int)Objs.Intro_Stars:
                PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Triangles), true);
                DOVirtual.DelayedCall(delay, () =>
                {
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Stars));

                    _uiManager.PopFromZeroInstructionUI(CurrentMainSeqNum == (int)MainSeq.OnColor?"노란색":"별");
                    _uiManager.ActivateNextButton(2f);
                });
                break;
            case (int)Objs.Intro_Circles:
                PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Stars), true);
                DOVirtual.DelayedCall(delay, () =>
                {
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Circles));

                    _uiManager.PopFromZeroInstructionUI(CurrentMainSeqNum == (int)MainSeq.OnColor?"초록색":"동그라미");
                    _uiManager.ActivateNextButton(2f);
                });
                break;
            case (int)Objs.Intro_Squares:
                PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Circles), true);
                DOVirtual.DelayedCall(delay, () =>
                {
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Squares));

                    _uiManager.PopFromZeroInstructionUI(CurrentMainSeqNum == (int)MainSeq.OnColor?"파란색":"네모");
                    _uiManager.ActivateNextButton(2f);
                });
                break;
            case (int)Objs.Intro_Flowers:
                PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Squares), true);
                DOVirtual.DelayedCall(delay, () =>
                {
                    PlayScaleAnimOnColor(GetObject((int)Objs.Intro_Flowers));
                    _uiManager.PopFromZeroInstructionUI(CurrentMainSeqNum == (int)MainSeq.OnColor?"보라색":"꽃");
                    _uiManager.ActivateNextButton(2f);
                });
                break;
        }
    }

    private Dictionary<int, Transform> _onColorBalloonMap = new();
    private void PlayScaleAnimOnColor(GameObject gameObject,bool isToZero = false)
    {
        gameObject.SetActive(true);
        
        
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform child = gameObject.transform.GetChild(i);
         
                
                int instanceID = child.GetInstanceID();
                
                _onColorBalloonMap.TryAdd(instanceID, child);
             
               
                _sequenceMap.TryAdd(instanceID, DOTween.Sequence());
                
               _sequenceMap[instanceID]?.Kill();
               _sequenceMap[instanceID] = DOTween.Sequence();
               
               
                _sequenceMap[instanceID].
                    Append(child.DOScale(
                        isToZero? Vector3.zero : _defaultSizeMap[child.GetInstanceID()], 0.5f).SetEase(Ease.OutBounce));
                _sequenceMap[instanceID].AppendInterval(0.5f);
                _sequenceMap[instanceID].Join(child.DOShakePosition(100f, Random.Range(0.1f, 0.1f),vibrato:2));
                _sequenceMap[instanceID].Append(child.DOShakeScale(100f, Random.Range(0.1f, 0.2f),vibrato:2));
                
            
        }
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();

        DOVirtual.DelayedCall(1.5f, () =>
        {
            initialMessage = "색깔 풍선이 나무에 걸려있어요~";
            _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
            CurrentMainSeqNum = (int)MainSeq.OnBalloonFind;
        });
        
    }
    
    private void OnNextButtonClicked()
    {
        //카메라 등 게임매니져 컨트롤러 제어
        _currentSubSeqNum++;

        // 실행함수 제어 
        curruentIntroObjNum++;

        if (CurrentMainSeqNum == (int)MainSeq.OnColor && _currentSubSeqNum >(int)AnimSeqOnColor.Pink)
        {
            CurrentMainSeqNum = (int)MainSeq.OnShape;
            
        }
        else if (CurrentMainSeqNum == (int)MainSeq.OnShape && _currentSubSeqNum > (int)AnimSeqOnShape.Flower)
        {
            CurrentMainSeqNum = (int)MainSeq.OnBalloonFind;
        }
        else
        {
            if (CurrentMainSeqNum == (int)MainSeq.OnColor)
            {
                
                PlayAnimForOnColorOrShapeSeq(curruentIntroObjNum);
                Logger.ContentTestLog($"NextBtnClicked : Current Seq number: {(AnimSeqOnColor)_currentSubSeqNum}");
            }
            else if(CurrentMainSeqNum == (int)MainSeq.OnShape)
            {
     
                Logger.ContentTestLog($"NextBtnClicked : Current Seq number: {(AnimSeqOnShape)curruentIntroObjNum}");
                PlayAnimForOnColorOrShapeSeq(curruentIntroObjNum);
            }
     
            mainAnimator.SetInteger(SUB_SEQ_NUM, _currentSubSeqNum);
        }
    }

    private void OnNextBtnClickedOnColorSeq()
    {
        
    }

    private void OnNextBtnClickedOnShapeSeq()
    {
        
    }

    private Dictionary<int,Transform> introBalloons = new Dictionary<int, Transform>();
    private void InitBalloonsForIntro()
    {

        for (int i = 0; i < GetObject((int)Objs.Balloons).transform.childCount; i++)
        {
            Transform child = GetObject((int)Objs.Balloons).transform.GetChild(i);
         
            int ID = child.GetInstanceID();
            
            introBalloons.Add(ID, child);
            _defaultSizeMap.TryAdd(ID, child.transform.localScale);
            
            child.transform.localScale = Vector3.zero;
        }
        
        GetObject((int)Objs.Balloons).SetActive(false);
    }

    private void PlayIntroBalloonsAnim(float delay = 1.2f)
    {
        GetObject((int)Objs.Balloons).SetActive(true);
        GetObject((int)Objs.Balloons).transform.localScale = _defaultSizeMap[(int)Objs.Balloons];

        foreach (int key in introBalloons.Keys.ToArray())
        {
            _sequenceMap.Add(key, DOTween.Sequence());

            _sequenceMap[key].SetDelay(Random.Range(delay - 0.7f, delay + 0.7f));

            _sequenceMap[key].Append(introBalloons[key].DOScale(_defaultSizeMap[key], 1.25f)
                .SetEase(Ease.OutBounce)
                .SetDelay(0.1f * introBalloons.Count));
            _sequenceMap[key].Join(introBalloons[key].DOShakePosition(30f, Random.Range(0.1f, 0.1f),vibrato:3));
            _sequenceMap[key].Append(introBalloons[key].DOShakeScale(30f, Random.Range(0.1f, 0.2f),vibrato:3));
        }
    }

    private void KillIntroBalloonsAnim()
    {
        Logger.ContentTestLog("Kill Intro Balls -----------------");
        foreach (int key in _sequenceMap.Keys.ToArray())
        {
            _sequenceMap[key]?.Kill();
        }
        
        foreach (int key in introBalloons.Keys.ToArray())
        {
            introBalloons[key]?.DOScale(Vector3.zero, Random.Range(0.15f,0.8f)).SetDelay(Random.Range(0.05f,0.15f)).OnComplete(() =>
            {
                PlayParticleEffect( introBalloons[key].position);
            });
        
        }
    }
    

    #endregion
    
}
