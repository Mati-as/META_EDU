using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BallData", menuName = "BallData/BallInfo", order = int.MaxValue )]
public class BallInfo : ScriptableObject
{
    public enum BallSize
    {
        Small,
        Medium,
        Large
    }

    public enum BallColor
    {
        Red,
        Yellow,
        Blue
    }
    [Header("Color Settings")] 
    public float sizeRandomInterval;

    [SerializeField] public float[] ballSizes;
    [SerializeField]
    public Color[] colorDef;

    [Header("Path Settings")] 
    public float offset;
    public float depth;
     public float durationIntoHole;
     public float durationSpawnerToGround;

    [Header("Sound Settings")] public float volume;
    public int audioSize = 5;

    [Header("path")] public Vector3 departure;
    public Vector3 arrival;

}
