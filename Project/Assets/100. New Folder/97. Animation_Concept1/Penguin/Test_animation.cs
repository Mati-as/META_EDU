using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Test_animation : MonoBehaviour
{

    public GameObject Penguin7;
    public GameObject Penguin_position;



    //������ �ټ��� ������Ʈ�� ������ �װ� ����� �� ���� ����

    //���, in sequence �迭, out sequence �迭�� ����
    //0~6��
    private List<Sequence> In_seq = new List<Sequence>();
    private List<Sequence> Out_seq = new List<Sequence>();


    void Start()
    {

        //p7_in.Append(Penguin7.transform.DORotate(P7_pfrom_to.transform.rotation.eulerAngles, 3f))
        //.Append(Penguin7.transform.DOMove(P7_pfrom_to.transform.position, 3f).SetEase(Ease.InOutQuad));
        //    .AppendCallback(Penguin7.transform.DOMove(P7_pto.transform.position, 1f).SetEase(Ease.InOutQuad));
    }

    //�׷��ٸ� ��ŷ� �̵��ϴ� ������
    //��ſ��� ������ ������
    //�̷��� 2������ �ʿ��Ѱ�?

    void Init_Act_fromto()
    {
        //1.��� �������� ������ �ٲ�
        //2.��� �������� �̵���
        //3.������ �����ؼ� ��� ������ ��
        //4.���� �����س��� �������� ȸ����
        //5.�ִϸ��̼� �����
        //p7_in.Append(Penguin7.transform.DORotate(P7_pto.transform.rotation.eulerAngles, 0.2f));

    }

    void Init_Seq_penguin()
    {
        //in.out ������ ���� �ѹ��� �ʱ�ȭ
        for (int i = 0; i < Penguin_position.transform.childCount; i++)
        {
            In_seq[i] = DOTween.Sequence();
            Out_seq[i] = DOTween.Sequence();

            //���� ���� ������Ʈ�� �ִ� ������ �����ͼ� �����ϴ� �ɷ�
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
    //        //4���� �߶� n�� �ʱ�ȭ
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
