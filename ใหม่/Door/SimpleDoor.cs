using UnityEngine;
using System.Collections;

public class SimpleDoor : MonoBehaviour, IInteractable
{
    [Header("⚙️ ตั้งค่าประตู")]
    public float openAngle = 90f;
    public float speed = 2f;
    public bool canClose = true;

    [Header("👻 ส่วนเสริม (ห้องมืด/ข้อความ)")]
    public GameObject blackOverlay; // แผ่นสีดำบังห้อง
    public GameObject interactText; // ข้อความ Press E

    [Header("🔊 เสียง")]
    public AudioClip doorSound;
    private AudioSource audioSource;

    private bool isOpen = false;
    private Quaternion closedRot;
    private Quaternion openRot;
    private Coroutine moveCoroutine;

    public bool isInteractable { get; set; } = true;

    void Start()
    {
        closedRot = transform.localRotation;
        openRot = closedRot * Quaternion.Euler(0, openAngle, 0);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        // เริ่มเกมมาซ่อนข้อความก่อน
        if (interactText != null) interactText.SetActive(false);
        // เปิดแผ่นบังห้องไว้
        if (blackOverlay != null) blackOverlay.SetActive(true);
    }

    public void Interact(Player player)
    {
        if (isOpen && !canClose) return;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        isOpen = !isOpen;

        // --- ส่วนจัดการห้องมืดและข้อความ ---
        if (isOpen)
        {
            // ถ้าเปิดประตู -> เอาแผ่นดำออก + ซ่อนข้อความ
            if (blackOverlay != null) blackOverlay.SetActive(false);
            if (interactText != null) interactText.SetActive(false);
        }
        // --------------------------------

        Quaternion target = isOpen ? openRot : closedRot;
        moveCoroutine = StartCoroutine(RotateDoor(target));

        if (doorSound != null) audioSource.PlayOneShot(doorSound);
    }

    IEnumerator RotateDoor(Quaternion targetRot)
    {
        while (Quaternion.Angle(transform.localRotation, targetRot) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * speed);
            yield return null;
        }
        transform.localRotation = targetRot;
    }

    // ✅ ตรวจจับการเดินเข้าใกล้เพื่อโชว์ข้อความ
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen) // โชว์เฉพาะตอนยังไม่เปิด
        {
            if (interactText != null) interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactText != null) interactText.SetActive(false);
        }
    }
}