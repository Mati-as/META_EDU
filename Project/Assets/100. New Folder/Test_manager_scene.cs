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

    //�ߺ�Ŭ������
    private bool _isClickable = true;

    // ���� ���� ------------------------------------------------------------------------

    protected override void Init()
    {
        //���⿡�� �ʿ��� ī�޶�, ������Ʈ, �̷� �͵��� ������ ������ ���ָ� ���� �� ����
        //�׸��� �� ������ find���� �ʰ� �ν�����â���� �����ϴ°� ��� �ͤ���

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
            //���� ��� �����ϸ� �ǳ�?
            //var selectedIndex = FindIndexByName(hit.transform.gameObject.name);

            var randomChar = (char)Random.Range('A', 'F' + 1);

            //���Ⱑ Ŭ�� ȿ���� ����ϴ� �κ��� �� ����
            Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/�⺻������/Sandwich/Click_" + randomChar,
                0.3f);

            return;
        }
    }


    protected override void OnStartButtonClicked()
    {
        base.OnStartButtonClicked();

        //ó���� �����ϴ� �κ��� �� ����
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



    ////ȿ���� ��� ��������� �����
    //private void ScaleDown()
    //{
    //    foreach (var obj in _selectableIngredientsOnSmallPlates)
    //        obj.DOScale(Vector3.zero, 1.0f)
    //            .OnStart(() =>
    //            {
    //                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/�⺻������/Sandwich/Sandwich_Ing_Popup");
    //            })
    //            .SetEase(Ease.InOutBounce);
    //}

    ////��ƼŬ ��� ��������� �����
    //private void PlayParticle(float delay)
    //{
    //    DOVirtual.Float(0, 0, delay, _ => { }).OnComplete(() =>
    //    {
    //        _finishMakingPs.Stop();
    //        _finishMakingPs.Play();
    //    });
    //}


}