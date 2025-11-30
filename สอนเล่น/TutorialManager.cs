using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class TutorialStep
    {
        public string instruction;
        public TaskType taskType;
    }

    public enum TaskType
    {
        Move,           // ต้องกด W A S D ให้ครบ
        Jump,           // กระโดด
        Sprint,         // วิ่งเร็ว
        Attack,         // โจมตี
        OpenInventory,  // เปิดกระเป๋า
        Interact        // เก็บของ
    }

    [Header("UI References")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI textDisplay;

    [Header("Data")]
    public List<TutorialStep> steps;

    private int currentStepIndex = 0;
    private bool isCompleted = false;

    // ✅ ตัวแปรเช็คการเดิน 4 ทิศ
    private bool pressedW, pressedA, pressedS, pressedD;

    void Start()
    {
        if (steps.Count > 0) UpdateUI();
        else { tutorialPanel.SetActive(false); isCompleted = true; }
    }

    void Update()
    {
        if (isCompleted) return;
        CheckTask(steps[currentStepIndex].taskType);
    }

    void CheckTask(TaskType type)
    {
        bool taskDone = false;
        if (Keyboard.current == null || Mouse.current == null) return;

        switch (type)
        {
            case TaskType.Move:
                // เช็คทีละปุ่ม ว่าเคยกดหรือยัง
                if (Keyboard.current.wKey.wasPressedThisFrame) pressedW = true;
                if (Keyboard.current.aKey.wasPressedThisFrame) pressedA = true;
                if (Keyboard.current.sKey.wasPressedThisFrame) pressedS = true;
                if (Keyboard.current.dKey.wasPressedThisFrame) pressedD = true;

                // ✅ อัปเดตข้อความบนหน้าจอ ให้เห็นว่ากดตัวไหนไปแล้วบ้าง
                string baseText = steps[currentStepIndex].instruction; // "กด W A S D เพื่อเดิน"
                string status = $" [ W{(pressedW ? "✓" : " ")} A{(pressedA ? "✓" : " ")} S{(pressedS ? "✓" : " ")} D{(pressedD ? "✓" : " ")} ]";
                textDisplay.text = baseText + "\n" + status;

                // ผ่านก็ต่อเมื่อกดครบทั้ง 4 ตัว
                if (pressedW && pressedA && pressedS && pressedD)
                {
                    taskDone = true;
                }
                break;

            case TaskType.Jump:
                if (Keyboard.current.spaceKey.wasPressedThisFrame) taskDone = true;
                break;

            case TaskType.Sprint:
                if (Keyboard.current.leftShiftKey.isPressed) taskDone = true;
                break;

            case TaskType.Attack:
                if (Mouse.current.leftButton.wasPressedThisFrame) taskDone = true;
                break;

            case TaskType.OpenInventory:
                if (Keyboard.current.tabKey.wasPressedThisFrame) taskDone = true;
                break;

            case TaskType.Interact:
                if (Keyboard.current.eKey.wasPressedThisFrame) taskDone = true;
                break;
        }

        if (taskDone)
        {
            GoToNextStep();
        }
    }

    void GoToNextStep()
    {
        currentStepIndex++;

        // รีเซ็ตค่าการเดิน สำหรับรอบหน้า (เผื่อมี)
        pressedW = false; pressedA = false; pressedS = false; pressedD = false;

        if (currentStepIndex >= steps.Count)
        {
            FinishTutorial();
        }
        else
        {
            UpdateUI();
            Debug.Log("Task Complete! Next step...");
        }
    }

    void UpdateUI()
    {
        tutorialPanel.SetActive(true);
        textDisplay.text = steps[currentStepIndex].instruction;
    }

    void FinishTutorial()
    {
        isCompleted = true;
        tutorialPanel.SetActive(false);
        Debug.Log("Tutorial Finished!");
    }
}