using System;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private void Start()
    {
        GameManager.Instance.OnTurnPlayed += GameManager_OnTurnPlayed;
        GameManager.Instance.OnGameWinner += GameManager_OnGameWinner;
    }

    private void GameManager_OnTurnPlayed(object sender, EventArgs e)
    {
        AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
    }

    private void GameManager_OnGameWinner(object sender, GameManager.PlayerType e)
    {
        if (e == GameManager.Instance.GetLocalPlayerType())
        {
            AudioSource.PlayClipAtPoint(winSound, Camera.main.transform.position);
        }
        else
        {
            AudioSource.PlayClipAtPoint(loseSound, Camera.main.transform.position);
        }
    }   
}
