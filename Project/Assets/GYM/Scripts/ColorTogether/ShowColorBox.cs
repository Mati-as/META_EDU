using System.Collections;
using DG.Tweening;
using UnityEngine;

public class ShowColorBox : MonoBehaviour
{
    public float totalStepstoScale = 100f;
    private Vector3 step;

    public Vector3 OriginalScale = new Vector3(2.7f, 2f, 1.8f);
    public Vector3 targetScale = new Vector3(14.45f, 8.27f, 10.96f);

    public Material originalMaterial;
    public Material shineMaterial;

    private Renderer _renderer;
    private Vector3 _baseScale;
    private int _clickCount;
    private Coroutine _scaleRoutine;

    [SerializeField] private float scaleDuration = 0.1f;

    private ColorTogether_Manager manager;

    void Awake()
    {
        _baseScale = OriginalScale;
        transform.localScale = _baseScale;

        step = (targetScale - OriginalScale) / totalStepstoScale;

        _renderer = GetComponent<Renderer>();
        originalMaterial = Resources.Load<Material>("Material/ColorTogether/Color");
        shineMaterial = Resources.Load<Material>("Material/ColorTogether/Emission");

        manager = FindObjectOfType<ColorTogether_Manager>();
    }

    void OnEnable()
    {
        Reset();
    }

    public void ColorClicked()
    {
        _clickCount++;

        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

        Vector3 target = _baseScale + step * _clickCount;

        _scaleRoutine = StartCoroutine(ScaleToTarget(target, scaleDuration));

        Flash();
        StartCoroutine(ShakePosition(0.1f, 1.5f));

    }

    private IEnumerator ScaleToTarget(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.localScale = target;
    }

    private IEnumerator ShakePosition(float duration, float magnitude)
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * magnitude;
            yield return null;
        }
        transform.position = originalPos;
    }
    private void Flash()
    {
        _renderer.material = shineMaterial;
        DOVirtual.DelayedCall(0.15f, () => _renderer.material = originalMaterial);
    }

    private void Reset()
    {
        _clickCount = 0;
        transform.localScale = _baseScale;
    }

}
