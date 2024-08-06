using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AttendanceCheck_UIManager : UI_Base
{
    private AttendanceCheck_GameManager _gm;
    private GameObject[] _uiObjs;

    private GameObject[] _nameOnListObjs;
    public Text[] text_namesOnList { get; private set; }
    private Text _placeholder; 
   
    private Dictionary<int, Text> _textMap;
    
    private Button[] _btns;
    private InputField _inputField;

    
    public int activeNameCount { get; private set; }
    public static event Action OnNameInputFinished;
    private enum UI_Type
    {
        Name_InputField,
        NameList,
    }
    private enum Btn_Type
    {
        Btn_Input,
        Btn_Complete
    }

    public override bool Init()
    {
        _gm = GameObject.FindWithTag("GameManager").GetComponent<AttendanceCheck_GameManager>();

        _placeholder = GameObject.Find("Placeholder").GetComponent<Text>();
        BindObject((typeof(UI_Type)));
        BindButton((typeof(Btn_Type)));
        InitializeUI();
        
        _inputField = _uiObjs[(int)UI_Type.Name_InputField].GetComponent<InputField>();
        
        Debug.Assert(_inputField!=null);
        Debug.Assert(_btns!=null);
        
        _btns[(int)Btn_Type.Btn_Input].gameObject.BindEvent(OnInput);
        _btns[(int)Btn_Type.Btn_Complete].gameObject.BindEvent(OnCompleteNameList);

        return true;
    }

    /// <summary>
    /// 이름 '입력' 버튼 클릭시 
    /// </summary>
    private void OnInput()
    {
        if (_inputField.text == string.Empty || _inputField.text == "" || _inputField.text.Length < 2 || _inputField.text.Length > 5)
        {
            
            _placeholder.text = "올바른 이름을 입력해주세요";
            DOVirtual.Float(0, 0, 2f, _ => { }).OnComplete(() =>
            {
                _placeholder.text = "이름을 입력해주세요";
            });
            _inputField.text = string.Empty;
            return;
        }
        
        for (int i = 0; i < _uiObjs[(int)UI_Type.NameList].transform.childCount; i++)
        {
            
            if (_nameOnListObjs[i] != null && !_nameOnListObjs[i].activeSelf)
            {
                _textMap[_nameOnListObjs[i].GetInstanceID()].text = _inputField.text;
                _inputField.text = string.Empty;
                _nameOnListObjs[i].SetActive(true);
                return;
            }
            
            _placeholder.text = "입력 성공!";
            DOVirtual.Float(0, 0, 2f, _ => { }).OnComplete(() =>
            {
                _placeholder.text = "이름을 입력해주세요";
            });
        }
    }
    
    /// <summary>
    /// 이름 입력 '완료' 버튼 클릭시 
    /// </summary>
    private void OnCompleteNameList()
    {
         activeNameCount = 0;
        foreach (var name in _nameOnListObjs)
        {
            if (name != null && !name.gameObject.activeSelf)
            {
                activeNameCount++;
            }
        }
        
        transform.GetComponent<CanvasGroup>().DOFade(0, 1f).SetDelay(1f);
        OnNameInputFinished?.Invoke();
    }

    private void InitializeUI()
    {
        _textMap = new Dictionary<int, Text>();
        
        _uiObjs = new GameObject[Enum.GetValues(typeof(UI_Type)).Length];
        _uiObjs[(int)UI_Type.Name_InputField] = GetObject((int)UI_Type.Name_InputField);
        _uiObjs[(int)UI_Type.NameList]= GetObject((int)UI_Type.NameList);
        
        text_namesOnList = new Text[_uiObjs[(int)UI_Type.NameList].transform.childCount];
        _btns  = new Button[Enum.GetValues(typeof(Btn_Type)).Length];
        _nameOnListObjs = new GameObject[_uiObjs[(int)UI_Type.NameList].transform.childCount];
        
        //인덱스 0은 TrainHead이므로 TMP가 없음. 따라서 인덱스 1부터 시작함에 주의
        for (int i = 0; i < _uiObjs[(int)UI_Type.NameList].transform.childCount; i++)
        {
            _nameOnListObjs[i] = _uiObjs[(int)UI_Type.NameList].transform.GetChild(i).gameObject;
            
            text_namesOnList[i] = _nameOnListObjs[i].GetComponentInChildren<Text>();
            _nameOnListObjs[i].SetActive(false);
            text_namesOnList[i].text = string.Empty;
            
            _textMap.Add(_nameOnListObjs[i].GetInstanceID(),text_namesOnList[i]);
            //Debug.Log($"name : {_nameOnListObjs[i].GetInstanceID()}, order : {i}");
        }
        
        for (var i = 0; i < Enum.GetValues(typeof(Btn_Type)).Length; i++)
        {
            _btns[i] = GetButton(i);
         //   _btnRects[i] = _uiBtns[i].transform.GetComponent<RectTransform>();
        }
    }
}
