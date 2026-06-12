using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    private bool _isDead = false;   // защита от двойной смерти

    public void Die()
    {
        if (_isDead) return;
        _isDead = true;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerAudio>().enabled = false;
    
        yield return StartCoroutine(UIManager.Instance.FadeToRed());
    
        // Показываем экран смерти вместо перезагрузки
        UIManager.Instance.ShowDeathScreen();
    }
}