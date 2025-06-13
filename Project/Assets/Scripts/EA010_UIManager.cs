using DG.Tweening;

public class EA010_UIManager : Base_UIManager
{

    public enum TMP
    {
       // MessageBox
    }

    public override bool InitEssentialUI()
    {
     //   BindTMP(typeof(TMP));
     EA010_AutumnalFruits_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
     EA010_AutumnalFruits_GameManager.SeqMessageEvent += OnGetMessageEventFromGm;
        base.InitEssentialUI();
    
       // GetTMP((int)TMP.MessageBox).text = string.Empty;
        return true;
    }


    private void OnGetMessageEventFromGm(string message)
    {
        
        Logger.Log($"Get Message ---- {message}");
        switch (message)
        {
            case nameof(EA010_AutumnalFruits_GameManager.SeqName.Default):
               // PopFromZeroInstructionUI( "string.Empty;)
                break;
            
             case "Q" :
                 PopFromZeroInstructionUI("어떤 열매 일까요?");
                 break;
             
            case nameof(EA010_AutumnalFruits_GameManager.MessageSequence.Intro):
                PopFromZeroInstructionUI( "가을에는 주렁주렁열매가 매달려요)\n어떤 열매가 있을까요?");
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Chestnut):
                DOVirtual.DelayedCall(1f, () =>
                {
                    PopFromZeroInstructionUI("밤");});
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Acorn):
                DOVirtual.DelayedCall(1f, () =>
                {
                    PopFromZeroInstructionUI("도토리"); });
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Apple):
                DOVirtual.DelayedCall(1f, () =>
                {
                    PopFromZeroInstructionUI("사과"); });
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Ginkgo):
                DOVirtual.DelayedCall(1f, () =>
                {
                    PopFromZeroInstructionUI("은행"); });
          
                break;

            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Persimmon):

                DOVirtual.DelayedCall(1f, () =>
                {
                    PopFromZeroInstructionUI("감"); });
                
                break;
        }
    }

    private void OnDestroy()
    {
        EA010_AutumnalFruits_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
    }
}
