using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(LevelGameplayData))]
public class LevelGameplayDataEditor : Editor
{
    private SerializedProperty blockedCellsProp;
    private SerializedProperty widthProp;
    private SerializedProperty heightProp;

    private LevelGameplayData levelData;

    private void OnEnable()
    {
        levelData = (LevelGameplayData)target;
        blockedCellsProp = serializedObject.FindProperty("blockedCells");
        widthProp = serializedObject.FindProperty("width");
        heightProp = serializedObject.FindProperty("height");
        InitializeBlockedCells();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- Standard fields ---
        EditorGUILayout.PropertyField(widthProp);
        EditorGUILayout.PropertyField(heightProp);
        DrawPropertiesExcluding(serializedObject, "blockedCells", "width", "height");

        EditorGUILayout.Space(10);

        // --- Button to update grid ---
        if (GUILayout.Button("Update Grid"))
        {
            InitializeBlockedCells();
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Blocked Cells:", EditorStyles.boldLabel);

        // --- Grid rendering ---
        DrawBlockedCellsGrid();

        serializedObject.ApplyModifiedProperties();
    }

    private void InitializeBlockedCells()
    {
        // Get current dimensions
        int width = widthProp.intValue;
        int height = heightProp.intValue;

        // Initialize blockedCells if needed
        if (blockedCellsProp.arraySize != height)
        {
            blockedCellsProp.ClearArray();
            for (int y = 0; y < height; y++)
            {
                blockedCellsProp.InsertArrayElementAtIndex(y);
                var row = blockedCellsProp.GetArrayElementAtIndex(y);
                row.FindPropertyRelative("cells").ClearArray();
            }
        }

        // Ensure that in each row cells.Count == width
        for (int y = 0; y < height; y++)
        {
            var row = blockedCellsProp.GetArrayElementAtIndex(y);
            var cells = row.FindPropertyRelative("cells");

            while (cells.arraySize < width)
            {
                cells.InsertArrayElementAtIndex(cells.arraySize);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBlockedCellsGrid()
{
    int width = widthProp.intValue;
    int height = heightProp.intValue;

    // Draw rows in reverse order (so Y=0 is at the bottom)
    for (int editorY = height - 1; editorY >= 0; editorY--)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Y-coordinate label (optional)
        EditorGUILayout.LabelField(editorY.ToString(), GUILayout.Width(20));

        // Draw cells in the row
        for (int x = 0; x < width; x++)
        {
            // Get data from array (game logic: Y=0 is the bottom row)
            int gameLogicY = editorY; // or (height - 1 - editorY) if inversion is needed
            var row = blockedCellsProp.GetArrayElementAtIndex(gameLogicY);
            var cells = row.FindPropertyRelative("cells");
            var cell = cells.GetArrayElementAtIndex(x);

            bool currentValue = cell.boolValue;
            GUI.backgroundColor = currentValue ? Color.red : Color.green;
            
            bool newValue = EditorGUILayout.Toggle(currentValue, GUILayout.Width(20));
            if (newValue != currentValue)
            {
                cell.boolValue = newValue;
            }

            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndHorizontal();
    }

    // X-coordinate labels (optional)
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField("", GUILayout.Width(20)); // Empty cell for alignment
    for (int x = 0; x < width; x++)
    {
        EditorGUILayout.LabelField(x.ToString(), GUILayout.Width(20));
    }
    EditorGUILayout.EndHorizontal();
}
}
