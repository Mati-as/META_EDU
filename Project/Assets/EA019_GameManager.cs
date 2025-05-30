using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.DemiLib;
using DG.Tweening;
using Unity.PlasticSCM.Editor.WebApi;
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
                    _uiManager.PopFromZeroInstructionUI("풍선이 날아가요~");
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

            
            //root.gameObject.transform.localScale = Vector3.one*0.2772007f;
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
        Objs.Balloon_BlueSquare,
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
                var instantiatedBalloon = Instantiate(GetObject((int)objEnum), PoolRoot.transform, true);
                
                instantiatedBalloon.name = ((Objs)objEnum).ToString() + $"{objEnum}".ToString();
                instantiatedBalloon.transform.rotation = GetObject(objEnum).transform.rotation;// 초기 회전값 설정
                _balloonClonePool[objEnum].Push(instantiatedBalloon);
                allObj.Add(instantiatedBalloon.transform.GetInstanceID(), instantiatedBalloon);
                _defaultSizeMap.Add(instantiatedBalloon.transform.GetInstanceID(),instantiatedBalloon.transform.localScale);
                
                Logger.ContentTestLog($"clone default rotation: {instantiatedBalloon.transform.localRotation.eulerAngles}");
                
                instantiatedBalloon.SetActive(false);

            }
        }
    }

    private string _currentBalloonNameToFind;
    private void SpawnBalloonsForCurrentRound()
    {
        if (_remainAnswerList.Count == 0)
        {
            Logger.ContentTestLog("🎉 모든 라운드 완료!");
            CurrentMainSeqNum = (int)MainSeq.OnFinish;
            return;
        }
      

        // 1. 정답 풍선 하나 뽑기
        int answerIndex = Random.Range(0, _remainAnswerList.Count);
        _currentBalloonNameToFind = ((Objs)_remainAnswerList[answerIndex]).ToString();
        Objs correctBalloonType = _remainAnswerList[answerIndex];
        _remainAnswerList.RemoveAt(answerIndex);
        
        Logger.ContentTestLog($"🎈 풍선 찾기 라운드 시작: " + _currentRound + $",정답 풍선: {_currentBalloonNameToFind}");
        
        switch ((int)correctBalloonType)
        {
            case (int)Objs.Balloon_RedHeart:
                _uiManager.PopFromZeroInstructionUI("빨간색 하트모양 풍선을 발로 터치해주세요!");
                break;
            case (int)Objs.Balloon_OrangeTriangle:
                _uiManager.PopFromZeroInstructionUI("주황색 세모모양 풍선을 발로 터치해주세요!");
                break;
            case (int)Objs.Balloon_YellowStar:
                _uiManager.PopFromZeroInstructionUI("노란색 별모양 풍선을 발로 터치해주세요!");
                break;
            case (int)Objs.Balloon_GreenCircle:
                _uiManager.PopFromZeroInstructionUI("초록색 동그라미 모양 풍선을 발로 터치해주세요!");
                break;
            case (int)Objs.Balloon_BlueSquare:
                _uiManager.PopFromZeroInstructionUI("파란색 네모 모양 풍선을 발로 터치해주세요!");
                break;
            case (int)Objs.Balloon_PinkFlower:
                _uiManager.PopFromZeroInstructionUI("보라색 꽃 모양 풍선을 발로 터치해주세요!");
                break;
        }
        
        List<Vector3> allPositions = _balloonFindPosArray
            .SelectMany(posRow => posRow)
            .Distinct()
            .OrderBy(_ => Random.value)
            .ToList();

        // 3. 정답 풍선 10개
        for (int i = 0; i < 10; i++)
        {
            GameObject balloon = GetBalloonFromPool((int)correctBalloonType);
            
            balloon.transform.position = allPositions[i];
            balloon.transform.localScale = Vector3.zero;
            balloon.SetActive(true);
            _spawnedBalloons.Add(balloon);

            // DOTween 바람 애니메이션
            int id = balloon.transform.GetInstanceID();
            _sequenceMap.TryAdd(id, DOTween.Sequence());
            _sequenceMap[id]?.Kill();
            _sequenceMap[id] = DOTween.Sequence();
            
            _sequenceMap[id].Append(balloon.transform.DOScale(_defaultSizeMap[balloon.transform.GetInstanceID()], 0.5f)
                .SetDelay(Random.Range(0.1f, 1f))
                .SetEase(Ease.OutBack));
           
            //_sequenceMap[id].Append(balloon.transform.DOShakeScale(100f, Random.Range(0.1f, 0.2f),vibrato:2));
            _sequenceMap[id].Join(balloon.transform.DOShakePosition(100f, Random.Range(0.2f, 0.35f),vibrato:1));
            _sequenceMap[id].Join(balloon.transform.DOShakeRotation(100f, Random.Range(0.2f, 0.35f),vibrato:1).SetDelay(Random.Range(0.1f,0.2f)));
            
        }

        // 4. 오답 풍선 11개
        var wrongList = new List<Objs> // 남은 정답 후보
        {
            Objs.Balloon_RedHeart,
            Objs.Balloon_OrangeTriangle,
            Objs.Balloon_YellowStar,
            Objs.Balloon_GreenCircle,
            Objs.Balloon_BlueSquare,
            Objs.Balloon_PinkFlower
        };

        wrongList.RemoveAt(answerIndex);
        // 4. 오답 풍선 11개
        var wrongTypes = wrongList.ToList();
        //while (wrongTypes.Count < 4) wrongTypes.Add(correctBalloonType); // 오답 풍선 부족 방지용

        for (int i = 10; i < 21; i++)
        {
          
            Objs wrongType;
            do
            {
                wrongType = wrongTypes[Random.Range(0, wrongTypes.Count)];
            } while (wrongType == correctBalloonType);

            GameObject balloon = GetBalloonFromPool((int)wrongType);
            
            int id = balloon.transform.GetInstanceID();
            _sequenceMap.TryAdd(id, DOTween.Sequence());
            _sequenceMap[id]?.Kill();
            
            balloon.transform.position = allPositions[i];
            balloon.transform.localScale = Vector3.zero;
            balloon.SetActive(true);
            _spawnedBalloons.Add(balloon);

            balloon.transform.DOScale(_defaultSizeMap[balloon.transform.GetInstanceID()], 0.4f)
                .SetDelay(Random.Range(0.1f, 1f))
                .SetEase(Ease.OutBack);
        }
    }
    
    
    
    
    private Dictionary<GameObject, int> _balloonOriginMap = new(); // GameObject → Objs enum
    private GameObject GetBalloonFromPool(int objEnum)
    {
        GameObject obj;
    
        if (_balloonClonePool[objEnum].Count == 0)
        {
            Logger.ContentTestLog($"⚠ 풀 부족! {objEnum} 인스턴스 생성");
            obj = Instantiate(GetObject(objEnum), PoolRoot.transform);
            allObj[obj.transform.GetInstanceID()] = obj;
            _defaultSizeMap[obj.transform.GetInstanceID()] = obj.transform.localScale;
        }
        else
        {
            obj = _balloonClonePool[objEnum].Pop();
        }

        _balloonOriginMap[obj] = objEnum; // ✅ 여기서 enum 기록
        return obj;
    }
    private void StartBalloonFindRound()
    {
        foreach (var key in _sequenceMap.Keys.ToArray())
        {
            _sequenceMap[key]?.Kill();
            _sequenceMap[key] = DOTween.Sequence();
        }
        
        
        
        SpawnBalloonsForCurrentRound();

        
        DOVirtual.DelayedCall(2f, () =>
        {
            Logger.ContentTestLog("🎈 풍선 찾기 라운드 시작--------------- 이제 부터 클릭 가능 ");
            _isRoundFinished = false;
        });
    }

