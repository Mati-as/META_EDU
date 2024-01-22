using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class Hopscotch_PathAnimationController : MonoBehaviour
{
    public Transform airplane;
    public Transform airBalloon;
    //public Transform fish;
    
    
    public Transform airplanePath;
    public Transform airBalloonPath;
    
    private Transform[] _pathAirplane;
    private Transform[] _pathAirBalloon;
    //public Transform[] pathFish;
    
    private Vector3[] _pathVecAirPlane;
    private Vector3[] _pathVecAirBalloon;
   // private Vector3[] _pathVecFish;

    public float moveDurationAirplane;
    public float moveDurationAirBalloon;
//    public float moveDurationFish;


    private void Awake()
    {
        DOTween.Init();

        _pathAirplane = airplanePath.GetComponentsInChildren<Transform>()
                                   .Where(t=>t.position != transform.position)
                                   .ToArray();
        
        _pathAirBalloon = airBalloonPath.GetComponentsInChildren<Transform>()
            .Where(t=>t != transform)
            .ToArray();
    }

    private void Start()
    {
        Init();
    }

    private void OnDestroy()
    {
        SetSubscription(isSubscribe: false);
    }

    private void SetSubscription(bool isSubscribe = true)
    {
        if (isSubscribe)
        {
            Hopscotch_GameManager.onStageClear -=OnReplay;
            Hopscotch_GameManager.onStageClear +=OnReplay;
        }
        else
        {
            Hopscotch_GameManager.onStageClear -=OnReplay;
        }
       
    }


    private void Init()
    {
        SetSubscription();
        
        _pathVecAirPlane = new Vector3[_pathAirplane.Length];
        _pathVecAirBalloon = new Vector3[_pathAirBalloon.Length];
      //  _pathVecFish = new Vector3[pathFish.Length];
        
        for (int i = 0; i < _pathAirplane.Length; i++)
        {
            _pathVecAirPlane[i] = _pathAirplane[i].position;
        }
        
        for (int i = 0; i < _pathAirBalloon.Length; i++)
        {
            _pathVecAirBalloon[i] = _pathAirBalloon[i].position;
        }
        //
     
    }

    private void OnReplay()
    {
        Move(airplane, _pathVecAirPlane,moveDurationAirplane);
        Move(airBalloon, _pathVecAirBalloon,moveDurationAirBalloon);
       // Move(fish, _pathVecFish,moveDurationFish);
    }

    private void Move(Transform trans,Vector3[] path,float duration=4.5f)
    {
        trans.position = path[0];
        
        trans.DOPath(path, duration,PathType.CatmullRom).OnComplete(() =>
        {
            trans.position = path[0];
        });
    }
}
