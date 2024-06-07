using System.Collections.Generic;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevelopmentUIManager : MonoBehaviour
{
    private Stack<Image> _imagesPool;
    private TextMeshProUGUI _fpsCounter;
    private bool _currentStatus = false;
    private GameObject _developerMenu;

    private void Start()
    {
        _imagesPool = new Stack<Image>();
        _developerMenu = transform.GetChild(2).gameObject;
        var images = _developerMenu.GetComponentsInChildren<Image>();
        _fpsCounter = _developerMenu.GetComponentInChildren<TextMeshProUGUI>();
        foreach (var image in images)
        {
            _imagesPool.Push(image);
        }
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

        _fpsCounter.enabled = _currentStatus;

        // _developerMenu.SetActive(_currentStatus);

    }
}