using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class BoxDropper : MonoBehaviour
{
    public ColorTogether_Manager manager;
    public List<GameObject> boxes;         // 미리 배치된 6개 박스
    public Transform spawnPoint;           // 박스 떨어뜨릴 위치 기준
    public List<Mesh> boxMeshes;               // 변경될 메쉬 목록
    public GameObject Btn_NextBoxDrop;

    public int dropIndex = 0;

    private DG.Tweening.Sequence sequence;

    private bool onButtonClicked = false;

    void Start()
    {
        manager = FindObjectOfType<ColorTogether_Manager>();
    }

    public void StartDropCycle()
    {
        //Btn_NextBoxDrop.SetActive(true);
        foreach (var box in boxes)
        {
            box.SetActive(true);

            Rigidbody rb = box.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            MeshFilter mf = box.GetComponent<MeshFilter>();
            if (mf != null)
            {
                mf.mesh = boxMeshes[dropIndex];
            }

            Vector3 startRandomPosition = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
            box.transform.position = spawnPoint.position + startRandomPosition;
        }

        AudioClip clip = Resources.Load<AudioClip>("Audio/ColorTogether/audio_" + (dropIndex + 2));
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);
        manager.narrationBG.transform.localScale = new Vector3(0.8f, 1, 1);
        manager.TakeTextImg("Image/ColorTogether/Img_" + (dropIndex + 2));

        dropIndex++;
    }

    public void NextDropBoxButton()
    {
        if (!onButtonClicked)
        {
            onButtonClicked = true;

            if (dropIndex >= boxMeshes.Count) //박스 선택 종료 스테이지 이동 타이밍
            {
                manager.narrationBG.transform.localScale = new Vector3(1f, 1, 1);
                manager.narrationImgGameObject.SetActive(false);
                manager.StartNarrationCoroutine();
                foreach (var box in boxes)
                {
                    box.SetActive(false);
                }

                sequence = DOTween.Sequence();
                sequence.AppendCallback(() =>
                {
                    Btn_NextBoxDrop.SetActive(false);
                    manager.StartCamera.Priority = 10;
                    manager.StageCamera.Priority = 20;

                });
                sequence.AppendInterval(2f);
                sequence.AppendCallback(() =>
                {
                    manager.StageCamera.Priority = 10;
                    manager.GameCamera.Priority = 20;
                    manager.SpawnRandomPair();

                });

                return;
            }

            foreach (var box in boxes)
            {
                box.SetActive(true);

                Rigidbody rb = box.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                MeshFilter mf = box.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mf.mesh = boxMeshes[dropIndex];
                }

                Vector3 startRandomPosition = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
                box.transform.position = spawnPoint.position + startRandomPosition;
            }


            manager.narrationBG.transform.localScale = new Vector3(0.8f, 1, 1);
            AudioClip clip = Resources.Load<AudioClip>("Audio/ColorTogether/audio_" + (dropIndex + 2));
            Managers.Sound.Play(SoundManager.Sound.Narration, clip);
            manager.TakeTextImg("Image/ColorTogether/Img_" + (dropIndex + 2));
            DOVirtual.DelayedCall(0.5f, () => manager.nextBoxDropDelay = false);

            dropIndex++;

            DOVirtual.DelayedCall(0.1f, () => onButtonClicked = false);
        }
    }
}
