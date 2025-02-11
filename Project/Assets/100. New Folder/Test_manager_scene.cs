using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class Test_manager_scene : Base_GameManager
{
   
    private static GameObject s_UIManager;

    private ParticleSystem _finishMakingPs;
   
    public static bool isGameStart { get; private set; }

    //중복클릭방지
    private bool _isClickable = true;

    // 기존 내용 ------------------------------------------------------------------------

    protected override void Init()
    {
        //여기에서 필요한 카메라, 오브젝트, 이런 것들을 사전에 저장을 해주면 좋을 것 같음
        //그리고 난 무조건 find하지 않고 인스펙터창에서 저장하는게 어떨까 싶ㅇ므

        _finishMakingPs = GameObject.Find("CFX_FinishMaking").GetComponent<ParticleSystem>();

        base.Init();
    }


    private void Start()
    {
        Debug.Assert(isInitialized);
        //InitUI();
        //StackCamera();
    }

    protected override void BindEvent()
    {
        base.BindEvent();

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

    }

    private bool _isRoundFinished;
    private readonly int NO_VALID_OBJECT = -1;
    private RaycastHit[] _raycastHits;

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        _raycastHits = Physics.RaycastAll(GameManager_Ray);

        if (_isRoundFinished) return;
        if (!isGameStart) return;


        if (!_isClickable)
        {
            return;
        }

        foreach (var hit in _raycastHits)
        {
            //여긴 어떻게 관리하면 되나?
            //var selectedIndex = FindIndexByName(hit.transform.gameObject.name);

            var randomChar = (char)Random.Range('A', 'F' + 1);

            //여기가 클릭 효과음 재생하는 부분인 것 같고
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Click_" + randomChar,
                0.3f);

            return;
        }
    }


    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();

        //처음에 시작하는 부분인 것 같고
    }


    // methods ------------------------------------------------------------------------

    //private void SetClickableAfterDelay(float delay)
    //{
    //    DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() => { _isClickable = true; });
    //}

  
    private void StackCamera()
    {
        var uiCamera = s_UIManager.GetComponentInChildren<Camera>();

        if (Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out var mainCameraData))
            mainCameraData.cameraStack.Add(uiCamera);
        else
            Debug.LogError("Main camera does not have UniversalAdditionalCameraData component.");
    }



    ////효과음 재생 참고용으로 살려둠
    //private void ScaleDown()
    //{
    //    foreach (var obj in _selectableIngredientsOnSmallPlates)
    //        obj.DOScale(Vector3.zero, 1.0f)
    //            .OnStart(() =>
    //            {
    //                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/Sandwich/Sandwich_Ing_Popup");
    //            })
    //            .SetEase(Ease.InOutBounce);
    //}

    ////파티클 재생 참고용으로 살려둠
    //private void PlayParticle(float delay)
    //{
    //    DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
    //    {
    //        _finishMakingPs.Stop();
    //        _finishMakingPs.Play();
    //    });
    //}


}