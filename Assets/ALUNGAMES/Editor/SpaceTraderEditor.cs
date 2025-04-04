using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;

public class SpaceTraderEditor : EditorWindow
{
    [MenuItem("ASCII Space Trader/Game Editor")]
    public static void ShowWindow()
    {
        SpaceTraderEditor wnd = GetWindow<SpaceTraderEditor>();
        wnd.titleContent = new GUIContent("ASCII Space Trader Editor");
    }

    public void CreateGUI()
    {
        // 创建根容器
        var root = rootVisualElement;
        
        // 添加样式
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ALUNGAMES/Editor/SpaceTraderEditor.uss");
        if (styleSheet != null)
        {
            root.styleSheets.Add(styleSheet);
        }

        // 创建主要按钮
        var setupButton = new Button(SetupGame) { text = "在场景中设置游戏" };
        setupButton.AddToClassList("setup-button");
        root.Add(setupButton);

        // 添加分割线
        var separator = new VisualElement();
        separator.AddToClassList("separator");
        root.Add(separator);

        // 创建配置部分
        var configSection = new VisualElement();
        configSection.AddToClassList("config-section");
        
        // 添加标题
        var configTitle = new Label("游戏配置");
        configTitle.AddToClassList("section-title");
        configSection.Add(configTitle);

        // 添加配置选项
        var startingCreditsField = new IntegerField("初始资金") { value = 1000 };
        startingCreditsField.AddToClassList("config-field");
        configSection.Add(startingCreditsField);

        var startingFuelField = new IntegerField("初始燃料") { value = 100 };
        startingFuelField.AddToClassList("config-field");
        configSection.Add(startingFuelField);

        var cargoCapacityField = new IntegerField("货舱容量") { value = 10 };
        cargoCapacityField.AddToClassList("config-field");
        configSection.Add(cargoCapacityField);

        // 添加保存配置按钮
        var saveConfigButton = new Button(() => SaveConfig(startingCreditsField.value, startingFuelField.value, cargoCapacityField.value))
        {
            text = "保存配置"
        };
        saveConfigButton.AddToClassList("config-button");
        configSection.Add(saveConfigButton);

        root.Add(configSection);
    }

    private void SetupGame()
    {
        // 检查场景是否为空
        if (GameObject.FindObjectOfType<SpaceTradeController>() != null)
        {
            if (!EditorUtility.DisplayDialog("警告", 
                "场景中已存在游戏控制器。是否要重新创建？", 
                "是", "否"))
            {
                return;
            }
        }

        // 创建游戏控制器
        var controllerGO = new GameObject("GameController");
        var controller = controllerGO.AddComponent<SpaceTradeController>();

        // 创建UI Document
        var uiDocumentGO = new GameObject("UI_SpaceTrader");
        var uiDocument = uiDocumentGO.AddComponent<UIDocument>();

        // 设置UXML
        var uxmlPath = "Assets/ALUNGAMES/Scenes/Trade/SpaceTrade.uxml";
        var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        if (uxmlAsset == null)
        {
            Debug.LogError($"找不到UXML文件: {uxmlPath}");
            return;
        }
        uiDocument.visualTreeAsset = uxmlAsset;

        // 将UI Document引用设置到控制器
        var serializedObject = new SerializedObject(controller);
        var uiDocumentProperty = serializedObject.FindProperty("uiDocument");
        uiDocumentProperty.objectReferenceValue = uiDocument;
        serializedObject.ApplyModifiedProperties();

        // 选中游戏控制器
        Selection.activeGameObject = controllerGO;

        Debug.Log("游戏设置完成！");
    }

    private void SaveConfig(int credits, int fuel, int capacity)
    {
        // 这里可以保存配置到ScriptableObject或其他配置文件
        Debug.Log($"保存配置: 初始资金={credits}, 初始燃料={fuel}, 货舱容量={capacity}");
    }
} 