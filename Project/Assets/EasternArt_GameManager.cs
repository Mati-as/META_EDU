using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EasternArt_GameManager : MonoBehaviour
{
    public Transform camera;
    public SpriteRenderer _spriteRenderer;

    public Transform arrivalB;
    public Transform lookAt;
    public Transform[] cameraPath;
    private Vector3[] _pathVector;

    private Vector3[] _newVector;
    void Start()
    {
        
      
        _pathVector = new Vector3[3];
        for (int i = 0; i < cameraPath.Length ; i++)
        {
            _pathVector[i] = cameraPath[i].position;
        }
        _newVector = new Vector3[2];
        camera.DOPath(_pathVector, 3.5f, PathType.CatmullRom)
            .SetLookAt(lookAt,stableZRotation:true)
            .OnComplete(() =>
            {
                _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
                _newVector[0] = transform.position;
                _newVector[1] = arrivalB.position;

                camera.DOPath(_newVector, 1.5f);

            });
        
    }


}
