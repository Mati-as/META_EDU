using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

public class ShootOut_AnimalController : MonoBehaviour
{
    private enum Animal
    {
        Bear,
        Squirrel,
        Goose,
        Dog,
        Rabbit,
        Hedgehog,
        Max
    }

    private enum Path
    {
        LinearPathA,
        LinearPathB,
        LinearPathC,
        Max
    }

    private enum LookAt
    {
        OnPath,
        OnPop
    }

    // 공 차는 이벤트 (OnBallLaunch) 이벤트가 없어도 동작하는 seq 단, 이벤트 시작시 시퀀스는 중단됩니다.
    private Sequence _autoSeq;
    //공을 찰때 발생하는 이벤트
    private Sequence _eventSeq;
    
    private int currentAutoAnimationgAnimIndex;
    


    private Transform[] _animals;
    private readonly int PATH_VERTICES_COUNT;

    //private Dictionary<string, Vector3[]> _animalAnimPathMap;
    private Vector3[][] _animalPaths;
    private Vector3[] _popUpPos;
    private Vector3[] _lookAts;
   
    
    

    private void Awake()
    {
      
        //_animalAnimPathMap = new Dictionary<string, Vector3[]>();

        var lookAtParent = GameObject.Find("AnimalLookAts").transform;
        _lookAts = new Vector3[lookAtParent.childCount];
        for (int i = 0; i < lookAtParent.childCount ; i++)
        {
            _lookAts[i] = lookAtParent.GetChild(i).position;
        }
        
        _animals = new Transform[(int)Animal.Max];

        for (int i = 0; i < (int) Animal.Max; i++)
        {
            _animals[i] = transform.GetChild(i);
        }
        
        FindAndSetPath();
     
        ShootOut_GameManager.OnLaunchBall -= OnLaunchBall;
        ShootOut_GameManager.OnLaunchBall += OnLaunchBall;
    }

    private void Start()
    {
        DoAutoAnimation();
    }

    private void OnDestroy()
    {
        ShootOut_GameManager.OnLaunchBall -= OnLaunchBall;
        DOTween.Clear();
    }



    private void FindAndSetPath()
    {

        var animalPathParent = GameObject.Find("AnimalPaths").transform;
        _animalPaths = new Vector3[(int)Path.Max][];
        
        for (int i = 0; i < (int)Path.Max; i++)
        {
            var path = animalPathParent.GetChild(i);
            var pathVerticesCount = path.childCount;
            _animalPaths[i] = new Vector3[pathVerticesCount];
            
            for (int k = 0; k < pathVerticesCount; k++)
            {
                _animalPaths[i][k] = path.GetChild(k).position;
            }
           
        }

        var popUpPosParent = GameObject.Find("PopUpPoints").transform;
        _popUpPos = new Vector3[popUpPosParent.childCount];
        
        for (int i = 0; i < popUpPosParent.childCount; i++)
        {
            _popUpPos[i] = popUpPosParent.GetChild(i).position;
        }
        


        // var bearPathParent =  GameObject.Find("BearPath");
        // var pathBear = bearPathParent.GetComponentsInChildren<Transform>().Where(x=>x!=bearPathParent.transform).ToArray();
        // Vector3[] positionsBe = Array.ConvertAll(pathBear, t => t.position);
        // _animalAnimPathMap.TryAdd(_animals[(int)Animal.Bear].gameObject.name, positionsBe);
        //
        // var squirrelPathParent =  GameObject.Find("SquirrelPath");
        // var pathSquirrel = squirrelPathParent.GetComponentsInChildren<Transform>().Where(x=>x!=squirrelPathParent.transform).ToArray();
        // Vector3[] positionsSq = Array.ConvertAll(pathSquirrel, t => t.position);
        // _animalAnimPathMap.TryAdd(_animals[(int)Animal.Squirrel].gameObject.name, positionsSq);
        //
        // var goosePathParent =  GameObject.Find("GoosePath");
        // var pathGoose = goosePathParent.GetComponentsInChildren<Transform>().Where(x=>x!=goosePathParent.transform).ToArray();
        //
        // Vector3[] positionsGo = Array.ConvertAll(pathGoose, t => t.position);
        // _animalAnimPathMap.TryAdd(_animals[(int)Animal.Goose].gameObject.name, positionsGo);
    }



