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
        Pink,
        Yellow,
        Blue
    }
    [Header("Color Setting")] 
    public float sizeRandomInterval;

    [SerializeField] public float[] ballSizes;
    [SerializeField]
    public Color[] colorDef;

    [Header("Path Setting")] 
    public float offset;
    public float depth;
     public float durationIntoHole;
     public float durationSpawnerToGround;

    [Header("Sound Setting")] public float volume;
    public int audioSize = 5;

    [Header("path")] public Vector3 departure;
    public Vector3 arrival;

    [Header("Respon Setting")] 
    public float respawnPower;
    public float respawnWaitTime;

}
