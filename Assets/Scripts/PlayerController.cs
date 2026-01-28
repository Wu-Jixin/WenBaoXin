using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float groundCheckDistance = 0.2f; // 缩短检测距离
    [SerializeField] private LayerMask groundLayer;

    [Header("蹲下设置")]
    [SerializeField] private float crouchHeight = 1f;      // 蹲下时的高度
    [SerializeField] private float standingHeight = 2f;    // 站立时的高度
    [SerializeField] private float crouchSpeed = 3f;       // 蹲下移动速度

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

    // 蹲下相关变量
    private float currentHeight;
    private float targetHeight;
    private bool canStandUp = true;

    // 工具系统变量
    private GameObject currentTool;
    private bool hasToolEquipped = false;

    // 状态跟踪变量
    private bool m_lastGroundedState = false;

    // ========== 公共状态字段 ==========
    [Header("状态")]
    public bool isCrouching = false;
    public bool isSprinting = false;
    public float currentSpeed = 0f;

    // 稳定化变量
    private float lastCeilingCheckTime = 0f;
    private float ceilingCheckCooldown = 0.2f;

    // ========== 蹲下状态机 ==========
    public enum CrouchState
    {
        Standing,
        Crouching,
        TransitioningToCrouch,
        TransitioningToStand
    }

    private CrouchState crouchState = CrouchState.Standing;
    private float crouchTransitionTime = 0f;
    private const float crouchTransitionDuration = 0.3f;

    void Start()
    {
        // 获取组件
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("PlayerController 需要 CharacterController 组件！");
        }

        playerTransform = transform;

        // 初始化高度
        currentHeight = standingHeight;
        targetHeight = standingHeight;
        characterController.height = currentHeight;
        characterController.center = new Vector3(0, currentHeight / 2f, 0);

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

        // 初始化摄像机位置
        UpdateCameraPosition();

        Debug.Log("PlayerController 初始化完成");
    }

    void Update()
    {
        // 1. 检查地面
        CheckGrounded();

        // 2. 处理蹲下输入和状态
        HandleCrouchState();

        // 3. 处理视角旋转
        HandleMouseLook();

        // 4. 处理玩家移动
        HandleMovement();

        // 5. 处理跳跃
        HandleJump();

        // 6. 应用重力
        ApplyGravity();

        // 7. 更新角色高度
        UpdatePlayerHeight();

        // 8. 执行移动
        characterController.Move(velocity * Time.deltaTime);

        // 9. 工具交互
        HandleToolInteraction();
    }

    void CheckGrounded()
    {
        // 方法1: 使用 CharacterController.isGrounded (最简单可靠)
        isGrounded = characterController.isGrounded;

        // 方法2: 使用射线检测作为备用
        if (!isGrounded)
        {
            Vector3 rayStart = playerTransform.position;
            rayStart.y += 0.1f;

            // 简化射线检测
            isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance, groundLayer);

            // 绘制调试射线
            Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance,
                isGrounded ? Color.green : Color.red);
        }

        // 方法3: 使用球体检测作为第二备用
        if (!isGrounded)
        {
            Vector3 spherePos = playerTransform.position;
            spherePos.y -= (characterController.height / 2f) - characterController.radius;
            isGrounded = Physics.CheckSphere(spherePos, characterController.radius * 0.9f, groundLayer);
        }

        // 只在状态变化时输出日志
        if (isGrounded != m_lastGroundedState)
        {
            Debug.Log($"地面状态变化: {m_lastGroundedState} -> {isGrounded}");
            m_lastGroundedState = isGrounded;
        }
    }

    void HandleCrouchState()
    {
        // 记录输入状态
        bool crouchKeyDown = Input.GetKeyDown(KeyCode.LeftControl);

        // 更新蹲下状态机
        switch (crouchState)
        {
            case CrouchState.Standing:
                if (crouchKeyDown)
                {
                    // 开始蹲下
                    crouchState = CrouchState.TransitioningToCrouch;
                    crouchTransitionTime = 0f;
                    isCrouching = true;
                    targetHeight = crouchHeight;
                    Debug.Log("开始蹲下");
                }
                break;

            case CrouchState.Crouching:
                if (crouchKeyDown)
                {
                    // 尝试站立
                    if (CheckCeilingClearance())
                    {
                        crouchState = CrouchState.TransitioningToStand;
                        crouchTransitionTime = 0f;
                        isCrouching = false;
                        targetHeight = standingHeight;
                        Debug.Log("开始站立");
                    }
                    else
                    {
                        Debug.Log("头顶空间不足，保持蹲下");
                    }
                }
                break;

            case CrouchState.TransitioningToCrouch:
                crouchTransitionTime += Time.deltaTime;
                if (crouchTransitionTime >= crouchTransitionDuration)
                {
                    crouchState = CrouchState.Crouching;
                    Debug.Log("蹲下完成");
                }
                break;

            case CrouchState.TransitioningToStand:
                crouchTransitionTime += Time.deltaTime;
                if (crouchTransitionTime >= crouchTransitionDuration)
                {
                    crouchState = CrouchState.Standing;
                    Debug.Log("站立完成");
                }
                break;
        }

        // 如果玩家在站立状态且头顶有障碍物，强制蹲下
        if (crouchState == CrouchState.Standing && Time.time > lastCeilingCheckTime + ceilingCheckCooldown)
        {
            canStandUp = CheckCeilingClearance();
            lastCeilingCheckTime = Time.time;

            // 如果无法站立，强制蹲下
            if (!canStandUp)
            {
                crouchState = CrouchState.TransitioningToCrouch;
                crouchTransitionTime = 0f;
                isCrouching = true;
                targetHeight = crouchHeight;
                Debug.Log("强制蹲下：头顶有障碍物");
            }
        }
    }

    // 检查头顶是否有足够的空间站立
    bool CheckCeilingClearance()
    {
        // 简化检测逻辑
        Vector3 capsuleTop = transform.position + Vector3.up * (standingHeight - 0.1f);

        // 只使用一个中心射线检测
        return !Physics.Raycast(capsuleTop, Vector3.up, 0.2f, groundLayer);
    }

    void UpdatePlayerHeight()
    {
        // 根据状态机决定高度
        float transitionProgress = Mathf.Clamp01(crouchTransitionTime / crouchTransitionDuration);

        if (crouchState == CrouchState.TransitioningToCrouch || crouchState == CrouchState.TransitioningToStand)
        {
            // 使用缓动函数实现平滑过渡
            float t = EaseInOutCubic(transitionProgress);
            currentHeight = Mathf.Lerp(
                crouchState == CrouchState.TransitioningToCrouch ? standingHeight : crouchHeight,
                crouchState == CrouchState.TransitioningToCrouch ? crouchHeight : standingHeight,
                t
            );
        }
        else
        {
            // 非过渡状态，直接设置目标高度
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * 15f);
        }

        // 确保高度不会过度变化
        if (Mathf.Abs(currentHeight - targetHeight) < 0.01f)
        {
            currentHeight = targetHeight;
        }

        // 更新CharacterController的高度和中心点
        if (characterController != null)
        {
            characterController.height = currentHeight;

            // 计算新的中心点
            Vector3 newCenter = new Vector3(0, currentHeight / 2f, 0);

            // 直接设置中心点，避免插值引起的抖动
            characterController.center = newCenter;
        }

        // 更新摄像机位置
        UpdateCameraPosition();
    }

    // 缓动函数：三次缓入缓出
    float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    void UpdateCameraPosition()
    {
        if (cameraTransform != null)
        {
            Vector3 camPos = cameraTransform.localPosition;
            float targetY = currentHeight - 0.1f; // 稍微低于头顶

            // 平滑更新摄像机高度
            float smoothSpeed = 15f;
            camPos.y = Mathf.Lerp(camPos.y, targetY, Time.deltaTime * smoothSpeed);
            cameraTransform.localPosition = camPos;
        }
    }

    void HandleMouseLook()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 使用平滑的旋转
        playerTransform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // 使用Slerp实现平滑旋转
        Quaternion targetRotation = Quaternion.Euler(xRotation, 0f, 0f);
        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation,
            targetRotation,
            Time.deltaTime * 15f
        );
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

        // 获取当前移动速度（考虑蹲下状态）
        float targetSpeed = GetCurrentMoveSpeed();

        // 冲刺条件：按住Shift、有移动输入、不处于蹲下状态、在地面上
        if (shiftPressed && (horizontal != 0 || vertical != 0) && crouchState != CrouchState.Crouching && isGrounded)
        {
            targetSpeed = runSpeed;
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        // 平滑过渡速度
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

        // 计算速度
        Vector3 horizontalVelocity = moveDirection * currentSpeed;
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;
    }

    // 获取当前移动速度（蹲下时变慢）
    float GetCurrentMoveSpeed()
    {
        if (crouchState == CrouchState.Crouching)
        {
            return crouchSpeed;
        }

        // 过渡期间使用蹲下速度
        if (crouchState == CrouchState.TransitioningToCrouch || crouchState == CrouchState.TransitioningToStand)
        {
            return Mathf.Lerp(walkSpeed, crouchSpeed, crouchTransitionTime / crouchTransitionDuration);
        }

        return isSprinting ? runSpeed : walkSpeed;
    }

    void HandleJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            // 如果在蹲下状态，先检查能否站立
            if (crouchState == CrouchState.Crouching || crouchState == CrouchState.TransitioningToCrouch)
            {
                if (CheckCeilingClearance())
                {
                    // 强制站立
                    crouchState = CrouchState.Standing;
                    isCrouching = false;
                    targetHeight = standingHeight;
                    currentHeight = standingHeight;
                    Debug.Log("强制站立并跳跃");
                }
                else
                {
                    Debug.Log("头顶空间不足，无法跳跃");
                    return; // 无法跳跃
                }
            }

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log($"执行跳跃！垂直速度: {velocity.y}");
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 轻微向下力，确保贴地
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
        if (cameraTransform == null) return;

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
            Rigidbody rb = currentTool.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = currentTool.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false;
            Debug.Log($"丢弃工具: {currentTool.name}");
            currentTool = null;
            hasToolEquipped = false;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 250));

        GUILayout.Label($"=== 玩家状态 ===");
        GUILayout.Label($"位置: {playerTransform.position:F2}");
        GUILayout.Label($"状态: {(isGrounded ? "在地面" : "在空中")}");
        GUILayout.Label($"姿势: {GetPostureDescription()}");
        GUILayout.Label($"蹲下状态: {crouchState}");
        GUILayout.Label($"高度: {currentHeight:F2}");
        GUILayout.Label($"水平速度: {new Vector3(velocity.x, 0, velocity.z).magnitude:F2} m/s");
        GUILayout.Label($"垂直速度: {velocity.y:F2}");
        GUILayout.Label($"工具: {(hasToolEquipped ? currentTool.name : "无")}");

        GUILayout.Label($"");
        GUILayout.Label($"=== 控制说明 ===");
        GUILayout.Label($"WASD: 移动");
        GUILayout.Label($"鼠标: 视角");
        GUILayout.Label($"左Shift: 冲刺");
        GUILayout.Label($"左Ctrl: 蹲下/站立");
        GUILayout.Label($"空格: 跳跃");
        GUILayout.Label($"E: 使用/拾取工具");
        GUILayout.Label($"Q: 丢弃工具");

        GUILayout.EndArea();
    }

    string GetPostureDescription()
    {
        switch (crouchState)
        {
            case CrouchState.Standing:
                return isSprinting ? "冲刺中" : "站立中";
            case CrouchState.Crouching:
                return "蹲下中";
            case CrouchState.TransitioningToCrouch:
                return "蹲下中...";
            case CrouchState.TransitioningToStand:
                return "站立中...";
            default:
                return "站立中";
        }
    }

    // ========== 公共方法（可选，用于外部控制） ==========

    /// <summary>
    /// 强制设置蹲下状态
    /// </summary>
    public void SetCrouching(bool crouch)
    {
        if (crouch)
        {
            crouchState = CrouchState.Crouching;
            isCrouching = true;
            targetHeight = crouchHeight;
        }
        else if (CheckCeilingClearance())
        {
            crouchState = CrouchState.Standing;
            isCrouching = false;
            targetHeight = standingHeight;
        }
    }

    /// <summary>
    /// 强制设置冲刺状态
    /// </summary>
    public void SetSprinting(bool sprint)
    {
        isSprinting = sprint;
    }

    /// <summary>
    /// 获取当前速度向量
    /// </summary>
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    /// <summary>
    /// 获取是否在地面
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// 获取当前蹲下状态
    /// </summary>
    public CrouchState GetCrouchState()
    {
        return crouchState;
    }
}