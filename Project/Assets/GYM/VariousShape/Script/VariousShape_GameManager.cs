using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEditor;
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

    private float startZ1;
    private float startZ2;
    private float startZ3;
    private float startZ4;
    private float startZ5;

    public ParticleSystem workParticlePrefab;
    public int poolSize = 10;
    private List<ParticleSystem> _particlePool;


    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        _particlePool = new List<ParticleSystem>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var ps = Instantiate(workParticlePrefab, transform);
            ps.gameObject.SetActive(false);
            _particlePool.Add(ps);
        }

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

        startZ1 = flowerImg.transform.localEulerAngles.z;
        flowerImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ1 - 10);
        flowerImg.transform
            .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine)
            .SetLoops(40, LoopType.Yoyo);
        startZ2 = squareImg.transform.localEulerAngles.z;
        DOVirtual.DelayedCall(0.3f, () =>
        {
            squareImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ2 - 10);
            squareImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        });
       
        startZ3 = starImg.transform.localEulerAngles.z;
        DOVirtual.DelayedCall(0.6f, () =>
        {
            starImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ3 - 10);
        starImg.transform
            .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine)
            .SetLoops(40, LoopType.Yoyo);
        });
        startZ4 = triangleImg.transform.localEulerAngles.z;
        DOVirtual.DelayedCall(0.9f, () =>
        {
            triangleImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ4 - 10);
        triangleImg.transform
            .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine)
            .SetLoops(40, LoopType.Yoyo);
        });
        startZ5 = circleImg.transform.localEulerAngles.z;
        DOVirtual.DelayedCall(0.5f, () =>
        {
            circleImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ5 - 10);
        circleImg.transform
            .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine)
            .SetLoops(40, LoopType.Yoyo);
        });


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
        .AppendCallback(() => circleImg.transform.DOKill())
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
        .AppendCallback(() =>
        {
            circleImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ5 - 10);
            circleImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(3f)

        .AppendCallback(() => squareImg.transform.DOKill())
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
        .AppendCallback(() =>
        {
            squareImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ2 - 10);
            squareImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(3f)

        .AppendCallback(() => starImg.transform.DOKill())
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
        .AppendCallback(() =>
        {
            starImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ3 - 10);
            starImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(3f)

        .AppendCallback(() => flowerImg.transform.DOKill())
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
        .AppendCallback(() =>
        {
            flowerImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ1 - 10);
            flowerImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(3f)

        .AppendCallback(() => triangleImg.transform.DOKill())
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
        .AppendCallback(() =>
        {
            triangleImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ4 - 10);
            triangleImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(3f)

        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("이제부터 모양 친구들과 놀아볼까요~", "audio_6_이제부터_모양_친구들과_놀아볼까요_")))
        .AppendInterval(3f)
        .AppendCallback(() => IntroStage.transform.DOScale(3f, 0.5f).SetEase(Ease.Linear))
        .AppendInterval(0.5f)
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
            if (hit.collider.CompareTag("toWork"))
            {
                char randomLetter = (char)('A' + Random.Range(0, 6));
                Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/Click_{randomLetter}");

                var ps = _particlePool.Find(x => !x.isPlaying);
                if (ps != null)
                {
                    ps.transform.position = hit.point;
                    ps.gameObject.SetActive(true);
                    ps.Play();
                }
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
