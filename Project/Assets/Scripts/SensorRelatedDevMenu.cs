using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.UI;

public class SensorRelatedDevMenu : UI_PopUp
{
    private enum Btn
    {
        Btn_Open,
        Btn_Image,
        Btn_Normal,
        Btn_Real,
        Btn_TouchZone,
        Btn_RealRay,
        Btn_NormalRay
    }

    private enum TMP
    {
        TMP_NormalRay,
        TMP_RealRay
    }

    private DevelopmentUIManager _devUIManager;

    private Animator _animator;
    private bool isOpen =false;
    private readonly int _isOpen = Animator.StringToHash("isOn");

    private bool _clickable =true;
    // 통일된 스타일 적용용 함수
    void UpdateButtonVisual(Btn btn, bool isActive)
    {
        var buttonGO = GetObject((int)btn);
        var img = buttonGO.GetComponent<Image>();
        img.color = isActive ? Color.white : new Color(0.3f, 0.3f, 0.3f); // 어두운 회색
    }
    public override bool Init()
    {
        _devUIManager = GameObject.FindWithTag("LidarMenu").GetComponent<DevelopmentUIManager>();
        _animator = GetComponent<Animator>();
        BindObject(typeof(Btn));



        GetObject((int)Btn.Btn_Open).gameObject.BindEvent(() =>
        {
            if (!_clickable) return;
            _clickable = false; DOVirtual.DelayedCall(0.5f, () => _clickable = true);
            
            
            isOpen = !isOpen;
            _animator.SetInteger(_isOpen,isOpen ? 1:0);
           
        });
        
        // 이미지 전체 끄는 버튼
        GetObject((int)Btn.Btn_Image).gameObject.BindEvent(() =>
        {
            if (!_clickable) return;
            _clickable = false; DOVirtual.DelayedCall(0.5f, () => _clickable = true);
            
            Logger.Log("이미지 전체 끄기");
            SensorManager.isRealImageActive = false;
            SensorManager.isNormalImageActive = false;
            SensorManager.isTouchZoneImageActive = false;

            UpdateButtonVisual(Btn.Btn_Normal, SensorManager.isNormalImageActive);
            UpdateButtonVisual(Btn.Btn_Real, SensorManager.isRealImageActive);
            UpdateButtonVisual(Btn.Btn_TouchZone, SensorManager.isTouchZoneImageActive);
        });

        // 각각의 버튼에 이벤트 및 비주얼 업데이트 추가
        GetObject((int)Btn.Btn_Normal).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
            _clickable = false; DOVirtual.DelayedCall(0.5f, () => _clickable = true);
            
            SensorManager.isNormalImageActive = !SensorManager.isNormalImageActive;
            UpdateButtonVisual(Btn.Btn_Normal, SensorManager.isNormalImageActive);
        });

        GetObject((int)Btn.Btn_Real).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
            _clickable = false; DOVirtual.DelayedCall(0.5f, () => _clickable = true);
            
            SensorManager.isRealImageActive = !SensorManager.isRealImageActive;
            UpdateButtonVisual(Btn.Btn_Real, SensorManager.isRealImageActive);
        });

        GetObject((int)Btn.Btn_TouchZone).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
            _clickable = false; DOVirtual.DelayedCall(0.5f, () => _clickable = true);
            
            SensorManager.isTouchZoneImageActive = !SensorManager.isTouchZoneImageActive;
            UpdateButtonVisual(Btn.Btn_TouchZone, SensorManager.isTouchZoneImageActive);
        });

        
        
        GetObject((int)Btn.Btn_RealRay).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
            _clickable = false; DOVirtual.DelayedCall(0.5f, () => _clickable = true);
            
            SensorManager.isRealRayActive = !SensorManager.isRealRayActive;
            UpdateButtonVisual(Btn.Btn_RealRay, SensorManager.isRealRayActive);
        });

        GetObject((int)Btn.Btn_NormalRay).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
            _clickable = false; DOVirtual.DelayedCall(0.5f, () => _clickable = true);
            
            SensorManager.isNormalRayActive = !SensorManager.isNormalRayActive;
            UpdateButtonVisual(Btn.Btn_NormalRay, SensorManager.isNormalRayActive);
        });

        // 👉 초기 상태 반영
        UpdateButtonVisual(Btn.Btn_Normal, SensorManager.isNormalImageActive);
        UpdateButtonVisual(Btn.Btn_Real, SensorManager.isRealImageActive);
        UpdateButtonVisual(Btn.Btn_TouchZone, SensorManager.isTouchZoneImageActive);
        UpdateButtonVisual(Btn.Btn_RealRay, SensorManager.isRealRayActive);
        UpdateButtonVisual(Btn.Btn_NormalRay, SensorManager.isNormalRayActive);
        
        Logger.Log($"센서 메뉴 초기화 완료 NormalRay:{SensorManager.isNormalRayActive} : RealRay:{SensorManager.isRealRayActive}");

        return true;
    }

    private void DelayClickable()
    {
        
    }
}