using DG.Tweening;
using UnityEngine;

namespace 기본컨텐츠.다양한악기놀이
{
    public class MusicIntruments_Piano : MonoBehaviour,IMusicInstrumentsIOnClick
    {
        private Sequence _seq;
        private float moveAmount = 0.035f;
        private Vector3 _defaultLocation;

        private void Start()
        {
            _defaultLocation = transform.position;
        }
        public void OnClicked()
        {
            PlayBeadsDrumAnimation(transform);
            
            Managers.Sound.Play(SoundManager.Sound.Effect,
                "Audio/BasicContents/MusicInstruments/" + gameObject.name ,
                0.1f,pitch:Random.Range(1.4f,1.4f));
        }
    
        private void PlayBeadsDrumAnimation(Transform obj)
        {
            if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;
#if UNITY_EDITOR
            Debug.Log("Piano");
#endif
        
            _seq = DOTween.Sequence();

            _seq
                .Append(obj.DOMove(_defaultLocation + -transform.up *moveAmount, 0.15f).SetEase(Ease.InOutSine))
                .AppendInterval(0.05f)
                .Append(obj.DOMove(_defaultLocation, 0.15f).SetEase(Ease.InOutSine))
                .AppendInterval(1.0f);

        }
    }
}
