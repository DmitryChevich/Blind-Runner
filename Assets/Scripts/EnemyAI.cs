using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    // Состояния врага
    public enum State { Patrol, Investigate, Chase }
    public State CurrentState { get; private set; } = State.Patrol;

    [Header("Патрулирование")]
    public Transform[] waypoints;           // точки маршрута
    public float waypointStopDistance = 0.5f;

    [Header("Расследование")]
    public float investigateDuration = 3.5f; // сколько ищет на месте

    [Header("Преследование")]
    public float chaseSpeed = 1.5f;
    public float patrolSpeed = 1;
    public float catchDistance = 1.5f;      // расстояние поимки игрока
    public float loseDistance = 7f;        // расстояние потери игрока
    public float loseTime = 10f;            // время до потери если не догнал

    private NavMeshAgent _agent;
    private PlayerNoise _playerNoise;
    private Transform _player;

    private int _currentWaypoint = 0;
    private float _investigateTimer = 0f;
    private float _chaseTimer = 0f;
    private Vector3 _investigateTarget;     // точка куда идём расследовать

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _player = GameObject.FindWithTag("Player").transform;
        _playerNoise = _player.GetComponent<PlayerNoise>();

        _agent.speed = patrolSpeed;
        GoToNextWaypoint();
    }

    void Update()
    {
        CheckHearing();

        switch (CurrentState)
        {
            case State.Patrol:      UpdatePatrol();      break;
            case State.Investigate: UpdateInvestigate(); break;
            case State.Chase:       UpdateChase();       break;
        }

        UpdateAnimator();
    }

    void UpdateAnimator()
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null) return;

        // Передаём текущую скорость NavMeshAgent в Animator
        anim.SetFloat("Speed", _agent.velocity.magnitude);
    }

    void CheckHearing()
    {
        if (CurrentState == State.Chase) return;    // уже преследуем

        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        float noiseRadius = _playerNoise.NoiseRadius;

        // Слышим если игрок в радиусе шума
        if (noiseRadius > 0 && distToPlayer <= noiseRadius)
            EnterInvestigate(_player.position);
    }

    // --- ПАТРУЛЬ ---
    void UpdatePatrol()
    {
        if (waypoints.Length == 0) return;

        // Дошли до точки — идём к следующей
        if (_agent.remainingDistance <= waypointStopDistance && !_agent.pathPending)
            GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_currentWaypoint].position);
        _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;  // зацикливаем маршрут
    }

    // --- РАССЛЕДОВАНИЕ ---
    void EnterInvestigate(Vector3 target)
    {
        CurrentState = State.Investigate;
        _investigateTarget = target;
        _investigateTimer = 0f;
        _agent.speed = patrolSpeed;
        _agent.SetDestination(_investigateTarget);
    }

    void UpdateInvestigate()
    {
        // Дошли до точки — стоим и ищем
        if (_agent.remainingDistance <= waypointStopDistance)
        {
            _investigateTimer += Time.deltaTime;
            if (_investigateTimer >= investigateDuration)
                EnterPatrol();  // не нашли — возвращаемся к патрулю
        }

        // Видим игрока близко — начинаем преследование
        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distToPlayer <= catchDistance * 3f)
            EnterChase();
    }

    // --- ПРЕСЛЕДОВАНИЕ ---
    void EnterChase()
    {
        CurrentState = State.Chase;
        _agent.speed = chaseSpeed;
        _chaseTimer = 0f;
    }

    void CatchPlayer()
    {
        _player.GetComponent<PlayerHealth>().Die();
    }

    void UpdateChase()
    {
        _agent.SetDestination(_player.position);    // постоянно обновляем цель
        _chaseTimer += Time.deltaTime;

        float distToPlayer = Vector3.Distance(transform.position, _player.position);

        // Поймали игрока
        if (distToPlayer <= catchDistance)
        {
            CatchPlayer();
            return;
        }

        // Потеряли игрока — далеко или долго гонимся
        if (distToPlayer >= loseDistance || _chaseTimer >= loseTime)
            EnterInvestigate(_player.position);     // идём к последнему месту игрока
    }

    void EnterPatrol()
    {
        CurrentState = State.Patrol;
        _agent.speed = patrolSpeed;
        GoToNextWaypoint();
    }
}