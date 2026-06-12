using System.Collections;
using UnityEngine;

public class PingSystem : MonoBehaviour
{
    // Событие отдельно — до любых [Header]
    public event System.Action OnPing;

    [Header("Параметры пинга")]
    public float pingMaxRadius = 15f;
    public float pingDuration = 2f;
    public float pingCooldown = 4f;
    public float pingSpeed = 13f;
    public KeyCode pingKey = KeyCode.Space;

    [Header("Остаточное свечение")]
    public float trailFadeDuration = 2f;

    private bool _isPinging = false;
    private bool _onCooldown = false;
    private float _cooldownTimer = 0f;

    public float CooldownProgress => _onCooldown ? _cooldownTimer / pingCooldown : 1f;

    void Update()
    {
        HandleCooldown();

        if (Input.GetKeyDown(pingKey) && !_isPinging && !_onCooldown)
            StartCoroutine(DoPing());
    }

    void HandleCooldown()
    {
        if (!_onCooldown) return;

        _cooldownTimer += Time.deltaTime;
        if (_cooldownTimer >= pingCooldown)
        {
            _onCooldown = false;
            _cooldownTimer = 0f;
        }
    }

    IEnumerator DoPing()
    {
        _isPinging = true;
        OnPing?.Invoke();   // уведомляем подписчиков (PlayerAudio играет звук)
        _onCooldown = true;
        _cooldownTimer = 0f;

        Vector3 origin = transform.position;
        Shader.SetGlobalVector("_PingOrigin", origin);

        // --- ФАЗА 1: кольцо расширяется ---
        float radius = 0f;
        while (radius < pingMaxRadius)
        {
            radius += pingSpeed * Time.deltaTime;
            radius = Mathf.Min(radius, pingMaxRadius);

            Shader.SetGlobalFloat("_PingRadius", radius);
            Shader.SetGlobalFloat("_TrailRadius", radius);
            Shader.SetGlobalFloat("_TrailIntensity", 1f);

            yield return null;
        }

        Shader.SetGlobalFloat("_PingRadius", -1f);

        // --- ФАЗА 2: след угасает ---
        float trailElapsed = 0f;
        while (trailElapsed < trailFadeDuration)
        {
            trailElapsed += Time.deltaTime;
            float intensity = 1f - (trailElapsed / trailFadeDuration);
            Shader.SetGlobalFloat("_TrailIntensity", intensity);

            yield return null;
        }

        // Сбрасываем всё
        Shader.SetGlobalFloat("_PingRadius", -1f);
        Shader.SetGlobalFloat("_TrailRadius", -1f);
        Shader.SetGlobalFloat("_TrailIntensity", 0f);

        _isPinging = false;
    }

    void OnDestroy()
    {
        // Останавливаем все корутины и сбрасываем шейдер
        StopAllCoroutines();
        Shader.SetGlobalFloat("_PingRadius", -1f);
        Shader.SetGlobalFloat("_TrailRadius", -1f);
        Shader.SetGlobalFloat("_TrailIntensity", 0f);
    }
}