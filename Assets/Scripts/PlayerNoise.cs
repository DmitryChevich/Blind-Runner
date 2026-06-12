using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    // Уровни шума по действиям (0 = тихо, 1 = громко)
    public const float NOISE_IDLE   = 0.0f;
    public const float NOISE_SNEAK  = 0.2f;
    public const float NOISE_WALK   = 0.4f;
    public const float NOISE_RUN    = 0.8f;
    public const float NOISE_PING   = 1.0f;

    public float NoiseRadius => _currentNoise * 10f;  // радиус слышимости в юнитах
    public float CurrentNoise => _currentNoise;

    private float _currentNoise = 0f;
    private float _pingNoiseTimer = 0f;     // сколько ещё держится шум от пинга
    private const float PING_NOISE_DURATION = 0.5f;

    private PlayerMovement _player;
    private PingSystem _ping;

    void Start()
    {
        _player = GetComponent<PlayerMovement>();
        _ping = GetComponent<PingSystem>();

        _ping.OnPing += OnPingFired;    // подписываемся на пинг
    }

    void Update()
    {
        UpdateNoise();
    }

    void UpdateNoise()
    {
        // Шум от пинга держится короткое время
        if (_pingNoiseTimer > 0f)
        {
            _pingNoiseTimer -= Time.deltaTime;
            _currentNoise = NOISE_PING;
            return;
        }

        // Иначе шум зависит от скорости
        float speed = _player.CurrentSpeed;

        if (speed < 0.1f)
            _currentNoise = NOISE_IDLE;
        else if (speed <= _player.sneakSpeed)
            _currentNoise = NOISE_SNEAK;
        else if (speed <= _player.walkSpeed)
            _currentNoise = NOISE_WALK;
        else
            _currentNoise = NOISE_RUN;
    }

    void OnPingFired()
    {
        _pingNoiseTimer = PING_NOISE_DURATION;  // пинг = громкий шум на полсекунды
    }

    void OnDestroy()
    {
        if (_ping != null)
            _ping.OnPing -= OnPingFired;
    }
}