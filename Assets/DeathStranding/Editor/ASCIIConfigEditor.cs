using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ASCIIConfig))]
public class ASCIIConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector
        DrawDefaultInspector();

        // 获取ASCIIConfig实例
        ASCIIConfig config = (ASCIIConfig)target;

        // 添加一个空行
        EditorGUILayout.Space();

        // 添加设置所有颜色为白色的按钮
        if (GUILayout.Button("Set All Colors to White"))
        {
            // 记录对象的修改，以支持撤销
            Undo.RecordObject(config, "Set All Colors to White");
            
            // 调用方法设置所有颜色为白色
            config.SetAllColorsToWhite();
            
            // 标记对象为已修改
            EditorUtility.SetDirty(config);
        }
    }
} 