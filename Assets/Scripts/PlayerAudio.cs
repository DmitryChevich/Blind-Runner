using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Шаги")]
    public AudioClip[] footstepSneakClips;  // крадётся (Ctrl)
    public AudioClip[] footstepWalkClips;   // идёт обычно
    public AudioClip[] footstepRunClips;    // бежит (Shift)

    [Header("Остальные звуки")]
    public AudioClip pingClip;
    public AudioClip ambienceClip;

    [Header("Настройки шагов")]
    public float sneakStepInterval = 1f;
    public float walkStepInterval = 0.9f;
    public float runStepInterval = 0.3f;

    private AudioSource _footstepSource;
    private AudioSource _ambienceSource;
    private float _stepTimer = 0f;
    private int _lastSneakIndex = -1;
    private int _lastWalkIndex = -1;
    private int _lastRunIndex = -1;

    private PlayerMovement _player;
    private PingSystem _ping;

    // Берём пороги скоростей прямо из PlayerMovement
    private float sneakSpeed;
    private float walkSpeed;



    void Start()
    {
        _player = GetComponent<PlayerMovement>();
        _ping = GetComponent<PingSystem>();

        _footstepSource = gameObject.AddComponent<AudioSource>();
        _footstepSource.spatialBlend = 1f;
        _footstepSource.volume = 0.5f;

        _ambienceSource = gameObject.AddComponent<AudioSource>();
        _ambienceSource.spatialBlend = 0f;
        _ambienceSource.loop = true;
        _ambienceSource.volume = 0.15f;
        _ambienceSource.clip = ambienceClip;
        _ambienceSource.Play();

        _ping.OnPing += PlayPingSound;

        sneakSpeed = _player.sneakSpeed;
        walkSpeed = _player.walkSpeed;
    }

    void Update()
    {
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        float speed = _player.CurrentSpeed;

        // Стоим — сбрасываем таймер и выходим
        if (speed < 0.1f)
        {
            _stepTimer = 0f;
            return;
        }

        // Выбираем интервал и индекс по скорости
        float interval;
        AudioClip[] clips;
        ref int lastIndex = ref _lastWalkIndex;

        if (speed <= sneakSpeed)
        {
            interval = sneakStepInterval;
            clips = footstepSneakClips;
            lastIndex = ref _lastSneakIndex;
        }
        else if (speed <= walkSpeed)
        {
            interval = walkStepInterval;
            clips = footstepWalkClips;
            lastIndex = ref _lastWalkIndex;
        }
        else
        {
            interval = runStepInterval;
            clips = footstepRunClips;
            lastIndex = ref _lastRunIndex;
        }

        // Играем шаг только когда таймер достиг интервала
        _stepTimer += Time.deltaTime;
        if (_stepTimer >= interval)
        {
            PlayRandomClip(clips, ref lastIndex);
            _stepTimer = 0f;    // сбрасываем таймер после шага
        }
    }

    // Заменяем bool isRun на enum или int
    void PlayRandomClip(AudioClip[] clips, ref int lastIndex)
    {
        if (clips == null || clips.Length == 0) return;

        int index;
        if (clips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do { index = Random.Range(0, clips.Length); }
            while (index == lastIndex);
        }

        lastIndex = index;
        _footstepSource.PlayOneShot(clips[index]);
    }

    void PlayPingSound()
    {
        _footstepSource.PlayOneShot(pingClip, 1.0f);
    }

    void OnDestroy()
    {
        if (_ping != null)
            _ping.OnPing -= PlayPingSound;
    }
}