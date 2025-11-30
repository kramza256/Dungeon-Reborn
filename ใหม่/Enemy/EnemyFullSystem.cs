using UnityEngine;
using System.Collections;
using System;
using GameInventory;

[System.Serializable]
public class EnemyLoot
{
    public SO_Item item;
    public int amount = 1;
    [Range(0, 100)] public float dropChance = 50f;
}

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class EnemyFullSystem : MonoBehaviour, Idestoryable
{
    // ==========================================
    // Interface Idestoryable (Health Properties)
    // ==========================================
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;
    public int health { get { return _currentHealth; } set { _currentHealth = Mathf.Clamp(value, 0, _maxHealth); } }
    public int maxHealth { get => _maxHealth; set => _maxHealth = value; }
    public event Action<Idestoryable> OnDestory;

    // ==========================================
    // AI Settings & Combat
    // ==========================================
    public enum AttackType { Melee, Ranged }

    [Header("--- Combat Style ---")]
    public AttackType combatStyle = AttackType.Melee;
    public float damage = 10f;
    public float moveSpeed = 3f;
    public float attackRange = 3.5f;
    public float attackCooldown = 1.5f;

    [Header("--- Ranged Settings ---")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    [Header("--- Vision & Detection ---")]
    public float sightRange = 15f;          // ✅ ระยะสูงสุดที่มองเห็น
    public LayerMask obstacleMask;          // ✅ Layers ที่ใช้บังสายตา
    public float visionCheckRate = 0.2f;
    private float visionTimer = 0f;
    private bool isPlayerVisible = false; // สถานะการมองเห็น

    [Header("--- Skill System ---")]
    public SO_Skill[] enemySkills;
    [Range(0, 100)] public float skillUseChance = 50f;
    public float skillGlobalCooldown = 8f;
    private float nextSkillTime = 0f;
    private float lastAttackTime = 0f;

    [Header("--- Loot Drops ---")]
    public EnemyLoot[] lootTable;

    private Animator anim;
    private Rigidbody rb;
    private Transform targetPlayer;
    private bool isDead = false;

    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        rb.freezeRotation = true;
        FindPlayer();
    }

    void Update()
    {
        if (isDead) return;

        // 1. เช็คสายตาเป็นระยะ (ลดภาระ CPU)
        visionTimer -= Time.deltaTime;
        if (visionTimer <= 0)
        {
            CheckForTarget();
            visionTimer = visionCheckRate;
        }

        // 2. ถ้ามองไม่เห็น หรือผู้เล่นตาย/หายไป ให้ยืนนิ่ง
        if (targetPlayer == null || !isPlayerVisible)
        {
            StopMoving();
            return;
        }

        // --- Combat & Movement Logic (ถ้าเห็นตัวเรา) ---
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        LookAtPlayer();

        float effectiveRange = (combatStyle == AttackType.Ranged) ? attackRange + 5f : attackRange;

        if (distance <= effectiveRange)
        {
            StopMoving();

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                bool isSkillReady = (enemySkills.Length > 0 && Time.time >= nextSkillTime);
                bool rngCheck = (UnityEngine.Random.Range(0, 100) <= skillUseChance);

                if (isSkillReady && rngCheck) UseRandomSkill();
                else PerformBasicAttack();

                lastAttackTime = Time.time;
            }
        }
        else
        {
            MoveToPlayer();
        }
    }

    // =========================================================
    // VISION LOGIC
    // =========================================================
    void FindPlayer() { GameObject p = GameObject.FindGameObjectWithTag("Player"); if (p != null) targetPlayer = p.transform; }

    void CheckForTarget()
    {
        if (targetPlayer == null) { isPlayerVisible = false; return; }

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        if (distance > sightRange)
        {
            isPlayerVisible = false;
            return;
        }

        Vector3 directionToTarget = (targetPlayer.position - transform.position).normalized;

        // ยิง Ray จากระดับสายตาไปหาผู้เล่น
        if (Physics.Raycast(transform.position + Vector3.up, directionToTarget, out RaycastHit hit, distance, obstacleMask))
        {
            // ถ้าชนอะไรที่ไม่ใช่ผู้เล่นก่อนถึงตัวผู้เล่น ถือว่ามองไม่เห็น
            isPlayerVisible = false;
            Debug.DrawRay(transform.position + Vector3.up, directionToTarget * distance, Color.yellow, visionCheckRate);
        }
        else
        {
            isPlayerVisible = true;
        }
    }


    // ... (ส่วน Combat, Skill, Move, TakeDamage, Die เหมือนเดิม) ...
    void PerformBasicAttack()
    {
        anim.SetTrigger("Attack");

        if (combatStyle == AttackType.Melee)
        {
            StartCoroutine(MeleeDamageDelay(0.5f));
        }
        else
        {
            StartCoroutine(RangedShotDelay(0.3f));
        }
    }

    IEnumerator MeleeDamageDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) <= attackRange + 1f)
        {
            var pHp = targetPlayer.GetComponent<Idestoryable>();
            if (pHp != null) pHp.TakeDamage((int)damage);
            else { var pChar = targetPlayer.GetComponent<Character>(); if (pChar != null) pChar.TakeDamage((int)damage); }
        }
    }

    IEnumerator RangedShotDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + Vector3.up;
            GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            bullet.transform.LookAt(targetPlayer.position + Vector3.up);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null) bulletRb.linearVelocity = bullet.transform.forward * projectileSpeed;
            EnemyProjectile ep = bullet.GetComponent<EnemyProjectile>();
            if (ep != null) ep.damage = (int)damage;
        }
    }

    void UseRandomSkill()
    {
        int index = UnityEngine.Random.Range(0, enemySkills.Length);
        SO_Skill skillToUse = enemySkills[index];

        Debug.Log($"<color=red>Enemy Casting: {skillToUse.skillName}!</color>");

        if (skillToUse.effectPrefab != null)
            Instantiate(skillToUse.effectPrefab, transform.position, Quaternion.identity);

        anim.SetTrigger("Skill");

        switch (skillToUse.type)
        {
            case SO_Skill.SkillType.Heal:
                Heal((int)skillToUse.value);
                break;
            case SO_Skill.SkillType.BuffSpeed:
                StartCoroutine(BuffSpeedRoutine(skillToUse.value, 5f));
                break;
            case SO_Skill.SkillType.AttackAOE:
                Collider[] hits = Physics.OverlapSphere(transform.position, 5f);
                foreach (var h in hits)
                {
                    if (h.CompareTag("Player"))
                    {
                        var hp = h.GetComponent<Idestoryable>();
                        if (hp != null) hp.TakeDamage((int)skillToUse.value);
                    }
                }
                break;
        }

        nextSkillTime = Time.time + skillGlobalCooldown;
    }

    IEnumerator BuffSpeedRoutine(float extraSpeed, float duration) { float originalSpeed = moveSpeed; moveSpeed += extraSpeed; Debug.Log("Boss Enraged! Speed Up!"); yield return new WaitForSeconds(duration); moveSpeed = originalSpeed; }
    void LookAtPlayer() { if (targetPlayer == null) return; Vector3 direction = (targetPlayer.position - transform.position).normalized; direction.y = 0; if (direction != Vector3.zero) { Quaternion lookRot = Quaternion.LookRotation(direction); transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f); } }
    void MoveToPlayer() { anim.SetBool("Attack", false); Vector3 dir = (targetPlayer.position - transform.position).normalized; rb.linearVelocity = new Vector3(dir.x * moveSpeed, rb.linearVelocity.y, dir.z * moveSpeed); anim.SetFloat("Speed", moveSpeed); }
    void StopMoving() { rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); anim.SetFloat("Speed", 0); }
    public void TakeDamage(int amount) { if (isDead) return; health -= amount; if (health <= 0) Die(); }
    public void Heal(int amount) { health += amount; if (health > maxHealth) health = maxHealth; }
    void Die() { isDead = true; OnDestory?.Invoke(this); StopMoving(); anim.SetTrigger("Die"); GetComponent<Collider>().enabled = false; rb.isKinematic = true; DropLoot(); Destroy(gameObject, 4f); }
    void DropLoot() { if (targetPlayer == null) return; var playerInv = targetPlayer.GetComponent<Player>()?.inventorySystem; if (playerInv == null) playerInv = FindFirstObjectByType<Inventory>(); if (playerInv != null) { foreach (var loot in lootTable) { if (UnityEngine.Random.Range(0f, 100f) <= loot.dropChance) playerInv.AddItem(loot.item, loot.amount); } } }
    private void OnDrawGizmosSelected() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange); }
}
