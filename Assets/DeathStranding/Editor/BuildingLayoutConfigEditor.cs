using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

namespace ALUNGAMES.Editor
{
    [CustomEditor(typeof(BuildingLayoutConfig))]
    public class BuildingLayoutConfigEditor : UnityEditor.Editor
    {
        private const int CELL_SIZE = 25;
        private const int GRID_PADDING = 10;

        private string[,] gridData;
        private string currentChar = ".";
        private string layoutName = "New Layout";
        private Vector2 scrollPosition;
        private bool showGridEditor = false;
        private BuildingType selectedBuildingType = BuildingType.Bar;
        private SizeCategory selectedSizeCategory = SizeCategory.Small;
        private List<SpecialPointType> selectedRequiredPoints = new List<SpecialPointType>();
        private List<SpecialPointType> selectedOptionalPoints = new List<SpecialPointType>();
        
        // 编辑现有布局的变量
        private int selectedLayoutIndex = -1;
        private bool isEditingExisting = false;

        // 字符映射
        private readonly Dictionary<string, string> specialPointSymbols = new Dictionary<string, string>
        {
            { "Wall", "#" },
            { "Door", "|" },
            { "Empty", "." },
            { "DeliveryPoint", "□" },
            { "PickupPoint", "■" },
            { "RestPoint", "+" }
        };

        private void InitializeGrid()
        {
            Vector2Int dimensions = GetSizeDimensions(selectedSizeCategory);
            if (gridData == null || 
                gridData.GetLength(0) != dimensions.y || 
                gridData.GetLength(1) != dimensions.x)
            {
                gridData = new string[dimensions.y, dimensions.x];
                
                // 初始化边框
                for (int y = 0; y < dimensions.y; y++)
                {
                    for (int x = 0; x < dimensions.x; x++)
                    {
                        if (y == 0 || y == dimensions.y - 1 || x == 0 || x == dimensions.x - 1)
                            gridData[y, x] = "#"; // 墙
                        else
                            gridData[y, x] = "."; // 空地
                    }
                }
            }
        }

