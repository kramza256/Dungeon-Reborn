using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifeTime = 5f;
    public GameObject hitEffect;

    void Start()
    {
        Destroy(gameObject, lifeTime); // ทำลายตัวเองเมื่อหมดเวลา
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ทำดาเมจใส่ผู้เล่น
            var hp = other.GetComponent<Idestoryable>();
            if (hp != null) hp.TakeDamage(damage);
            else
            {
                var charBase = other.GetComponent<Character>();
                if (charBase != null) charBase.TakeDamage(damage);
            }

            Hit();
        }
        else if (!other.CompareTag("Enemy") && !other.isTrigger)
        {
            Hit(); // ชนกำแพงก็ระเบิด
        }
    }

    void Hit()
    {
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}