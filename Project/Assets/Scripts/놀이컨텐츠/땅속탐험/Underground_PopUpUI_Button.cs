using System;
using UnityEngine;
using DG.Tweening;
using Button = UnityEngine.UI.Button;

public class Underground_PopUpUI_Button : MonoBehaviour
{
    [SerializeField] 
    private GroundGameManager gameManager;

    private Button _button;

    [SerializeField] private AudioSource uiAudioSource;
    public AudioClip buttonSound;

    public static event Action onPopUpButtonEvent; 

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
        isClickable = true;
    }

    private bool isClickable;
    // Popup이벤트 action list
    void OnButtonClicked()
    {
        if (isClickable)
        {
            isClickable = false;
            
            onPopUpButtonEvent?.Invoke();
        
            // 단순 delay및 콜백설정용 DoVirtual..
            DOVirtual.Float(0, 1, 3f, val => val++)
                .OnComplete(() =>
                {
                    isClickable = true;
                    _tween.Kill();
                    gameObject.SetActive(false);
                });
            
            if (uiAudioSource != null)
            {
                uiAudioSource.clip = buttonSound;
                uiAudioSource.Play();
            }

            
            if (FootstepManager.currentFootstepGroupOrder >= gameManager.TOTAL_ANIMAL_COUNT )
            {
                gameManager.isGameFinishedRP.Value = true;
            }
        }
        
    }

    private Tween _tween;

    private void Start()
    {
        _defaultSize = transform.localScale.x;
        
    }
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    private bool _isClicked =false;
    public float scaleUpSize;
    public float sizeChangeDuration;
    public float _defaultSize;
    [SerializeField]
    
    [Header("Audio")] 
    private AudioSource _audioSource;
    
    private SpriteRenderer _spriteRenderer;
    [Space(20f)] [Header("Tween Parameters")]
    public float maximizedSize;
    private Transform _buttonRectTransform;
    private void UpScale()
    {
        _tween.Kill();
            #if UNITY_EDITOR
            Debug.Log($"{this.gameObject.name} : DoScale is Working...");
            #endif
            transform.localScale = _defaultSize * Vector3.one;
            _tween = transform.DOScale( Vector3.one * (_defaultSize * scaleUpSize), sizeChangeDuration)
                .SetEase(Ease.OutBounce)
                .OnComplete(() => DownScale());
        
  
    }
    

    public void DownScale()
    {
        _tween.Kill();
            transform.localScale = Vector3.one *_defaultSize* scaleUpSize ;
            _tween = transform.DOScale(Vector3.one * _defaultSize, sizeChangeDuration)
                .SetEase(Ease.OutBounce)
                .OnComplete(() => UpScale());
        
 
    }

}
