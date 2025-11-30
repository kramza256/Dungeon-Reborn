using UnityEngine;
using UnityEditor;
using GameInventory;

[CustomEditor(typeof(UpgradeData))]
public class UpgradeDataEditor : Editor
{
    private SerializedProperty inputEquipmentProp;
    private SerializedProperty upgradeMaterialProp;
    private SerializedProperty materialCostProp;
    private SerializedProperty successOutputProp;

    private SerializedProperty successChanceProp;
    private SerializedProperty breakOnFailProp;
    private SerializedProperty failOutputProp;
    private SerializedProperty upgradeNameProp;

    private void OnEnable()
    {
        inputEquipmentProp = serializedObject.FindProperty("inputEquipment");
        upgradeMaterialProp = serializedObject.FindProperty("upgradeMaterial");
        successOutputProp = serializedObject.FindProperty("successOutput");

        materialCostProp = serializedObject.FindProperty("materialCost");
        successChanceProp = serializedObject.FindProperty("successChance");

        breakOnFailProp = serializedObject.FindProperty("breakOnFail");
        failOutputProp = serializedObject.FindProperty("failOutput");
        upgradeNameProp = serializedObject.FindProperty("upgradeName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- Header ---
        EditorGUILayout.Space(10);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
        EditorGUILayout.LabelField("UPGRADE RECIPE", headerStyle);
        EditorGUILayout.Space(10);

        // =========================================================
        // 📦 ส่วนแสดงผล (แนวตั้ง + จัดกึ่งกลาง)
        // =========================================================

        // เริ่มกรอบ
        EditorGUILayout.BeginVertical("HelpBox");
        EditorGUILayout.Space(10);

        // 1. Base Item (บนสุด)
        DrawCenteredSlot("Base Item", inputEquipmentProp);

        // เครื่องหมาย +
        DrawCenteredSymbol("+");

        // 2. Material (กลาง)
        DrawCenteredSlot("Material", upgradeMaterialProp, materialCostProp);

        // เครื่องหมายลูกศรลง หรือ =
        DrawCenteredSymbol("⬇");

        // 3. Result (ล่างสุด)
        DrawCenteredSlot("Result (Success)", successOutputProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.EndVertical(); // จบกรอบ Recipe
        // =========================================================

        EditorGUILayout.Space(20);

        // --- Settings Zone ---
        EditorGUILayout.BeginVertical("GroupBox");
        EditorGUILayout.LabelField("Upgrade Settings", EditorStyles.boldLabel);

        if (upgradeNameProp != null) EditorGUILayout.PropertyField(upgradeNameProp);

        EditorGUILayout.Space(5);

        // Custom Slider
        float chance = successChanceProp.floatValue;
        Rect r = EditorGUILayout.GetControlRect(false, 20);
        EditorGUI.DrawRect(r, new Color(0.8f, 0.2f, 0.2f)); // แดง
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width * (chance / 100f), r.height), new Color(0.2f, 0.8f, 0.2f)); // เขียว
        EditorGUI.LabelField(r, $"Success Chance: {chance:0}%", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });

        EditorGUILayout.PropertyField(successChanceProp, new GUIContent("Adjust Chance"));

        EditorGUILayout.Space(10);

        // Fail Settings
        EditorGUILayout.LabelField("Failure Consequence", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(breakOnFailProp);

        if (!breakOnFailProp.boolValue)
        {
            EditorGUILayout.PropertyField(failOutputProp, new GUIContent("Downgrade To"));
            if (failOutputProp.objectReferenceValue == null)
                EditorGUILayout.HelpBox("If empty -> Item level stays the same.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("WARNING: Item will be DESTROYED on failure!", MessageType.Error);
        }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    // ฟังก์ชันวาด Slot แบบจัดกึ่งกลางหน้าจอ (Horizontal FlexibleSpace บีบข้าง)
    private void DrawCenteredSlot(string label, SerializedProperty itemProp, SerializedProperty amountProp = null)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // ดันซ้าย

        DrawFixedItemSlot(label, itemProp, amountProp); // วาดกล่อง

        GUILayout.FlexibleSpace(); // ดันขวา
        EditorGUILayout.EndHorizontal();
    }

    // ฟังก์ชันวาดเครื่องหมายเชื่อม (+, =, ↓) แบบจัดกึ่งกลาง
    private void DrawCenteredSymbol(string symbol)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        EditorGUILayout.LabelField(symbol, new GUIStyle(EditorStyles.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        }, GUILayout.Width(40), GUILayout.Height(30));

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // ฟังก์ชันวาดกล่อง 80x80 (เหมือนเดิม แต่เอาไปใช้ใน DrawCenteredSlot)
    private void DrawFixedItemSlot(string label, SerializedProperty itemProp, SerializedProperty amountProp = null)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(80));

        EditorGUILayout.LabelField(label, new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter });

        Rect r = GUILayoutUtility.GetRect(80, 80, GUILayout.Width(80), GUILayout.Height(80));

        // พื้นหลัง
        EditorGUI.DrawRect(r, new Color(0.15f, 0.15f, 0.15f, 1f));

        // รูปไอคอน
        SO_Item item = itemProp.objectReferenceValue as SO_Item;
        if (item != null && item.icon != null)
        {
            EditorGUI.DrawTextureTransparent(r, item.icon.texture, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.LabelField(r, "None", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.gray } });
        }

        // ขอบ
        float t = 2f;
        Color border = new Color(0.1f, 0.1f, 0.1f, 1f);
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), border);
        EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), border);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), border);
        EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), border);

        // Object Field
        EditorGUILayout.PropertyField(itemProp, GUIContent.none, GUILayout.Width(80));

        // Amount
        if (amountProp != null)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("x", GUILayout.Width(10));
            EditorGUILayout.PropertyField(amountProp, GUIContent.none, GUILayout.Width(40));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
}