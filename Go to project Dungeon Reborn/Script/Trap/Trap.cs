using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Move Setting")]
    public Transform pointA;         // จุดเริ่มต้น
    public Transform pointB;         // จุดปลาย
    public float speed = 2f;         // ความเร็วการเคลื่อนที่

    [Header("Damage Setting")]
    public float damage = 10f;       // ดาเมจที่กับดักจะทำ
    public float detectRange = 1f;   // ระยะตรวจผู้เล่น

    private Transform player;
    private bool goingToB = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        MoveTrap();
        DetectPlayer();
    }

    // เคลื่อนที่ไป-กลับระหว่าง A <-> B
    void MoveTrap()
    {
        Transform target = goingToB ? pointB : pointA;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            goingToB = !goingToB;
        }
    }

    // ตรวจว่าผู้เล่นเข้าใกล้หรือไม่
    void DetectPlayer()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectRange)
        {
            // เรียกฟังก์ชันรับดาเมจของ Player
            player.GetComponent<Character>().TakeDamage((int)damage);
        }
    }

    // ถ้าอยากให้ชนแล้วทำดาเมจเลย เติม Collider และเปิด IsTrigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Character>().TakeDamage((int)damage);
        }
    }
}
