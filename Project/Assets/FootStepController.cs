using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FootStepController : MonoBehaviour
{
    [Space(10)]
    [Header("Footstep Positions")]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] firstFootstepsGroup    = new GameObject[4]; 
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] secondFootstepsGroup   = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] thirdFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] fourthFootstepsGroup   = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] fifthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] sixthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] seventhFootstepsGroup  = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] eighthFootstepsGroup   = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] ninthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] tenthFootstepsGroup    = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] eleventhFootstepsGroup = new GameObject[4];
    [Space(10)]
    [NamedArrayAttribute(new string[]
    {
        "1st", "2nd", "3rd", "4th"
    })]
    public GameObject[] twelfthFootstepsGroup  = new GameObject[4];

}
