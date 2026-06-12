using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Кулдаун пинга")]
    public Image pingCooldownBar;       // заполняемый бар
    public TextMeshProUGUI pingText;    // текст PING

    private PingSystem _pingSystem;

    void Start()
    {
        _pingSystem = FindObjectOfType<PingSystem>();
    }

    void Update()
    {
        UpdatePingHUD();
    }

    void UpdatePingHUD()
    {
        if (_pingSystem == null) return;

        float progress = _pingSystem.CooldownProgress;  // 0 = кулдаун, 1 = готов

        // Заполняем бар
        pingCooldownBar.fillAmount = progress;

        // Меняем цвет — серый на кулдауне, синий когда готов
        pingCooldownBar.color = progress >= 1f
            ? new Color(0f, 0.6f, 1f, 0.8f)    // синий — готов
            : new Color(0.3f, 0.3f, 0.3f, 0.8f); // серый — кулдаун

        // Текст меняется
        pingText.text = progress >= 1f ? "PING" : "...";
    }
}