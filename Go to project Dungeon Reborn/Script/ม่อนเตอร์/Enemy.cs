using UnityEngine;

public class Enemy : Character
{
    protected enum State { idle, cheses, attack, death }
    protected State currentState = State.idle;

    [SerializeField]
    private float TimeToAttack = 1f;
    protected float timer = 0f;

    // ... (ส่วน Update ให้ใช้ของ EnemyMovetoPlayer อันเดิมได้เลย) ...

    public override void TakeDamage(int amount)
    {
        if (currentState == State.death) return;
        Debug.Log($"{gameObject.name} took {amount} damage! (Current HP: {health})");
        base.TakeDamage(amount);
        if (health <= 0) Die();
    }

    public virtual void Die()
    {
        currentState = State.death;
        animator.SetBool("Attack", false);
        animator.SetFloat("Speed", 0); // หยุดเดินตอนตาย
        animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    // ✅ แก้ฟังก์ชันนี้ครับ
    protected virtual void Attack(Player _player)
    {
        if (timer <= 0 && currentState != State.death)
        {
            // 1. สั่งหยุดขา (สำคัญมาก! ไม่งั้นมันจะเดินย่ำเท้าอยู่กับที่)
            animator.SetFloat("Speed", 0);

            // 2. ทำดาเมจ
            _player.TakeDamage((int)Damage);

            // 3. สั่งเล่นท่าโจมตี (ใช้ Trigger ดีกว่า Bool)
            // *ต้องไปแก้ใน Animator ให้ตัวแปร Attack เป็นแบบ Trigger ด้วยนะครับ*
            animator.SetTrigger("Attack");
            // หรือถ้ายังใช้ Bool อยู่ ให้ใช้บรรทัดนี้แทน: animator.SetBool("Attack", true);

            timer = TimeToAttack;
        }
    }
}