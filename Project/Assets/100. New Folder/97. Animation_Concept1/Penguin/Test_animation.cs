using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Test_animation : MonoBehaviour
{

    public GameObject Penguin7;
    public GameObject Penguin_position;



    //앞으로 다수의 오브젝트가 나오면 그걸 오토로 쓸 수가 없음

    //펭귄, in sequence 배열, out sequence 배열로 관리
    //0~6번
    private List<Sequence> In_seq = new List<Sequence>();
    private List<Sequence> Out_seq = new List<Sequence>();


    void Start()
    {

        //p7_in.Append(Penguin7.transform.DORotate(P7_pfrom_to.transform.rotation.eulerAngles, 3f))
        //.Append(Penguin7.transform.DOMove(P7_pfrom_to.transform.position, 3f).SetEase(Ease.InOutQuad));
        //    .AppendCallback(Penguin7.transform.DOMove(P7_pto.transform.position, 1f).SetEase(Ease.InOutQuad));
    }

    //그렇다면 썰매로 이동하는 시퀀스
    //썰매에서 나오는 시퀀스
    //이렇게 2가지가 필요한가?

    void Init_Act_fromto()
    {
        //1.썰매 방향으로 방향을 바꿈
        //2.썰매 방향으로 이동함
        //3.직전에 점프해서 썰매 안으로 들어감
        //4.사전 설정해놓은 세팅으로 회전함
        //5.애니메이션 재생함
        //p7_in.Append(Penguin7.transform.DORotate(P7_pto.transform.rotation.eulerAngles, 0.2f));

    }

    void Init_Seq_penguin()
    {
        //in.out 시퀀스 각각 한번씩 초기화
        for (int i = 0; i < Penguin_position.transform.childCount; i++)
        {
            In_seq[i] = DOTween.Sequence();
            Out_seq[i] = DOTween.Sequence();

            //각각 하위 오브젝트로 있는 포지션 가져와서 저장하는 걸로
            Transform p1 = Penguin_position.transform.GetChild(i).transform;
            Transform p2 = Penguin_position.transform.GetChild(i+1).transform;
            Transform p3 = Penguin_position.transform.GetChild(i+2).transform;
            Transform p4 = Penguin_position.transform.GetChild(i+3).transform;
        }
}
    //}
    //        //0,1,2,3
    //        //    4,5,6,7
    //        //    8,9,10,11
    //        //4개씩 잘라서 n번 초기화
    //        UI_Text_array[i] = Penguin_position.transform.GetChild(i).gameObject;


    //        In_seq[i] = DOTween.Sequence();
    //        Out_seq[i] = DOTween.Sequence();

    //        In_seq[i].Append(Penguin7.transform.DORotate(P7_pfrom_to.transform.rotation.eulerAngles, 3f));
    //        In_seq[i].Append(Penguin7.transform.DOMove(P7_pfrom_to.transform.position, 3f).SetEase(Ease.InOutQuad));
    //        In_seq[i].Append(Penguin7.transform.DOJump(P7_pto.transform.position, 1f, 1, 1f));
    //        In_seq[i].Append(Penguin7.transform.DORotate(P7_pto.transform.rotation.eulerAngles, 1f));

    //        Out_seq[i].Append(Penguin7.transform.DORotate(P7_pto_from.transform.rotation.eulerAngles, 3f));
    //        Out_seq[i].Append(Penguin7.transform.DOMove(P7_pto_from.transform.position, 3f).SetEase(Ease.InOutQuad));
    //        Out_seq[i].Append(Penguin7.transform.DOJump(P7_pfrom.transform.position, 1f, 1, 1f));
    //        Out_seq[i].Append(Penguin7.transform.DORotate(P7_pfrom.transform.rotation.eulerAngles, 1f));
    //    }
    //}
}
