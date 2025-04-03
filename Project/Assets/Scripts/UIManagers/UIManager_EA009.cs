using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using DG.Tweening;
public class UIManager_EA009 : UI_PopUp
{
 public enum TMP
    {
        MessageBox,
        SparrowCount
    }

    public override bool Init()
    {
        BindText(typeof(TMP));

        EA009_HealthyFood_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
        EA009_HealthyFood_GameManager.SeqMessageEvent += OnGetMessageEventFromGm;
        

        GetText((int)TMP.MessageBox).text = string.Empty;
        return true;
    }


    private void OnGetMessageEventFromGm(int message)
    {
        
        Logger.Log($"Get Message ---- {message}");
        switch (message)
        {
            case (int)EA009_HealthyFood_GameManager.SeqNar.HungryTimeToEat:
                GetText((int)TMP.MessageBox).text = string.Empty;
                break;
                
            case (int)EA009_HealthyFood_GameManager.SeqNar.ChangeToGoodFood:
                GetText((int)TMP.MessageBox).text = string.Empty;
                break;
                
            case (int)EA009_HealthyFood_GameManager.SeqNar.Delicious:
                GetText((int)TMP.MessageBox).text = "몸에 좋은 음식을 먹고 튼튼해져요!";
                break;
           
        }
    }
    

    private void OnDestroy()
    {
     
        EA009_HealthyFood_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
    }
}
