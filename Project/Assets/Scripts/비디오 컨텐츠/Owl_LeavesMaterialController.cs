using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


public class LeafInfo
{
    public string name;
    public bool isDarkend;
}

public class Owl_LeavesMaterialController : MonoBehaviour
{
    private Material[][] materalGroup;
    private Transform[] tempChildren;

    private List<LeafInfo> _leafs;
    public float intensity = 1.15f;

    public Dictionary<Material, Color> defaultColorMap;
    public Dictionary<Material, Sequence> sequenceMap;
    public Dictionary<string, Material[]> matByNames;

    private Ray rayForShader;
    private void Start()
    {
        sequenceMap = new Dictionary<Material, Sequence>();
        Init();
        BlinkLeaves();
        BindEvent();
    }

    

    private void BlinkLeaves()
    {
#if UNITY_EDITOR       
        Debug.Log("Material Brightness Changing");
#endif

        foreach (var mat in materalGroup[0])
        {
            if (mat != null)
            {
                LoopBlink(mat);
            }
        }
        
        foreach (var mat in materalGroup[1])
        {
            if (mat != null)
            {
                LoopBlink(mat);
            }
        }
        
        foreach (var mat in materalGroup[2])
        {
            if (mat != null)
            {
                LoopBlink(mat);
            }
        }
        
        foreach (var mat in materalGroup[3])
        {
            if (mat != null)
            {
                LoopBlink(mat);
            }
        }
    }

    private void BindEvent()
    {
        IGameManager.On_GmRay_Synced -= OnClicked;
        IGameManager.On_GmRay_Synced += OnClicked;
    }

    private void OnDestroy()
    {
        IGameManager.On_GmRay_Synced -= OnClicked;
    }

    private void LoopBlink(Material mat)
    {

        mat.DOColor(mat.color * intensity, 2).SetDelay(4.5f);


    }

    private void OnClicked()
    {
#if UNITY_EDITOR       
        Debug.Log("Shader Ray Launched");
#endif
        rayForShader = IGameManager.GameManager_Ray;
        RaycastHit hit;
        
        if (Physics.Raycast(rayForShader, out hit))
        {
#if UNITY_EDITOR       
            Debug.Log("Shader Ray Launched");
#endif
            foreach (var leaf in _leafs)
            {
                if (hit.transform.gameObject.name == leaf.name)
                {
                    if (!leaf.isDarkend)
                    {
                        DarkenLeaf(leaf.name);
                        leaf.isDarkend = true;
                    }
                 
                }
            }
        
               
            
        }
    }

    private void DarkenLeaf(string objName)
    {
        foreach (var mat in matByNames[objName])
        {
            
            if (mat != null)
            {
                mat.DOColor(mat.color / 4, 2.2f);
            }
         
#if UNITY_EDITOR       
            Debug.Log("Darkening.......");
#endif

        }
       
    }

    private void Init()
    {
        
        int CHILD_COUNT = transform.childCount;
        matByNames = new Dictionary<string, Material[]>();
        materalGroup = new Material[CHILD_COUNT][];

        _leafs = new List<LeafInfo>();
        tempChildren = new Transform[CHILD_COUNT];
        
        for (int i = 0; i < CHILD_COUNT; ++i)
        {
            materalGroup[i] = new Material[30]; 
        }
        
        for (int i = 0; i < CHILD_COUNT; ++i)
        {
            tempChildren[i] = transform.GetChild(i);

           
            //복사본 가져오는 방식.
            Renderer[] childRenderer = tempChildren[i].GetComponentsInChildren<Renderer>();

            defaultColorMap = new Dictionary<Material, Color>();
            int count = 0;
            
            foreach (var renderer in childRenderer)
            {
                
                if (renderer is ParticleSystemRenderer)
                {
                    continue;
                }
                
                if (renderer != null && renderer.material != null) // null 체크
                {
                    materalGroup[i][count] = renderer.material;
                    defaultColorMap.TryAdd(materalGroup[i][count], renderer.material.color);
                   
                    count++;
#if UNITY_EDITOR       
                    Debug.Log("머터리얼 할당 및 캐싱");
#endif

                }

                LeafInfo leaf= new LeafInfo();
              
                
                leaf.name = tempChildren[i].gameObject.name;
                //클릭시 이름을 비교하여 쉐이더 컨트롤 
                
                matByNames.TryAdd(leaf.name,materalGroup[i]);
                
                _leafs.Add(leaf);
            }
            
        }
        
    }
}
