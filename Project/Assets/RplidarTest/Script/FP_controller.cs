using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FP_controller : MonoBehaviour
{
    //화면상에 활성화 되어있는 FP RT리스트
    private List<RectTransform> FP_pos_controller = new List<RectTransform>();

    private float FP_x, FP_y;
    private float FP_Gap=80;

    void Start()
    {
        
    }

    public bool Check_FPposition(RectTransform FP)
    {
        for(int i =0;i< FP_pos_controller.Count; i++)
        {
           // Debug.Log("존재하는 FP 카운트" + FP_pos_controller.Count);
            FP_x = FP_pos_controller[i].anchoredPosition.x;
            FP_y = FP_pos_controller[i].anchoredPosition.y;

            if (FP_x - FP.anchoredPosition.x < -FP_Gap || FP_x - FP.anchoredPosition.x > FP_Gap)
            {
                if (FP_y - FP.anchoredPosition.y < -FP_Gap || FP_y - FP.anchoredPosition.y > FP_Gap)
                {
                    //동시에 여러 좌표가 찍히는 순간 서로 간섭하는 문제 발생함
                    //순식간에 2영역의 좌표들이 입력되면서 해당 영역 근처의 볼을 서로의 영역에서 참으로 판별함

                    //for문 다 돌고 최종적으로 false 피해가면 true 판정

                    //일단 보류
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //for문을 애초에 돌지 않음 최종 true 판정
        return true;
    }

    public void Add_FPposition(RectTransform FP)
    {
        FP_pos_controller.Add(FP);
    }

    public void Delete_FPposition()
    {
        FP_pos_controller.RemoveAt(0);
    }
}
