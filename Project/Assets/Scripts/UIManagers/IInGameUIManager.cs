namespace UIManagers
{
    public interface IInGameUIManager
    {
        void InitInstructionUI();
        void PopInstructionUIFromScaleZero(string instruction, float duration = 10f, float delay = 0f, string narrationPath = null);
        void PopAndChangeUI(string instruction, float delayAndShutTme = 0f);
        void ShutInstructionUI(string instruction = "");
    }
}