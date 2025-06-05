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
        public NarrationMessage(string narration, string audioPath)
        {
            Narration = narration;
            AudioPath = audioPath;
        }
    }
}