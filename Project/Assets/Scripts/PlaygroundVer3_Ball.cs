using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlaygroundVer3_Ball : Playground_Ball_Base
{
    private static Stack<ParticleSystem> _collisionPs;
    private readonly int DEFAULT_POOL_SIZE = 30;
    private GameObject _loadedPrefab;
    private Rigidbody _rb;

    protected override void Init()
    {
        _collisionPs = new Stack<ParticleSystem>();
        _rb = GetComponent<Rigidbody>();

        _loadedPrefab = Resources.Load<GameObject>("게임별분류/기본컨텐츠/PlayGroundEX1/CFX_OnBallCollision");


        for (var i = 0; i < DEFAULT_POOL_SIZE; i++)
        {
            var instance = Instantiate(_loadedPrefab).GetComponent<ParticleSystem>();
            if (instance != null)
                _collisionPs.Push(instance);
            else
                Debug.LogError("Failed to instantiate particle system.");

            _collisionPs.Push(instance);
        }

        base.Init();
    }

    public override void OnCollisionEnter(Collision other)
    {
        if (other.transform.gameObject.name.Contains("Ball") &&
            (Mathf.Abs(_rb.velocity.x) > 0.5f || Mathf.Abs(_rb.velocity.y) > 0.5f || Mathf.Abs(_rb.velocity.z) > 0.5f))
            PlayParticle(other.contacts[0].point);
        base.OnCollisionEnter(other);
    }

    private void PlayParticle(Vector3 position)
    {
        if (_collisionPs.Count <= 0)
        {
            var instance = Instantiate(_loadedPrefab).GetComponent<ParticleSystem>();
            _collisionPs.Push(instance);

            return;
        }


        var currentPs = _collisionPs.Pop();
        currentPs.Stop();
        currentPs.transform.position = position;
        currentPs.Play();

        DOVirtual.Float(0, 0, 1f, _ => { })
            .OnComplete(() => { _collisionPs.Push(currentPs); });
    }
}