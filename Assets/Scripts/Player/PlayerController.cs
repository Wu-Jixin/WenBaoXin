using UnityEngine;
using UnityEngine.UI; // 添加UI命名空间

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("蹲下设置")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchSpeed = 3f;

    [Header("体力系统")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaConsumptionRate = 20f;
    [SerializeField] private float staminaRecoveryRate = 15f;
    [SerializeField] private float minStaminaToSprint = 10f;
    private float currentStamina;

    [Header("UI引用")]
    public Slider staminaSlider; // 体力条引用

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

        // 初始化体力
        currentStamina = maxStamina;
        UpdateStaminaUI();

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

        // 4. 处理玩家移动（包含体力系统）
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
        // 方法1: 使用 CharacterController.isGrounded
        isGrounded = characterController.isGrounded;

        // 方法2: 使用射线检测作为备用
        if (!isGrounded)
        {
            Vector3 rayStart = playerTransform.position;
            rayStart.y += 0.1f;

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
        Vector3 capsuleTop = transform.position + Vector3.up * (standingHeight - 0.1f);
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
            Vector3 newCenter = new Vector3(0, currentHeight / 2f, 0);
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
            float targetY = currentHeight - 0.1f;

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

        // 冲刺检测（包含体力检查）
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        bool hasMovementInput = (horizontal != 0 || vertical != 0);
        bool hasEnoughStamina = currentStamina >= minStaminaToSprint;

        float targetSpeed = GetCurrentMoveSpeed();

        if (shiftPressed && hasMovementInput && crouchState != CrouchState.Crouching && isGrounded && hasEnoughStamina)
        {
            targetSpeed = runSpeed;
            isSprinting = true;

            // 消耗体力
            currentStamina -= staminaConsumptionRate * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }
        else
        {
            isSprinting = false;

            // 恢复体力（不冲刺时）
            if (!isSprinting && currentStamina < maxStamina)
            {
                currentStamina += staminaRecoveryRate * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }

        // 如果体力不足，强制退出冲刺
        if (currentStamina < minStaminaToSprint)
        {
            isSprinting = false;
            targetSpeed = walkSpeed;
        }

        // 平滑过渡速度
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

        // 计算速度
        Vector3 horizontalVelocity = moveDirection * currentSpeed;
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        // 更新体力UI
        UpdateStaminaUI();
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

    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina / maxStamina;

            // 低体力警告
            if (currentStamina < 20f)
            {
                staminaSlider.fillRect.GetComponent<Image>().color = Color.red;
            }
            else if (currentStamina < 50f)
            {
                staminaSlider.fillRect.GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                staminaSlider.fillRect.GetComponent<Image>().color = Color.green;
            }
        }
    }

    void HandleToolInteraction()
    {
        // 鼠标左键使用工具
        if (Input.GetMouseButtonDown(0))
        {
            ToolSystem toolSystem = FindObjectOfType<ToolSystem>();
            if (toolSystem != null)
            {
                toolSystem.UseCurrentTool();
            }
        }

        // E键保持拾取功能
        if (Input.GetKeyDown(interactKey))
        {
            if (!hasToolEquipped)
            {
                TryPickupTool();
            }
            else
            {
                // 如果有工具系统，使用工具
                ToolSystem toolSystem = FindObjectOfType<ToolSystem>();
                if (toolSystem != null)
                {
                    toolSystem.UseCurrentTool();
                }
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
        GUILayout.Label($"体力: {currentStamina:F0}/{maxStamina}");
        GUILayout.Label($"工具: {(hasToolEquipped ? currentTool.name : "无")}");

        GUILayout.Label($"");
        GUILayout.Label($"=== 控制说明 ===");
        GUILayout.Label($"WASD: 移动");
        GUILayout.Label($"鼠标: 视角");
        GUILayout.Label($"左Shift: 冲刺");
        GUILayout.Label($"左Ctrl: 蹲下/站立");
        GUILayout.Label($"空格: 跳跃");
        GUILayout.Label($"鼠标左键: 使用工具");
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

    // ========== 公共方法 ==========

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

    /// <summary>
    /// 获取当前体力值
    /// </summary>
    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    /// <summary>
    /// 设置体力值（用于调试）
    /// </summary>
    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0, maxStamina);
        UpdateStaminaUI();
    }
}