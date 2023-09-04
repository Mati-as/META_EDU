using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEffect : MonoBehaviour
{
    public LayerMask clickableLayer; // 인스펙터에서 클릭 가능한 레이어를 지정합니다.
    private ParticleSystem _pts; // 재생할 파티클 시스템을 지정합니다.

    private void Awake()
    {
        _pts = GetComponent<ParticleSystem>();

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 왼쪽 마우스 버튼이 클릭됐는지 확인합니다.
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
            {
               _pts.Stop(); // 현재 재생 중인 파티클이 있다면 중지합니다.
               _pts.transform.position = hit.point; // 파티클 시스템을 클릭한 위치로 이동시킵니다.
               _pts.Play(); // 파티클 시스템을 재생합니다.
            }
        }
    }
}
