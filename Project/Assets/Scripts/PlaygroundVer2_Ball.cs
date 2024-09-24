using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundVer2_Ball : Playground_Ball_Base
{

    private PlaygroundBaseVer2GameManager _gm;

    

    protected override void Init()
    {
        base.Init();
        _gm = GameObject.FindWithTag("GameManager").GetComponent<PlaygroundBaseVer2GameManager>();
    }

    public override void OnTriggerEnter(Collider other)
    {
        //스코어 중복방지
        if (_isRespawning) return;
        
        if (other.transform.gameObject.name.Contains("NetLeft"))
        {
#if UNITY_EDITOR
            Debug.Log("leftGoal");
#endif
            _gm.scoreLeft++;
        }
        if (other.transform.gameObject.name.Contains("NetRight"))
        {
#if UNITY_EDITOR
            Debug.Log("RightGoal");
#endif
            _gm.scoreRight++;
        }
        
        base.OnTriggerEnter(other);
    }

 
}
