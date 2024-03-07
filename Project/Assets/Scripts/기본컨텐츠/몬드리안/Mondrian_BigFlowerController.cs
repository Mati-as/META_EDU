
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mondrian_BigFlowerController : MonoBehaviour
{
    private GameObject[] _vegetations;
    private int _currentVeggie = 0;
    public Vector3 flowerAppearPosition { get; set; }
    private Dictionary<int, Vector3> _targetSizeMap;
    
    void Start()
    {
        _targetSizeMap = new Dictionary<int, Vector3>();
        flowerAppearPosition = Vector3.zero;
        
        int childrenCount = transform.childCount;
        _vegetations = new GameObject[childrenCount];

        for (int i = 0; i < childrenCount; i++) {
            _vegetations[i] = transform.GetChild(i).gameObject;
            _targetSizeMap.TryAdd(_vegetations[i].transform.GetInstanceID(), _vegetations[i].transform.localScale);
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
            var currentTransformID = _vegetations[_currentVeggie % _vegetations.Length].transform.GetInstanceID();
            _vegetations[_currentVeggie % _vegetations.Length].transform.localScale = Vector3.zero;
            _vegetations[_currentVeggie % _vegetations.Length].transform.position = flowerAppearPosition + offset;
            _vegetations[_currentVeggie % _vegetations.Length].transform.DORotateQuaternion(_rotations[Random.Range(0, 2)],0.1f);
            _vegetations[_currentVeggie % _vegetations.Length].transform.DOScale(_targetSizeMap[currentTransformID], 0.8f)
                .SetEase(Ease.InOutBounce)
                .SetDelay(0.2f)
                .OnComplete(() =>
                {
                    _vegetations[_currentVeggie % _vegetations.Length].transform.DOScale(0f, 1.0f)
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
