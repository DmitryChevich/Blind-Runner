using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Скорости")]
    public float sneakSpeed = 1.5f;
    public float walkSpeed = 3.0f;  
    public float runSpeed = 4.0f;

    [Header("Физика")]
    public float gravity = -20f;

    private CharacterController _controller;
    private Vector3 _velocity;      // вертикальная скорость (гравитация)
    private float _currentSpeed;
    public bool IsMovingInput => _isMovingInput;

    private Transform _modelTransform;  // ссылка на дочернюю модель

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        // Находим модель среди дочерних объектов
        _modelTransform = GetComponentInChildren<Animator>().transform;
    }

    void Update()
    {
        HandleMovement();
        ApplyGravity();
        UpdateAnimator();   // добавь вызов
    }

    void UpdateAnimator()
    {
        // Ищем Animator в дочерних объектах
        Animator anim = GetComponentInChildren<Animator>();
        if (anim == null) return;

        anim.SetFloat("Speed", _currentSpeed);
        anim.SetBool("IsCrouching", Input.GetKey(KeyCode.LeftControl));
    }

    private bool _isMovingInput;

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
    
        _isMovingInput = h != 0f || v != 0f;
    
        // Сбрасываем скорость если нет ввода
        if (!_isMovingInput)
        {
            _currentSpeed = 0f;
            return;  // выходим сразу — не двигаемся
        }

        // Направление относительно камеры
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        Vector3 direction = (camForward * v + camRight * h).normalized;

        if (Input.GetKey(KeyCode.LeftControl))
            _currentSpeed = sneakSpeed;
        else if (Input.GetKey(KeyCode.LeftShift))
            _currentSpeed = runSpeed;
        else
            _currentSpeed = walkSpeed;

        if (direction.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            // Поворачиваем только модель — компенсируем offset в 180 градусов
            _modelTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }

        _controller.Move(direction * _currentSpeed * Time.deltaTime);
    }

    void ApplyGravity()
    {
        if (_controller.isGrounded && _velocity.y < 0)
            _velocity.y = -2f;  // прижимаем к земле

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    // Публичное свойство — враги и PingSystem будут его читать
    public float CurrentSpeed => _currentSpeed;
}