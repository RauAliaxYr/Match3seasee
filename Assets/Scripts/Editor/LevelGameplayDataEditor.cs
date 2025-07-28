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

        // --- Стандартные поля ---
        EditorGUILayout.PropertyField(widthProp);
        EditorGUILayout.PropertyField(heightProp);
        DrawPropertiesExcluding(serializedObject, "blockedCells", "width", "height");

        EditorGUILayout.Space(10);

        // --- Кнопка для обновления сетки ---
        if (GUILayout.Button("Обновить сетку"))
        {
            InitializeBlockedCells();
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Заблокированные клетки:", EditorStyles.boldLabel);

        // --- Отрисовка сетки ---
        DrawBlockedCellsGrid();

        serializedObject.ApplyModifiedProperties();
    }

    private void InitializeBlockedCells()
    {
        // Получаем текущие размеры
        int width = widthProp.intValue;
        int height = heightProp.intValue;

        // Инициализируем blockedCells, если нужно
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

        // Убедимся, что в каждой строке cells.Count == width
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

    // Отрисовываем строки в обратном порядке (чтобы Y=0 был внизу)
    for (int editorY = height - 1; editorY >= 0; editorY--)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Подпись Y-координаты (опционально)
        EditorGUILayout.LabelField(editorY.ToString(), GUILayout.Width(20));

        // Отрисовка клеток в строке
        for (int x = 0; x < width; x++)
        {
            // Получаем данные из массива (игровая логика: Y=0 — нижний ряд)
            int gameLogicY = editorY; // или (height - 1 - editorY), если нужно инвертировать
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

    // Подписи X-координат (опционально)
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField("", GUILayout.Width(20)); // Пустая ячейка для выравнивания
    for (int x = 0; x < width; x++)
    {
        EditorGUILayout.LabelField(x.ToString(), GUILayout.Width(20));
    }
    EditorGUILayout.EndHorizontal();
}
}
