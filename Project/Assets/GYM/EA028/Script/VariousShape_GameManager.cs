using System.Collections.Generic;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;

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

    private Vector3 _originalFlowerPos;
    private Vector3 _originalFlowerSize;
    private Vector3 _originalFlowerEuler;
 
    private Vector3 _originalSquarePos;
    private Vector3 _originalSquareSize;
    private Vector3 _originalSquareEuler;

    private Vector3 _originalStarPos;
    private Vector3 _originalStarSize;
    private Vector3 _originalStarEuler;

    private Vector3 _originalCirclePos;
    private Vector3 _originalCircleSize;
    private Vector3 _originalCircleEuler;

    private Vector3 _originalTrianglePos;
    private Vector3 _originalTriangleSize;
    private Vector3 _originalTriangleEuler;

    private readonly Vector3 _targetPosition = new Vector3(2.9f, 0, -0.591f);
    private readonly Vector3 _shakeX = new Vector3(0, 0, 15f);

    [SerializeField] private bool introFunction = false; //인트로 소개 기능 체크
    [SerializeField] private GameObject introNextBtn;
    [SerializeField] private GameObject introStage;

    public GameStage gameStage;

    Sequence introSeq;

    public bool isIntroducing = false;

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
        //BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        _particlePool = new List<ParticleSystem>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var ps = Instantiate(workParticlePrefab, transform);
            ps.gameObject.SetActive(false);
            _particlePool.Add(ps);
        }

        _originalFlowerPos = flowerImg.transform.position;
        _originalFlowerEuler = flowerImg.transform.eulerAngles;
        _originalFlowerSize = flowerImg.transform.localScale;

        _originalSquarePos = squareImg.transform.position;
        _originalSquareEuler = squareImg.transform.eulerAngles;
        _originalSquareSize = squareImg.transform.localScale;

        _originalStarPos =  starImg.transform.position;
        _originalStarEuler = starImg.transform.eulerAngles;
        _originalStarSize = starImg.transform.localScale;

        _originalCirclePos = circleImg.transform.position;
        _originalCircleEuler = circleImg.transform.eulerAngles;
        _originalCircleSize = circleImg.transform.localScale;

        _originalTrianglePos =  triangleImg.transform.position;
        _originalTriangleEuler = triangleImg.transform.eulerAngles;
        _originalTriangleSize = triangleImg.transform.localScale;

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
        DOVirtual.DelayedCall(0.1f, () =>
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
        .AppendInterval(5f)
        .AppendCallback(() => circleImg.transform.DOKill())
        .Append(circleImg.transform.DOMove(_targetPosition, moveSpeed))
        .Join(circleImg.transform.DOScale(_originalCircleSize * 4f, moveSpeed))
        .Join(circleImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(circleImg.transform.DOShakeRotation(duration: 0.5f, strength: _shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, "VariousShape/Audio/BoingSound_1"))
        .AppendCallback(() => { Messenger.Default.Publish(new NarrationMessage("동그라미", "audio_1_동그라미_")); })
        .AppendInterval(2f)
        .Append(circleImg.transform.DOMove(_originalCirclePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(circleImg.transform.DOScale(_originalCircleSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(circleImg.transform.DOLocalRotate(_originalCircleEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendCallback(() =>
        {
            circleImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ5 - 10);
            circleImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(2f)

        .AppendCallback(() => squareImg.transform.DOKill())
        .Append(squareImg.transform.DOMove(_targetPosition, moveSpeed))
        .Join(squareImg.transform.DOScale(_originalSquareSize * 4f, moveSpeed))
        .Join(squareImg.transform.DOLocalRotate(new Vector3(0, 0, -2f), moveSpeed))
        .Append(squareImg.transform.DOShakeRotation(duration: 0.5f, strength: _shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_2"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("네모", "audio_2_네모_")))
        .AppendInterval(2f)
        .Append(squareImg.transform.DOMove(_originalSquarePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(squareImg.transform.DOScale(_originalSquareSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(squareImg.transform.DOLocalRotate(_originalSquareEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendCallback(() =>
        {
            squareImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ2 - 10);
            squareImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(0.7f)

        .AppendCallback(() => starImg.transform.DOKill())
        .Append(starImg.transform.DOMove(_targetPosition, moveSpeed))
        .Join(starImg.transform.DOScale(_originalStarSize * 3.7f, moveSpeed))
        .Join(starImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(starImg.transform.DOShakeRotation(duration: 0.5f, strength: _shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_3"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("별", "audio_30_별")))
        .AppendInterval(2f)
        .Append(starImg.transform.DOMove(_originalStarPos, moveSpeed).SetEase(Ease.InQuad))
        .Join(starImg.transform.DOScale(_originalStarSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(starImg.transform.DOLocalRotate(_originalStarEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendCallback(() =>
        {
            starImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ3 - 10);
            starImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(1.2f)

        .AppendCallback(() => flowerImg.transform.DOKill())
        .Append(flowerImg.transform.DOMove(_targetPosition, moveSpeed))
        .Join(flowerImg.transform.DOScale(_originalFlowerSize * 4f, moveSpeed))
        .Join(flowerImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(flowerImg.transform.DOShakeRotation(duration: 0.5f, strength: _shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_2"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("꽃", "audio_4_꽃_")))
        .AppendInterval(2f)
        .Append(flowerImg.transform.DOMove(_originalFlowerPos, moveSpeed).SetEase(Ease.InQuad))
        .Join(flowerImg.transform.DOScale(_originalFlowerSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(flowerImg.transform.DOLocalRotate(_originalFlowerEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendCallback(() =>
        {
            flowerImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ1 - 10);
            flowerImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(1.2f)

        .AppendCallback(() => triangleImg.transform.DOKill())
        .Append(triangleImg.transform.DOMove(_targetPosition, moveSpeed))
        .Join(triangleImg.transform.DOScale(_originalTriangleSize * 4f, moveSpeed))
        .Join(triangleImg.transform.DOLocalRotate(Vector3.zero, moveSpeed))
        .Append(triangleImg.transform.DOShakeRotation(duration: 0.5f, strength: _shakeX))
        .JoinCallback(() => Managers.Sound.Play(SoundManager.Sound.Effect, $"VariousShape/Audio/BoingSound_1"))
        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("세모", "audio_5_세모_")))
        .AppendInterval(2f)
        .Append(triangleImg.transform.DOMove(_originalTrianglePos, moveSpeed).SetEase(Ease.InQuad))
        .Join(triangleImg.transform.DOScale(_originalTriangleSize, moveSpeed).SetEase(Ease.InQuad))
        .Join(triangleImg.transform.DOLocalRotate(_originalTriangleEuler, moveSpeed).SetEase(Ease.InQuad))
        .AppendCallback(() =>
        {
            triangleImg.transform.localRotation = Quaternion.Euler(0f, 0f, startZ4 - 10);
            triangleImg.transform
                .DOLocalRotate(new Vector3(0f, 0f, 10 * 2f), 1, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine)
                .SetLoops(40, LoopType.Yoyo);
        })
        .AppendInterval(1f)

        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("이제부터 모양 친구들과 놀아볼까요~", "audio_6_이제부터_모양_친구들과_놀아볼까요_")))
        .AppendInterval(3f)
        .AppendCallback(() => introStage.transform.DOScale(3f, 0.5f).SetEase(Ease.Linear))
        .AppendInterval(0.5f)
        .AppendCallback(() =>                   //게임스테이지 시작
        {
            introStage.SetActive(false);
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
                if (!isIntroducing)
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
    

    // private void ShapeAni(Image obj, int value)
    // {
    //     int randomValue = Random.Range(0, 2);
    //
    //     var rect = obj.rectTransform;
    //     float originalY = rect.localPosition.y;
    //
    //     var targetRotation = rect.localEulerAngles;
    //     targetRotation.z += 20f;
    //
    //     switch (value)
    //     {
    //         case 0:
    //             //위아래로 둥실둥실 애니메이션
    //             DOVirtual.DelayedCall(randomValue, () => rect.DOLocalMoveY(originalY + 80f, 1.5f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo));
    //             
    //             break;
    //         case 1:
    //             //좌우로 흔드는 애니메이션
    //             DOVirtual.DelayedCall(randomValue, () => rect.DOLocalRotate(targetRotation, 1.5f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo));
    //
    //             break;
    //         case 2:
    //             //미정
    //
    //             break;
    //
    //
    //     }
    // }

    // private void ShapeAni(Image obj)
    // {
    //     int value = Random.Range(0, 3);
    //
    //     var rect = obj.rectTransform;
    //     float originalY = rect.localPosition.y;
    //
    //     var targetRotation = rect.localEulerAngles;
    //     targetRotation.z += 20f;
    //
    //     switch (value)
    //     {
    //         case 0:
    //             //위아래로 둥실둥실 애니메이션
    //             rect.DOLocalMoveY(originalY + 80f, 2f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
    //
    //             break;
    //         case 1:
    //             //좌우로 흔드는 애니메이션
    //             rect.DOLocalRotate(targetRotation, 1f).SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
    //
    //             break;
    //         case 2:
    //             //미정
    //
    //             break;
    //
    //
    //     }
    // }

    protected override void OnDestroy()
    {
        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;
        base.OnDestroy();
    }

}
