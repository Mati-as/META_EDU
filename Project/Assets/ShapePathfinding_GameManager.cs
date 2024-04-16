using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ShapePathfinding_GameManager : IGameManager
{
    
    /*
     *1.랜덤으로 쉐이프, 컬러 할당하기
     *2.주사위 판별 로직 만들기
     *3.주사위 던지기 로직 만들기 (시간초) 버튼 클릭방식으로 
     *4.카운트 및 돌아가는 로직만들기 
     *5.이펙트 추가
     *6.사운드 추가
     *7.클릭성공시 애니메이션 추가
     */
    
    private Transform[][] _steps; 
    private MeshRenderer[][] _meshRenderers;
    private Material[] _shapeMat;
    private Material[] _colorMat;

    enum ShapeMat
    {
        Circle,
        Square,
        Triangle,
        Heart,
        MaxCount
    }

    enum ColorMat
    {
        Red,
        Blue,
        Green,
        Violet,
        MaxCount
    }


    protected override void Init()
    {
        base.Init();
        LoadResource();
        SetSteps();
    }


    private void LoadResource()
    {
        _colorMat = new Material[(int)ColorMat.MaxCount];
        _shapeMat = new Material[(int)ShapeMat.MaxCount];
        
        // 컬러머터리얼 리소스 로딩 ----------------------------------------------
        var matRed = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Red");
        _colorMat[(int)ColorMat.Red] = matRed;
        
        var matBlue = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Blue");
        _colorMat[(int)ColorMat.Blue] = matBlue;
      
        var matGreen = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Red");
        _colorMat[(int)ColorMat.Green] = matGreen;
       
        var matViolet = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Violet");
        _colorMat[(int)ColorMat.Violet] = matViolet;
        
        
        // 도형머터리얼 리소스 로딩 ----------------------------------------------
        var matCircle = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Circle");
        _shapeMat[(int)ShapeMat.Circle] = matCircle;
        
        var matSquare = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Square");
        _shapeMat[(int)ShapeMat.Square] = matSquare;
        
        var matTriangle = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Triangle");
        _shapeMat[(int)ShapeMat.Triangle] = matTriangle;
        
        var matHeart = Resources.Load<Material>("게임별분류/기본컨텐츠/ShapePathFinder/Material/RuntimeMaterial/M_Heart");
        _shapeMat[(int)ShapeMat.Heart] = matHeart;
    }

    private void SetSteps()
    {
        var stepParent = GameObject.Find("Steps").transform;

        var columnCount = stepParent.childCount;
        _steps = new Transform[columnCount][];
        _meshRenderers = new MeshRenderer[columnCount][];
        
        
        var rowCount = stepParent.GetChild(0).childCount;
        
        for (int i = 0; i < columnCount; i++)
        {
            _steps[i] = new Transform[rowCount];
            _meshRenderers[i] = new MeshRenderer[rowCount];
            for (int k = 0; k < rowCount;k++)
            {
                _steps[i][k] = stepParent.GetChild(i).GetChild(k);
            }
        }
    }

    private void SuffleIntex<T>() where T : UnityEngine.Object
    {

    }






}


