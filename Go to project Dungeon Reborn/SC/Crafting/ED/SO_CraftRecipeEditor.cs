

using UnityEngine;
using UnityEditor;
using GameInventory;

[CustomEditor(typeof(CraftRecipe))]
public class CraftRecipeEditor : Editor
{
    private SerializedProperty ingredientsProp;
    private SerializedProperty outputItemProp;
    private SerializedProperty outputAmountProp;

    private const int kGridSize = 3; // 3x3 Grid

    private void OnEnable()
    {
        ingredientsProp = serializedObject.FindProperty("ingredients");
        outputItemProp = serializedObject.FindProperty("outputItem");
        outputAmountProp = serializedObject.FindProperty("outputAmount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Crafting Grid", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EnsureIngredientArraySize(kGridSize * kGridSize);

        // วาด 3x3 Grid
        for (int row = 0; row < kGridSize; row++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            for (int col = 0; col < kGridSize; col++)
            {
                int idx = row * kGridSize + col;
                EditorGUILayout.BeginVertical(GUILayout.Width(80));
                DrawIngredientCell(ingredientsProp.GetArrayElementAtIndex(idx));
                EditorGUILayout.EndVertical();
                if (col < kGridSize - 1) GUILayout.Space(10);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            if (row < kGridSize - 1) GUILayout.Space(10);
        }

        EditorGUILayout.Space(20);

        // วาด Result
        EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        EditorGUILayout.PropertyField(outputItemProp, new GUIContent("Item"));
        EditorGUILayout.PropertyField(outputAmountProp, new GUIContent("Amount"));
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIngredientCell(SerializedProperty ingredientProp)
    {
        SerializedProperty itemProp = ingredientProp.FindPropertyRelative("item");
        SerializedProperty amountProp = ingredientProp.FindPropertyRelative("amount");

        SO_Item item = itemProp.objectReferenceValue as SO_Item;

        Rect r = GUILayoutUtility.GetRect(80, 80);
        if (item != null && item.icon != null)
            EditorGUI.DrawTextureTransparent(r, item.icon.texture, ScaleMode.ScaleToFit);
        else
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.1f));

        float t = 2f;
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, t), Color.black);
        EditorGUI.DrawRect(new Rect(r.x, r.y, t, r.height), Color.black);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - t, r.width, t), Color.black);
        EditorGUI.DrawRect(new Rect(r.xMax - t, r.y, t, r.height), Color.black);

        EditorGUILayout.PropertyField(itemProp, GUIContent.none, GUILayout.Width(80));
        EditorGUILayout.PropertyField(amountProp, GUIContent.none, GUILayout.Width(40));
    }

    private void EnsureIngredientArraySize(int size)
    {
        if (ingredientsProp.arraySize != size)
            ingredientsProp.arraySize = size;
    }
}
