using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Цель")]
    public Transform target;

    [Header("Вращение")]
    public float mouseSensitivity = 3f;
    public float pitchMin = 10f;
    public float pitchMax = 80f;

    [Header("Авто-разворот за спину")]
    public float autoFollowSpeed = 0.5f;
    public float autoFollowDelay = 1.0f;
    private float _autoFollowAccumulator = 0f;

    [Header("Сброс камеры (T)")]
    public float defaultZoom = 8f;
    public float defaultPitch = 45f;
    public float resetSpeed = 5f;

    [Header("Зум")]
    public float zoomSpeed = 2f;
    public float minZoom = 4f;
    public float maxZoom = 12f;

    [Header("Следование")]
    public float followSpeed = 10f;

    private float _yaw = 0f;
    private float _pitch = 45f;
    private float _currentZoom = 8f;
    private float _timeSinceMouseMove = 0f;
    private bool _isResetting = false;
    private PlayerMovement _playerMovement;  // кэшируем раз в Start

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _playerMovement = target.GetComponent<PlayerMovement>();  // один раз
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleRotation();
        HandleZoom();
        HandleReset();
        HandleAutoFollow();
        HandleFollow();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            _yaw += mouseX * mouseSensitivity;
            _pitch -= mouseY * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

            _timeSinceMouseMove = 0f;
            _isResetting = false;
        }
        else
        {
            _timeSinceMouseMove += Time.deltaTime;
        }
    }

    void HandleAutoFollow()
    {
        if (_timeSinceMouseMove < autoFollowDelay) return;
        if (_isResetting) return;
        if (_playerMovement == null) return;

        // Спрашиваем у PlayerMovement — есть ли ввод движения
        bool isMovingInput = _playerMovement.IsMovingInput;

        if (isMovingInput)
        {
            _autoFollowAccumulator += Time.deltaTime;
            _autoFollowAccumulator = Mathf.Clamp(_autoFollowAccumulator, 0f, 3f);
        }
        else
        {
            _autoFollowAccumulator = 0f;
        }

        float speed = _playerMovement.CurrentSpeed;
        if (speed < 0.1f) return;

        float speedFactor = speed / _playerMovement.runSpeed;
        float timeFactor = _autoFollowAccumulator / 3f;
        float followRate = autoFollowSpeed * speedFactor * timeFactor;

        float targetYaw = target.eulerAngles.y;
        _yaw = Mathf.LerpAngle(_yaw, targetYaw, Time.deltaTime * followRate);
    }

    void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.T))
            _isResetting = true;

        if (_isResetting)
        {
            float targetYaw = target.eulerAngles.y;
            _yaw = Mathf.LerpAngle(_yaw, targetYaw, Time.deltaTime * resetSpeed);
            _pitch = Mathf.Lerp(_pitch, defaultPitch, Time.deltaTime * resetSpeed);
            _currentZoom = Mathf.Lerp(_currentZoom, defaultZoom, Time.deltaTime * resetSpeed);

            if (Mathf.Abs(Mathf.DeltaAngle(_yaw, targetYaw)) < 0.5f &&
                Mathf.Abs(_pitch - defaultPitch) < 0.1f &&
                Mathf.Abs(_currentZoom - defaultZoom) < 0.1f)
            {
                _isResetting = false;
            }
        }
    }

    void HandleZoom()
    {
        if (_isResetting) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _currentZoom -= scroll * zoomSpeed * 10f;
        _currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
    }

    void HandleFollow()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -_currentZoom);

        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        transform.LookAt(target.position);
    }
}