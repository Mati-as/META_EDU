using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEngine;

[CreateAssetMenu(fileName = "SceneTextData", menuName = "ScriptableObjects/SceneTextData")]
public class SceneTextData : ScriptableObject
{
    public List<SceneTextEntry> entries;
    
    
}

[System.Serializable]
public class SceneTextEntry
{
    public string sceneId;
    [TextArea(3, 10)]
    public string body;
    
    
    
}