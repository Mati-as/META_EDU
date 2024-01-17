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
   
    //씬 컨트롤러의 다음 스탭
    //씬 컨트롤러의 다음 챕터
    //씬 컨트롤러의 마지막 챕터

    public void ActNextstep()
    {
        GameManager.AddStep();
        step = GameManager.GetStep();

        StartCoroutine(Set_Footstep(step));
        Animal_animation("Jump");
    }

    public void ActNextchapter()
    {
        GameManager.AddStep();
        GameManager.AddChapter();
        step = GameManager.GetStep();
        chapter = GameManager.GetChapter();

        StartCoroutine(Set_Footstep(step));
        StartCoroutine(Set_Animal(chapter));
        Animal_animation("Next animal");
    }

    public void ActNextlevel()
    {
        GameManager.AddLevel();
        level = GameManager.GetLevel();  

        //마지막일 경우
        //isGameStarted =true
        //레벨 전달하면서 

        if (level < 4)
        {
            GameManager.SetisRoudnFinished();
        }
        else
        {
            Debug.Log("END CHECK");
            GameManager.SetisGameFinished();
            Set_AnimalALL();
        }

    }
    IEnumerator Set_Footstep(int num_step)
    {
        FootstepObj = GameManager.GetFootstep(num_step - 1);
        FootstepObj.SetActive(false);

       //yield return new WaitForSeconds(1f);

        FootstepObj = GameManager.GetFootstep(num_step);
        FootstepObj.SetActive(true);
        yield break;
    }

    IEnumerator Set_Animal(int num_chap)
    {
        AnimalObj_prev = GameManager.GetAnimal(num_chap - 1);
        AnimalObj_next = GameManager.GetAnimal(num_chap);
        Set_Message(AnimalObj_prev, 1, true);
        AnimalObj_next.SetActive(true);

        yield return new WaitForSeconds(1f);

        Set_Message(AnimalObj_next, 0, true);
        AnimalObj_prev.SetActive(false);

        yield return new WaitForSeconds(1f);

        Set_Message(AnimalObj_next, 0, false);

        yield break;
    }

    //테스트 단계에서 일시적으로 만듬, 추후 직접 코드 넣는 방식으로?
    void Set_Message(GameObject animal,int num_message,bool onoff)
    {
        //동물 오브젝트의 0: follow, 1:found
        animal.transform.GetChild(num_message).gameObject.SetActive(onoff);
    }

    void Set_AnimalALL()
    {
        //12시작 일 듯
        for(int i= chapter-1; i== 0; i--)
        {
            Debug.Log("Check"+i);
                
            AnimalObj_next = GameManager.GetAnimal(i);
            AnimalObj_next.SetActive(true);
        }
    }
    void Animal_animation(string anim)
    {
        //그냥 더미 함수
        Debug.Log("Animal animation :"+ anim);
    }
}
