using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Launcher_DevelopmentUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    private Stack<Image> _imagesPool =new ();
    private TextMeshProUGUI _fpsCounter = new ();
    private bool _currentStatus = true;
    private GameObject _developerMenu;

    private void Start()
    {
       
        var images = transform.GetComponentsInChildren<Image>();
//        _fpsCounter = Utils.FindChild(gameObject, "NewFPSCounter",recursive:true).GetComponent<TextMeshProUGUI>();
        
        foreach (var image in images)
        {
            _imagesPool.Push(image);
        }
        DisableAllImages();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) DisableAllImages();
    }

    private void DisableAllImages()
    {
        _currentStatus = !_currentStatus;
        foreach (var image in _imagesPool)
        {
            image.enabled = _currentStatus;
        }

        if (_fpsCounter != null)
        {
            _fpsCounter.enabled = _currentStatus;
        }
        

        // _developerMenu.SetActive(_currentStatus);

    }
}
