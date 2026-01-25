using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float groundCheckDistance = 1.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("视角设置")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 90f;
    [SerializeField] private Transform cameraTransform;

    [Header("工具交互")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 3f;

    // 组件引用
    private CharacterController characterController;
    private Transform playerTransform;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private float currentSpeed;

    // 工具系统变量
    private GameObject currentTool;
    private bool hasToolEquipped = false;

    // 状态跟踪变量
    private bool m_lastGroundedState = false;
    private bool isSprinting = false;

    void Start()
    {
        // 获取组件
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("PlayerController 需要 CharacterController 组件！");
        }

        playerTransform = transform;

        // 锁定并隐藏光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化速度
        currentSpeed = walkSpeed;

        // 检查摄像机引用
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("未找到主摄像机，请手动在Inspector中指定 cameraTransform。");
            }
        }
    }

    void Update()
    {
        // 1. 检查地面
        CheckGrounded();

        // 2. 处理视角旋转
        HandleMouseLook();

        // 3. 处理玩家移动
        HandleMovement();

        // 4. 处理跳跃
        HandleJump();

        // 5. 应用重力
        ApplyGravity();

        // 6. 执行移动
        characterController.Move(velocity * Time.deltaTime);

        // 7. 工具交互
        HandleToolInteraction();
    }

    void CheckGrounded()
    {
        Vector3 rayStart = playerTransform.position;
        rayStart.y += 0.1f;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, groundLayer);

        // 只在状态变化时输出日志
        if (isGrounded != m_lastGroundedState)
        {
            Debug.Log($"地面状态变化: {m_lastGroundedState} -> {isGrounded}");
            m_lastGroundedState = isGrounded;
        }

        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    void HandleMouseLook()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        playerTransform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 计算移动方向
        Vector3 moveDirection = (playerTransform.right * horizontal + playerTransform.forward * vertical).normalized;

        // 冲刺检测
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        bool oldSprinting = isSprinting;

        if (shiftPressed && (horizontal != 0 || vertical != 0))
        {
            currentSpeed = runSpeed;
            isSprinting = true;
        }
        else
        {
            currentSpeed = walkSpeed;
            isSprinting = false;
        }

        // 冲刺状态变化时输出日志
        if (isSprinting != oldSprinting)
        {
            Debug.Log($"冲刺状态变化: {oldSprinting} -> {isSprinting}");
        }

        // 计算速度
        Vector3 horizontalVelocity = moveDirection * currentSpeed;
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;
    }

    void HandleJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log($"执行跳跃！");
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    void HandleToolInteraction()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (!hasToolEquipped)
            {
                TryPickupTool();
            }
            else
            {
                UseCurrentTool();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && hasToolEquipped)
        {
            DropCurrentTool();
        }
    }

    void TryPickupTool()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.CompareTag("Tool"))
            {
                currentTool = hit.collider.gameObject;
                EquipTool(currentTool);
                Debug.Log($"已拾取工具: {currentTool.name}");
            }
        }
    }

    void EquipTool(GameObject tool)
    {
        hasToolEquipped = true;
        Debug.Log($"装备工具: {tool.name}");
    }

    void UseCurrentTool()
    {
        if (currentTool != null)
        {
            Debug.Log($"使用工具: {currentTool.name}");
        }
    }

    void DropCurrentTool()
    {
        if (currentTool != null)
        {
            currentTool.transform.SetParent(null);
            currentTool.AddComponent<Rigidbody>();
            Debug.Log($"丢弃工具: {currentTool.name}");
            currentTool = null;
            hasToolEquipped = false;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 220));

        GUILayout.Label($"=== 玩家状态 ===");
        GUILayout.Label($"位置: {playerTransform.position:F2}");
        GUILayout.Label($"状态: {(isGrounded ? "在地面" : "在空中")}");
        GUILayout.Label($"移动: {(isSprinting ? "冲刺中" : "行走中")}");
        GUILayout.Label($"水平速度: {new Vector3(velocity.x, 0, velocity.z).magnitude:F2} m/s");
        GUILayout.Label($"垂直速度: {velocity.y:F2}");
        GUILayout.Label($"工具: {(hasToolEquipped ? currentTool.name : "无")}");

        GUILayout.Label($"");
        GUILayout.Label($"=== 控制说明 ===");
        GUILayout.Label($"WASD: 移动");
        GUILayout.Label($"鼠标: 视角");
        GUILayout.Label($"左Shift: 冲刺");
        GUILayout.Label($"空格: 跳跃");
        GUILayout.Label($"E: 使用/拾取工具");
        GUILayout.Label($"Q: 丢弃工具");

        GUILayout.EndArea();
    }
}