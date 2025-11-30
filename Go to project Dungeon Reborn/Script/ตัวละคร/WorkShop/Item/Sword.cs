using UnityEngine;

public class Sword : Item

{

    public int Damage = 25;

    public AudioClip SwordSound;



    // ❌ ลบส่วน Constructor ที่ error ออกไปเลยครับ

    // public Sword(Sword sword) : base(sword) { ... }



    public override void OnCollect(Player player)

    {

        // 1. เรียก base เพื่อให้ระบบเก็บของเข้า Inventory ทำงานก่อน

        base.OnCollect(player);



        // ⚠️ หมายเหตุ: ถ้า base.OnCollect ทำลายวัตถุ (Destroy) ไปแล้ว 

        // โค้ดข้างล่างนี้อาจจะไม่ทำงาน หรือ error ได้

        // แต่ถ้าคุณต้องการให้ดาบ "ถือในมือ" แทนการ "เก็บเข้ากระเป๋า" 

        // คุณอาจจะต้องเลือกว่าจะให้มันทำงานแบบไหน



        // ถ้าต้องการให้ถือในมือด้วย (Visual):

        Vector3 swordUp = new Vector3(90, 0, 0);



        // ✅ แก้ไข: เรียก Collider ในตัวมันเอง

        Collider col = GetComponent<Collider>();

        if (col != null) col.enabled = false;



        if (player.RightHand != null)

        {

            transform.parent = player.RightHand;

            transform.localPosition = Vector3.zero;

            transform.localRotation = Quaternion.Euler(swordUp);

        }



        // เพิ่ม Damage ให้ Player (ต้องมั่นใจว่า Player มีตัวแปร Damage)

        // เนื่องจากเราลบ Damage ออกจาก Player.cs ไปแล้ว (ใช้ Status แทน)

        // ต้องแก้เป็นแบบนี้ครับ:

        if (player.status != null)

        {

            player.status.damage += Damage;

        }

        // หรือถ้าคุณยังเก็บตัวแปร Damage ไว้ใน Player ก็ใช้บรรทัดเดิมได้

        // player.Damage += Damage; 



        if (SoundManager.instance != null && SwordSound != null)

        {

            SoundManager.instance.PlaySFX(SwordSound);

        }

    }

}