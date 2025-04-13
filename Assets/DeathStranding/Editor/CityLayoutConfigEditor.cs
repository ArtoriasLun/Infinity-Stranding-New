using UnityEngine;
using UnityEditor;
using System.Text;

namespace ALUNGAMES.Editor
{
    [CustomEditor(typeof(CityLayoutConfig))]
    public class CityLayoutConfigEditor : UnityEditor.Editor
    {
        private const int MAX_GRID_SIZE = 50;
        private const int CELL_SIZE = 20;
        private const int GRID_PADDING = 10;

        private string[,] gridData;
        private string currentChar = "#";
        private string layoutName = "New Layout";
        private Vector2 scrollPosition;
        private bool showGridEditor = false;
        
        // 添加网格尺寸设置
        private int gridWidth = 24;
        private int gridHeight = 24;
        private bool needReinitialize = false;

        private void InitializeGrid()
        {
            if (gridData == null || needReinitialize)
            {
                // 确保网格尺寸在合理范围内
                gridWidth = Mathf.Clamp(gridWidth, 1, MAX_GRID_SIZE);
                gridHeight = Mathf.Clamp(gridHeight, 1, MAX_GRID_SIZE);

                // 保存旧数据
                string[,] oldData = gridData;
                
                // 创建新网格
                gridData = new string[gridHeight, gridWidth];
                
                // 初始化新网格
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        // 如果有旧数据且在范围内，则保留
                        if (oldData != null && y < oldData.GetLength(0) && x < oldData.GetLength(1))
                        {
                            gridData[y, x] = oldData[y, x];
                        }
                        else
                        {
                            gridData[y, x] = ".";
                        }
                    }
                }
                
                needReinitialize = false;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 绘制默认的Inspector
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cityLayouts"));

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Grid Editor", EditorStyles.boldLabel);

            showGridEditor = EditorGUILayout.Foldout(showGridEditor, "Show Grid Editor");
            if (showGridEditor)
            {
                // 网格尺寸设置
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                
                int newWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
                int newHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
                
                if (EditorGUI.EndChangeCheck())
                {
                    if (newWidth != gridWidth || newHeight != gridHeight)
                    {
                        gridWidth = Mathf.Clamp(newWidth, 1, MAX_GRID_SIZE);
                        gridHeight = Mathf.Clamp(newHeight, 1, MAX_GRID_SIZE);
                        needReinitialize = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                InitializeGrid();

                // 当前绘制字符输入
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current Character:", GUILayout.Width(100));
                currentChar = EditorGUILayout.TextField(currentChar, GUILayout.Width(30));
                if (string.IsNullOrEmpty(currentChar)) currentChar = ".";
                if (currentChar.Length > 1) currentChar = currentChar.Substring(0, 1);
                EditorGUILayout.EndHorizontal();

                // 布局名称输入
                layoutName = EditorGUILayout.TextField("Layout Name", layoutName);

                // 网格编辑区域
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                Rect gridRect = GUILayoutUtility.GetRect(
                    gridWidth * CELL_SIZE + GRID_PADDING * 2,
                    gridHeight * CELL_SIZE + GRID_PADDING * 2);

                // 绘制网格
                DrawGrid(gridRect);

                // 处理鼠标输入
                HandleMouseInput(gridRect);

                EditorGUILayout.EndScrollView();

                // 操作按钮
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Grid"))
                {
                    for (int y = 0; y < gridHeight; y++)
                        for (int x = 0; x < gridWidth; x++)
                            gridData[y, x] = ".";
                }

                if (GUILayout.Button("Add Layout"))
                {
                    AddCurrentLayout();
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid(Rect gridRect)
        {
            // 绘制背景
            EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f));

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Rect cellRect = new Rect(
                        gridRect.x + GRID_PADDING + x * CELL_SIZE,
                        gridRect.y + GRID_PADDING + y * CELL_SIZE,
                        CELL_SIZE - 1,
                        CELL_SIZE - 1
                    );

                    // 绘制单元格背景
                    EditorGUI.DrawRect(cellRect, new Color(0.3f, 0.3f, 0.3f));

                    // 绘制字符
                    GUI.Label(cellRect, gridData[y, x], new GUIStyle()
                    {
                        normal = { textColor = Color.white },
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 12
                    });
                }
            }
        }

        private void HandleMouseInput(Rect gridRect)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown || (e.type == EventType.MouseDrag && e.button == 0))
            {
                Vector2 mousePos = e.mousePosition;
                if (gridRect.Contains(mousePos))
                {
                    int x = Mathf.FloorToInt((mousePos.x - gridRect.x - GRID_PADDING) / CELL_SIZE);
                    int y = Mathf.FloorToInt((mousePos.y - gridRect.y - GRID_PADDING) / CELL_SIZE);

                    if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                    {
                        gridData[y, x] = currentChar;
                        e.Use();
                        Repaint();
                    }
                }
            }
        }

        private void AddCurrentLayout()
        {
            var config = target as CityLayoutConfig;
            if (config == null) return;

            StringBuilder layoutBuilder = new StringBuilder();
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    layoutBuilder.Append(gridData[y, x]);
                }
                if (y < gridHeight - 1)
                    layoutBuilder.AppendLine();
            }

            var newLayout = new CityLayoutConfig.CityLayout
            {
                layoutName = layoutName,
                layoutData = layoutBuilder.ToString()
            };

            var layouts = config.CityLayouts;
            layouts.Add(newLayout);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            // 重置布局名称
            layoutName = "New Layout";
        }
    }
} 