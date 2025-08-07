using UnityEngine;
using DG.Tweening;

public class SS010_Crocodile : MonoBehaviour
{
    [Header("순찰 지점들")]
    public Transform[] patrolPoints;
    [Header("이동 시간")]
    public float moveDuration = 6f;       // 각 지점으로 이동할 때 걸리는 시간
    
    private Tween _moveTween;             
    private Vector3 _designatedTarget;
    
    public void MoveCrocodile(Vector3 targetPos)
    {
        if (_moveTween != null && _moveTween.IsActive()) 
            _moveTween.Kill();

        _designatedTarget = new Vector3(targetPos.x, -3f, targetPos.z);

        _moveTween = transform
            .DOMove(_designatedTarget, moveDuration)
            .OnUpdate(() =>
            {
                Vector3 dir = (_designatedTarget - transform.position);
                dir.y = 0;  // y축 회전만 처리하고 싶다면 높이 성분 무시

                if (dir.sqrMagnitude > 0.001f)  // 벡터 크기가 충분히 클 때만
                    transform.rotation = Quaternion.LookRotation(dir.normalized);
            })
            .OnComplete(PatrolRandom);
    }

    public void PatrolRandom()
    {
        if (_moveTween != null && _moveTween.IsActive()) 
            _moveTween.Kill();

        int idx = Random.Range(0, patrolPoints.Length);
        Vector3 nextPos = new Vector3(patrolPoints[idx].position.x, -3f, patrolPoints[idx].position.z);

        _moveTween = transform
            .DOMove(nextPos, moveDuration)
            .OnUpdate(() =>
            {
                Vector3 dir = (nextPos - transform.position);
                dir.y = 0;  // y축 회전만 처리하고 싶다면 높이 성분 무시

                if (dir.sqrMagnitude > 0.001f)  // 벡터 크기가 충분히 클 때만
                    transform.rotation = Quaternion.LookRotation(dir.normalized);
            })
            .OnComplete(PatrolRandom);
    }

}