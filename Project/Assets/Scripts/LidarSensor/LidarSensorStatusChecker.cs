using DG.Tweening;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class LidarSensorStatusChecker : MonoBehaviour
{
    private enum Status
    {
        Connected,
        Disconnected,
        Max
    }

    private Image[] _statusImages;

    private void Start()
    {
        _statusImages = new Image[(int)Status.Max];
        _statusImages[(int)Status.Connected] = transform.GetChild((int)Status.Connected).GetComponent<Image>();
        _statusImages[(int)Status.Disconnected] = transform.GetChild((int)Status.Disconnected).GetComponent<Image>();

        _statusImages[(int)Status.Connected].enabled = false;
        _statusImages[(int)Status.Disconnected].enabled = false;

        SensorManager.OnSenSorInit -= ShowSensorStatusImage;
        SensorManager.OnSenSorInit += ShowSensorStatusImage;
    }

    private Sequence imageBlinkerseq;

    private void ShowSensorStatusImage(bool isSensorOn)
    {
        _statusImages[(int)Status.Connected].enabled = isSensorOn;
        _statusImages[(int)Status.Disconnected].enabled = !isSensorOn;

        var currentActiveImage =
            isSensorOn ? _statusImages[(int)Status.Connected] : _statusImages[(int)Status.Disconnected];


        if (imageBlinkerseq.IsActive()) imageBlinkerseq.Kill();
        imageBlinkerseq = null;

        imageBlinkerseq = DOTween.Sequence();
        imageBlinkerseq.AppendCallback(() => { currentActiveImage.enabled = true; });
        imageBlinkerseq.AppendInterval(0.45f);
        imageBlinkerseq.AppendCallback(() => { currentActiveImage.enabled = false; });
        imageBlinkerseq.SetLoops(12, LoopType.Yoyo);
        imageBlinkerseq.AppendCallback(() => { currentActiveImage.enabled = true; });
    }


}