        private Vector2Int GetSizeDimensions(SizeCategory size)
        {
            switch (size)
            {
                case SizeCategory.Small:
                    return new Vector2Int(5, 5);
                case SizeCategory.Medium:
                    return new Vector2Int(7, 7);
                case SizeCategory.Large:
                    return new Vector2Int(9, 9);
                case SizeCategory.LargeL:
                    return new Vector2Int(11, 9);
                default:
                    return new Vector2Int(5, 5);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var config = target as BuildingLayoutConfig;
            if (config == null) return;

            // 显示现有布局列表
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Existing Layouts", EditorStyles.boldLabel);
            
            if (config.layouts != null && config.layouts.Count > 0)
            {
                for (int i = 0; i < config.layouts.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{i + 1}. {config.layouts[i].layoutName} ({config.layouts[i].buildingType})");
                    
                    if (GUILayout.Button("Edit", GUILayout.Width(60)))
                    {
                        LoadLayoutForEditing(i);
                    }
                    
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Layout", 
                            $"Are you sure you want to delete layout '{config.layouts[i].layoutName}'?", 
                            "Yes", "No"))
                        {
                            config.layouts.RemoveAt(i);
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
            EditorGUILayout.LabelField(isEditingExisting ? "Edit Layout" : "New Layout", EditorStyles.boldLabel);

            showGridEditor = EditorGUILayout.Foldout(showGridEditor, isEditingExisting ? "Edit Layout" : "Create New Layout");
            if (showGridEditor)
            {
                // 建筑类型选择
                EditorGUI.BeginDisabledGroup(isEditingExisting);
                selectedBuildingType = (BuildingType)EditorGUILayout.EnumPopup("Building Type", selectedBuildingType);
                
                // 尺寸类型选择
                EditorGUI.BeginChangeCheck();
                selectedSizeCategory = (SizeCategory)EditorGUILayout.EnumPopup("Size Category", selectedSizeCategory);
                if (EditorGUI.EndChangeCheck() && !isEditingExisting)
                {
                    InitializeGrid();
                }
                EditorGUI.EndDisabledGroup();

                // 布局名称输入
                layoutName = EditorGUILayout.TextField("Layout Name", layoutName);

                // 特殊点选择
                EditorGUILayout.LabelField("Tile Selection", EditorStyles.boldLabel);
                
                // 当前绘制字符选择
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Basic:", GUILayout.Width(70));
                if (GUILayout.Button("Wall", EditorStyles.miniButtonLeft)) currentChar = "#";
                if (GUILayout.Button("Door", EditorStyles.miniButtonMid)) currentChar = "|";
                if (GUILayout.Button("Empty", EditorStyles.miniButtonRight)) currentChar = ".";
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Special:", GUILayout.Width(70));
                if (GUILayout.Button("Delivery", EditorStyles.miniButtonLeft)) currentChar = "□";
                if (GUILayout.Button("Pickup", EditorStyles.miniButtonMid)) currentChar = "■";
                if (GUILayout.Button("Rest", EditorStyles.miniButtonRight)) currentChar = "+";
                EditorGUILayout.EndHorizontal();

                // 根据建筑类型自动设置必需点
                if (selectedBuildingType == BuildingType.Yard)
                {
                    selectedRequiredPoints.Clear();
                    selectedRequiredPoints.Add(SpecialPointType.DeliveryPoint);
                }
                else if (selectedBuildingType == BuildingType.Hotel)
                {
                    selectedRequiredPoints.Clear();
                    selectedRequiredPoints.Add(SpecialPointType.RestPoint);
                }

                if (!isEditingExisting)
                {
                    InitializeGrid();
                }

                // 网格编辑区域
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                Vector2Int dimensions = GetSizeDimensions(selectedSizeCategory);
                Rect gridRect = GUILayoutUtility.GetRect(
                    dimensions.x * CELL_SIZE + GRID_PADDING * 2,
                    dimensions.y * CELL_SIZE + GRID_PADDING * 2);

                // 绘制网格
                DrawGrid(gridRect);

                // 处理鼠标输入
                HandleMouseInput(gridRect);

                EditorGUILayout.EndScrollView();

                // 操作按钮
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

                if (GUILayout.Button(isEditingExisting ? "Save Changes" : "Add Layout"))
                {
                    if (isEditingExisting)
                    {
                        SaveLayoutChanges();
                    }
                    else
                    {
                        AddCurrentLayout();
                    }
                }

                if (isEditingExisting && GUILayout.Button("Cancel Editing"))
                {
                    CancelEditing();
                }
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void LoadLayoutForEditing(int index)
        {
            var config = target as BuildingLayoutConfig;
            if (config == null || index < 0 || index >= config.layouts.Count) return;

            var layout = config.layouts[index];
            selectedLayoutIndex = index;
            isEditingExisting = true;
            layoutName = layout.layoutName;
            selectedBuildingType = layout.buildingType;
            selectedSizeCategory = layout.sizeCategory;
            selectedRequiredPoints = new List<SpecialPointType>(layout.requiredPoints);
            selectedOptionalPoints = new List<SpecialPointType>(layout.optionalPoints);

            // 加载布局数据
            Vector2Int dimensions = BuildingLayoutUtils.SizeDimensions[layout.sizeCategory];
            gridData = new string[dimensions.y, dimensions.x];
            string[] rows = layout.layoutData.Split('\n');
            
            for (int y = 0; y < dimensions.y && y < rows.Length; y++)
            {
                string row = rows[y];
                for (int x = 0; x < dimensions.x && x < row.Length; x++)
                {
                    gridData[y, x] = row[x].ToString();
                }
            }

            showGridEditor = true;
        }

        private void SaveLayoutChanges()
        {
            var config = target as BuildingLayoutConfig;
            if (config == null || selectedLayoutIndex < 0 || selectedLayoutIndex >= config.layouts.Count) return;

            if (string.IsNullOrEmpty(layoutName))
            {
                EditorUtility.DisplayDialog("Error", "Layout name cannot be empty!", "OK");
                return;
            }

            if (!ValidateRequiredPoints())
            {
                EditorUtility.DisplayDialog("Error", "Missing required special points for the selected building type!", "OK");
                return;
            }

            StringBuilder layoutBuilder = new StringBuilder();
            Vector2Int dimensions = BuildingLayoutUtils.SizeDimensions[selectedSizeCategory];
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    layoutBuilder.Append(gridData[y, x]);
                }
                if (y < dimensions.y - 1)
                    layoutBuilder.AppendLine();
            }

            var layout = config.layouts[selectedLayoutIndex];
            layout.layoutName = layoutName;
            layout.layoutData = layoutBuilder.ToString();
            layout.requiredPoints = new List<SpecialPointType>(selectedRequiredPoints);
            layout.optionalPoints = new List<SpecialPointType>(selectedOptionalPoints);

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            CancelEditing();
        }

        private void CancelEditing()
        {
            isEditingExisting = false;
            selectedLayoutIndex = -1;
            layoutName = "New Layout";
            selectedRequiredPoints.Clear();
            selectedOptionalPoints.Clear();
            InitializeGrid();
        }

        private void DrawSpecialPointsList(ref List<SpecialPointType> points)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 显示已选择的点
            for (int i = 0; i < points.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                points[i] = (SpecialPointType)EditorGUILayout.EnumPopup(points[i]);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    points.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            // 添加新点按钮
            if (GUILayout.Button("Add Point", GUILayout.Width(100)))
            {
                points.Add(SpecialPointType.DeliveryPoint);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawGrid(Rect gridRect)
        {
            // 绘制背景
            EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f));

            Vector2Int dimensions = GetSizeDimensions(selectedSizeCategory);
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
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
                    Vector2Int dimensions = GetSizeDimensions(selectedSizeCategory);
                    int x = Mathf.FloorToInt((mousePos.x - gridRect.x - GRID_PADDING) / CELL_SIZE);
                    int y = Mathf.FloorToInt((mousePos.y - gridRect.y - GRID_PADDING) / CELL_SIZE);

                    if (x >= 0 && x < dimensions.x && y >= 0 && y < dimensions.y)
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
            var config = target as BuildingLayoutConfig;
            if (config == null) return;

            // 验证布局名称
            if (string.IsNullOrEmpty(layoutName))
            {
                EditorUtility.DisplayDialog("Error", "Layout name cannot be empty!", "OK");
                return;
            }

            // 验证必需的特殊点
            if (!ValidateRequiredPoints())
            {
                EditorUtility.DisplayDialog("Error", "Missing required special points for the selected building type!", "OK");
                return;
            }

            StringBuilder layoutBuilder = new StringBuilder();
            Vector2Int dimensions = GetSizeDimensions(selectedSizeCategory);
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    layoutBuilder.Append(gridData[y, x]);
                }
                if (y < dimensions.y - 1)
                    layoutBuilder.AppendLine();
            }

            var newLayout = new BuildingLayoutConfig.BuildingLayout
            {
                layoutName = layoutName,
                buildingType = selectedBuildingType,
                sizeCategory = selectedSizeCategory,
                layoutData = layoutBuilder.ToString(),
                requiredPoints = new List<SpecialPointType>(selectedRequiredPoints),
                optionalPoints = new List<SpecialPointType>(selectedOptionalPoints)
            };

            config.layouts.Add(newLayout);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            // 重置布局名称
            layoutName = "New Layout";
            selectedRequiredPoints.Clear();
            selectedOptionalPoints.Clear();
            InitializeGrid();
        }

