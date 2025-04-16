using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

namespace ALUNGAMES.Editor
{
    [CustomEditor(typeof(CityLayoutConfig))]
    public class CityLayoutConfigEditor : UnityEditor.Editor
    {
        private const int CELL_SIZE = 25;
        private const int GRID_PADDING = 10;
        private const int BORDER_SIZE = 1;

        private string[,] gridData;
        private string[,] displayGrid;
        private string currentChar = "#";
        private string layoutName = "New Layout";
        private Vector2 scrollPosition;
        private bool showGridEditor = false;
        
        // 添加网格尺寸设置
        private int gridWidth = 24;
        private int gridHeight = 24;
        private bool needReinitialize = false;

        // 字符映射
        private readonly Dictionary<string, string> tileSymbols = new Dictionary<string, string>
        {
            { "Wall", "#" },
            { "Door", "|" }
        };

        private void InitializeGrid()
        {
            if (gridData == null || needReinitialize)
            {
                // 保存旧数据
                string[,] oldData = gridData;
                
                // 创建新网格
                gridData = new string[gridHeight, gridWidth];
                displayGrid = new string[gridHeight + BORDER_SIZE * 2, gridWidth + BORDER_SIZE * 2];
                
                // 初始化网格数据
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

                UpdateDisplayGrid();
                needReinitialize = false;
            }
        }

        private void UpdateDisplayGrid()
        {
            // 初始化显示网格
            for (int y = 0; y < displayGrid.GetLength(0); y++)
            {
                for (int x = 0; x < displayGrid.GetLength(1); x++)
                {
                    // 边界区域显示为灰色点
                    displayGrid[y, x] = ".";
                }
            }

            // 复制实际数据到显示网格的中心区域
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    displayGrid[y + BORDER_SIZE, x + BORDER_SIZE] = gridData[y, x];
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var config = target as CityLayoutConfig;
            if (config == null) return;

            // 显示现有布局列表
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Existing Layouts", EditorStyles.boldLabel);
            
            if (config.CityLayouts != null && config.CityLayouts.Count > 0)
            {
                for (int i = 0; i < config.CityLayouts.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i + 1}. {config.CityLayouts[i].layoutName}");
                    
                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                    {
                        LoadLayoutForEditing(i);
                    }
                    
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Layout", 
                            $"Are you sure you want to delete layout '{config.CityLayouts[i].layoutName}'?", 
                            "Yes", "No"))
                        {
                            config.CityLayouts.RemoveAt(i);
                            EditorUtility.SetDirty(target);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No layouts created yet.");
            }

