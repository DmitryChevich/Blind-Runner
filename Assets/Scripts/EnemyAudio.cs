using UnityEngine;

public class EnemyAudio : MonoBehaviour
{

    [Header("Звук обнаружения")]
    public AudioClip detectSound;   // резкий звук когда заметил игрока
    
    private EnemyAI.State _lastState;   // запоминаем предыдущее состояние

    [Header("Звуки шагов")]
    public AudioClip[] footstepClips;   // 2-3 варианта шагов

    [Header("Настройки")]
    public float stepInterval = 0.7f;   // время между шагами
    public float maxHearDistance = 20f; // на каком расстоянии слышно игроку

    private AudioSource _audioSource;
    private EnemyAI _enemyAI;
    private float _stepTimer = 0f;
    private int _lastClipIndex = -1;

    void Start()
    {
        _enemyAI = GetComponent<EnemyAI>();

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 1f;         // 3D звук — тише вдали
        _audioSource.maxDistance = maxHearDistance;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;  // линейное затухание
        _audioSource.volume = 0.7f;
    }
    
    void Update()
    {
        HandleFootsteps();
        CheckStateChange();
    }
    
    void CheckStateChange()
    {
        // Если только что перешли в Chase — играем звук обнаружения
        if (_enemyAI.CurrentState == EnemyAI.State.Chase && _lastState != EnemyAI.State.Chase)
        {
            if (detectSound != null)
                _audioSource.PlayOneShot(detectSound, 1.0f);
        }
    
        _lastState = _enemyAI.CurrentState;
    }

    void HandleFootsteps()
    {
        // Шаги только когда враг движется
        if (_enemyAI.CurrentState == EnemyAI.State.Patrol ||
            _enemyAI.CurrentState == EnemyAI.State.Chase ||
            _enemyAI.CurrentState == EnemyAI.State.Investigate)
        {
            // В Chase шаги быстрее
            float interval = _enemyAI.CurrentState == EnemyAI.State.Chase
                ? stepInterval * 0.5f   // бежит — вдвое быстрее
                : stepInterval;

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= interval)
            {
                PlayRandomStep();
                _stepTimer = 0f;
            }
        }
        else
        {
            _stepTimer = 0f;    // стоит — сбрасываем таймер
        }
    }

    void PlayRandomStep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        int index;
        if (footstepClips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do { index = Random.Range(0, footstepClips.Length); }
            while (index == _lastClipIndex);
        }

        _lastClipIndex = index;
        _audioSource.PlayOneShot(footstepClips[index]);
    }
}