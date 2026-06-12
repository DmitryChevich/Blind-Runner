using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        Shader.SetGlobalFloat("_PingRadius", -1f);
        Shader.SetGlobalFloat("_TrailRadius", -1f);
        Shader.SetGlobalFloat("_TrailIntensity", 0f);
    
        yield return null;
    
        UIManager.Instance.ShowVictoryScreen();
    }
}