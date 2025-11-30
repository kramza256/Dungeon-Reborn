using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameInventory;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Player : Character
{
    [Header("Inventory System")]
    public Inventory inventorySystem;

    [Header("UI Control")]
    public GameObject allCanvasUI;
    public GameObject minimapUI;
    public GameObject gameOverUI;

    [Header("Health UI")]
    public UnityEngine.UI.Slider hpSlider;
    [Header("Stamina UI")]
    public UnityEngine.UI.Slider staminaSlider;

    [Header("Inventory Sync")]
    public List<string> inventoryListDisplay = new List<string>();

    [Header("Hand Settings (สำหรับถืออาวุธ)")]
    public Transform RightHand; // ลาก GameObject ที่อยู่ในมือขวามาใส่
    public Transform LeftHand;

    // ตัวแปรเก็บโมเดลอาวุธที่ถืออยู่ปัจจุบัน
    private GameObject currentWeaponModel;

    // --- State Variables ---
    bool isMenuOpen = false;
    Vector3 _inputDirection;
    bool _isAttacking = false;
    bool _isInteract = false;

    // ตัวแปรสำหรับระบบกลิ้ง
    bool _isRolling = false;
    Vector3 _rollDirection;

    [Header("Status System")]
    public Status status;

    [Header("Player Specific Stats")]
    public float MaxHealth = 100;
    public float hp;
    public float MovementSpeed = 6f;
    public float TurnSpeed = 15f;

    [Header("Roll Settings")]
    public float rollSpeed = 12f;
    public float rollDuration = 0.8f;
    public float rollStaminaCost = 20f;

    [Header("Stamina Settings")]
    public float runDuration = 5f;
    public float recoveryDuration = 3f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackOffset = 1.0f;

    void Start()
    {
        if (!TryGetComponent(out rb)) Debug.LogError("Rigidbody Missing!");
        if (!TryGetComponent(out animator)) Debug.LogError("Animator Missing!");

        // ล็อคการหมุนทางฟิสิกส์ (กันตัวล้ม/หมุนติ้ว)
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (status != null)
        {
            this.MaxHealth = status.maxHP;
            this.hp = status.maxHP;
            status.stamina = status.maxStamina;
        }
        else
        {
            this.hp = this.MaxHealth;
        }

        if (hpSlider != null) { hpSlider.maxValue = MaxHealth; hpSlider.value = hp; }
        if (staminaSlider != null && status != null) { staminaSlider.maxValue = status.maxStamina; staminaSlider.value = status.stamina; }

        this.Name = "Player";
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (allCanvasUI != null) allCanvasUI.SetActive(false);
        if (minimapUI != null) minimapUI.SetActive(true);
        if (gameOverUI != null) gameOverUI.SetActive(false);
    }

    public void FixedUpdate()
    {
        if (isMenuOpen || hp <= 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        // Logic การเคลื่อนที่ตอนกลิ้ง
        if (_isRolling)
        {
            float currentY = rb.linearVelocity.y;
            Vector3 rollVel = _rollDirection * rollSpeed;

            rb.linearVelocity = new Vector3(rollVel.x, currentY, rollVel.z);
            rb.angularVelocity = Vector3.zero; // ฆ่าแรงหมุนทิ้ง
            return;
        }

        // --- ระบบเดินปกติ ---
        bool sprint = false;
        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
            sprint = true;

        if (status != null && status.stamina <= 0) sprint = false;

        float moveSpeed = status != null ? status.CurrentSpeed(sprint) : MovementSpeed;
        Vector3 move = _inputDirection.normalized * moveSpeed;

        Move(move);

        if (_inputDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * TurnSpeed);
        }

        rb.angularVelocity = Vector3.zero;

        Attack(_isAttacking);
        Interact(_isInteract);

        if (status != null)
        {
            if (sprint && _inputDirection.magnitude > 0.1f)
            {
                float drainAmount = (status.maxStamina / runDuration) * Time.fixedDeltaTime;
                status.DrainStamina(drainAmount);
            }
            else
            {
                float regenAmount = (status.maxStamina / recoveryDuration) * Time.fixedDeltaTime;
                if (status.stamina < status.maxStamina) status.stamina += regenAmount;
            }
            UpdateStaminaUI();
        }
    }

    protected override void Move(Vector3 move)
    {
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        float speed = new Vector2(move.x, move.z).magnitude;
        animator.SetFloat("Speed", speed);
    }

    public void Update()
    {
        if (hp <= 0) return;
        HandleInput();
        HandleMenuToggle();
    }

    private void HandleInput()
    {
        if (isMenuOpen) return;

        float x = 0;
        float y = 0;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) x -= 1;
            if (Keyboard.current.dKey.isPressed) x += 1;
            if (Keyboard.current.sKey.isPressed) y -= 1;
            if (Keyboard.current.wKey.isPressed) y += 1;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && !_isRolling && !_isAttacking)
            {
                StartCoroutine(PerformRoll());
            }
        }

        _inputDirection = new Vector3(x, 0, y).normalized;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            _isAttacking = true;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            _isInteract = true;
    }

    IEnumerator PerformRoll()
    {
        if (status != null && status.stamina < rollStaminaCost)
        {
            Debug.Log("Stamina ไม่พอ!");
            yield break;
        }

        if (status != null) status.DrainStamina(rollStaminaCost);

        _isRolling = true;

        if (_inputDirection.magnitude > 0.1f)
        {
            _rollDirection = _inputDirection.normalized;
            transform.rotation = Quaternion.LookRotation(_rollDirection);
        }
        else
        {
            _rollDirection = transform.forward;
        }

        animator.SetTrigger("Roll");
        yield return new WaitForSeconds(rollDuration);

        _isRolling = false;
        rb.linearVelocity = Vector3.zero;
    }

    // ✅✅✅ ส่วนจัดการระบบถืออาวุธ (Equip System) ✅✅✅
    public void EquipWeapon(SO_Item item)
    {
        // 1. ลบอาวุธเก่าทิ้งก่อน (ถ้ามี)
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        // 2. ถ้าไอเท็มมีโมเดล 3D และมีการตั้งค่ามือขวาไว้
        if (item.gamePrefab != null && RightHand != null)
        {
            // สร้างโมเดลอาวุธขึ้นมา
            currentWeaponModel = Instantiate(item.gamePrefab, RightHand);

            // รีเซ็ตตำแหน่งให้ตรงกับมือเป๊ะๆ
            currentWeaponModel.transform.localPosition = Vector3.zero;
            currentWeaponModel.transform.localRotation = Quaternion.identity;
        }

        // (Optional) เพิ่ม Stats ดาเมจตรงนี้ถ้าต้องการ
        // if (status != null) status.damage += item.damage; // สมมติว่าใน item มีค่า damage

        Debug.Log($"Equipped: {item.itemName}");
    }

    public void UnequipWeapon()
    {
        // ลบโมเดลทิ้ง
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        // (Optional) ลด Stats ดาเมจคืนตรงนี้

        Debug.Log("Unequipped Weapon");
    }
    // ✅✅✅ จบส่วนถืออาวุธ ✅✅✅

    private void HandleMenuToggle()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (allCanvasUI != null)
            {
                bool newState = !allCanvasUI.activeSelf;
                allCanvasUI.SetActive(newState);
                if (minimapUI != null) minimapUI.SetActive(!newState);

                isMenuOpen = newState;
                if (isMenuOpen) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
                else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            }
        }
    }

    public void AddItem(SO_Item item, int amount = 1)
    {
        if (inventorySystem != null) inventorySystem.AddItem(item, amount);
    }

    public void Attack(bool isAttacking)
    {
        if (isAttacking && !_isRolling)
        {
            animator.SetTrigger("Attack");

            Vector3 attackPoint = transform.position + (transform.forward * attackOffset) + (Vector3.up * 1.0f);
            Collider[] hitColliders = Physics.OverlapSphere(attackPoint, attackRange);

            foreach (var hit in hitColliders)
            {
                if (hit.gameObject == gameObject) continue;
                float dmg = status != null ? status.damage : 25;

                var character = hit.GetComponent<Character>();
                if (character != null) character.TakeDamage((int)dmg);

                var destoryable = hit.GetComponent<Idestoryable>();
                if (destoryable != null) destoryable.TakeDamage((int)dmg);
            }
            _isAttacking = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPoint = transform.position + (transform.forward * attackOffset) + (Vector3.up * 1.0f);
        Gizmos.DrawWireSphere(attackPoint, attackRange);
    }

    public override void TakeDamage(int amount)
    {
        if (status != null)
        {
            status.TakeDamage(amount);
            this.hp = status.hp;
        }
        else
        {
            this.hp -= amount;
            if (this.hp <= 0) this.hp = 0;
        }
        UpdateHealthUI();
        if (this.hp <= 0) Die();
    }

    public override void Heal(int amount)
    {
        if (status != null)
        {
            status.Heal(amount);
            this.hp = status.hp;
        }
        else
        {
            this.hp += amount;
            if (this.hp > MaxHealth) this.hp = MaxHealth;
        }
        UpdateHealthUI();
    }

    private void UpdateHealthUI() { if (hpSlider != null) hpSlider.value = this.hp; }
    private void UpdateStaminaUI() { if (staminaSlider != null && status != null) staminaSlider.value = status.stamina; }

    private void Die()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        this.enabled = false;
    }

    private void Interact(bool interactable)
    {
        if (interactable)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward, 1.5f);
            foreach (var hit in hitColliders)
            {
                IInteractable e = hit.GetComponent<IInteractable>();
                if (e != null)
                {
                    e.Interact(this);
                    break;
                }
            }
            _isInteract = false;
        }
    }
}