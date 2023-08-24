using UnityEngine;

public class FallenLeavesGM : MonoBehaviour
{
   public int TARGET_FRAME { get; private set; }

    private void Awake()
    {
       
    }

    private void Start()
    {
        TARGET_FRAME = 30;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TARGET_FRAME;
    }


    private void Update()
    {
    }
}