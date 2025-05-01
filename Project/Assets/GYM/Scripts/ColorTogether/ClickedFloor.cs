using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ClickedFloor : MonoBehaviour
{
    public ShowColorBox linkedBox;
    private char selectEffectSound;
    public char direction;
    ColorTogether_Manager manager;
    private void Start()
    {
        manager = FindObjectOfType<ColorTogether_Manager>();
    }

    public void OnClicked()
    {
        if (linkedBox == null || !linkedBox.canColorClicked)
            return;
            
        linkedBox.ColorClicked();
        selectEffectSound = (char)Random.Range((int)'A', (int)'F' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, $"ColorTogether/Click_{selectEffectSound}");
        if (direction == 'L')
        {
            manager.leftTeamScore++;
            if (manager.leftTeamScore > 50)
            {
                manager.leftTeamScore = 50;
                manager.leftTeamScoreTmp.text = $"{manager.leftTeamScore}";
            }
            else
                manager.leftTeamScoreTmp.text = $"{manager.leftTeamScore}";
        }
        if (direction == 'R')
        {
            manager.rightTeamScore++;
            if (manager.rightTeamScore > 50)
            {
                manager.rightTeamScore = 50;
                manager.rightTeamScoreTmp.text = $"{manager.rightTeamScore}";
            }
            else
                manager.rightTeamScoreTmp.text = $"{manager.rightTeamScore}";
        }
    }
}
