using System;
using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.WSA;


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
	    None,
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

	private UIType _UItab = UIType.None;

    private readonly string UI_CLICK_SOUND_PATH;
    private GameObject[] _UIs; 
    private Animation messageAnim;
    private List<string> _animClips = new List<string>();

    private void Awake()
    {
	    _raySynchronizer = GameObject.Find("RaySynchronizer").GetComponent<RaySynchronizer>();
	    LoadInitialScene.onInitialLoadComplete -= InitLauncher;
	    LoadInitialScene.onInitialLoadComplete += InitLauncher;
	    
	    RaySynchronizer.OnGetInputFromUser -= OnRaySynced;
	    RaySynchronizer.OnGetInputFromUser += OnRaySynced;
    }
    public void InitLauncher()
	{
		
		
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
		ShowTab(UIType.Home);
	}
    

    public void ShowTab(UIType tab)
	{
		if (_UItab == tab)
			return;

		_UItab = tab;
        
		
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
		
		ShowTab(UIType.Home);
	}

    private RaySynchronizer _raySynchronizer;

    public void OnRaySynced()
    {
#if UNITY_EDITOR
	    Debug.Log($"RAY SYNCED { _raySynchronizer.raycastResults.Count}");
#endif
	
	    UIType clickedUI = 0;


		    for (var i = 0; i < _raySynchronizer.raycastResults.Count; i++)
		    {
			    string originalName = _raySynchronizer.raycastResults[i].gameObject.name;
			    string modifiedName = originalName.Substring(0, originalName.Length - 6);
#if UNITY_EDITOR
			    Debug.Log($"modifiedName: {modifiedName}");
#endif
			    
			    if (Enum.TryParse(modifiedName, out clickedUI))
			    {
#if UNITY_EDITOR
				    Debug.Log($"Current UI : {clickedUI}");
#endif
				    ShowTab(clickedUI);
			    }

		    }
	    
		   
    }





    IEnumerator Active_false()
    {
	    yield return new WaitForSeconds(1f);
	    gameObject.SetActive(false);
    }


}
