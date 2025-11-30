using UnityEngine;
using UnityEngine.InputSystem; // ✅ เพิ่มบรรทัดนี้

public class MenuPause : MonoBehaviour
{
    public GameObject menuUI;    // Canvas หน้าเมนู Pause
    public Player player;        // อ้างถึง Player

    bool isPaused = false;

    void Update()
    {
        // ✅ แก้: เช็คปุ่ม Esc แบบระบบใหม่
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;

        menuUI.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ปิดการควบคุม Player
        if (player != null) player.enabled = false;
    }

    public void Resume()
    {
        isPaused = false;

        menuUI.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // เปิดการควบคุม Player
        if (player != null) player.enabled = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}

