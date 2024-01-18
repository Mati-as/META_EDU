using DG.Tweening;
using UnityEngine;

public class Underground_UIManager : MonoBehaviour
{
    private GroundGameManager _gameManager;

    private void Start()
    {
        _gameManager = GameObject.FindWithTag("GameController").GetComponent<GroundGameManager>();


        if (_gameManager != null)
        {
            TriggerStoryUI();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("GameController or GroundGameManager is null.");
#endif
        }
    }


    private void TriggerStoryUI()
    {
        DOVirtual.Float(0, 1, 2.3f, _ => _++).OnComplete(() =>
        {
            if (_gameManager.isStartButtonClicked.Value == false) _gameManager.isStartButtonClicked.Value = true;
        });
    }
}