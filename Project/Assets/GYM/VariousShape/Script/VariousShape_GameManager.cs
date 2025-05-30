using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VariousShape_GameManager : Base_GameManager
{
    [SerializeField] private GameObject flowerImg;
    [SerializeField] private GameObject squareImg;
    [SerializeField] private GameObject starImg;
    [SerializeField] private GameObject circleImg;

    //[SerializeField] private int flowerValue;
    //[SerializeField] private int squareValue;
    //[SerializeField] private int starValue;
    //[SerializeField] private int circleValue;

    [SerializeField]
    private float moveSpeed;

    Vector3 originalFlowerPos;
    Vector3 originalFlowerSize;

    Vector3 originalSquarePos;
    Vector3 originalSquareSize;
          
    Vector3 originalStarPos;
    Vector3 originalStarSize;
          
    Vector3 originalCirclePos;
    Vector3 originalCircleSize;

    Vector3 targetPostition = new Vector3(2.9f, 0, -0.591f);
    Vector3 shakeX = new Vector3(0, 0, 15f);

    [SerializeField] private bool introFunction = false; //인트로 소개 기능 체크
    [SerializeField] private GameObject introNextBtn;
    [SerializeField] private GameObject IntroStage;

    public GameStage gameStage;

    Sequence introSeq;

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        originalFlowerPos = flowerImg.transform.position;
        originalFlowerSize = flowerImg.transform.localScale;

        originalSquarePos = squareImg.transform.position;
        originalSquareSize = squareImg.transform.localScale;

        originalStarPos = starImg.transform.position;
        originalStarSize = starImg.transform.localScale;

        originalCirclePos = circleImg.transform.position;
        originalCircleSize = circleImg.transform.localScale;

        //Managers.Sound.Play(SoundManager.Sound.Bgm, "CrossRoad/Audio/CrossRoad_BGM");

        //if (mainCamera != null)
        //{
        //    mainCamera.rect = new Rect(
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
        //        XmlManager.Instance.ScreenSize,
        //        XmlManager.Instance.ScreenSize
        //    );
        //}

        //if (UICamera != null)
        //{
        //    UICamera.rect = new Rect(
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
        //        XmlManager.Instance.ScreenSize,
        //        XmlManager.Instance.ScreenSize
        //    );
        //}

        UI_InScene_StartBtn.onGameStartBtnShut += StartGame;
    }

    private void StartGame()
    {
        //Messenger.Default.Publish(new NarrationMessage("지금처럼 신호를 보고 난 후\n좌우도 꼭 살피고 안전하게 건너야해", "7_지금처럼_신호를_보고_난_후_좌우도_꼭_살피고_안전하게_건너야해_"));

        //ShapeAni(flowerImg, 1);
        //ShapeAni(squareImg, 0);
        //ShapeAni(starImg, 1);
        //ShapeAni(circleImg, 0);

        ////이걸 스타트나 init에 생성하고 pause걸어 놓고 startbtn에서 play
        //introSeq = DOTween.Sequence()
        ////.AppendCallback(() => 나레이션 기능) 
        ////.AppendInterval(나레이션시간)
        ////.AppendCallback(() => 나레이션 기능) 
        ////.AppendInterval(나레이션시간)
        //.Append(flowerImg.transform.DOMove(targetPostition, moveSpeed))
        //.Join(flowerImg.transform.DOScale(originalFlowerSize * 4.8f, moveSpeed))
        //.Append(flowerImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        //.AppendCallback(() => { })
        //.AppendInterval(1f)
        //.Append(flowerImg.transform.DOMove(originalFlowerPos, moveSpeed).SetEase(Ease.InQuad))
        //.Join(flowerImg.transform.DOScale(originalFlowerSize, moveSpeed).SetEase(Ease.InQuad))
        //.AppendInterval(1f)
        ////.AppendCallback(() => introSeq.Pause())

        //.Append(squareImg.transform.DOMove(targetPostition, moveSpeed))
        //.Join(squareImg.transform.DOScale(originalSquareSize * 6f, moveSpeed))
        //.Append(squareImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        //.AppendCallback(() => { })
        //.AppendInterval(1f)
        //.Append(squareImg.transform.DOMove(originalSquarePos, moveSpeed).SetEase(Ease.InQuad))
        //.Join(squareImg.transform.DOScale(originalSquareSize, moveSpeed).SetEase(Ease.InQuad))
        //.AppendInterval(1f)
        ////.AppendCallback(() => introSeq.Pause())

        //.Append(starImg.transform.DOMove(targetPostition, moveSpeed))
        //.Join(starImg.transform.DOScale(originalStarSize * 4.7f, moveSpeed))
        //.Append(starImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        //.AppendCallback(() => { })
        //.AppendInterval(1f)
        //.Append(starImg.transform.DOMove(originalStarPos, moveSpeed).SetEase(Ease.InQuad))
        //.Join(starImg.transform.DOScale(originalStarSize, moveSpeed).SetEase(Ease.InQuad))
        //.AppendInterval(1)
        ////.AppendCallback(() => introSeq.Pause())

        //.Append(circleImg.transform.DOMove(targetPostition, moveSpeed))
        //.Join(circleImg.transform.DOScale(originalCircleSize * 6.2f, moveSpeed))
        //.Append(circleImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        //.AppendCallback(() => { })
        //.AppendInterval(1f)
        //.Append(circleImg.transform.DOMove(originalCirclePos, moveSpeed).SetEase(Ease.InQuad))
        //.Join(circleImg.transform.DOScale(originalCircleSize, moveSpeed).SetEase(Ease.InQuad))
        //.AppendInterval(1f)
        ////.AppendCallback(() => introSeq.Pause())

        //.AppendCallback(() =>                   //게임스테이지 시작
        //{
        //    Debug.Log("시퀀스 종료");
        //    IntroStage.SetActive(false);
        //    
        //});

        SquareStageStart();
    }


    public override void OnRaySynced()
    {
        Debug.Log("레이싱크작동");
        if (!isStartButtonClicked)
        {
            Logger.Log("StartBtn Should be Clicked");

            return;
        }



        GameManager_Hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in GameManager_Hits)
        {
            Debug.Log($"클릭 객체 {hit.transform.gameObject.name}");
            if (hit.collider.CompareTag("toWork"))
            {
                Debug.Log("태그로 감지됨");
                if (hit.collider.TryGetComponent<ClickableMover>(out var clickable))
                {
                    Debug.Log("클릭어블클래스가져옴");
                    //gameStage.OnGameStart();
                    clickable.OnClicked();
                }
            }
        }
        if (GameManager_Hits.Length == 0) Logger.ContentTestLog("클릭된 객체 hit 없음");
    }

    public void NextStepIntro()
    {
        Debug.Log("버튼 클릭 중");
        introSeq.Play();
    }

    public void SquareStageStart()
    {
        gameStage.gameObject.SetActive(true);
        gameStage.StartSquareStage();
    }

    private void ShapeAni(Image obj, int value)
    {
        int randomvalue = Random.Range(0, 2);

        RectTransform rect = obj.rectTransform;
        float originalY = rect.localPosition.y;

        Vector3 targetRotation = rect.localEulerAngles;
        targetRotation.z += 20f;

        switch (value)
        {
            case 0:
                //위아래로 둥실둥실 애니메이션
                DOVirtual.DelayedCall(randomvalue, () => rect.DOLocalMoveY(originalY + 80f, 1.5f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo));
                
                break;
            case 1:
                //좌우로 흔드는 애니메이션
                DOVirtual.DelayedCall(randomvalue, () => rect.DOLocalRotate(targetRotation, 1.5f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo));

                break;
            case 2:
                //미정

                break;


        }
    }

    private void ShapeAni(Image obj)
    {
        int value = Random.Range(0, 3);

        RectTransform rect = obj.rectTransform;
        float originalY = rect.localPosition.y;

        Vector3 targetRotation = rect.localEulerAngles;
        targetRotation.z += 20f;

        switch (value)
        {
            case 0:
                //위아래로 둥실둥실 애니메이션
                rect.DOLocalMoveY(originalY + 80f, 2f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);

                break;
            case 1:
                //좌우로 흔드는 애니메이션
                rect.DOLocalRotate(targetRotation, 1f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);

                break;
            case 2:
                //미정

                break;


        }
    }






}
