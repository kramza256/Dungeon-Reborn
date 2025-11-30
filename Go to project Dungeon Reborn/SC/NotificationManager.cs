using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // อย่าลืม using

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI Settings")]
    public GameObject notificationPrefab; // Prefab ของข้อความที่จะเด้ง
    public Transform notificationParent;  // จุดที่จะให้ข้อความไปเกิด (Content ใน Scroll View หรือ Vertical Layout)
    public float showTime = 2f;           // เวลาที่โชว์ข้อความ

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowNotification(string itemName, int amount, Sprite icon = null)
    {
        if (notificationPrefab == null || notificationParent == null) return;

        // สร้างข้อความใหม่
        GameObject go = Instantiate(notificationPrefab, notificationParent);

        // ตั้งค่าข้อความ (สมมติว่าใน Prefab มี TextMeshProUGUI)
        TextMeshProUGUI textObj = go.GetComponentInChildren<TextMeshProUGUI>();
        if (textObj != null)
        {
            textObj.text = $"Received: {itemName} x{amount}";
        }

        // (Optional) ถ้ามีรูปไอคอนด้วย
        // Image img = go.GetComponentInChildren<Image>();
        // if(img != null && icon != null) img.sprite = icon;

        // ทำลายทิ้งตามเวลาที่กำหนด
        Destroy(go, showTime);
    }
}