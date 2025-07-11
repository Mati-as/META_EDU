using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ColorTogether_Manager : Base_GameManager
{
    private RaycastHit[] _hits;

    public bool isInStage = false;
    public BoxDropper boxDropper;
    private EndShowBox endShowBox;

    public Transform spawnPoint1;
    public Transform spawnPoint2;

    // 게임오브젝트(박스 프리팹) 페어 딕셔너리
    private Dictionary<int, (GameObject, GameObject)> colorPairs;

    // 프리팹들을 인스펙터에서 넣을 수 있게 public 선언
    public GameObject redBox;
    public GameObject blueBox;
    public GameObject greenBox;
    public GameObject yellowBox;
    public GameObject purpleBox;
    public GameObject orangeBox;

    private GameObject prefab1;
    private GameObject prefab2;

    private List<int> selectionOrder = new List<int>();
    [SerializeField] private int currentIndex = 0;

    private Sequence msgSeq;

    public GameObject narrationImgGameObject;
    public Image narrationTextImage;

    public CinemachineVirtualCamera StartCamera;
    public CinemachineVirtualCamera StageCamera;
    public CinemachineVirtualCamera GameCamera;

    public Camera mainCamera;
    public Camera UICamera;

    private int selectEffectSound;
    public GameObject Img_NextBoxDrop;

    public bool endCanClicked = false;

    public GameObject leftTeamScoreObj;
    public GameObject rightTeamScoreObj;
    public int leftTeamScore = 0;
    public int rightTeamScore = 0;
    public TextMeshProUGUI leftTeamScoreTmp;
    public TextMeshProUGUI rightTeamScoreTmp;

    public bool nextBoxDropDelay = false;

    [SerializeField] private GameObject ExplosionEffectPrefab;
    [SerializeField] private GameObject VictoryEffectPrefab;

    public RectTransform narrationBG;


    public GameObject TimerObject;
    public Text stageLeftTime;

    public int stageLeftTimeValue = 60;

    public ClickedFloor leftFloor;
    public ClickedFloor rightFloor;

    public GameObject leftBoxColor;
    public GameObject rightBoxColor;

    public Image leftBoxImage;
    public Image rightBoxImage;

    public List<Sprite> colorTextImg;


    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        //BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        narrationImgGameObject.SetActive(false);
        TimerObject.gameObject.SetActive(false);

        leftFloor = GameObject.Find("LeftFloor").GetComponent<ClickedFloor>();
        rightFloor = GameObject.Find("RightFloor").GetComponent<ClickedFloor>();

        //boxDropper = FindObjectOfType<BoxDropper>();
        endShowBox = FindObjectOfType<EndShowBox>();
        ExplosionEffectPrefab = Resources.Load<GameObject>("Effect/EA013/CFX_MagicPoof");
        VictoryEffectPrefab = Resources.Load<GameObject>("Effect/EA013/Confetti_directional_multicolor");
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/ColorTogether/audio_BGM");

        AssignCameras();

        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );
        }

        if (UICamera != null)
        {
            UICamera.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );
        }

        
        UI_InScene_StartBtn.onGameStartBtnShut += StartGame;
    }

    private void StartGame()
    {
        colorPairs = new Dictionary<int, (GameObject, GameObject)>
        {
            { 0 ,   (redBox, blueBox) },
            { 1, (greenBox, yellowBox) },
            { 2, (purpleBox, orangeBox) }
        };

        ShuffleSelectionOrder();
        narrationImgGameObject.SetActive(true);
        narrationImgGameObject.transform.localScale = Vector3.one;
        narrationImgGameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 10, 1);

        msgSeq?.Kill();
        msgSeq = DOTween.Sequence();

        for (int index = 0; index < 2; index++) // 인덱스 최대크기 임의로 할당
        {
            int captureIndex = index;
            AudioClip clip = Resources.Load<AudioClip>("Audio/ColorTogether/audio_" + captureIndex);
            float audioDuration = clip.length;

            msgSeq.AppendCallback(() =>
            {
                narrationBG.sizeDelta = new Vector2(1200, 140);
                Managers.Sound.Play(SoundManager.Sound.Narration, clip);
                TakeTextImg("Image/ColorTogether/Img_" + captureIndex);
            });

            msgSeq.AppendInterval(audioDuration + 2f);
        }

        msgSeq.AppendCallback(() => Img_NextBoxDrop.SetActive(true));
        msgSeq.AppendInterval(0.05f);
        msgSeq.AppendCallback(() => boxDropper.StartDropCycle());

        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;

    }

    void AssignCameras()
    {
        CinemachineVirtualCamera[] cams = GetComponentsInChildren<CinemachineVirtualCamera>();

        foreach (var cam in cams)
        {
            switch (cam.name)
            {
                case "StartCamera":
                    StartCamera = cam;
                    break;
                case "StageCamera":
                    StageCamera = cam;
                    break;
                case "GameCamera":
                    GameCamera = cam;
                    break;
            }
        }
    }

    private void Update()
    {
        if (stageLeftTimeValue < 7)
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/ColorTogether/Countdown", 0.5f);
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;

        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            var endbox = hit.collider.GetComponent<EndClickedBox>();
            if (endCanClicked == true && endbox != null)
                endbox.Clicked();

            var nextColorBox = hit.collider.GetComponent<BoxDropper>();
            if (nextColorBox != null && !nextBoxDropDelay)
            {
                nextBoxDropDelay = true;
                nextColorBox.transform.DOPunchScale(Vector3.one * 0.2f, 0.7f, 10, 1);
                nextColorBox.NextDropBoxButton();
            }

            var ColorBox = hit.collider.GetComponent<ClickedFloor>();
            if (ColorBox != null && isInStage)
            {
                ColorBox.OnClicked();
                if (leftTeamScore >= 100)
                {
                    OnBoxWin(leftFloor.linkedBox);

                    DOVirtual.DelayedCall(0.5F, () =>
                    {
                        stageLeftTimeValue = 60;
                        TimerObject.gameObject.SetActive(false);
                    });
                    return;
                }
                else if (rightTeamScore >= 100)
                {
                    OnBoxWin(rightFloor.linkedBox);

                    DOVirtual.DelayedCall(0.5F, () =>
                    {
                        stageLeftTimeValue = 60;
                        TimerObject.gameObject.SetActive(false);
                    });

                    return;
                }
                return;
            }

            var rb = hit.collider.GetComponent<Rigidbody>(); // 부딪힌 물체에 Rigidbody 컴포넌트가 있는지 확인합니다.
            if (rb != null)
            {

                Vector3 forceDirection = rb.transform.position - hit.point + Vector3.up * 2;
                rb.AddForce(forceDirection.normalized * 100, ForceMode.Impulse);

                ClickedSound();

            }
        }   // 돌고래 게임 공 팅기는 로직

    }

    public void  OnBoxWin(ShowColorBox box)
    {
        if (!isInStage) return;

        leftBoxColor.SetActive(false);
        rightBoxColor.SetActive(false);
        PauseCountdown();
        
        isInStage = false;
        AudioClip victoryAudioClip = Resources.Load<AudioClip>("Audio/ColorTogether/Victory");
        Managers.Sound.Play(SoundManager.Sound.Effect, victoryAudioClip);

        Vector3 AEffectTransform1 = new Vector3(-169.21f, 78.9f, -39.3f);
        Vector3 AEffectTransform2 = new Vector3(-167.09f, 78.9f, -46.32f);

        Vector3 BEffectTransform1 = new Vector3(-156.44f, 78.9f, -41.96f);
        Vector3 BEffectTransform2 = new Vector3(-155.78f, 78.9f, -45.49f);

        narrationBG.sizeDelta = new Vector2(840, 141.78f);

        switch (box.name)
        {
            case "Box_Red?":
                NarrationALL(20, 20);
                if (prefab1.name == box.name)
                {
                    GameObject loseFx = Instantiate(ExplosionEffectPrefab, BEffectTransform2, Quaternion.identity);
                    GameObject loseFx2 = Instantiate(ExplosionEffectPrefab, BEffectTransform1, Quaternion.identity);
                    GameObject winFx = Instantiate(VictoryEffectPrefab, AEffectTransform1, Quaternion.Euler(0f, -57.381f, 0f));
                    GameObject winFx2 = Instantiate(VictoryEffectPrefab, AEffectTransform1, Quaternion.Euler(0f, -125.341f, 0f));
                    prefab2.SetActive(false);
                    
                }
                break;

            case "Box_Blue":
                NarrationALL(21, 21);
                if (prefab2.name == box.name)
                {
                    GameObject loseFx2 = Instantiate(ExplosionEffectPrefab, AEffectTransform2, Quaternion.identity);
                    GameObject loseFx = Instantiate(ExplosionEffectPrefab, AEffectTransform1, Quaternion.identity);
                    GameObject winFx = Instantiate(VictoryEffectPrefab, BEffectTransform1, Quaternion.Euler(0f, -57.381f, 0f));
                    GameObject winFx2 = Instantiate(VictoryEffectPrefab, BEffectTransform1, Quaternion.Euler(0f, -125.341f, 0f));

                    prefab1.SetActive(false);
                }
                break;

            case "Box_Green":
                NarrationALL(22, 22);
                if (prefab1.name == box.name)
                {
                    GameObject loseFx = Instantiate(ExplosionEffectPrefab, BEffectTransform1, Quaternion.identity);
                    GameObject loseFx2 = Instantiate(ExplosionEffectPrefab, BEffectTransform2, Quaternion.identity);
                    GameObject winFx = Instantiate(VictoryEffectPrefab, AEffectTransform1, Quaternion.Euler(0f, -57.381f, 0f));
                    GameObject winFx2 = Instantiate(VictoryEffectPrefab, AEffectTransform1, Quaternion.Euler(0f, -125.341f, 0f));
                    prefab2.SetActive(false);
                }
                break;

            case "Box_Yellow":
                NarrationALL(23, 23);
                if (prefab2.name == box.name)
                {
                    GameObject loseFx = Instantiate(ExplosionEffectPrefab, AEffectTransform1, Quaternion.identity);
                    GameObject loseFx2 = Instantiate(ExplosionEffectPrefab, AEffectTransform2, Quaternion.identity);
                    GameObject winFx = Instantiate(VictoryEffectPrefab, BEffectTransform1, Quaternion.Euler(0f, -57.381f, 0f));
                    GameObject winFx2 = Instantiate(VictoryEffectPrefab, BEffectTransform1, Quaternion.Euler(0f, -125.341f, 0f));

                    prefab1.SetActive(false);
                }
                break;

            case "Box_Orange":
                NarrationALL(24, 24);
                if (prefab2.name == box.name)
                {
                    GameObject loseFx = Instantiate(ExplosionEffectPrefab, AEffectTransform1, Quaternion.identity);
                    GameObject loseFx2 = Instantiate(ExplosionEffectPrefab, AEffectTransform2, Quaternion.identity);
                    GameObject winFx = Instantiate(VictoryEffectPrefab, BEffectTransform1, Quaternion.Euler(0f, -57.381f, 0f));
                    GameObject winFx2 = Instantiate(VictoryEffectPrefab, BEffectTransform1, Quaternion.Euler(0f, -125.341f, 0f));

                    prefab1.SetActive(false);
                }
                break;

            case "Box_Purple":
                NarrationALL(25, 25);
                if (prefab1.name == box.name)
                {
                    GameObject loseFx = Instantiate(ExplosionEffectPrefab, BEffectTransform1, Quaternion.identity);
                    GameObject loseFx2 = Instantiate(ExplosionEffectPrefab, BEffectTransform2, Quaternion.identity);
                    GameObject winFx = Instantiate(VictoryEffectPrefab, AEffectTransform1, Quaternion.Euler(0f, -57.381f, 0f));
                    GameObject winFx2 = Instantiate(VictoryEffectPrefab, AEffectTransform1, Quaternion.Euler(0f, -125.341f, 0f));
                    prefab2.SetActive(false);
                }
                break;
        }
        DOVirtual.DelayedCall(5f, () => ResetGame());


    }

    private void ShuffleSelectionOrder()
    {
        selectionOrder = new List<int>(colorPairs.Keys);
        for (int i = 0; i < selectionOrder.Count; i++)
        {
            int randomNum = Random.Range(i, selectionOrder.Count);
            (selectionOrder[i], selectionOrder[randomNum]) = (selectionOrder[randomNum], selectionOrder[i]);
        }
        currentIndex = 0;
    }

    public void SpawnRandomPair()
    {
        int selectedKey = selectionOrder[currentIndex];
        currentIndex++;

        (prefab1, prefab2) = colorPairs[selectedKey];

        prefab1.SetActive(true);
        prefab1.transform.DOPunchScale(Vector3.one * 0.2f, 0.7f, 10, 1);
        prefab2.SetActive(true);
        prefab2.transform.DOPunchScale(Vector3.one * 0.2f, 0.7f, 10, 1);

        leftBoxColor.SetActive(true);
        rightBoxColor.SetActive(true);

        switch (selectedKey)
        {
            case 0:
                leftBoxImage.sprite = colorTextImg[0];
                rightBoxImage.sprite = colorTextImg[1];
                break;
            case 1:
                leftBoxImage.sprite = colorTextImg[2];
                rightBoxImage.sprite = colorTextImg[3];
                break;
            case 2:
                leftBoxImage.sprite = colorTextImg[4];
                rightBoxImage.sprite = colorTextImg[5];
                break;
        }

        prefab1.transform.position = spawnPoint1.position;
        prefab2.transform.position = spawnPoint2.position;

        prefab1.transform.localScale = new Vector3(2.7f, 2, 1.8f);
        prefab2.transform.localScale = new Vector3(2.7f, 2, 1.8f);

        leftFloor.linkedBox = prefab1.GetComponent<ShowColorBox>();
        rightFloor.linkedBox = prefab2.GetComponent<ShowColorBox>();
    }

    private void ResetGame()
    {
        if (currentIndex < selectionOrder.Count && currentIndex > 0)
        {
            StopCountdown();
            narrationBG.sizeDelta = new Vector2(1100, 143);
            string AudioPath = "Audio/ColorTogether/audio_13";
            AudioClip clip = Resources.Load<AudioClip>(AudioPath);
            float cliplength = clip.length;
            Managers.Sound.Play(SoundManager.Sound.Narration, clip);
            narrationImgGameObject.SetActive(true);
            TakeTextImg("Image/ColorTogether/Img_" + 13);
            prefab1.SetActive(false);
            prefab2.SetActive(false);

            leftTeamScore = 0;
            rightTeamScore = 0;
            leftTeamScoreTmp.text = $"{leftTeamScore}";
            rightTeamScoreTmp.text = $"{rightTeamScore}";

            DOVirtual.DelayedCall(5f, () =>
            {
                StartCoroutine(PlaySprites());

                narrationImgGameObject.SetActive(false);
                SpawnRandomPair();
                
            });
            DOVirtual.DelayedCall(11f, () => 
            {
                TimerObject.gameObject.SetActive(true);
                StartCountdown();
                isInStage = true;
            });



        }
        else
        {
            narrationBG.sizeDelta = new Vector2(1200, 143);
            prefab1.SetActive(false);
            prefab2.SetActive(false);
            isInStage = false;
            narrationImgGameObject.SetActive(true);
            TakeTextImg("Image/ColorTogether/Img_" + 26);
            Managers.Sound.Play(SoundManager.Sound.Narration, "Audio/ColorTogether/audio_26");
            StartCamera.Priority = 20;
            GameCamera.Priority = 10;
            endShowBox.ShowPositionBox();
            leftTeamScoreObj.SetActive(false);
            rightTeamScoreObj.SetActive(false);
            return;
        } //게임 종료 시점


    }

    
    public void NarrationALL(int audioPath, int narrationImgPath)
    {
        string AudioPath = "Audio/ColorTogether/audio_";
        AudioClip clip = Resources.Load<AudioClip>(AudioPath + audioPath);
        float cliplength = clip.length;
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);
        narrationImgGameObject.SetActive(true);
        TakeTextImg("Image/ColorTogether/Img_" + narrationImgPath);
        DOVirtual.DelayedCall(cliplength + 2f, () =>
        {
            narrationImgGameObject.transform.localScale = Vector3.one;
            narrationImgGameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 10, 1);
            narrationImgGameObject.SetActive(false);
        });
    }

    public void TakeTextImg(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var ImageText = Resources.Load<Sprite>(path);
        narrationTextImage.sprite = ImageText;
        narrationImgGameObject.transform.localScale = Vector3.one;
        Sequence seq = DOTween.Sequence();
        seq.Append(narrationImgGameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 10, 1));
        seq.Append(narrationImgGameObject.transform.DOScale(Vector3.one, 0.5f));
        //narrationImgGameObject.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 10, 1);
    }


    public IEnumerator PlayNarrationCoroutine()
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/ColorTogether/audio_" + 9);
        narrationBG.sizeDelta = new Vector2(995, 143);
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);
        narrationImgGameObject.SetActive(true);
        TakeTextImg("Image/ColorTogether/Img_" + 9);
        yield return new WaitForSeconds(clip.length + 2f);

        narrationImgGameObject.SetActive(false);

        yield return StartCoroutine(PlaySprites());

        StartCountdown();

        leftTeamScoreObj.SetActive(true);
        rightTeamScoreObj.SetActive(true);
        leftTeamScoreTmp.text = "0";
        rightTeamScoreTmp.text = "0";

        narrationBG.transform.localScale = Vector3.one;
        narrationImgGameObject.SetActive(false);
        isInStage = true;

    }

    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] sprites; // 길이가 5인 배열

    private IEnumerator PlaySprites()
    {
        targetImage.gameObject.SetActive(true);

        for (int i = 0; i < sprites.Length; i++)
        {
            targetImage.sprite = sprites[i];
            targetImage.gameObject.transform.DOShakeScale(0.8f, 0.4f);
            Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/ColorTogether/audio_{50 + i}");
            yield return new WaitForSeconds(1.5f);
            
        }

        targetImage.gameObject.SetActive(false);
    }

    [SerializeField] private int startTime = 60;

    private Tween _countdownTween;
    private int _currentTime;

    public void StartCountdown()
    {
        _countdownTween?.Kill();

        _currentTime = startTime;
        TimerObject.gameObject.SetActive(true);

        _countdownTween = DOVirtual.Float(
                startTime,       
                0,                
                startTime,        
                value =>
                {
                    _currentTime = Mathf.CeilToInt(value);
                    stageLeftTime.text = _currentTime.ToString();
                }
            )
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                TimerObject.gameObject.SetActive(false);
                OnTimerEnd();
            });
    }

    public void PauseCountdown()
    {
        _countdownTween?.Pause();
    }


    public void StopCountdown()
    {
        _countdownTween?.Kill();
        TimerObject.gameObject.SetActive(false);
    }

    private void OnTimerEnd()
    {
        if (leftTeamScore > rightTeamScore)
            OnBoxWin(leftFloor.linkedBox);
        else if (rightTeamScore > leftTeamScore)
            OnBoxWin(rightFloor.linkedBox);

        Debug.Log("시간 종료!");
    }


    public void ClickedSound()
    {
        char randomChar = (char)('A' + Random.Range(0, 6));
        Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/ColorTogether/Click_{randomChar}");
    }

    public void ReloadScene()
    {
        RestartScene(delay: 20);
    }
    
}