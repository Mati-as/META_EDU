using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
///     ui_Scene 으로, 항시 켜져있는 UI 표출중
/// </summary>
public class UI_Master : UI_Scene
{
    private static bool _isLoadFinished;
    private Vector3 screenPosition;
    private PointerEventData _launcherPED;
    private List<RaycastResult> _results;
    private GraphicRaycaster _launcherGR;
    private RaySynchronizer _raySynchronizer;


    public static string GameNameWaitingForConfirmation
    {
        get;
        private set;
    }

    public static string GameKoreanName
    {
        get;
        private set;
    }

    public enum UI
    {
        Btn_Home,

        //Btn_Setting,
        Btn_Quit,
        Btn_Back,
        UI_LoadInitialScene
    }
    
    public UI_LoadInitialScene UILoadInitialScene;

    private readonly bool _isClikcable = true;
   

    
    public override bool InitOnLoad()
    {
        BindObject(typeof(UI));

        UILoadInitialScene = GetObject((int)UI.UI_LoadInitialScene).GetComponent<UI_LoadInitialScene>();
        
        _raySynchronizer = GameObject.FindWithTag("RaySynchronizer").GetComponent<RaySynchronizer>();
        SetUIEssentials();

        // 널방지를 위한 딜레이 입니다.
        DOVirtual.Float(0, 0, 1f, _ =>
        {
        }).OnComplete(() =>
        {
            _isLoadFinished = true;
        });

        DOVirtual.DelayedCall(3.5f, () =>
        {
            Managers.IsGameStopped = false;
        });
        GetObject((int)UI.Btn_Home).BindEvent(() =>
        {
            Managers.UI.CloseAllPopupUI();
            Managers.UI.ShowPopupUI<UI_Home>();
        });
        // GetObject((int)UI.Btn_Setting).BindEvent(()=>
        // {
        //     Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
        // });
        GetObject((int)UI.Btn_Quit).BindEvent(() =>
        {
            Application.Quit();
        });
        GetObject((int)UI.Btn_Back).BindEvent(() =>
        {
            Logger.ContentTestLog($"{Managers.UI.GetUICounts()}");
        
        
            if (!(Managers.UI.currentPopupClass is UI_Home))
            {
                Managers.UI.ClosePopupUI();
        
                if (Managers.UI.GetUICounts() <= 0)
                {
                    Logger.Log("DEFAULT_UI갯수보다 미만..  홈 UI 활성화");
                    Managers.UI.ShowPopupUI<UI_Home>();
                }
            }
        });


        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
        RaySynchronizer.OnGetInputFromUser += OnRaySynced;
        return base.InitOnLoad();
    }


    private void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;


        Managers.Sound.Stop(SoundManager.Sound.Bgm);
    }

    private void SetUIEssentials()
    {
        _launcherGR = GameObject.FindWithTag("UIManager").GetComponent<GraphicRaycaster>();
        _launcherPED = new PointerEventData(EventSystem.current);
    }

    public void OnRaySynced()
    {
        // Logger.CoreClassLog("Raysync On Launcher -----------------");
        //마우스 및 포인터 위치를 기반으로 하고싶은경우.
        screenPosition = Mouse.current.position.ReadValue();
        if (_launcherPED != null) _launcherPED.position = screenPosition;
        //    else Logger.CoreClassLog("PointerEventData is null -----------------");

        _results = new List<RaycastResult>();
        if (_launcherGR != null) _launcherGR.Raycast(_launcherPED, _results);
        //  else Logger.CoreClassLog("laucnherGR is null -----------------");

        if (_results.Count <= 0) return;
        {
            // Logger.CoreClassLog("Raysync On Scene Selection -----------------");
            //OnSceneBtnClicked(_results);
        }
    }

    private string SetButtonString(string input)
    {
        if (!input.Contains("Button")) return null;

        string originalName = input;
        string modifiedName = originalName.Substring(0, originalName.Length - 6);
        return modifiedName;
    }


    public void OnSceneBtnClicked(string sceneId, string title)
    {
        DOVirtual.Float(0, 0, 0.1f, _ =>
            {
            })
            .OnComplete(() =>
            {
                MetaEduLauncher.UIType clickedUI = 0;


                GameNameWaitingForConfirmation = sceneId;
                GameKoreanName = title;
                // ** 씬 로드** ---------------------------------------------------------
                //    Logger.Log($"{result.gameObject.name}게임 씬 버튼 클릭 됨--------------------------------------");

                Managers.UI.ShowPopupUI<UI_Confirmation>();
            });
    }

    public override void OnPopupUI()
    {
        //첫실행에서는 리턴 
        if (Utils.IsLauncherScene()) UIOff();

        if (GetObject((int)UI.Btn_Home) == null) return;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (Managers.UI.FindPopup<UI_Home>())
        {
            SetBtnStatus(UI.Btn_Home, false);
            SetBtnStatus(UI.Btn_Quit, true);
            SetBtnStatus(UI.Btn_Back, false);
            // SetBtnStatus(UI.Btn_Setting, true);
            Logger.CoreClassLog("UI_MetaEduLauncherMaster RefreshUI() - Home UI 활성화");
        }
        else
        {
            SetBtnStatus(UI.Btn_Home, true);
            SetBtnStatus(UI.Btn_Quit, true);
            SetBtnStatus(UI.Btn_Back, Managers.UI.currentPopupClass.IsBackBtnClickable);
    
            // SetBtnStatus(UI.Btn_Setting, true);
        }
    }

    public void UIOff()
    {
        SetBtnStatus(UI.Btn_Home, false);
        SetBtnStatus(UI.Btn_Quit, false);
        SetBtnStatus(UI.Btn_Back, false);
        // SetBtnStatus(UI.Btn_Setting, false);
    }

    public void SetBtnStatus(UI btn, bool isActive)
    {
        GameObject targetBtnObj = GetObject((int)btn);
        if (targetBtnObj == null)
        {
            Logger.LogError($"SetBtnStatus: {btn} 오브젝트가 존재하지 않습니다.");
            return;
        }

        targetBtnObj.SetActive(isActive);
    }
}