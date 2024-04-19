using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class LauncherInstructionUI : MonoBehaviour
{

    public GameObject Gameboard;
    public GameObject Message_Intro_Howto;
    public GameObject Message_Intro_Story;


    public GameObject Message_Endchapter;
    public GameObject Message_Ready;

    public GameObject Message_Endgame;


    private RectTransform GB_transform;
    private List<Vector3> GB_positionList = new();

    //TEST
    private int GB_Listindex = 0;
    private Vector3 GB_targetVector3;

    private bool flag_num_1 = false;
    //[FormerlySerializedAs("startTimeOffset")]
    //public float startTimeOffsetSeconds; // ���ӽ����� �� �� �� UI����� ����


    private void Start()
    {
        GB_transform = Gameboard.GetComponent<RectTransform>();
        Init_GBPosition();
        Set_GBPosition(1);

        //Move_BGPosition(2);
    }

    void Update()
    {
        if (flag_num_1 == true)
        {
            //��Ʈ�ο����� �ӵ� 0.5, �� �ܿ� 1.5 ���� �ʿ�
            GB_transform.localPosition = GB_transform.localPosition
                = Vector3.Lerp(GB_transform.localPosition, GB_targetVector3, 0.5f * Time.deltaTime);

            float distance = Vector3.Distance(GB_transform.localPosition, GB_targetVector3);
            Debug.Log(distance);
            if (distance < 0.01f)
            {
                GB_transform.localPosition = GB_targetVector3;
                flag_num_1 = false;
            }
        }

    }

    private void Init_GBPosition()
    {
        GB_positionList.Add(new Vector3(0, 0, 0));
        GB_positionList.Add(new Vector3(0, 1080, 0));
        GB_positionList.Add(new Vector3(-1920, 1080, 0));
        GB_positionList.Add(new Vector3(-3840, 1080, 0));
        GB_positionList.Add(new Vector3(-5760, 1080, 0));

        //Debug.Log(BG_positionList.Count);
    }
    private void Set_GBPosition(int number)
    {
        if (number < GB_positionList.Count)
        {
            GB_targetVector3 = GB_positionList[number];
        }
        else
        {
            //��������� ��ü�� �����ֱ� ���� ����ó��
            GB_targetVector3 = GB_positionList[1];
        }
    }

    //������ġ 0 ����, 0~5����
    public void Move_GBPosition()
    {
        flag_num_1 = true;
        GB_Listindex++; //���� ��ġ�� �ڵ� ��ȯ�ϱ� ����
        Set_GBPosition(GB_Listindex);
    }

    IEnumerator Set_UIIntro()
    {
        //Introhowto �����ְ�
        //Introstory �����ְ�
        //������ ���� �غ� : ȭ�� �̵�

        Message_Intro_Howto.SetActive(true);

        yield return new WaitForSeconds(3f);

        Message_Intro_Howto.SetActive(false);
        Message_Intro_Story.SetActive(true);

        yield return new WaitForSeconds(3f);

        Message_Intro_Story.SetActive(false);

        Move_GBPosition();
        yield break;
    }

    IEnumerator Set_UINextlevel()
    {
        //ȭ�� �̵�
        //Endchapter �����ְ�
        //Ready �����ְ�

        Move_GBPosition();

        yield return new WaitForSeconds(1f);

        Message_Endchapter.SetActive(true);

        yield return new WaitForSeconds(3f);

        Message_Endchapter.SetActive(false);
        Message_Ready.SetActive(true);

        yield return new WaitForSeconds(3f);

        Message_Ready.SetActive(false);

        yield break;
    }
    IEnumerator Set_UIEndgame()
    {
        Message_Endgame.SetActive(true);

        yield return new WaitForSeconds(3f);

        Message_Endgame.SetActive(false);

        Move_GBPosition();

        yield break;
    }
    public void PlayIntroMessage()
    {
        Debug.Log("INTRO CHECK");
        StartCoroutine(Set_UIIntro());
    }

    public void PlayEndChapterMessage()
    {
        Debug.Log("END CHAPTER CHECK");
        StartCoroutine(Set_UINextlevel());
    }

    public void PlayFinishMessage()
    {
        Debug.Log("FINISH CHECK");
        StartCoroutine(Set_UIEndgame());
    }


}