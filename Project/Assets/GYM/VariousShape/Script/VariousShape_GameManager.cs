using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class VariousShape_GameManager : Base_GameManager
{
    [SerializeField] private GameObject flowerImg;
    [SerializeField] private GameObject squareImg;
    [SerializeField] private GameObject heartImg;
    [SerializeField] private GameObject circleImg;
    [SerializeField] private GameObject triangleImg;

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
          
    Vector3 originalHeartPos;
    Vector3 originalHeartSize;
          
    Vector3 originalCirclePos;
    Vector3 originalCircleSize;

    Vector3 originalTrianglePos;
    Vector3 originalTriangleSize;

    Vector3 targetPostition = new Vector3(2.9f, 0, -0.591f);
    Vector3 shakeX = new Vector3(0, 0, 15f);

    [SerializeField] private bool introFunction = false; //인트로 소개 기능 체크
    [SerializeField] private GameObject introNextBtn;
    [SerializeField] private GameObject IntroStage;

    public GameStage gameStage;

    Sequence introSeq;

    public bool isintroducing = false;

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

        //originalHeartPos =  heartImg.transform.position;
        //originalHeartSize = heartImg.transform.localScale;

        originalCirclePos = circleImg.transform.position;
        originalCircleSize = circleImg.transform.localScale;

        //originalTrianglePos =  triangleImg.transform.position;
        //originalTriangleSize = triangleImg.transform.localScale;

        Managers.Sound.Play(SoundManager.Sound.Bgm, "VariousShape/Audio/BGAudio");

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
        introSeq = DOTween.Sequence()
         //.AppendCallback(() => 나레이션 기능) 
         //.AppendInterval(나레이션시간)
         //.AppendCallback(() => 나레이션 기능) 
         //.AppendInterval(나레이션시간)
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("친구들과 함께 다양한 모양을 찾아봐요!", "audio_0_친구들과_함께_다양한_모양을_찾아봐요_")))
        .AppendInterval(6f)
        .Append(circleImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(circleImg.transform.DOScale(originalCircleSize, moveSpeed))
        .Append(circleImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .AppendCallback(() => { Messenger.Default.Publish(new NarrationMessage("동그라미", "audio_1_동그라미_")); })
        .AppendInterval(2f)
        .Append(circleImg.transform.DOMove(originalCirclePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(circleImg.transform.DOScale(originalCircleSize, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(5f)

        .Append(squareImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(squareImg.transform.DOScale(originalSquareSize * 6f, moveSpeed))
        .Append(squareImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("네모", "audio_2_네모_")))
        .AppendInterval(2f)
        .Append(squareImg.transform.DOMove(originalSquarePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(squareImg.transform.DOScale(originalSquareSize, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(5f)

        .Append(heartImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(heartImg.transform.DOScale(originalHeartSize * 6f, moveSpeed))
        .Append(heartImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("하트", "audio_3_하트_")))
        .AppendInterval(2f)
        .Append(heartImg.transform.DOMove(originalHeartPos, moveSpeed).SetEase(Ease.InQuad))
        .Join(heartImg.transform.DOScale(originalHeartSize, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(5f)

        .Append(flowerImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(flowerImg.transform.DOScale(originalFlowerSize * 6.2f, moveSpeed))
        .Append(flowerImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("꽃", "audio_4_꽃_")))
        .AppendInterval(2f)
        .Append(flowerImg.transform.DOMove(originalFlowerPos, moveSpeed).SetEase(Ease.InQuad))
        .Join(flowerImg.transform.DOScale(originalFlowerSize, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(3f)
        //.Append(triangleImg.transform.DOMove(targetPostition, moveSpeed))
        //.Join(triangleImg.transform.DOScale(originalTriangleSize * 6.2f, moveSpeed))
        //.Append(triangleImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        //.AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("세모", "audio_5_세모_")))
        //.AppendInterval(2f)
        //.Append(triangleImg.transform.DOMove(originalTrianglePos, moveSpeed).SetEase(Ease.InQuad))
        //.Join(triangleImg.transform.DOScale(originalTriangleSize, moveSpeed).SetEase(Ease.InQuad))
        //.AppendInterval(5f)

        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("이제부터 모양 친구들과 놀아볼까요~", "audio_6_이제부터_모양_친구들과_놀아볼까요_")))
        .AppendInterval(3f)
        .AppendCallback(() =>                   //게임스테이지 시작
        {
            IntroStage.SetActive(false);
            gameStage.OnGameStart();
        });

    }

    public override void OnRaySynced()
    {
        if (!isStartButtonClicked) return;

        GameManager_Hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in GameManager_Hits)
        {
            if (hit.collider.TryGetComponent<ClickableMover>(out var clickable))
            {
                clickable.OnClicked();
            }
            if (hit.collider.TryGetComponent<IntroduceSelf>(out var introduce))
            {
                if (!isintroducing)
                    introduce.IntroduceSelfShape();
            }
        }
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

    protected override void OnDestroy()
    {
        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;
        base.OnDestroy();
    }

}
