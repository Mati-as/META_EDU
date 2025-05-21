using DG.Tweening;
using UnityEngine;

public class ShowColorBox : MonoBehaviour
{
    public ColorTogether_Manager manager;

    public float totalStepstoScale;

    private Vector3 OriginalScale = new Vector3(2.7f, 2, 1.8f);
    private Vector3 targetScale = new Vector3(14.45f, 8.27F, 10.96f);

    private Vector3 step;

    public Material originalMaterial;
    public Material shineMaterial;

    private Renderer _renderer;

    public bool canColorClicked = true;

    void Start()
    {
        step = (targetScale - OriginalScale) / totalStepstoScale; //클릭 한번에 커질 정도

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
        if (manager.leftTeamScore >= 50 || manager.rightTeamScore >= 50 || !canColorClicked)
            return;
        canColorClicked = false;
        Transform originalTransform = transform;
        Vector3 bigScale = transform.localScale + step;

        //_renderer.material = shineMaterial;
        // DOVirtual.DelayedCall(0.3f, () => _renderer.material = originalMaterial);

        var seq = DOTween.Sequence();
        seq.AppendCallback(() => Flash());
        seq.Append(transform.DOShakePosition(0.1f, 1.5f)).OnComplete(() => transform.position = originalTransform.position);
        seq.Append(transform.DOScale(bigScale, 0.1f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                if (manager.leftTeamScore >= 50 || manager.rightTeamScore >= 50)
                {
                    manager.OnBoxWin(this);
                }
            }));
        seq.AppendInterval(0.15f);
        seq.AppendCallback(() => canColorClicked = true);
        //DoScale은 비동기 애니메이션이라서 DoScale이 작동이 끝나기전에 위if문이 실행되서 오류가 생길 수 있음
        //그래서 OnComplete로 완료 된 후 실행 하도록 변경

    }

    void Flash()
    {
        _renderer.material = shineMaterial;
        DOVirtual.DelayedCall(0.15f, () => _renderer.material = originalMaterial);
    }

    void Reset()
    {
        this.transform.localScale = OriginalScale;
    }


}
