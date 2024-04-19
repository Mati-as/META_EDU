
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class MusicInstruments_PianoAutoPlayer : MonoBehaviour
{
 
    private Transform _parentTransform;

    private float _shrinkAmount = 0.92f;
    private Vector3 _defaultSize;
    private Sequence _seq;
    private Sequence _stickSeq;
    private Quaternion _defaultQuat;

    //the number of the piano keys.
    private readonly int PLAYABLE_PIANO_KEY_COUNT = 13;
    private Transform[] _pianoKeys;
    private Vector3[] _defaultLocations;
    
    
 
    private float moveAmount = 0.030f;



    
    private void Start()
    {
        _pianoKeys = new Transform[PLAYABLE_PIANO_KEY_COUNT];
        _defaultLocations = new Vector3[PLAYABLE_PIANO_KEY_COUNT];
        
        for (int i = 0; i < PLAYABLE_PIANO_KEY_COUNT; i++)
        {
            _pianoKeys[i] = transform.GetChild(i);
            _defaultLocations[i] = _pianoKeys[i].position;
        }

        var random = Random.Range(0, PLAYABLE_PIANO_KEY_COUNT);
        PlayAutomatic(_pianoKeys[random],_defaultLocations[random]);
    }


    private void PlayAutomatic(Transform obj, Vector3 defaultLocation)
    {
#if UNITY_EDITOR
        Debug.Log("Piano");
#endif

        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;

        _seq = DOTween.Sequence();

        _seq
            .Append(obj.DOMove(defaultLocation + -transform.up * moveAmount, 0.15f).SetEase(Ease.InOutSine))
            .AppendInterval(0.05f)
            .Append(obj.DOMove(defaultLocation, 0.15f).SetEase(Ease.InOutSine))
            .AppendInterval(0.55f)
            .OnComplete(() =>
            {
                _seq = null;
                var random = Random.Range(0, PLAYABLE_PIANO_KEY_COUNT);
                PlayAutomatic(_pianoKeys[random], _defaultLocations[random]);
            });
    }
}
