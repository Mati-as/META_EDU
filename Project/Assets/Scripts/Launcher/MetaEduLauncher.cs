using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class MetaEduLauncher : UI_PopUp
{
	public enum Animation
	{
		On,
		Off,
		OnOff
	}

	public enum UIType
	{
		Home,
		Result,
		SelectMode,
		ContentA,
		ContentB,
		ContentC,
		Setting,
		Loading,
		//Login,
		//Survey
	}

	enum UIButtons
	{
		HomeButton,
		SelectModeButton,
		ContentAButton,
		ContentBButton,
		ContentCButton,
		SettingButton,
		ResultButton,
		//LoginButton,
		//SurveyButton
	}

	private int _UItab = -1;

	private readonly string UI_CLICK_SOUND_PATH;
	private GameObject[] _UIs;
	private Animation messageAnim;
	private List<string> _animClips = new List<string>();
	private float _clickableInterval = 0.5f;
	private bool _isClikcable = true;

	private bool _isLoadFinished;
    
	public static bool isBackButton { get; set; } // 뒤로가기의 경우, 씬로드 이후 게임선택화면이 나타나야합니다. 

	public Camera _uiCamera;
	
	private void Awake()
	{
		_raySynchronizer = GameObject.FindWithTag("RaySynchronizer").GetComponent<RaySynchronizer>();
		_uiCamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
		
;		LoadInitialScene.onInitialLoadComplete -= OnLoadFinished;
		LoadInitialScene.onInitialLoadComplete += OnLoadFinished;

		RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
		RaySynchronizer.OnGetInputFromUser += OnRaySynced;

		Destroy_prefab.onPrefabInput -= OnRaySyncByPrefab;
		Destroy_prefab.onPrefabInput += OnRaySyncByPrefab;
	}

	private void OnDestroy()
	{

		LoadInitialScene.onInitialLoadComplete -= OnLoadFinished;
		RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
		Destroy_prefab.onPrefabInput -= OnRaySyncByPrefab;

		Managers.Sound.Stop(SoundManager.Sound.Bgm);

	}

	private void OnLoadFinished()
	{
		InitLauncher();
		SetUIEssentials();
		_isLoadFinished = true;
	}

	public void InitLauncher()
	{

		Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/Bgm/Launcher", 0.05f);

		BindObject(typeof(UIType));
		BindButton(typeof(UIButtons));

		GetObject((int)UIType.Loading).gameObject.SetActive(false);
		GetObject((int)UIType.Home).gameObject.SetActive(false);
		GetObject((int)UIType.Result).gameObject.SetActive(false);
		GetObject((int)UIType.SelectMode).gameObject.SetActive(false);
		GetObject((int)UIType.ContentA).gameObject.SetActive(false);
		GetObject((int)UIType.ContentB).gameObject.SetActive(false);
		GetObject((int)UIType.ContentC).gameObject.SetActive(false);
		GetObject((int)UIType.Setting).gameObject.SetActive(false);
		GetObject((int)UIType.Setting).gameObject.SetActive(false);

		GetButton((int)UIButtons.HomeButton).gameObject.BindEvent(() => ShowTab(UIType.Home));
		GetButton((int)UIButtons.SelectModeButton).gameObject.BindEvent(() => ShowTab(UIType.SelectMode));
		GetButton((int)UIButtons.ContentAButton).gameObject.BindEvent(() => ShowTab(UIType.ContentA));
		GetButton((int)UIButtons.ContentBButton).gameObject.BindEvent(() => ShowTab(UIType.ContentB));
		GetButton((int)UIButtons.ContentCButton).gameObject.BindEvent(() => ShowTab(UIType.ContentC));
		GetButton((int)UIButtons.SettingButton).gameObject.BindEvent(() => ShowTab(UIType.Setting));
		GetButton((int)UIButtons.ResultButton).gameObject.BindEvent(() => ShowTab(UIType.Result));
		// GetButton((int)UIButtons.LoginButton).gameObject.BindEvent(() => ShowTab(UIType.Login));
		// GetButton((int)UIButtons.SurveyButton).gameObject.BindEvent(() => ShowTab(UIType.Survey));



#if UNITY_EDITOR
		Debug.Log("Launcher Init Completed");
#endif
		if (!isBackButton)
		{
			isBackButton = false;
			ShowTab(UIType.Home);
		}
		else ShowTab(UIType.SelectMode);
	}


	public void ShowTab(UIType tab)
	{

		if (!_isClikcable) return;
		_isClikcable = false;
		DOVirtual.Float(0, 0, _clickableInterval, _ => { })
			.OnComplete(() => { _isClikcable = true; });
		
		if ((UIType)_UItab == tab) return;

		_UItab = (int)tab;


		GetObject((int)UIType.Home).gameObject.SetActive(false);
		GetObject((int)UIType.Result).gameObject.SetActive(false);
		GetObject((int)UIType.SelectMode).gameObject.SetActive(false);
		GetObject((int)UIType.ContentA).gameObject.SetActive(false);
		GetObject((int)UIType.ContentB).gameObject.SetActive(false);
		GetObject((int)UIType.ContentC).gameObject.SetActive(false);
		GetObject((int)UIType.Setting).gameObject.SetActive(false);

		//GetObject((int)UIType.Login).gameObject.SetActive(false);
		//GetObject((int)UIType.Survey).gameObject.SetActive(false);

#if UNITY_EDITOR
		Debug.Log($"Current UI : {tab}");
#endif
		
		switch (tab)
		{
			case UIType.Home:
				Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
				GetObject((int)UIType.Home).gameObject.SetActive(true);
				GetObject((int)UIType.Home).GetComponent<ScrollRect>().ResetVertical();
				// GetButton((int)Buttons.AbilityButton).image.sprite = Managers.Resource.Load<Sprite>("Sprites/Main/Common/btn_18");
				// GetImage((int)Images.AbilityBox).sprite = Managers.Resource.Load<Sprite>("Sprites/Main/Common/btn_12");

				break;

			case UIType.SelectMode:
				Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
				GetObject((int)UIType.SelectMode).gameObject.SetActive(true);
				GetObject((int)UIType.SelectMode).GetComponent<ScrollRect>().ResetHorizontal();


				break;

			case UIType.ContentA:
				Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
				GetObject((int)UIType.ContentA).gameObject.SetActive(true);
				GetObject((int)UIType.ContentA).GetComponent<ScrollRect>().ResetHorizontal();
				break;

			case UIType.ContentB:
				Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
				GetObject((int)UIType.ContentB).gameObject.SetActive(true);
				GetObject((int)UIType.ContentB).GetComponent<ScrollRect>().ResetHorizontal();
				break;

			case UIType.ContentC:
				Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
				GetObject((int)UIType.ContentC).gameObject.SetActive(true);
				GetObject((int)UIType.ContentC).GetComponent<ScrollRect>().ResetHorizontal();
				break;

			case UIType.Setting:
				Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
				GetObject((int)UIType.Setting).gameObject.SetActive(true);
				GetObject((int)UIType.Setting).GetComponent<ScrollRect>().ResetHorizontal();
				break;

			case UIType.Result:
				Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
				GetObject((int)UIType.Result).gameObject.SetActive(true);
				GetObject((int)UIType.Result).GetComponent<ScrollRect>().ResetHorizontal();
				break;

			// case UIType.Login:
			// 	Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
			// 	GetObject((int)UIType.Login).gameObject.SetActive(true);
			// 	GetObject((int)UIType.Login).GetComponent<ScrollRect>().ResetHorizontal();
			// 	break;

			// case UIType.Survey:
			// 	Managers.Sound.Play(SoundManager.Sound.Effect, UI_CLICK_SOUND_PATH);
			// 	GetObject((int)UIType.Survey).gameObject.SetActive(true);
			// 	GetObject((int)UIType.Survey).GetComponent<ScrollRect>().ResetHorizontal();
			// 	break;
		}


	}




	private GraphicRaycaster _launcherGR;
	private PointerEventData _launcherPED;
	private GameObject _launcherCanvas;

	private void SetUIEssentials()
	{
		_launcherGR = _raySynchronizer.GR;
		_launcherPED = _raySynchronizer.PED;
	}

	private RaySynchronizer _raySynchronizer;
	private List<RaycastResult> _results;
	private Ray _ray;
	private Vector3 screenPosition;


	//Raysychronizer.cs와 동일한 로직사용. 
	public void OnRaySynced()
	{

		if (!_isLoadFinished) return;

		//마우스 및 포인터 위치를 기반으로 하고싶은경우.
		screenPosition = Mouse.current.position.ReadValue();
	//	_ray = Camera.main.ScreenPointToRay(screenPosition);
		_launcherPED.position = screenPosition;
#if UNITY_EDITOR
		Debug.Log($"클릭 시 PED Position : {_launcherPED.position}");
#endif
		_results = new List<RaycastResult>();
		_launcherGR.Raycast(_launcherPED, _results);

		if (_results.Count <= 0) return;
		ShowTabOrLoadScene(_results);
	}

	private List<RaycastResult> _resultsByPrefab;
	public Vector3 currentPrefabPosition { private get; set; }
	private Vector3 _screenPositionByPrefab;
	
	
	
	/// <summary>
	/// 하드웨어(빔 프로젝터) 상에서 프리팹으로 클릭하는 로직을 위한 OnRaySync 커스텀 이벤트 함수입니다.
	/// 씬변경 후 일반 게임로직에서는 동작하지 않습닌다
	/// </summary>
	private void OnRaySyncByPrefab()
	{

		 //_screenPositionByPrefab = _uiCamera.WorldToScreenPoint(currentPrefabPosition);
		// _ray = _uiCamera.ScreenPointToRay(_screenPositionByPrefab);
		
		_launcherPED.position = currentPrefabPosition;
		_resultsByPrefab = new List<RaycastResult>();
		_launcherGR.Raycast(_launcherPED, _resultsByPrefab);

#if UNITY_EDITOR
		Debug.Log($"프리팹 시 PED Position 변환 전: {currentPrefabPosition}");
#endif

#if UNITY_EDITOR
		Debug.Log($"프리팹 시 PED Position 변환 후 : {_launcherPED.position}");
#endif
		
		if (_resultsByPrefab != null)
		{
			ShowTabOrLoadScene(_resultsByPrefab);
		
		}
		
		
		else
		{
#if UNITY_EDITOR
			Debug.LogError("result is null");
#endif
		}
	}

	private void OnDrawGizmos()
	{
		
		Gizmos.color = Color.red;
        
		float radius = 10f;
		Gizmos.DrawSphere(currentPrefabPosition, radius);
	}


	public void ShowTabOrLoadScene(List<RaycastResult> results)
	{
		if(!_isClikcable) return;
		DOVirtual.Float(0, 0, 0.1f, _ => { })
			.OnComplete(() =>
			{
				UIType clickedUI = 0;
#if UNITY_EDITOR
				Debug.Log("LAUNCHER RAY");
#endif
				foreach (var result in results)
				{
#if UNITY_EDITOR
					Debug.Log($" result Name:{result.gameObject.name}");
#endif
					// 설정,홈,컨텐츠 **버튼** ---------------------------------------------------------
					if (Enum.TryParse(SetButtonString(result.gameObject.name), out clickedUI)) ShowTab(clickedUI);

					// ** 씬 로드** ---------------------------------------------------------
					if (result.gameObject.name.Contains("SceneName_")) LoadScene(result.gameObject.name);
				}
			});
	}


private void LoadScene(string sceneName)
    {  
	   

	    string originalName = sceneName;
	    string modifiedName = originalName.Substring("SceneName_".Length);
	    
	    gameObject.SetActive(false);
	    SceneManager.LoadScene(modifiedName);
    }

    private string SetButtonString(string input)
    {
	    if (!input.Contains("Button")) return null;

	    string originalName = input;
	    string modifiedName = originalName.Substring(0, originalName.Length - 6);
	    return modifiedName;
    }
    
 


    IEnumerator Active_false()
    {
	    yield return new WaitForSeconds(1f);
	    gameObject.SetActive(false);
    }


}
