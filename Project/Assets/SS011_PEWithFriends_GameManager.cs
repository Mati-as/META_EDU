using DG.Tweening;
using UnityEngine;
using Sequence = Unity.VisualScripting.Sequence;

public class SS011_PEWithFriends_GameManager : Ex_BaseGameManager
{
    public enum States
    {
        Default,

        OnMainIntro,
        TeamSelection,

        AssignColorToTeam,

        //WaitOutsideScreen
        GameA_HowToPlay,
        GameA_Playing,
        OnGameAFinish,

        //WaitOutsideScreen
        GameB_HowToPlay,
        GameB_Playing,
        OnGameBFinish,

        //WaitOutsideScreen
        GameC_HowToPlay,
        GameC_Playing,
        OnGameCFinish,

        OnMainFinish,


        WaitOutsideScreen = 123
    }

    public class Team
    {
        public string teamName;
        public Color teamColor;

        public Team(string name = null, Color color = default)
        {
            teamName = name;
            teamColor = color;
        }
    }

    public Team teamA = new();
    public Team teamB = new();
    public Team teamC = new();

    public class Block
    {
        private readonly Transform blockTransform;
        public Color currentColor;
        private Sequence blockSequence;

        public Block(string name = null, Color color = default, Transform transform = null)
        {
            blockTransform = transform;
            currentColor = color;
            if (blockTransform != null)
            {
                blockTransform.name = name;
                blockTransform.GetComponent<SpriteRenderer>().color = color;
            }
        }
    }
    
    int CurrentMainSeq
    {
        get => currentMainMainSequence;
        set
        {
            currentMainMainSequence = value;
            ChangeThemeSeqAnim(value);

            switch ((States)value)
            {
                case States.Default:
                    break;
                
                
                case States.OnMainIntro:
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        CurrentMainSeq = (int)States.TeamSelection;
                    });
                    break;
                
                
                
                case States.TeamSelection:
                    _uiManager.PopInstructionUIFromScaleZero("두 팀으로 나누어 놀이를 진행해요!",narrationPath:"");
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        CurrentMainSeq = (int)States.AssignColorToTeam;
                    });
                    //2,3팀으로 추가 개발 진행될 경우
                    // _uiManager.PopInstructionUIFromScaleZero("먼저 팀을 선택해주세요");
                    // _uiManager.ShowTeamSelectionBtn();
                    break; 
                case States.AssignColorToTeam:
                    _uiManager.PopInstructionUIFromScaleZero("팀에서 한 친구가 나와서 터치해주세요",narrationPath:"");
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        
                        _uiManager.PopInstructionUIFromScaleZero("색깔을 고르기위해 돌림판을 눌러주세요!",narrationPath:"");
                        _uiManager.TurnOnWheel();
                    });
                    break;
                
                
                
                case States.GameA_HowToPlay:
                    break;
                case States.GameA_Playing:
                    break;
                case States.OnGameAFinish:
                    break; 
                
                
                case States.GameB_HowToPlay:
                    break;
                case States.GameB_Playing:
                    break;
                case States.OnGameBFinish:
                    break;
                
                
                case States.GameC_HowToPlay:
                    break;
                case States.GameC_Playing:
                    break;
                case States.OnGameCFinish:
                    break;
                
                
                
                case States.OnMainFinish:
                    break;
            }
        }
    }

#if UNITY_EDITOR
    [SerializeField] private States START_SEQ;
#else
    private States START_SEQ = States.OnMainIntro;
#endif
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        CurrentMainSeq = (int)START_SEQ;
    }

    private SS011_UIManager _uiManager;
    protected override void Init()
    {
        base.Init();
        _uiManager = UIManagerObj.GetComponent<SS011_UIManager>();
    }
}