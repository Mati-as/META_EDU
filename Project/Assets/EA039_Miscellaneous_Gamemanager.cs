using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EA039_Miscellaneous_Gamemanager : Ex_BaseGameManager
{
    private enum Objs
    {
        Cars,Blocks,Shoes
    }
    private enum MiscellaneousType
    {
        Car,
        Block,
        Shoes
    }
    
    // 행동양식 다른부분 별도 클래스 선언없이 Pool형태로 관리
    private Dictionary<int,MiscellaneousType> _carPool =new ();
    private Dictionary<int,MiscellaneousType> _blockPool = new ();
    private Dictionary<int,MiscellaneousType> _shoesPool = new ();

    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));
        for(int i =0; i < GetObject((int)Objs.Cars).transform.childCount;i++)
        {
            var car = GetObject((int)Objs.Cars).transform.GetChild(i).gameObject;
            _carPool.Add(car.GetInstanceID(),MiscellaneousType.Car);
        }
        
        for(int i =0; i < GetObject((int)Objs.Blocks).transform.childCount;i++)
        {
            var block = GetObject((int)Objs.Blocks).transform.GetChild(i).gameObject;
            _blockPool.Add(block.GetInstanceID(),MiscellaneousType.Block);
        }
        
        for(int i =0; i < GetObject((int)Objs.Shoes).transform.childCount;i++)
        {
            var shoes = GetObject((int)Objs.Shoes).transform.GetChild(i).gameObject;
            _shoesPool.Add(shoes.GetInstanceID(),MiscellaneousType.Shoes);
        }

    }
}