// 다음 라운드로 갈 때 (정답 누르면):
    private void GoToNextBalloonFindRound()
    {
        ReturnBalloonsToPool();
        DOVirtual.DelayedCall(3f, () =>
        {
            SpawnBalloonsForCurrentRound();
            InitBalloonFindClickData();
        });

    }
    
    
    private void OnBalloonFindRoundFinished()
    {
        _uiManager.PopFromZeroInstructionUI("풍선을 전부 날려버렸어!");

        DOVirtual.DelayedCall(3f, () =>
        {
            GoToNextBalloonFindRound();
        });
    }
    private void InitBalloonFindClickData()
    {
        _currentClickedCountMap.Clear();
        _currentBalloonScaleMap.Clear();
        isDisappearedMap.Clear();
        currentBalloonFindCount = 0;
        _isRoundFinished = false;
    }
    private void ReturnBalloonsToPool()
    {
        foreach (var balloon in _spawnedBalloons)
        {
            if (!_balloonOriginMap.TryGetValue(balloon, out var objEnum))
            {
                Logger.ContentTestLog($"❌ Balloon origin type not found. Skipping.");
                continue;
            }

            // 애니메이션 + 비활성화 + 풀로 되돌리기
            balloon.transform.DOScale(Vector3.zero, Random.Range(0.5f, 0.7f))
                .SetDelay(Random.Range(0.1f, 0.2f))
                .OnComplete(() =>
                {
                    balloon.SetActive(false);
                    _balloonClonePool[objEnum].Push(balloon);
                });
        }

        _spawnedBalloons.Clear();
    }

    public override void OnRaySynced()
    {
        if(!PreCheckOnRaySync()) return;
        if (CurrentMainSeqNum == (int)MainSeq.OnBalloonFind)
        {
            if (_isRoundFinished) return; 
            
            OnRaySyncOnBalloonFind();
        }
    }


    private bool _isRoundFinished=true;
    private const int MAX_CLICK_COUNT = 3; // 풍선 클릭 최대 횟수
    private const int BALLOON_COUNT_TO_FIND =10 ; // 풍선 찾기 라운드에서 찾을 풍선 개수
    private int currentBalloonFindCount = 0; // 현재 라운드에서 찾은 풍선 개수
    private Dictionary<int, float> _currentBalloonScaleMap =new();
    private Dictionary<int, int> _currentClickedCountMap= new (); 
    private Dictionary<int,bool> isDisappearedMap = new();
    private const float BALLOON_RISE_Y_DISTANCE = 6.0f;
    private const float BALLOON_RISE_DURATION = 0.75f;
    private const string MUST_CONTAIN_STRING = "Balloon"; // 풍선 이름에 반드시 포함되어야 하는 문자열
    private void OnRaySyncOnBalloonFind()
    {
        foreach (var hit in GameManager_Hits)
        {
            if(hit.transform.gameObject.name.Contains(MUST_CONTAIN_STRING) == false)
                continue; // 풍선이 아닌 오브젝트는 무시
            
            
            Transform tf = hit.transform;
            int id = tf.GetInstanceID();

            _isClickableMap.TryAdd(id, true);
            if (!_isClickableMap[id]) continue;
            _isClickableMap[id] = false; // 클릭 후 더 이상 클릭 불가
            DOVirtual.DelayedCall(0.3f, () => _isClickableMap[id] = true); // 0.1초 후 다시 클릭 가능
            
            
            // 이미 사라진 풍선은 무시
            if (isDisappearedMap.TryGetValue(id, out var isGone) && isGone)
                continue;

            // 정답 풍선인지 확인
            if (!tf.gameObject.name.Contains(_currentBalloonNameToFind))
            {
                tf.DOScale(Vector3.zero, 0.35f).SetEase(Ease.OutBounce);
                isDisappearedMap[id] = true; // 클릭했지만 정답이 아닌 풍선은 사라짐
                PlayParticleEffect(hit.point);
                continue;
            }
            

            // 클릭 카운트 증가
            if (!_currentClickedCountMap.ContainsKey(id))
                _currentClickedCountMap[id] = 0;
            _currentClickedCountMap[id]++;

            int clickCount = _currentClickedCountMap[id];

            // 현재 scale 계산
            if (!_currentBalloonScaleMap.ContainsKey(id))
                _currentBalloonScaleMap[id] = _defaultSizeMap[id].x;

            float baseScale = _defaultSizeMap[id].x;
            float nextScale = clickCount == 1 ? baseScale * 1.25f :
                clickCount == 2 ? baseScale * 1.6f : baseScale;

            // scale 애니메이션
            tf.DOScale(Vector3.one * nextScale, 0.3f).SetEase(Ease.OutBack);

            // 세 번째 클릭 시 날아가기
            if (clickCount >= MAX_CLICK_COUNT)
            {
                float lastEffectTime = 0;
                isDisappearedMap[id] = true;
                tf.DOScale(Vector3.zero, 2.5f);
                tf.DOMove(new Vector3(tf.position.x+Random.Range(-2f,2f), 
                    tf.position.y + BALLOON_RISE_Y_DISTANCE, tf.position.z) ,BALLOON_RISE_DURATION).OnUpdate(() =>
                {
                    if (Time.time - lastEffectTime >= 0.05f)
                    {
                        PlayParticleEffect(tf.transform.position);
                        lastEffectTime = Time.time;
                    }
                }).SetEase(Ease.OutSine).OnComplete(() =>
                {
                    tf.gameObject.SetActive(false); // 풀로 안 돌려도 됨
                });
              
                currentBalloonFindCount++;
                Logger.ContentTestLog($"🎯 Found: {currentBalloonFindCount}/{BALLOON_COUNT_TO_FIND}");

                if (currentBalloonFindCount >= BALLOON_COUNT_TO_FIND && !_isRoundFinished)
                {
                    _isRoundFinished = true;
                    OnBalloonFindRoundFinished(); // 다음 라운드 트리거 등 처리
                }
            }
        }
    }

   
    #endregion
    
   
    
    

    #region Main Init ---------------------------------------------------------------------------------------------
    protected override void Init()
    {
        BindObject(typeof(Objs));
        
        DOTween.SetTweensCapacity(500,1000);
        psResourcePath = "Runtime/EA019/Fx_Click";
        base.Init();
        SetBalloonPool(); //zero로 초기화하기 전에 defaultsizemap에 저장 필요 주의 
        
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
                _sequenceMap[instanceID].Join(child.DOShakeRotation(100f, Random.Range(0.1f, 0.1f),vibrato:2));
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
