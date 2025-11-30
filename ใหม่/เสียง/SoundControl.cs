using UnityEngine;
using UnityEngine.InputSystem; // 1. เพิ่มบรรทัดนี้

public class SoundControl : MonoBehaviour
{
    public AudioSource myAudioSource;

    void Update()
    {
        // 2. แก้ตรงนี้: เช็ค Spacebar แบบระบบใหม่
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            myAudioSource.Play();
        }
    }
}
