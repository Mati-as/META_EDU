using UnityEngine;

/// <summary>
/// 스폰된 객체가 어느 그리드 셀(r,c)에서 생성되었는지 정보를 저장하는 컴포넌트입니다.
/// </summary>
public class CellInfo : MonoBehaviour
{
    /// <summary>생성된 셀의 행 인덱스</summary>
    public int row;

    /// <summary>생성된 셀의 열 인덱스</summary>
    public int col;

    /// <summary>이 객체를 관리하는 스포너 레퍼런스</summary>
    public EA036_PoolManager poolManager;
}