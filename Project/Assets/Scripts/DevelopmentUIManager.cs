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
        //각 콘텐츠 별로 처음에 안보이게 하기위해 남김
        DisableAllImages();
    }

    //[삭제]
    private void Update()
    {
        //버튼으로 대체만하면 이건 해결
        if (Input.GetKeyDown(KeyCode.Space)) DisableAllImages();
    }

    //뭔가 콘텐츠 안에서 계속해서 키게 만드는 무언가가 있음
    //0326 private -> public
    public void DisableAllImages()
    {
        Debug.Log("DAI Clicked");
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