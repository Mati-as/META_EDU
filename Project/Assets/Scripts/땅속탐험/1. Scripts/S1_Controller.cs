using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S1_Controller : MonoBehaviour
{
    // step > chatper > level
    //
    //

    private GameObject FollowMessage;
    private GameObject FoundMessage;

    private int step;
    public int chapter;
    public int level;

    private GameObject FootstepObj;
    private GameObject AnimalObj_prev;
    private GameObject AnimalObj_next;

    // Start is called before the first frame update
    void Start()
    {
        //chapter = 13;
        //Set_AnimalALL();    
    }
   
    //�� ��Ʈ�ѷ��� ���� ����
    //�� ��Ʈ�ѷ��� ���� é��
    //�� ��Ʈ�ѷ��� ������ é��

    public void ActNextstep()
    {
        Ground2DGameManager.AddStep();
        step = Ground2DGameManager.GetStep();

        StartCoroutine(Set_Footstep(step));
        Animal_animation("Jump");
    }

    public void ActNextchapter()
    {
        Ground2DGameManager.AddStep();
        Ground2DGameManager.AddChapter();
        step = Ground2DGameManager.GetStep();
        chapter = Ground2DGameManager.GetChapter();

        StartCoroutine(Set_Footstep(step));
        StartCoroutine(Set_Animal(chapter));
        Animal_animation("Next animal");
    }

    public void ActNextlevel()
    {
        Ground2DGameManager.AddLevel();
        level = Ground2DGameManager.GetLevel();  

        //�������� ���
        //isGameStarted =true
        //���� �����ϸ鼭 

        if (level < 4)
        {
            Ground2DGameManager.SetisRoudnFinished();
        }
        else
        {
            Debug.Log("END CHECK");
            Ground2DGameManager.SetisGameFinished();
            Set_AnimalALL();
        }

    }
    IEnumerator Set_Footstep(int num_step)
    {
        FootstepObj = Ground2DGameManager.GetFootstep(num_step - 1);
        FootstepObj.SetActive(false);

       //yield return new WaitForSeconds(1f);

        FootstepObj = Ground2DGameManager.GetFootstep(num_step);
        FootstepObj.SetActive(true);
        yield break;
    }

    IEnumerator Set_Animal(int num_chap)
    {
        AnimalObj_prev = Ground2DGameManager.GetAnimal(num_chap - 1);
        AnimalObj_next = Ground2DGameManager.GetAnimal(num_chap);
        Set_Message(AnimalObj_prev, 1, true);
        AnimalObj_next.SetActive(true);

        yield return new WaitForSeconds(1f);

        Set_Message(AnimalObj_next, 0, true);
        AnimalObj_prev.SetActive(false);

        yield return new WaitForSeconds(1f);

        Set_Message(AnimalObj_next, 0, false);

        yield break;
    }

    //�׽�Ʈ �ܰ迡�� �Ͻ������� ����, ���� ���� �ڵ� �ִ� �������?
    void Set_Message(GameObject animal,int num_message,bool onoff)
    {
        //���� ������Ʈ�� 0: follow, 1:found
        animal.transform.GetChild(num_message).gameObject.SetActive(onoff);
    }

    void Set_AnimalALL()
    {
        //12���� �� ��
        for(int i= chapter-1; i== 0; i--)
        {
            Debug.Log("Check"+i);
                
            AnimalObj_next = Ground2DGameManager.GetAnimal(i);
            AnimalObj_next.SetActive(true);
        }
    }
    void Animal_animation(string anim)
    {
        //�׳� ���� �Լ�
        Debug.Log("Animal animation :"+ anim);
    }
}
