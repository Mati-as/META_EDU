using UnityEngine;
using UnityEngine.InputSystem;

public class Space_EffectController : Base_EffectController
{

    public int waitCount;
    private int _currentCount;

    protected override void OnClicked()
    {
        if (_currentCount > waitCount)
        {
            hits = Physics.RaycastAll(ray_BaseController);
            foreach (var hit in hits)
            {
                PlayParticle(hit.point);
            }

            _currentCount = 0;
        }
        else
        {
            _currentCount++;
        }
       
    }

 
}