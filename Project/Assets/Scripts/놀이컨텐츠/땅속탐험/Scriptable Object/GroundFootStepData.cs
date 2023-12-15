using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FootstepData", menuName = "UnderFoot/Footstep_Data", order = int.MaxValue )]
public class GroundFootStepData : ScriptableObject
{

    public float scaleUpSize;
    public float sizeChangeDuration;
    public float defaultFootstepSize;
}
