using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;
using UnityEngine.UI;

public class VariousShape_GameManager : Base_GameManager
{
    [SerializeField] private GameObject flowerImg;
    [SerializeField] private GameObject squareImg;
    [SerializeField] private GameObject starImg;
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
    Vector3 originalFlowerEuler;

    Vector3 originalSquarePos;
    Vector3 originalSquareSize;
    Vector3 originalSquareEuler;

    Vector3 originalStarPos;
    Vector3 originalStarSize;
    Vector3 originalStarEuler;

    Vector3 originalCirclePos;
    Vector3 originalCircleSize;
    Vector3 originalCircleEuler;

    Vector3 originalTrianglePos;
    Vector3 originalTriangleSize;
    Vector3 originalTriangleEuler;

    Vector3 targetPostition = new Vector3(2.9f, 0, -0.591f);
    Vector3 shakeX = new Vector3(0, 0, 15f);

    [SerializeField] private bool introFunction = false; //인트로 소개 기능 체크
    [SerializeField] private GameObject introNextBtn;
    [SerializeField] private GameObject IntroStage;

    public GameStage gameStage;

    Sequence introSeq;

    public bool isintroducing = false;

    public bool isStageStart = false;

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        originalFlowerPos = flowerImg.transform.position;
        originalFlowerEuler = flowerImg.transform.eulerAngles;
        originalFlowerSize = flowerImg.transform.localScale;

        originalSquarePos = squareImg.transform.position;
        originalSquareEuler = squareImg.transform.eulerAngles;
        originalSquareSize = squareImg.transform.localScale;

        originalStarPos =  starImg.transform.position;
        originalStarEuler = starImg.transform.eulerAngles;
        originalStarSize = starImg.transform.localScale;

        originalCirclePos = circleImg.transform.position;
        originalCircleEuler = circleImg.transform.eulerAngles;
        originalCircleSize = circleImg.transform.localScale;

        originalTrianglePos =  triangleImg.transform.position;
        originalTriangleEuler = triangleImg.transform.eulerAngles;
        originalTriangleSize = triangleImg.transform.localScale;

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
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("친구들과 함께 다양한 모양을 찾아봐요!", "audio_0_친구들과_함께_다양한_모양을_찾아봐요_")))
        .AppendInterval(6f)
        .Append(circleImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(circleImg.transform.DOScale(originalCircleSize * 4f, moveSpeed))
        .Join(circleImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(circleImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, "VariousShape/Audio/BoingSound_1"))
        .AppendCallback(() => { Messenger.Default.Publish(new NarrationMessage("동그라미", "audio_1_동그라미_")); })
        .AppendInterval(2f)
        .Append(circleImg.transform.DOMove(originalCirclePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(circleImg.transform.DOScale(originalCircleSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(circleImg.transform.DOLocalRotate(originalCircleEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(3f)

        .Append(squareImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(squareImg.transform.DOScale(originalSquareSize * 4f, moveSpeed))
        .Join(squareImg.transform.DOLocalRotate(new Vector3(0, 0, -2f), moveSpeed))
        .Append(squareImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_2"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("네모", "audio_2_네모_")))
        .AppendInterval(2f)
        .Append(squareImg.transform.DOMove(originalSquarePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(squareImg.transform.DOScale(originalSquareSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(squareImg.transform.DOLocalRotate(originalSquareEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(3f)

        .Append(starImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(starImg.transform.DOScale(originalStarSize * 3.7f, moveSpeed))
        .Join(starImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(starImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_3"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("별", "audio_30_별")))
        .AppendInterval(2f)
        .Append(starImg.transform.DOMove(originalStarPos, moveSpeed).SetEase(Ease.InQuad))
        .Join(starImg.transform.DOScale(originalStarSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(starImg.transform.DOLocalRotate(originalStarEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(3f)

        .Append(flowerImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(flowerImg.transform.DOScale(originalFlowerSize * 4f, moveSpeed))
        .Join(flowerImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(flowerImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_2"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("꽃", "audio_4_꽃_")))
        .AppendInterval(2f)
        .Append(flowerImg.transform.DOMove(originalFlowerPos, moveSpeed).SetEase(Ease.InQuad))
        .Join(flowerImg.transform.DOScale(originalFlowerSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(flowerImg.transform.DOLocalRotate(originalFlowerEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(3f)

        .Append(triangleImg.transform.DOMove(targetPostition, moveSpeed))
        .Join(triangleImg.transform.DOScale(originalTriangleSize * 4f, moveSpeed))
        .Join(triangleImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(triangleImg.transform.DOShakeRotation(duration: 0.5f, strength: shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_1"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("세모", "audio_5_세모_")))
        .AppendInterval(2f)
        .Append(triangleImg.transform.DOMove(originalTrianglePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(triangleImg.transform.DOScale(originalTriangleSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(triangleImg.transform.DOLocalRotate(originalTriangleEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendInterval(3f)

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
                clickable.OnClicked(clickable.shapeType);
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
