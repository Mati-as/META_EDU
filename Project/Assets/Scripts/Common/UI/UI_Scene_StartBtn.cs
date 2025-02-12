using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Scene_StartBtn : MonoBehaviour
{
    private Message_anim_controller _animController;
    private Button _btn;
    private Image _btnImage;
    private TMP_Text _tmp_Start;
    private TMP_Text _tmp_Time;
    
    
    private int _remainTime;
    private bool _isClickable;


    /*
     * 1.onBtnClicked와 Message_anim_controller의 onIntroUIOff 이벤트는 같은 기능을 수행
     * 2.사용자가 버튼을 클릭하여 event를 수행하는지, 혹은 10초(대기시간 제한)이 지나서 이벤트가 수행되는지에 대한 구분만 되어있음.
     * 3.중복실행 방지하기위해 bool연산자 사용.
     */
    public static event Action onBtnShut;

    private void Awake()
    {
        _animController = FindActiveMessageAnimController();
    }

    private void Start()
    {
        _btn = GetComponent<Button>();
        _btnImage = GetComponent<Image>();
        _btn.onClick.AddListener(OnClicked);
        _tmp_Start = GetComponentInChildren<TMP_Text>();
        _tmp_Time = transform.GetChild(1).GetComponent<TMP_Text>();

        Message_anim_controller.onIntroUIOff -= OnAnimOff;
        Message_anim_controller.onIntroUIOff += OnAnimOff;
        onBtnShut -= OnButtonShut;
        onBtnShut += OnButtonShut;

        _btnImage.DOFade(0, 0.01f);
        _tmp_Start.DOFade(0, 0.01f);
        _tmp_Time.DOFade(0, 0.01f);

        _btnImage
            .DOFade(1, 0.5f)
            .SetDelay(3f);

        _tmp_Start.DOFade(1, 0.5f)
            .OnStart(() => { _isClickable = true; })
            .SetDelay(3f);
        
        _tmp_Start.DOFade(1, 0.5f)
            .OnStart(() => { _isClickable = true; })
            .SetDelay(3f);
        
        _tmp_Time.DOFade(1, 0.5f)
            .OnStart(() => { _isClickable = true; })
            .SetDelay(3f);
    }

    private Message_anim_controller FindActiveMessageAnimController()
    {
        foreach (Transform child in transform.parent)
            if (child.gameObject.activeInHierarchy)
            {
                var controller = child.GetComponent<Message_anim_controller>();
                if (controller != null) return controller;
            }

        return null;
    }

    private float _elapsedTime;
    private readonly float _offset = 0.66f; //시간 증가량 컨트롤
    private bool _isInvoked;
    private readonly float _autoShutTime = 12f;


    private void Update()
    {
        _elapsedTime += Time.deltaTime * _offset;
        _remainTime = (int)(_autoShutTime - _elapsedTime);
       // _tmp_Start.text = $"시작하기\n({_remainTime}초)";
        _tmp_Time.text = $"{_remainTime}초";
        if (_remainTime <= 0 && !_isInvoked)
        {
            _isInvoked = true;
            onBtnShut?.Invoke();
        }
    }


    private bool _isBtnEventInvoked;

    private void OnClicked()
    {
        if (!_isClickable) return;

        if (_animController != null)
        {
            _animController.Animation_Off();
            if (!_isBtnEventInvoked)
            {
                _isBtnEventInvoked = true;

                onBtnShut?.Invoke();
                FadeOutBtn();
            }
        }
        else
        {
            onBtnShut?.Invoke();
            FadeOutBtn();
#if UNITY_EDITOR
            Debug.Log("AnimationController is null");
#endif
        }
    }

    private void OnAnimOff()
    {
        if (!_isBtnEventInvoked)
        {
            _isBtnEventInvoked = true;

            onBtnShut?.Invoke();
            FadeOutBtn();
        }
    }

    private void OnButtonShut()
    {
        Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/Common/UI_Message_Button", 0.3f);

        _isBtnEventInvoked = true;
        _isClickable = false;

        _btnImage.DOFade(0, 0.5f);
        _tmp_Time.DOFade(0, 0.5f);
        _tmp_Start.DOFade(0, 0.5f);
    }

    private void FadeOutBtn()
    {
        _btnImage.DOFade(0, 0.5f)
            .OnComplete(() => { gameObject.SetActive(false); });
    }
}