            EditorGUILayout.Space(10);
            showGridEditor = EditorGUILayout.Foldout(showGridEditor, "Grid Editor");
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
                        gridWidth = Mathf.Clamp(newWidth, 1, 50);
                        gridHeight = Mathf.Clamp(newHeight, 1, 50);
                        needReinitialize = true;
                    }
                }
                EditorGUILayout.EndHorizontal();

                InitializeGrid();

                // 布局名称输入
                layoutName = EditorGUILayout.TextField("Layout Name", layoutName);

                // 绘制工具选择
                EditorGUILayout.LabelField("Tile Selection", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Wall (#)", EditorStyles.miniButtonLeft)) currentChar = "#";
                if (GUILayout.Button("Door (|)", EditorStyles.miniButtonMid)) currentChar = "|";
                if (GUILayout.Button("Empty (.)", EditorStyles.miniButtonRight)) currentChar = ".";
                EditorGUILayout.EndHorizontal();

                // 网格编辑区域
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                Rect gridRect = GUILayoutUtility.GetRect(
                    (gridWidth + BORDER_SIZE * 2) * CELL_SIZE + GRID_PADDING * 2,
                    (gridHeight + BORDER_SIZE * 2) * CELL_SIZE + GRID_PADDING * 2);

                // 绘制网格
                DrawGrid(gridRect);

                // 处理鼠标输入
                HandleMouseInput(gridRect);

                EditorGUILayout.EndScrollView();

                // 移动按钮
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Move Layout", EditorStyles.boldLabel);
                
                // 上按钮和缩放按钮
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    ScaleLayout(false);
                }
                if (GUILayout.Button("↑", GUILayout.Width(50)))
                {
                    MoveLayout(new Vector2Int(0, -1));
                }
                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    ScaleLayout(true);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // 左中右按钮
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("←", GUILayout.Width(50)))
                {
                    MoveLayout(Vector2Int.left);
                }
                GUILayout.Space(50);
                if (GUILayout.Button("→", GUILayout.Width(50)))
                {
                    MoveLayout(Vector2Int.right);
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // 下按钮
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("↓", GUILayout.Width(50)))
                {
                    MoveLayout(new Vector2Int(0, 1));
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                // 操作按钮
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Grid"))
                {
                    if (EditorUtility.DisplayDialog("Clear Grid", 
                        "Are you sure you want to clear the grid?", 
                        "Yes", "No"))
                    {
                        InitializeGrid();
                    }
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

            for (int y = 0; y < gridHeight + BORDER_SIZE * 2; y++)
            {
                for (int x = 0; x < gridWidth + BORDER_SIZE * 2; x++)
                {
                    Rect cellRect = new Rect(
                        gridRect.x + GRID_PADDING + x * CELL_SIZE,
                        gridRect.y + GRID_PADDING + y * CELL_SIZE,
                        CELL_SIZE - 1,
                        CELL_SIZE - 1
                    );

                    // 绘制单元格背景
                    bool isBorder = y < BORDER_SIZE || y >= gridHeight + BORDER_SIZE ||
                                  x < BORDER_SIZE || x >= gridWidth + BORDER_SIZE;
                    Color bgColor = isBorder ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);
                    EditorGUI.DrawRect(cellRect, bgColor);

                    // 绘制字符
                    GUI.Label(cellRect, displayGrid[y, x], new GUIStyle()
                    {
                        normal = { textColor = isBorder ? Color.gray : Color.white },
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 14,
                        fontStyle = FontStyle.Bold
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
                    int x = Mathf.FloorToInt((mousePos.x - gridRect.x - GRID_PADDING) / CELL_SIZE) - BORDER_SIZE;
                    int y = Mathf.FloorToInt((mousePos.y - gridRect.y - GRID_PADDING) / CELL_SIZE) - BORDER_SIZE;

                    if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                    {
                        gridData[y, x] = currentChar;
                        UpdateDisplayGrid();
                        e.Use();
                        Repaint();
                    }
                }
            }
        }

        private void LoadLayoutForEditing(int index)
        {
            var config = target as CityLayoutConfig;
            if (config == null || index < 0 || index >= config.CityLayouts.Count) return;

            var layout = config.CityLayouts[index];
            layoutName = layout.layoutName;

            // 解析布局数据
            string[] rows = layout.layoutData.Split('\n');
            gridHeight = rows.Length;
            gridWidth = rows[0].Length;
            needReinitialize = true;
            InitializeGrid();

            // 加载布局数据
            for (int y = 0; y < gridHeight && y < rows.Length; y++)
            {
                string row = rows[y];
                for (int x = 0; x < gridWidth && x < row.Length; x++)
                {
                    gridData[y, x] = row[x].ToString();
                }
            }

            UpdateDisplayGrid();
            showGridEditor = true;
        }

        private void MoveLayout(Vector2Int direction)
        {
            string[,] newGrid = new string[gridHeight, gridWidth];

            // 初始化新网格为空地
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    newGrid[y, x] = ".";
                }
            }

            // 移动布局
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    int newX = x + direction.x;
                    int newY = y + direction.y;

                    if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight)
                    {
                        newGrid[newY, newX] = gridData[y, x];
                    }
                }
            }

            gridData = newGrid;
            UpdateDisplayGrid();
            GUI.changed = true;
            Repaint();
        }

        private void ScaleLayout(bool scaleUp)
        {
            string[,] newGrid = new string[gridHeight, gridWidth];
            
            // 初始化新网格为空地
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    newGrid[y, x] = ".";
                }
            }

            // 计算缩放参数
            float scale = scaleUp ? (gridWidth + 2f) / gridWidth : (gridWidth - 2f) / gridWidth;
            
            // 复制并缩放布局
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    // 计算源坐标
                    int sourceX = Mathf.RoundToInt(x / scale);
                    int sourceY = Mathf.RoundToInt(y / scale);

                    // 检查源坐标是否在原网格范围内
                    if (sourceX >= 0 && sourceX < gridWidth && 
                        sourceY >= 0 && sourceY < gridHeight)
                    {
                        newGrid[y, x] = gridData[sourceY, sourceX];
                    }
                }
            }

            gridData = newGrid;
            UpdateDisplayGrid();
            GUI.changed = true;
            Repaint();
        }

        private void AddCurrentLayout()
        {
            var config = target as CityLayoutConfig;
            if (config == null) return;

            if (string.IsNullOrEmpty(layoutName))
            {
                EditorUtility.DisplayDialog("Error", "Layout name cannot be empty!", "OK");
                return;
            }

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

            config.CityLayouts.Add(newLayout);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            // 重置布局名称
            layoutName = "New Layout";
        }
    }
} 