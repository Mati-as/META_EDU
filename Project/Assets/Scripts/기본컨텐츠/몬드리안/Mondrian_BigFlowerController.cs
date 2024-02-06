
using UnityEngine;
using DG.Tweening;

public class Mondrian_BigFlowerController : MonoBehaviour
{
    private GameObject[] vegetations;
    private int _currentVeggie = 0;
    public Vector3 flowerAppearPosition { get; set; }
    
    void Start()
    {

        flowerAppearPosition = Vector3.zero;
        
        int childrenCount = transform.childCount;
        vegetations = new GameObject[childrenCount];

        for (int i = 0; i < childrenCount; i++) {
            vegetations[i] = transform.GetChild(i).gameObject;
        }
        
        Mondrian_GameManager.onBigCubeExplosion -=PlayAnim;
        Mondrian_GameManager.onBigCubeExplosion +=PlayAnim;
    }


    private void OnDestroy()
    {
        Mondrian_GameManager.onBigCubeExplosion -=PlayAnim;
    }

    [SerializeField] private float targetScale;
    [SerializeField] private Vector3 offset;

    public bool _onGrowing { get;private set; }

    [SerializeField] private Quaternion[] _rotations; 
    
    void PlayAnim()
    {
        if (!_onGrowing)
        {
            _onGrowing = true;
            
            vegetations[_currentVeggie % vegetations.Length].transform.localScale = Vector3.zero;
            vegetations[_currentVeggie % vegetations.Length].transform.position = flowerAppearPosition + offset;
            vegetations[_currentVeggie % vegetations.Length].transform.DORotateQuaternion(_rotations[Random.Range(0, 2)],0.1f);
            vegetations[_currentVeggie % vegetations.Length].transform.DOScale(targetScale, 0.8f)
                .SetEase(Ease.InOutBounce)
                .SetDelay(0.2f)
                .OnComplete(() =>
                {
                    vegetations[_currentVeggie % vegetations.Length].transform.DOScale(0f, 1.0f)
                        .SetDelay(1.5f)
                        .OnComplete(() =>
                        {
                            _currentVeggie++;
                            _onGrowing = false;
                        });


                });
        }
       

        
    }
}
