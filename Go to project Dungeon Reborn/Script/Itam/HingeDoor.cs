using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // 1. เพิ่มบรรทัดนี้เพื่อใช้ระบบใหม่

public class HingeDoor : MonoBehaviour
{
    public GameObject blackOverlay;
    public Light roomLight;
    public Transform door;
    public float openAngle = 90f;
    public float speed = 3f;
    public GameObject interactText;

    bool isOpen = false;
    bool isMoving = false;
    Quaternion closedRot;
    Quaternion openRot;

    void Start()
    {
        closedRot = Quaternion.Euler(0, 0, 0);
        openRot = Quaternion.Euler(0, openAngle, 0);

        if (interactText != null) interactText.SetActive(false);
        if (blackOverlay != null) blackOverlay.SetActive(true);
    }

    public void ToggleDoor()
    {
        if (isOpen || isMoving) return;

        StartCoroutine(RotateDoor(openRot));
        isOpen = true;

        if (roomLight != null) roomLight.enabled = true;
        if (blackOverlay != null) blackOverlay.SetActive(false);
    }

    IEnumerator RotateDoor(Quaternion targetRot)
    {
        isMoving = true;
        Quaternion start = door.localRotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            door.localRotation = Quaternion.Slerp(start, targetRot, t);
            yield return null;
        }

        door.localRotation = targetRot;
        isMoving = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) interactText.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) interactText.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        // 2. แก้ตรงนี้: เปลี่ยน Input.GetKeyDown(KeyCode.E) เป็นแบบใหม่
        if (other.CompareTag("Player"))
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ToggleDoor();
            }
        }
    }
}



