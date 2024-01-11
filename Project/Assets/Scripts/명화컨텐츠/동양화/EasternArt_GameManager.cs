using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class EasternArt_GameManager : MonoBehaviour
{
    [Header("gameObjs")] public Transform camera;
    public SpriteRenderer originalSpriteRenderer;

    [Space(15f)] [Header("LookAt")] public Transform lookAtA;
    public Transform lookAtB;


    public Transform[] cameraPath;
    public Transform arrivalB;

    private Vector3[] _pathVector;
    private Vector3[] _newVector;

    [Header("Skinned Picture")] public GameObject skinnedPicture;

    private Transform[] _skinnedPictureChildren;

    private void Awake()
    {
      
        _pathVector = new Vector3[3];
        _newVector = new Vector3[2];

        for (var i = 0; i < cameraPath.Length; i++)
        {
            _pathVector[i] = cameraPath[i].position;
            Debug.Log($"cameraPathLength: {cameraPath.Length}");
            Debug.Log($"i: {i}");
        }

        _skinnedPictureChildren = new Transform[skinnedPicture.transform.childCount];
        
        
        for (int i = 0; i < _skinnedPictureChildren.Length; i++)
        {
            _skinnedPictureChildren[i] = skinnedPicture.transform.GetChild(i);
        }

        newBackground.DOFade(0, 0.1f);

    }

    [SerializeField]
    private GameObject originalPicture;
    [SerializeField]
    private SpriteRenderer newBackground;


    
    private void Start()
    {
        camera.DOLookAt(lookAtA.position, 0.01f);


        camera.DOPath(_pathVector, 3.5f, PathType.CatmullRom)
            .SetLookAt(lookAtA, true)
            .OnComplete(() =>
            {
                newBackground.maskInteraction = SpriteMaskInteraction.None;
                newBackground.DOFade(1, 1.5f);
               //  _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
               originalSpriteRenderer.DOFade(0, 1.5f)
                     .OnComplete(() =>
                 {
                     originalPicture.SetActive(false);
                 });
               
                foreach (var obj in _skinnedPictureChildren) obj.gameObject.SetActive(true);

                _newVector[0] = camera.position;
                _newVector[1] = arrivalB.position;


                var currentLookat = new Vector3();

                camera.DOPath(_newVector, 3.5f)
                    .SetEase(Ease.InOutQuint
                    ).OnStart(() =>
                    {
                        DOVirtual.Float(0, 1, 3.8f,
                            reval =>
                            {
                                currentLookat = Vector3.Lerp(lookAtA.position, lookAtB.position, reval);
                                camera.DOLookAt(currentLookat, 0.01f);
                            });
                    });
            })
            .SetDelay(1.5f);
    }
}