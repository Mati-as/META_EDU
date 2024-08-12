using System.Collections.Generic;
using System.Net.Mime;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevelopmentUIManager : MonoBehaviour
{
    private Stack<Image> _imagesPool;
    private Stack<TextMeshProUGUI> _TMPPool;
    private Stack<Text> _textPool;
    
    private TextMeshProUGUI _fpsCounter;
    private bool _currentStatus = true;
    private GameObject _developerMenu;

    private void Start()
    {
        _TMPPool = new Stack<TextMeshProUGUI>();
        _imagesPool = new Stack<Image>();
        _textPool = new Stack<Text>();
        
        var images = gameObject.GetComponentsInChildren<Image>();
        var tmps = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        var texts = gameObject.GetComponentsInChildren<Text>();
        
        _fpsCounter = Utils.FindChild(gameObject,"FPSCounter").GetComponent<TextMeshProUGUI>();
        foreach (var image in images)
        {
            _imagesPool.Push(image);
        }

        foreach (var text in tmps)
        {
            _TMPPool.Push(text);
        }
        
        foreach (var text in texts)
        {
            _textPool.Push(text);
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

        foreach (var text in _TMPPool)
        {
            text.enabled = _currentStatus;
        }
        
        foreach (var text in _textPool)
        {
            text.enabled = _currentStatus;
        }
        
        _fpsCounter.enabled = _currentStatus;

        // _developerMenu.SetActive(_currentStatus);

    }
}