        private bool ValidateRequiredPoints()
        {
            // 检查布局中是否包含所有必需的特殊点
            HashSet<SpecialPointType> foundPoints = new HashSet<SpecialPointType>();
            Vector2Int dimensions = GetSizeDimensions(selectedSizeCategory);

            // 特殊处理：如果是包裹坞（Yard），必须有交货点
            if (selectedBuildingType == BuildingType.Yard && 
                !selectedRequiredPoints.Contains(SpecialPointType.DeliveryPoint))
            {
                return false;
            }

            // 特殊处理：如果是旅馆（Hotel），必须有休息点
            if (selectedBuildingType == BuildingType.Hotel && 
                !selectedRequiredPoints.Contains(SpecialPointType.RestPoint))
            {
                return false;
            }

            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    string symbol = gridData[y, x];
                    foreach (var pointType in selectedRequiredPoints)
                    {
                        if (GetSymbolForSpecialPoint(pointType) == symbol)
                        {
                            foundPoints.Add(pointType);
                        }
                    }
                }
            }

            return foundPoints.Count == selectedRequiredPoints.Count;
        }

        private string GetSymbolForSpecialPoint(SpecialPointType pointType)
        {
            switch (pointType)
            {
                case SpecialPointType.DeliveryPoint: return "□";
                case SpecialPointType.PickupPoint: return "■";
                case SpecialPointType.RestPoint: return "+";
                default: return ".";
            }
        }
    }
} 