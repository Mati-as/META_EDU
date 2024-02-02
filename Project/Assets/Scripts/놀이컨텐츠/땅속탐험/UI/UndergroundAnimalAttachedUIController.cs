using System.Collections;
using System.Xml;
using DG.Tweening;
using KoreanTyper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using MyCustomizedEditor;
#endif

public class UndergroundAnimalAttachedUIController : MonoBehaviour
{
    private enum messageID
    {
        OnEnable,
        OnNextAnimal
    }

    private enum tweenParam
    {
        Scale,
        Move
    }

    private enum waitTimeName
    {
        InitialOffset,
        MessagePlayingTime,
        IntervalBetweenFirstMessageAndSecond
    }

    private TMP_Text _tmp;
    private GameObject UIGameObj;
    private Vector3 _textBoxInitialPosition;

    private Coroutine _typingCoroutine;
    [Header("UI parts")] public float textPrintingSpeed;

    [FormerlySerializedAs("maximizedSize")] [Header("Tween Parameters")]
    public float UImaximizedSize;

    public float animalMaximizedSize;

    private Vector3 _maximizedSizeVec;

    [Space(10)]
#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "scale", "move", "3rd", "4th"
    })]
#endif

    public float[] durations = new float[4];


#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "initialOffset", "messagePlayingTime", "intervalBetweenFirstMessageAndSecond", "4th"
    })]
#endif
    public float[] waitTimes = new float[4];


#if UNITY_EDITOR
    [NamedArrayAttribute(new[]
    {
        "OnEnable", "OnNextAnimal", "3rd", "4th"
    })]
#endif


    private string OnNext;
    //public string[] messages  = new string[4];


    [TextArea] public string firstMessage;
    [TextArea] public string secondMessage;

    private void Awake()
    {
        //초기위치 정보 저장 후, 부모객체 위치로 이동 및 OnEnable에서 다시 UI재생위치로 이동.
        UIGameObj = transform.GetChild(transform.childCount - 1).gameObject;
        _textBoxInitialPosition = UIGameObj.transform.position;
        UIGameObj.transform.position = gameObject.transform.position;

        //gameObject.transform.localScale = Vector3.zero;

        _maximizedSizeVec = UImaximizedSize * Vector3.one;

        _tmp = GetComponentInChildren<TMP_Text>();


        Underground_PopUpUI_Button.onPopUpButtonEvent -= OnPopUpButtonClicked;
        Underground_PopUpUI_Button.onPopUpButtonEvent += OnPopUpButtonClicked;
    }

    private void OnDestroy()
    {
        Underground_PopUpUI_Button.onPopUpButtonEvent -= OnPopUpButtonClicked;
    }


    private TextAsset xmlAsset;
    private XmlNode soundNode;
    private XmlDocument _xmlDoc;

    private void Start()
    {
       
        // XML 파일 로드 (Resources 폴더 안에 있어야 함)
         xmlAsset = Resources.Load<TextAsset>("Data/AnimalNarrationStringData"); 
        _xmlDoc = new XmlDocument();
        _xmlDoc.LoadXml(xmlAsset.text);

        var node = _xmlDoc.SelectSingleNode($"//StringData[@ID='{gameObject.name + "In"}']");
        if (node != null)
        {
            string message = node.Attributes["string"].Value;
            _tmp.text = message;
        }
     
       

        // _tmp.text = $"{gameObject.name} 찾았다!";

        UIGameObj.SetActive(false);
        
    }

    private void OnDisable()
    {
        UIGameObj.transform.localScale = Vector3.zero;
    }

    private bool _isFirstlyEnabled = false;

    private void OnEnable()
    {
        if (!GroundGameController.isGameStartedbool)
        {
            // gameObject.SetActive(false);
        }
        else
        {
            //start에서 false하고, 부모객체가 함께 자동으로 Activate 되지않음에 유의. 반드시 활성화 수동으로 해줘야함. 
            UIGameObj.SetActive(true);

            gameObject.transform.localScale = Vector3.zero;
            //animal control section;


            transform.DOScale(Vector3.one * animalMaximizedSize, durations[(int)tweenParam.Scale])
                .SetEase(Ease.InOutBounce);

            //AttachedUI control section;
            _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0.45f));

            UIGameObj.transform.DOMove(_textBoxInitialPosition, durations[(int)tweenParam.Move]);
            UIGameObj.transform.DOScale(_maximizedSizeVec, durations[(int)tweenParam.Scale])
                .SetEase(Ease.InOutBounce)
                .OnComplete(() => { Debug.Log("sizeIncreasing Function worked"); });
        }
    }

    private void OnPopUpButtonClicked()
    {
        DOVirtual.Float(0, 1, 1.5f, val => val++)
            .OnComplete(() => { PlayNextMessageAnim(); });
    }


    private readonly float _waitTimeToDisappear = 3.75f;

    private void PlayNextMessageAnim()
    {
        if (gameObject.activeSelf)
        {
            StopCoroutine(_typingCoroutine);
            
#if UNITY_EDITOR
            Debug.Log("다음 메세지 재생");
#endif

            var node = _xmlDoc.SelectSingleNode($"//StringData[@ID='{gameObject.name + "Out"}']");
            var message = node.Attributes["string"].Value;
            _tmp.text = message;


            _typingCoroutine = StartCoroutine(TypeIn(_tmp.text, 0));

            if (gameObject.name != "여우" && GroundGameController.isGameFinishedbool == false)
            {
                Debug.Log("동물 사라지는 중");

                transform.DOScale(Vector3.zero, durations[(int)tweenParam.Scale])
                    .SetEase(Ease.InOutBounce)
                    .OnStart(() =>
                    {
                        transform.DOScale(Vector3.zero, durations[(int)tweenParam.Scale])
                            .SetEase(Ease.InOutBounce)
                            .OnComplete(() =>
                            {
                                StopCoroutine(_typingCoroutine);
                                gameObject.SetActive(false);
                            });
                    })
                    .SetDelay(_waitTimeToDisappear);
            }


        }


      
    }

    public IEnumerator TypeIn(string str, float offset)
    {
        Debug.Log("제시문 하단 종료 코루틴 ....");
        _tmp.text = ""; // 초기화
        yield return new WaitForSeconds(offset); 

        var strTypingLength = str.GetTypingLength(); 
        for (var i = 0; i <= strTypingLength; i++)
        {
            // 반복문
            _tmp.text = str.Typing(i); 
            yield return new WaitForSeconds(textPrintingSpeed);
        } // 0.03초 대기


        yield return new WaitForNextFrameUnit();
    }
}