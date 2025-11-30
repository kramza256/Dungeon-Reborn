using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // 1. เพิ่มบรรทัดนี้

public class InteractableBase : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string message = "กด [E] เพื่อสำรวจ";
    public TextMeshProUGUI promptText;

    protected bool isPlayerInRange = false;

    protected virtual void Update()
    {
        // 2. แก้ตรงนี้: เช็คปุ่ม E แบบระบบใหม่
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DoAction();
        }
    }

    private void Start()
    {
        if (promptText == null)
        {
            GameObject textObj = GameObject.Find("PromptText");
            if (textObj != null)
            {
                promptText = textObj.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    public virtual void DoAction()
    {
        Debug.Log("Interacted!");
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowPrompt(true);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ShowPrompt(false);
        }
    }

    void ShowPrompt(bool isShowing)
    {
        if (promptText != null)
        {
            if (isShowing)
            {
                promptText.text = message;
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
}