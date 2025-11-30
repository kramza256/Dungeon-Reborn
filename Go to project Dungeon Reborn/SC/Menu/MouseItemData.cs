using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameInventory;
using UnityEngine.InputSystem; // ✅ เพิ่ม

public class MouseItemData : MonoBehaviour
{
    public static MouseItemData Instance;

    public Image itemSprite;
    public TextMeshProUGUI itemCountText;

    [HideInInspector] public SO_Item assignedItem;
    [HideInInspector] public int assignedCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (itemSprite == null) itemSprite = GetComponent<Image>();
        if (itemCountText == null) itemCountText = GetComponentInChildren<TextMeshProUGUI>();

        if (itemSprite != null) itemSprite.raycastTarget = false;
        if (itemCountText != null) itemCountText.raycastTarget = false;

        ClearSlot();
    }

    private void Start()
    {
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            transform.SetParent(parentCanvas.transform);
            transform.SetAsLastSibling();
        }
    }

    private void Update()
    {
        if (assignedItem != null)
        {
            // ✅ แก้: รับตำแหน่งเมาส์แบบ New Input System
            if (Mouse.current != null)
            {
                transform.position = Mouse.current.position.ReadValue();
            }
        }
    }

    public void UpdateMouseItem(SO_Item item, int count)
    {
        assignedItem = item;
        assignedCount = count;

        if (itemSprite != null)
        {
            itemSprite.sprite = item.icon;
            // ✅ สำคัญ: ต้องปรับสีให้เป็นสีขาว (Alpha = 1) เพื่อให้มองเห็นรูป
            itemSprite.color = Color.white;
            // หรือใช้ itemSprite.enabled = true; ก็ได้
        }

        if (itemCountText != null)
        {
            itemCountText.text = count > 1 ? count.ToString() : "";
        }
    }

    public void ClearSlot()
    {
        assignedItem = null;
        assignedCount = 0;

        if (itemSprite != null)
        {
            itemSprite.sprite = null;
            // ✅ สำคัญ: ปรับสีให้ใส (Alpha = 0) เมื่อไม่มีของ
            itemSprite.color = Color.clear;
            // หรือใช้ itemSprite.enabled = false; ก็ได้
        }

        if (itemCountText != null)
        {
            itemCountText.text = "";
        }
    }
}