using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_MetaEduLauncherMaster : UI_Scene
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
    private enum UI
    {
        Btn_Home,
        Btn_Setting,
        Btn_Quit,
        Btn_Back
    }

    private readonly bool _isClikcable = true;

    public override bool InitEssentialUI()
    {
        BindObject(typeof(UI));

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
            Managers.isGameStopped = false;
        });
        GetObject((int)UI.Btn_Home).BindEvent(()=>{
            Managers.UI.CloseAllPopupUI();
            Managers.UI.ShowPopupUI<UI_Home>();
        });
        GetObject((int)UI.Btn_Setting).BindEvent(()=>
        {
            Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
        });
        GetObject((int)UI.Btn_Quit).BindEvent(()=>{
            Application.Quit();
        });
        GetObject((int)UI.Btn_Back).BindEvent(()=>
        {

            Managers.UI.ClosePopupUI();

            switch (Managers.UI.currentPopupClass)
            {
                case UI_ContentSortedByArea:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_Home>();
                    break;
                case UI_ContentSortedByTheme:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_Home>();
                    break;
                case UI_SensorSettingMain:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_Home>();
                    break;

                

                case UI_PA_ContentSelection:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_ContentSortedByArea>();
                    break;
                case UI_Art_ContentSelection:

                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_ContentSortedByArea>();
                    break;
                case UI_Communication_ContentSelection:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_ContentSortedByArea>();
                    break;
                case UI_Science_ContentSelection:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_ContentSortedByArea>();
                    break;
                case UI_Social_ContentSelection:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_ContentSortedByArea>();
                    break;


                case UI_SensorSetting:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
                    break;
                case UI_ScreenSetting:
                    Managers.UI.ClosePopupUI();
                    Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
                    break;
            }


        });
        
        GetObject((int)UI.Btn_Home).SetActive(false);
        GetObject((int)UI.Btn_Setting).SetActive(false);
        GetObject((int)UI.Btn_Quit).SetActive(false);
        GetObject((int)UI.Btn_Back).SetActive(false);
        
       
        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
        RaySynchronizer.OnGetInputFromUser += OnRaySynced;
        return base.InitEssentialUI();
    }


  
    private void OnDestroy()
    {
        RaySynchronizer.OnGetInputFromUser -= OnRaySynced;


        Managers.Sound.Stop(SoundManager.Sound.Bgm);
    }

    private void SetUIEssentials()
    {
        _launcherGR = GameObject.FindWithTag("UIManager").GetComponent<GraphicRaycaster>();
        _launcherPED =  new PointerEventData(EventSystem.current);
    }

    public void OnRaySynced()
    {
       // Logger.CoreClassLog("Raysync On Launcher -----------------");
        //마우스 및 포인터 위치를 기반으로 하고싶은경우.
        screenPosition = Mouse.current.position.ReadValue();
        if (_launcherPED != null) _launcherPED.position = screenPosition;
        else Logger.CoreClassLog("PointerEventData is null -----------------");

        _results = new List<RaycastResult>();
        if (_launcherGR != null) _launcherGR.Raycast(_launcherPED, _results);
        else Logger.CoreClassLog("laucnherGR is null -----------------");

        if (_results.Count <= 0) return;
        {
           // Logger.CoreClassLog("Raysync On Scene Selection -----------------");
            ShowTabOrLoadScene(_results);
        }
    }

    private string SetButtonString(string input)
    {
        if (!input.Contains("Button")) return null;

        string originalName = input;
        string modifiedName = originalName.Substring(0, originalName.Length - 6);
        return modifiedName;
    }


    public void ShowTabOrLoadScene(List<RaycastResult> results)
    {
        DOVirtual.Float(0, 0, 0.1f, _ =>
            {
            })
            .OnComplete(() =>
            {
                MetaEduLauncher.UIType clickedUI = 0;

                foreach (var result in results)
                    // 설정,홈,컨텐츠 **버튼** ---------------------------------------------------------
                    if (result.gameObject.name.Contains("SceneName"))
                    {
                        GameNameWaitingForConfirmation = result.gameObject.name;
                        GameKoreanName = result.gameObject.GetComponentInChildren<Text>().text;
                        // ** 씬 로드** ---------------------------------------------------------
                        Logger.Log($"{result.gameObject.name}게임 씬 버튼 클릭 됨--------------------------------------");
                 
                            Managers.UI.ShowPopupUI<UI_Confirmation>();
                            return;
                        


                        return;
                    }
            });
    }
    
    public override void OnPopupUI()
    {
        //첫실행에서는 리턴 
        if(GetObject((int)UI.Btn_Home) ==null)return;
        RefreshUI();
    }

    private void RefreshUI()
    {
     if(Managers.UI.FindPopup<UI_Home>())
     {
         GetObject((int)UI.Btn_Home).SetActive(false);
         GetObject((int)UI.Btn_Setting).SetActive(false);
         GetObject((int)UI.Btn_Quit).SetActive(false);
         GetObject((int)UI.Btn_Back).SetActive(false);
         
     }
     else
     {
         GetObject((int)UI.Btn_Home).SetActive(true);
         GetObject((int)UI.Btn_Setting).SetActive(true);
         GetObject((int)UI.Btn_Quit).SetActive(true);
         GetObject((int)UI.Btn_Back).SetActive(true);
     }
    }
    
    
}