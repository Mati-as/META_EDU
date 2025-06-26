using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class SeatSelectionController : Ex_MonoBehaviour
{
    public enum Objs
    {
        Seat_A,
        Seat_B,
        Seat_C,
        Seat_D,
        Seat_E,
        Seat_F,
        Seat_G,

        Bg
        //   Seatcount_Max
    }


    public event Action OnAllSeatSelected;
    private bool _isClickable;

    private Color _defaultColor;
    [SerializeField] private Color _selectedColor;
    SpriteRenderer _bgRenderer;

    private readonly Dictionary<int, bool> isSeatClickedMap = new();
    private readonly Dictionary<int, MeshRenderer> _seatMeshRendererMap = new();

    private int _seatClickedCount;

    protected override void Init()
    {
        BindObject(typeof(Objs));
        
        _bgRenderer = GetObject((int)Objs.Bg).GetComponent<SpriteRenderer>();
        GetObject((int)Objs.Bg).SetActive(false);

        for (int i = (int)Objs.Seat_A; i <= (int)Objs.Seat_G; i++)
        {
            int IndexCache = i;
            GetObject(i).BindEvent(() =>
            {
                OnSeatClicked(IndexCache);
            });

            var Seatrenderer = GetObject(i).transform.GetComponent<MeshRenderer>();
            _defaultColor = Seatrenderer.material.color;
            _seatMeshRendererMap.Add(i, Seatrenderer);
            isSeatClickedMap.Add(i, false);
            GetObject(i).SetActive(false);
        }
    }


    private void AnimateAllSeats()
    {
        for (int i = (int)Objs.Seat_A; i <= (int)Objs.Seat_G; i++)
        {
            Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(Objs)i}");
            GetObject(i).SetActive(true);
            AnimateSeatLoop((Objs)i);
        }
    }

    private void AnimateSeatLoop(Objs seat)
    {
        var SeatTransform = GetObject((int)seat).transform;

        _sequenceMap[(int)seat]?.Kill();
        _sequenceMap[(int)seat] = DOTween.Sequence();
        _sequenceMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 0.9f, 0.35f))
            .SetLoops(100, LoopType.Yoyo)
            .OnKill(() =>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });

        _sequenceMap[(int)seat].Play();
    }

    private void DeactivateAllSeats()
    {
        for (int i = (int)Objs.Seat_A; i <= (int)Objs.Seat_G; i++) _sequenceMap[i]?.Kill();

        TweenCallback _scaleCallback = () =>
        {
            for (int i = (int)Objs.Seat_A; i <= (int)Objs.Seat_G; i++)
            {
                var SeatTransform = GetObject(i).transform;
                _sequenceMap[i] = DOTween.Sequence();
                _sequenceMap[i].Append(SeatTransform.DOScale(Vector3.zero, 0.75f));
            }
        };

        DOVirtual.DelayedCall(1f, _scaleCallback);
    }


    public void StartSeatSelection()
    {
        transform.localScale = Vector3.one;
        Managers.Sound.Play(SoundManager.Sound.Effect,"Audio/Common/LetsSeat");
        
        _seatClickedCount = 0;
        _isClickable = true;
        
        _bgRenderer.transform.gameObject.SetActive(true);
        _bgRenderer.DOFade(0, 0.0001f).OnComplete(() =>
        {
            _bgRenderer.DOFade(1, 1f);
        });
       
        
        foreach (int key in _seatMeshRendererMap.Keys.ToArray())
            _seatMeshRendererMap[key].material.DOColor(_defaultColor, 0.35f);

        foreach (int key in isSeatClickedMap.Keys.ToArray()) isSeatClickedMap[key] = false;
        AnimateAllSeats();
    }

    
    private void OnSeatClicked(int clickedSeat, Color color = default)
    {
        if (!_isClickable) return;

    

        if (isSeatClickedMap[clickedSeat]) return;
        isSeatClickedMap[clickedSeat] = true;


        _seatMeshRendererMap[clickedSeat].material.DOColor(_selectedColor, 0.35f);
        _seatClickedCount++;
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Seat_" + _seatClickedCount);


        _sequenceMap[clickedSeat]?.Kill();

        bool isAllSeatClicked = true;
        
        foreach (int key in isSeatClickedMap.Keys)
            if (!isSeatClickedMap[key])
                isAllSeatClicked = false;

        if (isAllSeatClicked)
        {
            OnAllSeatClicked();


            DOVirtual.DelayedCall(0.10f, () =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/Common/OnAllSeatSelected");
            });
           
            Logger.ContentTestLog("모든 자리가 선택되었습니다--------");

            _isClickable = false;
            OnAllSeatSelected?.Invoke();
        }
    }
    
    private void OnAllSeatClicked()
    {
        _isClickable = false;
        _bgRenderer.DOFade(0, 0.7f).OnComplete(() =>
        {
            _bgRenderer.transform.gameObject.SetActive(false);
        });
        _seatClickedCount = 0;
        DeactivateAllSeats();
     
    }
}