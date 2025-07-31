namespace MyGame.Messages
{
    public class NarrationMessage
    {
        public string Narration
        {
            get;
        }
        public string AudioPath
        {
            get;
        }

        // 남겨둘 필드: 직접 지정한 지속시간(초)
        public float? CustomDuration
        {
            get;
        }

        public NarrationMessage(string narration, string audioPath, float? customDuration = null)
        {
            Narration = narration;
            AudioPath = audioPath;
            CustomDuration = customDuration;
        }
    }
}