    private void OnLaunchBall()
    {
     
        var randomAnimalIndex = Random.Range((int)Animal.Bear, (int)Animal.Max);
        var count = 0;
        while (currentAutoAnimationgAnimIndex == randomAnimalIndex || count > 10)
        {
            randomAnimalIndex = Random.Range((int)Animal.Bear, (int)Animal.Max);
            count++;
        }
       
        
        var randomChance = Random.Range(0, 100);
        if (randomChance > 50)
        {
            
            DoAnimation(_animals[randomAnimalIndex]);
        }
        else
        {
            DoPopUpAnimation(_animals[randomAnimalIndex]);
        }
    
    }

    private void DoAutoAnimation()
    {
        _autoSeq = DOTween.Sequence();

        _autoSeq
            .AppendCallback(() =>
            {
                
                   
                
                currentAutoAnimationgAnimIndex = Random.Range(0, (int)Animal.Max);
                
                var randomAnimal = _animals[currentAutoAnimationgAnimIndex];
                var randomPath = _animalPaths[Random.Range(0, (int)Path.Max)];
                var randomDuration = Random.Range(1.6f,2.7f);

                
                randomAnimal.transform.position = randomPath[0];
                randomAnimal.DOLookAt(_lookAts[(int)LookAt.OnPath],0.01f);
                randomAnimal.DORotateQuaternion(Quaternion.Euler(0,0,30), 0.5f);
                randomAnimal.transform.DOPath(randomPath, randomDuration, PathType.CatmullRom)
                    .SetEase(Ease.InOutSine);
            })
            .SetLoops(-1, LoopType.Restart)
            .AppendInterval(Random.Range( 9f, 15f))
            .OnStepComplete(() =>
            {
#if UNITY_EDITOR
                Debug.Log("AutoAnim Loop Completed, setting up next animation");
#endif
            });
    }

    private void DoAnimation(Transform animal)
    {
        
#if UNITY_EDITOR
        Debug.Log($"PathAnim Playing");
#endif
        var randomIndex = Random.Range(0, (int)Path.Max);
        
        var currentPath = _animalPaths[randomIndex];
        
        animal.position = currentPath[0];
        animal.DOLookAt(_lookAts[(int)LookAt.OnPath], 0.01f);
        animal.DORotateQuaternion(Quaternion.Euler(0,0,30), 0.5f);
        animal.DOPath(currentPath, Random.Range(1.6f,2.5f),PathType.CatmullRom).SetEase(Ease.InOutSine);
    }

    private Vector3 _currentDefaultPos;
    private Vector3 _currentDefaultSize;
    private void DoPopUpAnimation(Transform animal)
    {

        _currentDefaultPos = animal.position;
        _currentDefaultSize = animal.localScale;
        animal.localScale = Vector3.zero;
        animal.position = _popUpPos[Random.Range(0, _popUpPos.Length)];
#if UNITY_EDITOR
        Debug.Log($"PopUpAnim Playing");
#endif

        
        animal.DOScale(_currentDefaultSize, 1.2f).SetEase(Ease.InOutSine)
            .OnStart(() =>
            {
                animal.DOLookAt(_lookAts[(int)LookAt.OnPop],0.12f);
            })
            .OnComplete(() =>
        {
            animal.DOScale(Vector3.zero, Random.Range(0.8f,1.4f)).SetEase(Ease.InOutSine)
                .SetEase(Ease.InOutBounce)
                .SetDelay(0.5f)
                .OnComplete(() =>
                {
                    animal.localScale = _currentDefaultSize;
                    animal.position = _currentDefaultPos;
                });
        });
    }
}
