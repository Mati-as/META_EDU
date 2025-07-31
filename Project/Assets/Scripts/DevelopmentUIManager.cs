using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mime;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class DevelopmentUIManager : MonoBehaviour
{
    private Stack<Image> _imagesPool;
    private Stack<TextMeshProUGUI> _TMPPool;
    private Stack<Text> _textPool;
    
  //  private TextMeshProUGUI _fpsCounter;
    private bool _currentStatus = true;

    private void Start()
    {
        _TMPPool = new Stack<TextMeshProUGUI>();
        _imagesPool = new Stack<Image>();
        _textPool = new Stack<Text>();
        
        var images = gameObject.GetComponentsInChildren<Image>();
        var tmps = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        var texts = gameObject.GetComponentsInChildren<Text>();
        
//        _fpsCounter = Utils.FindChild(gameObject,"FPSCounter").GetComponent<TextMeshProUGUI>();
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
        ToggleAllImages();
    }


    [Conditional("DevOnly")]
    private void DisableImageWithSpaceKey()
    {
        //버튼으로 대체만하면 이건 해결
            ToggleAllImages();
    }
    
 //  2026.06.22 스페이스바 관련 비활성화. 버튼으로만 동작
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)&& SceneManager.GetActiveScene().name.Contains("LAUNCHER"))DisableImageWithSpaceKey();
    }



    //뭔가 콘텐츠 안에서 계속해서 키게 만드는 무언가가 있음
    //0326 private -> public
    
    private bool _isClickable = true;
    
    /// <summary>
    /// 03/28/25
    /// 개발자 관련 전체이미지를 키거나 끄는 용도로만 활용 중
    /// 실제로 개별 FP_real,New이미지를 컨트롤 하는변수는 SensorImage에 있음
    /// SensorRelatedDevMenu.cs 참고
    /// </summary>
    public void ToggleAllImages()
    {
        Debug.Log($"DAI Clicked {_currentStatus}");
      

        if (!_isClickable) return;
        DOVirtual.DelayedCall(0.5f, () =>
        {
            _isClickable = true;
        });
        _isClickable = false;
        
        
        
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


    }
}