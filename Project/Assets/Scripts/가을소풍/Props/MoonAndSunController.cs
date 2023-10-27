using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MoonAndSunController : MonoBehaviour
{
    public float waitTime;
    public float movingTimeSec;
    public float MovingDownSec;


    public Transform _inPlayPosition;
    public Transform _defaultPosition;
    private float elapsedTime;

    private readonly int _ALBEDO = Shader.PropertyToID("_Albedo");


    private MeshRenderer _meshRenderer;
    private Material _mat;
    private Color _defaultColor;
    
    



    private void Start() //called only when it's activated..
    {  
        
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        transform.position = _defaultPosition.position;
        _mat = _meshRenderer.material;
        _defaultColor = _mat.GetColor(_ALBEDO) * DEFAULT_INTENSITY;
        _mat.SetColor(_ALBEDO,_defaultColor); 
      
    }

    [FormerlySerializedAs("DEFAULT")] public float DEFAULT_INTENSITY;
    // Update is called once per frame
    private void Update()
    {
        if (GameManager.isGameStarted)
        {
            MoveUp(waitTime,movingTimeSec);
        }

        if (GameManager.isRoundReady)
        {
            InitializeLerpParams(false);
        }

        if (GameManager.isCorrected)
        {
            SetColorIntensity(_mat.GetColor(_ALBEDO),_defaultColor*targetIntensity);
        }

        if (GameManager.isRoundFinished)
        {
            if (!_isElpasedInitialized)
            {
                InitializeLerpParams(true);
            }
            SetColorIntensity(_mat.GetColor(_ALBEDO),_defaultColor);
        }
        
        if (GameManager.isGameFinished)
        {
            if (!_isElapseInitialized)
            {
                _isElapseInitialized = true;
                elapsedTime = 0f;
            }
            

            MoveDown(0f,MovingDownSec);
        }
    }

    private bool _isElapseInitialized;

    private void InitializeLerpParams(bool value)
    {
        _isElpasedInitialized = value;
        _colorLerp = 0f;
    }

    private bool _isElpasedInitialized;
    /// <summary>
    /// 달이 떠오르게 하는 함수입니다. 떠오르는 타이밍은 GameManager의 isGameStarted에 따라 지정합니다. 
    /// </summary>
    /// <param name="movingTimeTotal"> 달이 떠오르는 데 걸리는 시간</param>
    private void MoveUp(float waitTimeTotal, float movingTimeTotal)
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > waitTimeTotal)
        {
            // Lerp의 t값을 계산 (0 ~ 1 사이)
            var t = Mathf.Clamp01(elapsedTime / movingTimeTotal);
            t = Lerp2D.EaseInQuad(0, 1, t);


            transform.position = Vector3.Lerp(transform.position, _inPlayPosition.position, t);
        }
    }
    
    private void MoveDown(float waitTimeTotal, float movingTimeTotal)
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime > waitTimeTotal)
        {
            // Lerp의 t값을 계산 (0 ~ 1 사이)
            var t = Mathf.Clamp01(elapsedTime / movingTimeTotal);
            t = Lerp2D.EaseInQuad(0, 1, t);


            transform.position = Vector3.Lerp(transform.position, _defaultPosition.position, t);
        }
    }
    
    public float intensityChangeSpeed;
    [Range(0,5)]
    public float targetIntensity;
    private float _colorLerp;
    private void SetColorIntensity(Color initialColor, Color targetColor)
    {
      
        _colorLerp += Time.deltaTime * intensityChangeSpeed;
        Color color = Color.Lerp(initialColor, targetColor, _colorLerp);
        _mat.SetColor(_ALBEDO,color); 
    }
    
    
}