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
        
        ,Toggle_IsSensorFilterMode
        ,FPSCounter
    }

    private enum TMP
    {
        // TMP_NormalRay,
        // TMP_RealRay,
        TMP_Log
    }
    private Animator _animator;
    private bool isUIOpen =false;
    private bool _isAllPrefabImageActive = false;
    private readonly int _isOpen = Animator.StringToHash("isOn");
    
    private bool _clickable =true;
    private Toggle _toggleIsSensorFilterMode;

    private Sequence _sensorClickSeq;
    // 통일된 스타일 적용용 함수
    void UpdateButtonVisual(Btn btn, bool isActive)
    {
        var buttonGO = GetObject((int)btn);
        var img = buttonGO.GetComponent<Image>();
        img.color = isActive ? Color.white : new Color(0.3f, 0.3f, 0.3f); // 어두운 회색
    }

    private void SetClickable()
    {
        _sensorClickSeq?.Kill(); _sensorClickSeq = DOTween.Sequence();
            
        _sensorClickSeq.AppendInterval(0.5f);
        _sensorClickSeq.AppendCallback(() => _clickable = true);
    }
    public override bool InitOnLoad()
    {
        
        _animator = GetComponent<Animator>();
        BindObject(typeof(Btn));
        BindTMP(typeof(TMP));
        GetObject((int)Btn.FPSCounter).SetActive(false);
    
        
        _toggleIsSensorFilterMode = GetObject((int)Btn.Toggle_IsSensorFilterMode).gameObject.GetComponent<Toggle>();


        _toggleIsSensorFilterMode.isOn = SensorManager.isSensorSensitivityFilterModeOn;
        _toggleIsSensorFilterMode.onValueChanged.AddListener((isOn) =>
        {
            
            if (!_clickable) return;
            _clickable = false;

            SetClickable();
            
            SensorManager.isSensorSensitivityFilterModeOn = isOn;
            Logger.Log($"센서 필터 모드 : {isOn}");
        });
        
        GetObject((int)Btn.Btn_Open).gameObject.BindEvent(() =>
        {
            if (!_clickable) return;
            _clickable = false;

            SetClickable();
         
            isUIOpen = !isUIOpen;
            
      
            _animator.SetInteger(_isOpen,isUIOpen ? 1:0);
            
            
            Logger.CoreClassLog($"isUIOpen? : {isUIOpen}");
            GetObject((int)Btn.FPSCounter).SetActive(isUIOpen);
           
        });
        
        // 이미지 전체 끄는 버튼
        GetObject((int)Btn.Btn_Image).gameObject.BindEvent(() =>
        {
            if (!_clickable) return;
           SetClickable();
            
            _isAllPrefabImageActive = !_isAllPrefabImageActive;
            Logger.Log("이미지 전체 끄기");
            SensorManager.isRealImageActive = _isAllPrefabImageActive;
            SensorManager.isNormalImageActive = _isAllPrefabImageActive;
            SensorManager.isTouchZoneImageActive = _isAllPrefabImageActive;
            
            

            UpdateButtonVisual(Btn.Btn_Normal, SensorManager.isNormalImageActive);
            UpdateButtonVisual(Btn.Btn_Real, SensorManager.isRealImageActive);
            UpdateButtonVisual(Btn.Btn_TouchZone, SensorManager.isTouchZoneImageActive);
        });

        // 각각의 버튼에 이벤트 및 비주얼 업데이트 추가
        GetObject((int)Btn.Btn_Normal).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
           SetClickable();
            
            SensorManager.isNormalImageActive = !SensorManager.isNormalImageActive;
            UpdateButtonVisual(Btn.Btn_Normal, SensorManager.isNormalImageActive);
        });

        GetObject((int)Btn.Btn_Real).gameObject.BindEvent(() =>
        { 
             if (!_clickable) return;
           SetClickable();
            
            SensorManager.isRealImageActive = !SensorManager.isRealImageActive;
            UpdateButtonVisual(Btn.Btn_Real, SensorManager.isRealImageActive);
        });

        GetObject((int)Btn.Btn_TouchZone).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
           SetClickable();
            
            SensorManager.isTouchZoneImageActive = !SensorManager.isTouchZoneImageActive;
            UpdateButtonVisual(Btn.Btn_TouchZone, SensorManager.isTouchZoneImageActive);
        });

        
        
        GetObject((int)Btn.Btn_RealRay).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
           SetClickable();
            
            SensorManager.isRealRayActive = !SensorManager.isRealRayActive;
            UpdateButtonVisual(Btn.Btn_RealRay, SensorManager.isRealRayActive);
        });

        GetObject((int)Btn.Btn_NormalRay).gameObject.BindEvent(() =>
        {
             if (!_clickable) return;
           SetClickable();
            
            SensorManager.isNormalRayActive = !SensorManager.isNormalRayActive;
            UpdateButtonVisual(Btn.Btn_NormalRay, SensorManager.isNormalRayActive);
        });

        
        
        
        
        
        // 👉 초기 상태 반영

        _isAllPrefabImageActive = false;       
        SensorManager.isRealImageActive = _isAllPrefabImageActive;
        SensorManager.isNormalImageActive = _isAllPrefabImageActive;
        SensorManager.isTouchZoneImageActive = _isAllPrefabImageActive;
 
        UpdateButtonVisual(Btn.Btn_Normal, SensorManager.isNormalImageActive);
        UpdateButtonVisual(Btn.Btn_Real, SensorManager.isRealImageActive);
        UpdateButtonVisual(Btn.Btn_TouchZone, SensorManager.isTouchZoneImageActive);
        
        UpdateButtonVisual(Btn.Btn_Normal, SensorManager.isNormalImageActive);
        UpdateButtonVisual(Btn.Btn_Real, SensorManager.isRealImageActive);
        UpdateButtonVisual(Btn.Btn_TouchZone, SensorManager.isTouchZoneImageActive);
        UpdateButtonVisual(Btn.Btn_RealRay, SensorManager.isRealRayActive);
        UpdateButtonVisual(Btn.Btn_NormalRay, SensorManager.isNormalRayActive);
        
        Logger.Log($"센서 메뉴 초기화 완료 NormalRay:{SensorManager.isNormalRayActive} : RealRay:{SensorManager.isRealRayActive}");

        RefreshSensorParameterText();
      
        
        return true;
    }

    private void RefreshSensorParameterText()
    {

        var logText = $"<b>[XML 설정값]</b>\n" +
                      $"ScreenSize:{XmlManager.Instance.ScreenSize}, Ratio:{XmlManager.Instance.ScreenRatio}, TouchRange:{XmlManager.Instance.TouchRange}\n" +
                      $"SensorPos({XmlManager.Instance.SensorPosX},{XmlManager.Instance.SensorPosY}), " +
                      $"Offset({XmlManager.Instance.ScreenPositionOffsetX},{XmlManager.Instance.ScreenPositionOffsetY}), " +
                      $"SensorOffset({XmlManager.Instance.SensorOffsetX},{XmlManager.Instance.SensorOffsetY})\n" +
                      $"Threshold:{XmlManager.Instance.ClusterThreshold}, MaxZones:{XmlManager.Instance.MaxTouchzones}, " +
                      $"Lifetime:{XmlManager.Instance.TouchzoneLifetime}";

        GetTMP((int)TMP.TMP_Log).text = logText;
        

    }
}