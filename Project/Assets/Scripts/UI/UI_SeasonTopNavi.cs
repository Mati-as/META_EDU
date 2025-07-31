using System;
using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using Unity.VisualScripting;
using UnityEngine;

public class UI_SeasonTopNavi : UI_PopUp
{

    private enum Btns
    {
        BtnA, // 3,6,9,12
        BtnB, // 1,4,7,10
        BtnC, // 2,5,8,11
    }

    private enum Texts
    {
        TextA,
        TextB,
        TextC,
    }
    public override bool InitOnLoad()
    {
        BindButton(typeof(Btns));
BindText(typeof(Texts));

        
        if (Enum.TryParse(Managers.UI.Master.currentSeason, out UI_SeasonSelection.UI season))
        {
            switch (season)
            {
                case UI_SeasonSelection.UI.Spring:
                    
                    GetText((int)Texts.TextA).text = "3월";
                    GetText((int)Texts.TextB).text = "4월";
                    GetText((int)Texts.TextC).text = "5월";
                    
                    GetButton((int)Btns.BtnA).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Mar>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnB).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Apr>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnC).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_May>(path:"UI_Month");
                    });
                    // 봄 처리
                    break;
                case UI_SeasonSelection.UI.Summer:
                    
                    GetText((int)Texts.TextA).text = "6월";
                    GetText((int)Texts.TextB).text = "7월";
                    GetText((int)Texts.TextC).text = "8월";

                    GetButton((int)Btns.BtnA).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_June>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnB).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_July>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnC).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Aug>(path:"UI_Month");
                    });
                    break;
                    // 여름 처리
                    
                case UI_SeasonSelection.UI.Fall:
                    
                    GetText((int)Texts.TextA).text = "9월";
                    GetText((int)Texts.TextB).text = "10월";
                    GetText((int)Texts.TextC).text = "11월";

                    GetButton((int)Btns.BtnA).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Sep>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnB).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Oct>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnC).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Nov>(path:"UI_Month");
                    });
                    break;
                    // 여름 처리
                    
                case UI_SeasonSelection.UI.Winter:
                    
                    GetText((int)Texts.TextA).text = "12월";
                    GetText((int)Texts.TextB).text = "1월";
                    GetText((int)Texts.TextC).text = "2월";
                    GetButton((int)Btns.BtnA).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Dec>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnB).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Jan>(path:"UI_Month");
                    });

                    GetButton((int)Btns.BtnC).onClick.AddListener(() =>
                    {
                        Managers.UI.ClosePopupUI();
                        Managers.UI.ShowPopupUI<UI_Feb>(path:"UI_Month");
                    });
                    // 여름 처리
                    break;
            }
        }


        return base.InitOnLoad();
    }
}
