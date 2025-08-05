using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using System.Linq;
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

    }

    private enum Objects
    {


    }

    private enum Particle
    {
        Victory1,
        Victory2
    }

    private SS010_UIManager _uiManager;
    private Vector3 _clickEffectPos;
    private AudioClip[] _clickClips;
    private string _victorySound;

    protected override void Init()
    {
        BindObject(typeof(Objects));
        Bind<ParticleSystem>(typeof(Particle));
        Bind<CinemachineVirtualCamera>(typeof(Cameras));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<SS010_UIManager>();

        psResourcePath = "SS010/Asset/Fx_Click"; //주소변경
        SetPool(); //클릭 이펙트 용 풀

        Get<CinemachineVirtualCamera>(0).Priority = 12; //카메라들 우선순위 초기화
        for (int i = 1; i <= 1; i++)
            Get<CinemachineVirtualCamera>(i).Priority = 10;

        Managers.Sound.Play(SoundManager.Sound.Bgm, "");

        _clickClips = new AudioClip[5]; //오디오 char 캐싱 
        for (int i = 0; i < _clickClips.Length; i++)
            _clickClips[i] = Resources.Load<AudioClip>($"SS010/Audio/Click_{(char)('A' + i)}");

        _victorySound = "EA038/Audio/audio_Victory";

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
        ChangeStage(GameSequence.Intro);
        //ChangeStage(EA038_MainSeq.ObjectGameStageSequence);
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
                //_uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                //형님처럼 할 수 잇어요
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //(다리를 건너야 해요~)
                //형님처럼 씩씩하게 외나무 다리를 건너볼까요?
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
                //_uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                //형님을 따라 두판을 벌리고 외나무 다리를 건너주세요
                //(형님이 먼저 외나무 다리를 건너는 것을 보여줄 거에요)
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //형님 아바타가 길을 건너감 
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //준비~시작
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                // 게임 시작
                // 이때 부터 시간 초 스타트
                //다음으로 넘어가는 기능은 시간제에 있음 

                // (친구들 한줄로 천천히 외나무 다리를 건너주세요) > 해당 텍스트 중간중간 재생

            });

    }

    private void OnFirstTransitionStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                //용암 게임 장소로 이동

                //_uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                //다 건넜어요! 형님처럼 잘 할 수 있어요!
                //아바타 성공 애니메이션 재생
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //(바닥에 용암이 흐르고 있어요!)
                //다시 한번 형님처럼 씩씩하게 징검다리를 건너볼까요?

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
                //_uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                //(형님을 따라) 이번엔 두발을 모아 점프해 징검다리를 건너주세요
                //(형님이 먼저 징검 다리를 건너는 것을 보여줄 거에요)
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
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
                // 게임 시작
                // 이때 부터 시간 초 스타트
                //다음으로 넘어가는 기능은 시간제에 있음 

                // (친구들 한줄로 천천히 외나무 다리를 건너주세요) > 해당 텍스트 중간중간 재생
            });

    }

    private void OnSecondTransitionStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                //_uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                //다 건넜어요! 형님처럼 잘 할 수 있어요!
                //아바타 성공 애니메이션 재생
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                ChangeStage(GameSequence.SecondGamePlay);
            })
            ;

    }

    private void OnThirdGameStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                //_uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                //(형님을 따라) 이번엔 한발로 점프해 징검다리를 건너주세요
                //(형님이 먼저 징검 다리를 건너는 것을 보여줄 거에요)
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
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
                // 게임 시작
                // 이때 부터 시간 초 스타트
                //다음으로 넘어가는 기능은 시간제에 있음 

                // (친구들 한줄로 천천히 외나무 다리를 건너주세요) > 해당 텍스트 중간중간 재생
            });

    }


    private void OnOutroStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                //_uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                //다 건넜어요!
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                //와 이제부터 우리 친구들도 형님처럼 뭐든 할 수 있어요!
                //강 끝나는 지점에 아바타 3명이 같이 있고 카메라를 조금 더 가까이 확대 해 마무리 하는 애니메이션 재생
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                RestartScene(delay: 10);
            });
    }

    #endregion


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_currentSequence == GameSequence.FirstGamePlay)
        {
            foreach (var hit in GameManager_Hits)
            {
                var clickedObj = hit.collider.gameObject;

                // if (_cardByCollider.TryGetValue(hit.collider, out var card))
                // {
                //     if (card.cardValue == gamePlayAge && card.canClicked)
                //     {
                //         clickEffectPos = hit.point;
                //         clickEffectPos.y += 0.2f;
                //         PlayParticleEffect(clickEffectPos);
                //         PlayClickSound();
                //
                //         correctCardClickedCount++;
                //         Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");
                //
                //         card.canClicked = false;
                //         card.KillShake();
                //
                //         if (correctCardClickedCount == gamePlayAge)
                //         {
                //             cardGamePlayCount++;
                //             canPlayGame = false;
                //
                //             foreach (var cards in ea038_Cards)
                //             {
                //                 cards.canClicked = false;
                //             }
                //
                //             PlayVictorySoundAndEffect();
                //
                //             float delaySeq = gamePlayAge;
                //
                //             DOTween.Sequence()
                //                 .AppendInterval(1f)
                //                 .AppendCallback(ShowNarrationGamePlayAge)
                //                 .AppendInterval(3f)
                //                 .AppendCallback(() =>
                //                 {
                //                     wrongCardClickedCount = 0;
                //                     correctCardClickedCount = 0;
                //
                //                     foreach (Transform child in GetObject((int)Objects.CardPool).transform)
                //                     {
                //                         child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
                //                     }
                //                 })
                //                 .AppendInterval(2f)
                //                 .AppendCallback(() =>
                //                 {
                //                     for (int i = 0; i < correctObjectList.Count; i++)
                //                     {
                //                         int idx = i; // 클로저 이슈 방지
                //                         float delay = idx;
                //
                //                         DOVirtual.DelayedCall(delay, () =>
                //                         {
                //                             var obj = correctObjectList[idx];
                //                             // 위치 세팅
                //                             obj.transform.position =
                //                                 GetObject((int)Objects.ShowCorrectObjectPositions)
                //                                     .transform.GetChild(gamePlayAge - 3)
                //                                     .GetChild(idx).position;
                //                             // 활성화 & 스케일 트윈
                //                             obj.SetActive(true);
                //                             obj.transform.DOScale(originalCardScale * 2f, 1f);
                //                         });
                //                     }
                //                 })
                //                 .AppendInterval(delaySeq)
                //                 .AppendCallback(() =>
                //                 {
                //                     for (int i = 0; i < correctObjectList.Count; i++)
                //                     {
                //                         correctObjectList[i].transform.DOScale(0, 1f);
                //                     }
                //                 })
                //                 ;
                //         }
                //     }
                // }
            }
        }
            
        if (_currentSequence == GameSequence.SecondGamePlay)
        {
            foreach (var hit in GameManager_Hits)
            {
                var clickedObj = hit.collider.gameObject;

                // if (_cardByCollider.TryGetValue(hit.collider, out var card))
                // {
                //     if (card.cardValue == gamePlayAge && card.canClicked)
                //     {
                //         clickEffectPos = hit.point;
                //         clickEffectPos.y += 0.2f;
                //         PlayParticleEffect(clickEffectPos);
                //         PlayClickSound();
                //
                //         correctCardClickedCount++;
                //         Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");
                //
                //         card.canClicked = false;
                //         card.KillShake();
                //
                //         if (correctCardClickedCount == gamePlayAge)
                //         {
                //             cardGamePlayCount++;
                //             canPlayGame = false;
                //
                //             foreach (var cards in ea038_Cards)
                //             {
                //                 cards.canClicked = false;
                //             }
                //
                //             PlayVictorySoundAndEffect();
                //
                //             float delaySeq = gamePlayAge;
                //
                //             DOTween.Sequence()
                //                 .AppendInterval(1f)
                //                 .AppendCallback(ShowNarrationGamePlayAge)
                //                 .AppendInterval(3f)
                //                 .AppendCallback(() =>
                //                 {
                //                     wrongCardClickedCount = 0;
                //                     correctCardClickedCount = 0;
                //
                //                     foreach (Transform child in GetObject((int)Objects.CardPool).transform)
                //                     {
                //                         child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
                //                     }
                //                 })
                //                 .AppendInterval(2f)
                //                 .AppendCallback(() =>
                //                 {
                //                     for (int i = 0; i < correctObjectList.Count; i++)
                //                     {
                //                         int idx = i; // 클로저 이슈 방지
                //                         float delay = idx;
                //
                //                         DOVirtual.DelayedCall(delay, () =>
                //                         {
                //                             var obj = correctObjectList[idx];
                //                             // 위치 세팅
                //                             obj.transform.position =
                //                                 GetObject((int)Objects.ShowCorrectObjectPositions)
                //                                     .transform.GetChild(gamePlayAge - 3)
                //                                     .GetChild(idx).position;
                //                             // 활성화 & 스케일 트윈
                //                             obj.SetActive(true);
                //                             obj.transform.DOScale(originalCardScale * 2f, 1f);
                //                         });
                //                     }
                //                 })
                //                 .AppendInterval(delaySeq)
                //                 .AppendCallback(() =>
                //                 {
                //                     for (int i = 0; i < correctObjectList.Count; i++)
                //                     {
                //                         correctObjectList[i].transform.DOScale(0, 1f);
                //                     }
                //                 })
                //                 ;
                //         }
                //     }
                // }
            }
        }
        
        if (_currentSequence == GameSequence.ThirdGamePlay)
        {
            foreach (var hit in GameManager_Hits)
            {
                var clickedObj = hit.collider.gameObject;

                // if (_cardByCollider.TryGetValue(hit.collider, out var card))
                // {
                //     if (card.cardValue == gamePlayAge && card.canClicked)
                //     {
                //         clickEffectPos = hit.point;
                //         clickEffectPos.y += 0.2f;
                //         PlayParticleEffect(clickEffectPos);
                //         PlayClickSound();
                //
                //         correctCardClickedCount++;
                //         Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");
                //
                //         card.canClicked = false;
                //         card.KillShake();
                //
                //         if (correctCardClickedCount == gamePlayAge)
                //         {
                //             cardGamePlayCount++;
                //             canPlayGame = false;
                //
                //             foreach (var cards in ea038_Cards)
                //             {
                //                 cards.canClicked = false;
                //             }
                //
                //             PlayVictorySoundAndEffect();
                //
                //             float delaySeq = gamePlayAge;
                //
                //             DOTween.Sequence()
                //                 .AppendInterval(1f)
                //                 .AppendCallback(ShowNarrationGamePlayAge)
                //                 .AppendInterval(3f)
                //                 .AppendCallback(() =>
                //                 {
                //                     wrongCardClickedCount = 0;
                //                     correctCardClickedCount = 0;
                //
                //                     foreach (Transform child in GetObject((int)Objects.CardPool).transform)
                //                     {
                //                         child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
                //                     }
                //                 })
                //                 .AppendInterval(2f)
                //                 .AppendCallback(() =>
                //                 {
                //                     for (int i = 0; i < correctObjectList.Count; i++)
                //                     {
                //                         int idx = i; // 클로저 이슈 방지
                //                         float delay = idx;
                //
                //                         DOVirtual.DelayedCall(delay, () =>
                //                         {
                //                             var obj = correctObjectList[idx];
                //                             // 위치 세팅
                //                             obj.transform.position =
                //                                 GetObject((int)Objects.ShowCorrectObjectPositions)
                //                                     .transform.GetChild(gamePlayAge - 3)
                //                                     .GetChild(idx).position;
                //                             // 활성화 & 스케일 트윈
                //                             obj.SetActive(true);
                //                             obj.transform.DOScale(originalCardScale * 2f, 1f);
                //                         });
                //                     }
                //                 })
                //                 .AppendInterval(delaySeq)
                //                 .AppendCallback(() =>
                //                 {
                //                     for (int i = 0; i < correctObjectList.Count; i++)
                //                     {
                //                         correctObjectList[i].transform.DOScale(0, 1f);
                //                     }
                //                 })
                //                 ;
                //         }
                //     }
                // }
            }
        }
        
    }

    
    private void PlayClickSound()
    {
        int idx = Random.Range(0, _clickClips.Length);

        if (_clickClips[idx] == null)
            Logger.Log("사운드 경로에 없음");

        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);

    }

    private void PlayVictorySoundAndEffect()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, _victorySound);

        Get<ParticleSystem>((int)Particle.Victory1).Play();
        Get<ParticleSystem>((int)Particle.Victory2).Play();
    }



}
