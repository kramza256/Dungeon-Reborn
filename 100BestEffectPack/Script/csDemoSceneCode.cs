using UnityEngine;
using UnityEngine.UI; // ✅ ต้องมีอันนี้
using UnityEngine.InputSystem; // ✅ ต้องมีอันนี้

public class csDemoSceneCode : MonoBehaviour
{
    public string[] EffectNames;
    public string[] Effect2Names;
    public Transform[] Effect;

    // เปลี่ยนจาก GUIText เป็น Text (อย่าลืมลาก Text ใน Canvas มาใส่ใหม่นะ)
    public Text Text1;

    int i = 0;
    int a = 0;

    void Start()
    {
        if (Effect.Length > i && Effect[i] != null)
            Instantiate(Effect[i], new Vector3(0, 5, 0), Quaternion.identity);
    }

    void Update()
    {
        if (Text1 != null && i < EffectNames.Length)
            Text1.text = (i + 1) + ":" + EffectNames[i];

        if (Keyboard.current == null) return;

        // กด Z: ถอยหลัง
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            if (i <= 0) i = Effect.Length - 1;
            else i--;
            SpawnCurrentEffect();
        }

        // กด X: ถัดไป
        if (Keyboard.current.xKey.wasPressedThisFrame)
        {
            if (i >= Effect.Length - 1) i = 0;
            else i++;
            SpawnCurrentEffect();
        }

        // กด C: เล่นซ้ำ
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            SpawnCurrentEffect();
        }
    }

    void SpawnCurrentEffect()
    {
        if (i >= Effect.Length || Effect[i] == null) return;

        bool specialSpawn = false;
        for (a = 0; a < Effect2Names.Length; a++)
        {
            if (i < EffectNames.Length && EffectNames[i] == Effect2Names[a])
            {
                Instantiate(Effect[i], new Vector3(0, 0.2f, 0), Quaternion.identity);
                specialSpawn = true;
                break;
            }
        }

        if (!specialSpawn)
            Instantiate(Effect[i], new Vector3(0, 5, 0), Quaternion.identity);
    }
}
