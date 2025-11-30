using UnityEngine;
using GameInventory; // เรียกใช้ระบบ Inventory ของคุณ

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Mouse Look")]
    [SerializeField] private Transform cameraT;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minLookX = -90f;
    [SerializeField] private float maxLookX = 60f;

    [Header("Jump & Ground")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckRadius = 0.3f;

    [Header("Crouch")]
    [SerializeField] private float normalHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 8f;

    [Header("Sprint & Stamina")]
    [SerializeField] private float maxStamina = 5f;
    [SerializeField] private float staminaRegenRate = 1f;
    [SerializeField] private float sprintDrainRate = 1f;

    [Header("FOV Kick")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovTransitionSpeed = 6f;

    [Header("UI Control")]
    [SerializeField] private GameObject allCanvasUI;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayer;

    private bool isMenuOpen = false;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private float currentLookX;
    private float currentStamina;
    private bool isGrounded;
    private float targetHeight;
    private float cameraOriginalY;

    // Cache ระบบต่างๆ
    private Inventory myInventory;
    private Player friendPlayerScript; // ✅ ตัวแปรสำหรับเก็บสคริปต์ Player ของเพื่อน

    void Awake()
    {
        if (!TryGetComponent(out rb)) Debug.LogError("Rigidbody missing!");
        if (!TryGetComponent(out capsule)) Debug.LogError("CapsuleCollider missing!");

        // Unity 6: ใช้ FindFirstObjectByType จะเร็วกว่า FindObjectOfType
        myInventory = FindFirstObjectByType<Inventory>();

        // ✅ ค้นหา Script 'Player' (ของเพื่อน) ที่แปะอยู่บนตัวละครนี้
        if (!TryGetComponent(out friendPlayerScript))
        {
            // ถ้าหาไม่เจอในตัว ลองหาในลูกๆ (เผื่อไว้)
            friendPlayerScript = GetComponentInChildren<Player>();
        }

        rb.freezeRotation = true;
    }

    void Start()
    {
        currentStamina = maxStamina;
        targetHeight = normalHeight;

        if (cameraT != null) cameraOriginalY = cameraT.localPosition.y;
        if (playerCamera != null) playerCamera.fieldOfView = normalFOV;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (allCanvasUI != null) allCanvasUI.SetActive(false);
    }

    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;
    }

    void Update()
    {
        HandleMenuToggle();

        if (!isMenuOpen)
        {
            CheckGround();
            HandleMouseLook();
            HandleInteraction(); // เช็คการกดปุ่ม E

            bool wantsCrouch = Input.GetKey(KeyCode.LeftControl);
            HandleCrouch(wantsCrouch);

            bool wantsSprint = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0f && !wantsCrouch;
            HandleMovement(wantsSprint, wantsCrouch);

            HandleSprintStamina(wantsSprint);
            HandleFOV(wantsSprint);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
                Jump();
        }
    }

    // ---------------------------------------------------------
    // ✅ ฟังก์ชันเชื่อมต่อระบบ (Interaction Bridge)
    // ---------------------------------------------------------
    private void HandleInteraction()
    {
        // กด E เพื่อโต้ตอบ
        if (Input.GetKeyDown(KeyCode.E))
        {
            // ยิง Ray จากกลางหน้าจอ
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
            {
                // 1. ระบบเก็บของ (Inventory ของคุณ)
                // เช็คว่าสิ่งที่ชนมีสคริปต์ ItemObject หรือไม่
                if (hit.collider.TryGetComponent(out ItemObject itemObj))
                {
                    if (myInventory != null)
                    {
                        bool added = myInventory.AddItem(itemObj.item, itemObj.amount);
                        if (added)
                        {
                            // ถ้าเก็บสำเร็จ ให้ทำลายของในฉากทิ้ง
                            Destroy(itemObj.gameObject);
                        }
                        else
                        {
                            Debug.Log("Inventory Full!");
                        }
                    }
                    return; // จบการทำงาน ถ้าเป็นไอเท็ม ไม่ต้องเช็คอย่างอื่นต่อ
                }

                // 2. ระบบ Interact ของเพื่อน (IInteractable) เช่น ประตู, NPC, สวิตช์
                // เช็คว่าสิ่งที่ชนมี Interface IInteractable หรือไม่
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    // ✅ แก้ไข Error ตรงนี้: ส่ง 'friendPlayerScript' เข้าไปในฟังก์ชัน
                    if (friendPlayerScript != null)
                    {
                        interactable.Interact(friendPlayerScript);
                    }
                    else
                    {
                        Debug.LogError("Error: ไม่พบสคริปต์ 'Player' (ของเพื่อน) บนตัวละครนี้! กรุณาลากไฟล์ Player.cs ใส่ที่ตัวละคร");
                    }
                }
            }
        }
    }

    // ---------------------------------------------------------
    // Movement Logic (คงเดิม)
    // ---------------------------------------------------------
    private void HandleMenuToggle()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (allCanvasUI != null)
            {
                bool newState = !allCanvasUI.activeSelf;
                allCanvasUI.SetActive(newState);
                SetMenuOpen(newState);
            }
        }
    }

    private void SetMenuOpen(bool open)
    {
        if (isMenuOpen == open) return;
        isMenuOpen = open;
        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void CheckGround()
    {
        Vector3 spherePos = transform.position + Vector3.down * (capsule.height * 0.5f - capsule.radius + 0.05f);
        isGrounded = Physics.CheckSphere(spherePos, groundCheckRadius, groundMask);
    }

    private void HandleMouseLook()
    {
        if (cameraT == null) return;
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mx, 0f);
        currentLookX = Mathf.Clamp(currentLookX - my, minLookX, maxLookX);
        cameraT.localRotation = Quaternion.Euler(currentLookX, 0f, 0f);
    }

    private void HandleMovement(bool sprint, bool crouch)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = (transform.forward * v + transform.right * h).normalized;

        float speed = crouch ? crouchSpeed : (sprint ? runSpeed : walkSpeed);

        // Unity 6: ใช้ linearVelocity แทน velocity (แต่ velocity ก็ยังใช้ได้)
        Vector3 currentVel = rb.linearVelocity;
        Vector3 targetVel = dir * speed;

        rb.linearVelocity = new Vector3(targetVel.x, currentVel.y, targetVel.z);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void HandleCrouch(bool wantsCrouch)
    {
        if (cameraT == null) return;
        targetHeight = wantsCrouch ? crouchHeight : normalHeight;

        float lastHeight = capsule.height;
        capsule.height = Mathf.Lerp(capsule.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        float heightDiff = lastHeight - capsule.height;
        Vector3 center = capsule.center;
        center.y -= heightDiff * 0.5f;
        capsule.center = center;

        float camTargetY = cameraOriginalY - (normalHeight - targetHeight);
        Vector3 camPos = cameraT.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, camTargetY, Time.deltaTime * crouchTransitionSpeed);
        cameraT.localPosition = camPos;
    }

    private void HandleSprintStamina(bool sprinting)
    {
        if (sprinting)
            currentStamina = Mathf.Max(0f, currentStamina - sprintDrainRate * Time.deltaTime);
        else
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
    }

    private void HandleFOV(bool sprinting)
    {
        if (!playerCamera) return;
        float target = sprinting ? sprintFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, target, Time.deltaTime * fovTransitionSpeed);
    }
}