using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    [Header("Экраны")]
    public GameObject deathScreen;
    public GameObject victoryScreen;
    public GameObject pauseScreen;  // добавь это

    [Header("Post Processing")]
    public Volume postProcessVolume;

    private Bloom _bloom;

    private bool _isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;

        pauseScreen.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1f;  // останавливаем время при паузе

        Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isPaused;
    }

    public void ResumeGame()
    {
        TogglePause();
    }

    void Awake()
    {
        Instance = this;
        fadeImage.color = new Color(1, 0, 0, 0);

        Shader.SetGlobalFloat("_PingRadius", -1f);
        Shader.SetGlobalFloat("_TrailRadius", -1f);
        Shader.SetGlobalFloat("_TrailIntensity", 0f);

        // Прячем экраны при старте
        deathScreen.SetActive(false);
        victoryScreen.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(InitBloom());
    }

    IEnumerator InitBloom()
    {
        yield return null;
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out _bloom))
            _bloom.intensity.value = 1.5f;
    }

    public IEnumerator FadeToRed()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / fadeDuration;
            fadeImage.color = new Color(1, 0, 0, alpha);
            yield return null;
        }
    }

    public void ShowDeathScreen()
    {
        deathScreen.SetActive(true);
        // Показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowVictoryScreen()
    {
        victoryScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Кнопки смерти
    public void RestartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);  // главное меню = индекс 0
    }

    public void NextLevel()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;
    
        // Если следующего уровня нет — остаёмся на последнем
        if (nextScene >= SceneManager.sceneCountInBuildSettings)
            nextScene = currentScene;
    
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(nextScene);
    }
}