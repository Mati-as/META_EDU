using UnityEngine;

public class FallenLeavesGM : MonoBehaviour
{
   public int TARGET_FRAME { get; private set; }

   
   private Vector3 m_vecMouseDownPos;
   public ParticleSystem clickParticleSystem;
   public LayerMask UIInteractableLayer;

    private void Start()
    {
        TARGET_FRAME = 30;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TARGET_FRAME;
    }


    private void Update()
    {
    }
    private void PlayClickOnScreenEffect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_vecMouseDownPos = Input.mousePosition;
            
            var raySecond = Camera.main.ScreenPointToRay(m_vecMouseDownPos);
            RaycastHit hitSecond;
            
            if (Physics.Raycast(raySecond, out hitSecond, Mathf.Infinity, UIInteractableLayer) )
            {
                
                Debug.Log("파티클 재생");
                clickParticleSystem.Stop(); // 현재 재생 중인 파티클이 있다면 중지합니다.
                clickParticleSystem.transform.position = hitSecond.point; // 파티클 시스템을 클릭한 위치로 이동시킵니다.
                clickParticleSystem.Play(); // 파티클 시스템을 재생합니다.
            }
         
        }
    }
}