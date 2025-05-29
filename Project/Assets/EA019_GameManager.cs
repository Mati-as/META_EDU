using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA019_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default,
        OnIntro,
        OnColor,
        OnShape,
        OnBalloonFind,
        OnOutro,
        OnFinish
    }

    private enum AnimSeqOnColor
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Pink,
    }
    
    private enum AnimSeqOnShape
    {
        Heart,
        Triangle,
        Star,
        Square,
        Balloon,
        Flower,
    }
   

    private enum Objs
    {
        Balloon_RedHeart,
        Balloon_OrangeTriangle,
        Balloon_YellowStar,
        Balloon_GreenBalloon,
        Balloon_BlueSquare,
        Balloon_PinkFlower,
    }


    public int currentSeqNum
    {
        get => _currentMainSequence;
        set
        {

            _currentMainSequence = value;
            
            switch (value)
            {
                case (int)MainSeq.Default:
                    
                    break;

                case (int)MainSeq.OnIntro:
                    break;

                case (int)MainSeq.OnColor:
                    break;

                case (int)MainSeq.OnShape:
                    break;

                case (int)MainSeq.OnBalloonFind:
                    break;

                case (int)MainSeq.OnOutro:
                    break;

                case (int)MainSeq.OnFinish:
                    break;

            }
        }
    }
    
    private readonly Dictionary<int,bool> _isPosEmptyMap = new(); // 재생성관련 , true인경우 좋은음식은 여기서 생성
    private readonly Dictionary<int, GameObject> allObj = new();
    // 각 풍선 담아놓는 풀
    private readonly Dictionary<int, Stack<GameObject>> _foodClonePool = new(); 
    
    public GameObject PoolRoot
    {
        get
        {
            var root = GameObject.Find("@BalloonPoolRoot");
            if (root == null) root = new GameObject { name = "@BalloonPoolRoot" };

            
            root.gameObject.transform.localScale = Vector3.one*0.2772007f;
            return root;
        }
    }
    private void SetBalloonPol()
    {
                                                                            
        for (int objEnum = (int)Objs.Balloon_RedHeart; objEnum <= (int)Objs.Balloon_PinkFlower; objEnum++)
        {
            
            allObj.Add(GetObject(objEnum).transform.GetInstanceID(), GetObject(objEnum));
            _defaultSizeMap.TryAdd(GetObject(objEnum).transform.GetInstanceID(), GetObject(objEnum).transform.localScale);
            _foodClonePool.Add(objEnum, new Stack<GameObject>());
       
            _defaultPosMap.Add(objEnum, GetObject(objEnum).transform.position);
            _isPosEmptyMap.Add(objEnum, false);
            
            
            for (int count = 0; count < 35; count++)
            {
                var instantiatedFood = Instantiate(GetObject((int)objEnum), PoolRoot.transform, true);
                instantiatedFood.name = ((Objs)objEnum).ToString() +$"{objEnum}".ToString();
                _foodClonePool[objEnum].Push(instantiatedFood);
                allObj.Add(instantiatedFood.transform.GetInstanceID(), instantiatedFood);
                _defaultSizeMap.TryAdd(instantiatedFood.transform.GetInstanceID(), instantiatedFood.transform.localScale);
                instantiatedFood.SetActive(false);
                
            }
        }
   
    }


    protected override void Init()
    {
        base.Init();
    }
}
