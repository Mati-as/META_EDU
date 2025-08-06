using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class SS010_GameManager : Ex_BaseGameManager
{
    private enum GameSequence
    {
        Intro,
        FirstGamePlay,
        FirstGameTransition,
        SecondGamePlay,
        SecondGameTransition,
        ThirdGamePlay,
        Outro
    }

    private GameSequence _currentSequence;

    private enum Cameras
    {
        Camera1,
        Camera2,
    }

    private enum Objects
    {
        LogBridge,
        Water1,
        Water2,
        WaterClickEffect,
        Lava,
        LavaClickEffect,
        Crocodile,
        Avatar,
        Rocks1,
        Rocks2,
        
    }

    private enum Particle
    {
        Victory1,
        Victory2
    }

    private SS010_UIManager _uiManager;
    private Vector3 _clickEffectPos;
    private AudioClip[] _clickClips;
    private AudioClip _victorySound;

    private int footStepAudioCount = 0;
    private int woodCreakAudioCount = 0;
    private AudioClip[] _footStepAudioClips;
    private AudioClip[] _woodCreakAudioClips;

    public float gameTimer = 30;
    public bool playingGame = false;

    private GameObject logBridge;
    private GameObject water1;
    private GameObject water2;

    private List<GameObject> rocks1 = new(8);
    private List<GameObject> rocks2 = new(8);
    
    private GameObject lava;

    private Transform waterEffectPoolTransform;

    private GameObject crocodile;

    private Vector3 RightStartPos;
    private Vector3 LeftStartPos;
    private GameObject rightChild;
    private Animator rightChildAnimator;
    
    protected override void Init()
    {
        BindObject(typeof(Objects));
        Bind<ParticleSystem>(typeof(Particle));
        Bind<CinemachineVirtualCamera>(typeof(Cameras));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<SS010_UIManager>();

        psResourcePath = "SS010/Asset/Fx_Click"; //주소변경

        SetPool(); //나무 클릭 이펙트 용 풀

        // Get<CinemachineVirtualCamera>(0).Priority = 12; //카메라들 우선순위 초기화
        // for (int i = 1; i <= 1; i++)
        //     Get<CinemachineVirtualCamera>(i).Priority = 10;

        Managers.Sound.Play(SoundManager.Sound.Bgm, ""); //배경음 추후 삽입

        _clickClips = new AudioClip[5]; //클릭 오디오 캐싱 
        for (int i = 0; i < _clickClips.Length; i++)
            _clickClips[i] = Resources.Load<AudioClip>($"SS010/Audio/Click_{(char)('A' + i)}");

        _footStepAudioClips = new AudioClip[11]; //발자국 오디오 캐싱 
        for (int i = 0; i < _footStepAudioClips.Length; i++)
            _footStepAudioClips[i] = Resources.Load<AudioClip>($"SS010/Audio/FootStepSound{i + 1}");

        _woodCreakAudioClips = new AudioClip[8]; //나무 삐걱거리는 오디오 캐싱
        for (int i = 0; i < _woodCreakAudioClips.Length; i++)
            _woodCreakAudioClips[i] = Resources.Load<AudioClip>($"SS010/Audio/WoodCreak{i + 1}");

        _victorySound = Resources.Load<AudioClip>("SS010/Audio/audio_Victory");

        logBridge = GetObject((int)Objects.LogBridge);
        water1 = GetObject((int)Objects.Water1);
        water2 = GetObject((int)Objects.Water2);

        var rocksParent1 = GetObject((int)Objects.Rocks1).transform;
        foreach (GameObject obj in rocksParent1)
        {
            rocks1.Add(obj);
        }
        
        var rocksParent2 = GetObject((int)Objects.Rocks2).transform;
        foreach (GameObject obj in rocksParent2)
        {
            rocks2.Add(obj);
        }
        
        lava = GetObject((int)Objects.Lava);

        waterEffectPoolTransform = GetObject((int)Objects.WaterClickEffect).transform;

        for (int i = 0; i < waterEffectPoolTransform.childCount; i++)
            waterEffectPoolTransform.GetChild(i).gameObject.SetActive(false);

        crocodile = GetObject((int)Objects.Crocodile);
        crocodile.transform.position = new Vector3(-11, -7f, 9);

        RightStartPos = GetObject((int)Objects.Avatar).transform.GetChild(0).position;
        LeftStartPos = GetObject((int)Objects.Avatar).transform.GetChild(1).position;

        rightChild = GetObject((int)Objects.Avatar).transform.GetChild(2).gameObject;
        rightChildAnimator = rightChild.GetComponent<Animator>();
        rightChild.SetActive(false);
        
        
    }

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();

        StartContent();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UI_InScene_StartBtn.onGameStartBtnShut -= StartContent;
    }

    private void StartContent()
    {
        //if (_stage == MainSeq.OnStart)
        //ChangeStage(GameSequence.Intro);
        ChangeStage(GameSequence.FirstGamePlay);
    }

    private void ChangeStage(GameSequence next)
    {
        _currentSequence = next;
        switch (next)
        {
            case GameSequence.Intro: OnIntroStage(); break;
            case GameSequence.FirstGamePlay: OnFirstGameStage(); break;
            case GameSequence.FirstGameTransition: OnFirstTransitionStage(); break;
            case GameSequence.SecondGamePlay: OnSecondGameStage(); break;
            case GameSequence.SecondGameTransition: OnSecondTransitionStage(); break;
            case GameSequence.ThirdGamePlay: OnThirdGameStage(); break;
            case GameSequence.Outro: OnOutroStage(); break;
        }

        Logger.Log($"{next}스테이지로 변경");
    }
    

    #region 각 스테이지 별 진행 시퀀스

    private void OnIntroStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("형님처럼 할 수 잇어요!", 3f, narrationPath: "SS010/Audio/audio_0_형님처럼_할_수_잇어요");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("다리를 건너야 해요!", 3f, narrationPath: "SS010/Audio/audio_1_다리를_건너야_해요");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("형님처럼 씩씩하게 외나무 다리를 건너볼까요?", 3f, narrationPath: "SS010/Audio/audio_2_형님처럼_씩씩하게_외나무_다리를_건너볼까요_");
                //아이 아바타가 화면에 한명 등장하고 화면을 올려다보며 손을 흔드는 구도
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                ChangeStage(GameSequence.FirstGamePlay);
            })
            ;

    }

    private void OnFirstGameStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("형님을 따라 두 팔을 벌리고 외나무 다리를 건너주세요!", 3f, narrationPath: "SS010/Audio/audio_3_형님을_따라_두_팔을_벌리고_외나무_다리를_건너주세요");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("형님이 먼저 외나무 다리를 건너는 것을 보여줄 거에요!", 3f, narrationPath: "SS010/Audio/audio_4_형님이_먼저_외나무_다리를_건너는_것을_보여줄거에요");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //형님 아바타가 길을 건너감 
                rightChild.SetActive(true);
                rightChild.transform.position = RightStartPos;
                rightChild.transform.DOMove(LeftStartPos, 4f).OnComplete(() => rightChild.SetActive(false));
                //rightChildAnimator.SetBool("");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.PlayReadyAndStart(() =>
                {
                    _uiManager.StartTimer(() => ChangeStage(GameSequence.FirstGameTransition)); //타이머 등장 연출관련 시간 문제 이슈
                    playingGame = true;
                });
            })
            ;

        if (_uiManager._currentGameTimer % 11 == 0 && _uiManager._currentGameTimer != 0)        //위치 수정 예정
        {
              Managers.Sound.Play(SoundManager.Sound.Narration, "SS010/Audio/audio_5_친구들_한줄로_천천히_외나무_다리를_건너주세요");
        }

    }

    private void OnFirstTransitionStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                logBridgeClickedCount = 0;
                waterClickedCount = 0;
                
                
                //용암 게임 장소로 이동
                _uiManager.PopInstructionUIFromScaleZero("다 건넜어요! 형님처럼 잘 할 수 있어요!", 3f, narrationPath: "SS010/Audio/audio_7_다_건넜어요__형님처럼_잘_할_수_있어요_");
                //아바타 성공 애니메이션 재생
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("바닥에 용암이 흐르고 있어요!", 3f, narrationPath: "SS010/Audio/audio_8_바닥에_용암이_흐르고_있어요_");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("다시 한번 형님처럼 씩씩하게 징검다리를 건너볼까요?", 3f, narrationPath: "SS010/Audio/audio_9_다시_한번_형님처럼_씩씩하게_징검다리를_건너_볼까요_");
                //아이 아바타가 화면에 한명 등장하고 화면을 올려다보며 손을 흔드는 구도
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                ChangeStage(GameSequence.SecondGamePlay);
            })
            ;

    }

    private void OnSecondGameStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("이번엔 두발을 모아 점프해 징검다리를 건너주세요!", 3f, narrationPath: "SS010/Audio/audio_10_형님을_따라_이번엔_두발을_모아_점프해_징검_다리를_건너주세요_");
                //(형님이 먼저 징검 다리를 건너는 것을 보여줄 거에요)
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("형님이 먼저 징검 다리를 건너는 것을 보여줄 거에요!", 3f, narrationPath: "SS010/Audio/audio_11_형님이_먼저_징검다리를_건너는_것을_보여줄거에요_");
                //형님 아바타가 길을 건너감 두발을 모아 점프하며
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //준비~시작
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.PlayReadyAndStart(() =>
                {
                    _uiManager.StartTimer(() => ChangeStage(GameSequence.SecondGameTransition)); //타이머 등장 연출관련 시간 문제 이슈
                    playingGame = true;
                });
            })
            ;


        if (_uiManager._currentGameTimer % 11 == 0 && _uiManager._currentGameTimer != 0) //위치 수정 예정
        {
            Managers.Sound.Play(SoundManager.Sound.Narration, "SS010/Audio/audio_12_친구들_한줄로_천천히_징검_다리를_건너주세요");
        }

    }

    private void OnSecondTransitionStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                rocksClickedCount = 0;
                lavaClickedCount = 0;
                
                _uiManager.PopInstructionUIFromScaleZero("다 건넜어요! 형님처럼 잘 할 수 있어요!", 3f, narrationPath: "SS010/Audio/audio_13_다_건넜어요__형님처럼_잘_할_수_있어요_");
                //아바타 성공 애니메이션 재생
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                ChangeStage(GameSequence.ThirdGamePlay);
            })
            ;

    }

    private void OnThirdGameStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("이번엔 한발로 점프해 징검다리를 건너주세요!", 3f, narrationPath: "SS010/Audio/audio_14_형님을_따라_이번엔_한발로_점프해_징건마디를_건너주세요_");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "SS010/Audio/audio_15_형님이_먼저_징검다리를_건너는_것을_보여줄거에요_");
                //형님 아바타가 길을 건너감 한발로 점프하며
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //준비~시작
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.PlayReadyAndStart(() =>
                {
                    _uiManager.StartTimer(() => ChangeStage(GameSequence.Outro)); //타이머 등장 연출관련 시간 문제 이슈
                    playingGame = true;
                });
            })
            ;


        if (_uiManager._currentGameTimer % 11 == 0 && _uiManager._currentGameTimer != 0)      //추후 수정 예정
        {
            Managers.Sound.Play(SoundManager.Sound.Narration, "SS010/Audio/audio_16_친구들_한줄로_천천히_징검다리를_건너주세요_");
        }

    }


    private void OnOutroStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("다 건넜어요!", 3f, narrationPath: "SS010/Audio/audio_17_다_건넜어요_");
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("와 이제부터 우리 친구들도 형님처럼 뭐든 할 수 있어요!", 3f, narrationPath: "SS010/Audio/audio_18_와___이제부터_우리_친구들도_형님처럼_뭐든_할_수_있어요_");
                //강 끝나는 지점에 아바타 3명이 같이 있고 카메라를 조금 더 가까이 확대 해 마무리 하는 애니메이션 재생
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                RestartScene(delay: 10);
            });
    }

    #endregion

    
    [SerializeField] private int logBridgeClickedCount = 0;
    private int waterClickedCount = 0;

    [SerializeField] private int rocksClickedCount = 0;
    private int lavaClickedCount = 0;
    
    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_currentSequence == GameSequence.FirstGamePlay && playingGame)
        {
            var hit = GameManager_Hits[0];
            var clickedObj = hit.collider.gameObject;
            var clickedPos = hit.point;

            if (clickedObj == logBridge)
            {
                logBridgeClickedCount++;

                clickedPos.y += 0.6f;
                PlayParticleEffect(clickedPos);
                PlayClickSound();

                if (logBridgeClickedCount % 2 == 0 && logBridgeClickedCount != 0)
                {
                    PlayFootstepSound();
                }

                if (logBridgeClickedCount % 6 == 0 && logBridgeClickedCount != 0)
                {
                    PlayWoodCreakSound();
                }

                if (logBridgeClickedCount % 8 == 0 && logBridgeClickedCount != 0)
                {
                    ShakeLogBridge();
                }
            }

            else if (clickedObj == water1)
            {
                waterClickedCount++;

                clickedPos.y += 0.7f;
                PlayWaterClickParticle(clickedPos);

                if (waterClickedCount == 2)
                {
                    crocodile.transform.DOMoveY(-3, 2f);
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SS010/Audio/audio_1_강에_떨어지면_악어가_나타나요_떨어지지_않게_조심해요_");
                }

                if (waterClickedCount % 3 == 0 && waterClickedCount != 0)
                {
                    //물에 터치가 지속적으로 되면 악어가 해당 지점으로 천천히 이동
                }

            }
        }
        else if (_currentSequence == GameSequence.SecondGamePlay && playingGame)
        {
            var hit = GameManager_Hits[0];
            var clickedObj = hit.collider.gameObject;
            var clickedPos = hit.point;

            if (rocks1.Contains(clickedObj)) //O(n)이라 느림 딕셔너리로 바꾸면 좋을것같음
            {
                rocksClickedCount++;

                clickedPos.y += 0.6f;
                PlayParticleEffect(clickedPos);
                PlayClickSound();

                //각돌이 두번씩 눌리면 잠깐 잠기는 기능
                
                if (rocksClickedCount % 2 == 0)
                {
                    PlayFootstepSound();
                }

            }

            else if (clickedObj == lava)
            {
                lavaClickedCount++;
                
                clickedPos.y += 0.7f;
                //PlayWaterClickParticle(clickedPos); 용암이 튀는 이펙트로 대체

                if (lavaClickedCount == 2)
                {
                    //crocodile.transform.DOMoveY(-3, 2f);
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SS010/Audio/audio_0_용암에_떨어지면_용암_악어가_나타나요__떨어지지_않게_조심해요_");
                }

                if (lavaClickedCount % 3 == 0 && lavaClickedCount != 0)
                {
                    //용암에 터치가 지속적으로 되면 악어가 해당 지점으로 천천히 이동
                }

            }
        }
        else if (_currentSequence == GameSequence.ThirdGamePlay && playingGame)
        {
            var hit = GameManager_Hits[0];
            var clickedObj = hit.collider.gameObject;
            var clickedPos = hit.point;

            if (rocks2.Contains(clickedObj)) //O(n)이라 느림 딕셔너리로 바꾸면 좋을것같음
            {
                rocksClickedCount++;

                clickedPos.y += 0.6f;
                PlayParticleEffect(clickedPos);
                PlayClickSound();

                //각돌이 두번씩 눌리면 잠깐 잠기는 기능
                
                if (rocksClickedCount % 2 == 0)
                {
                    PlayFootstepSound();
                }

            }

            else if (clickedObj == water2)
            {
                waterClickedCount++;

                clickedPos.y += 0.7f;
                PlayWaterClickParticle(clickedPos);

                if (waterClickedCount == 2)
                {
                    //crocodile.transform.DOMoveY(-3, 2f); //세번째 악어로 변경해야됨
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SS010/Audio/audio_1_강에_떨어지면_악어가_나타나요_떨어지지_않게_조심해요_");
                }

                if (waterClickedCount % 3 == 0 && waterClickedCount != 0)
                {
                    //물에 터치가 지속적으로 되면 악어가 해당 지점으로 천천히 이동
                    //세번째 게임은 악어가 더욱 적극적으로 움직여야함
                }

            }
        }
    }

    private void PlayWaterClickParticle(Vector3 spawnPosition)
    {
        foreach (Transform child in waterEffectPoolTransform)
        {
            var go = child.gameObject;
            if (!go.activeSelf)
            {
                child.position = spawnPosition;
            
                go.SetActive(true);
                var ps = go.GetComponent<ParticleSystem>();
                ps.Play();

                StartCoroutine(ParticleDone(ps, go));

                break;
            }
        }
    }
    
    private IEnumerator ParticleDone(ParticleSystem ps, GameObject go)
    {
        // 파티클 재생 완료를 기다렸다가
        yield return new WaitUntil(() => !ps.IsAlive(true));
        go.SetActive(false);
    }

    
    private void PlayClickSound()
    {
        if (_clickClips.Length <= 0)
            Logger.Log("클릭 효과음 없음");

        int idx = Random.Range(0, _clickClips.Length);

        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);

    }

    public void PlayVictorySoundAndEffect()
    {
        if (_victorySound == null)
            Logger.Log("승리 사운드 없음");

        Managers.Sound.Play(SoundManager.Sound.Effect, _victorySound);

        Get<ParticleSystem>((int)Particle.Victory1).Play();
        Get<ParticleSystem>((int)Particle.Victory2).Play();
    }

    private void PlayFootstepSound()
    {
        footStepAudioCount++;

        if (footStepAudioCount > 11)
            footStepAudioCount = 1;

        Managers.Sound.Play(SoundManager.Sound.Effect, _footStepAudioClips[footStepAudioCount - 1]);
    }

    private void PlayWoodCreakSound()
    {
        woodCreakAudioCount++;

        if (woodCreakAudioCount > 8)
            woodCreakAudioCount = 1;

        Managers.Sound.Play(SoundManager.Sound.Effect, _woodCreakAudioClips[woodCreakAudioCount - 1]);
    }

    private void ShakeLogBridge()
    {
        DOTween.Kill(logBridge);

        DOTween.Sequence()
            .Append(logBridge.transform.DOLocalRotate
                (
                    new Vector3(0f, 0f, 7f),
                    0.3f,
                    RotateMode.LocalAxisAdd
                )
                .SetEase(Ease.InOutSine)
            )
            .Append(logBridge.transform.DOLocalRotate
                (
                    new Vector3(0f, 0f, -14f),
                    0.6f,
                    RotateMode.LocalAxisAdd
                )
                .SetEase(Ease.InOutSine)
            )
            .Append(logBridge.transform.DOLocalRotate
                (
                    new Vector3(0f, 0f, 7f),
                    0.3f,
                    RotateMode.LocalAxisAdd
                )
                .SetEase(Ease.InOutSine)
            );
    }
    
}
