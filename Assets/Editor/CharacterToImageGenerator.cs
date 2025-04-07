using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class CharacterToImageGenerator : EditorWindow
{
    private char inputCharacter = 'A';
    private int imageSize = 256;
    private Color backgroundColor = Color.white;
    private Color foregroundColor = Color.black;
    private Font customFont;
    private int fontSize = 128;
    private TextAnchor textAlignment = TextAnchor.MiddleCenter;
    private string savePath = "Assets/GeneratedImages";
    private string fileName = "CharImage";
    private bool showPreview = true;
    private Texture2D previewTexture;

    [MenuItem("😊/Character To Image Generator")]
    public static void ShowWindow()
    {
        GetWindow<CharacterToImageGenerator>("Character To Image");
    }

    private void OnEnable()
    {
        customFont = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        UpdatePreviewTexture();
    }

    private void OnGUI()
    {
        GUILayout.Label("Character to Image Generator", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input Settings", EditorStyles.boldLabel);
        
        // Character input
        GUI.SetNextControlName("CharacterField");
        string charInput = EditorGUILayout.TextField("Character", inputCharacter.ToString());
        if (charInput.Length > 0)
        {
            inputCharacter = charInput[0];
            UpdatePreviewTexture();
        }

        // Image settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Image Settings", EditorStyles.boldLabel);
        
        int newSize = EditorGUILayout.IntSlider("Image Size", imageSize, 32, 128);
        if (newSize != imageSize)
        {
            imageSize = newSize;
            UpdatePreviewTexture();
        }
        
        Color newBgColor = EditorGUILayout.ColorField("Background Color", backgroundColor);
        if (newBgColor != backgroundColor)
        {
            backgroundColor = newBgColor;
            UpdatePreviewTexture();
        }
        
        Color newFgColor = EditorGUILayout.ColorField("Foreground Color", foregroundColor);
        if (newFgColor != foregroundColor)
        {
            foregroundColor = newFgColor;
            UpdatePreviewTexture();
        }

        // Font settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Font Settings", EditorStyles.boldLabel);
        
        Font newFont = (Font)EditorGUILayout.ObjectField("Custom Font", customFont, typeof(Font), false);
        if (newFont != customFont)
        {
            customFont = newFont;
            UpdatePreviewTexture();
        }
        
        int newFontSize = EditorGUILayout.IntSlider("Font Size", fontSize, 10, imageSize);
        if (newFontSize != fontSize)
        {
            fontSize = newFontSize;
            UpdatePreviewTexture();
        }
        
        TextAnchor newAlignment = (TextAnchor)EditorGUILayout.EnumPopup("Text Alignment", textAlignment);
        if (newAlignment != textAlignment)
        {
            textAlignment = newAlignment;
            UpdatePreviewTexture();
        }

        // Save settings
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);
        
        savePath = EditorGUILayout.TextField("Save Path", savePath);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        // Preview
        EditorGUILayout.Space();
        showPreview = EditorGUILayout.Foldout(showPreview, "Preview");
        if (showPreview && previewTexture != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(previewTexture, GUILayout.Width(Mathf.Min(imageSize, 256)), GUILayout.Height(Mathf.Min(imageSize, 256)));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        // Generate button
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Image"))
        {
            GenerateAndSaveImage();
        }

        // Focus the character field by default
        if (Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() == "")
        {
            EditorGUI.FocusTextInControl("CharacterField");
        }
    }

    private void UpdatePreviewTexture()
    {
        previewTexture = GenerateCharacterTexture();
    }

    private Texture2D GenerateCharacterTexture()
    {
        // Create a new texture with the specified size
        Texture2D texture = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);
        
        // Fill the texture with the background color
        Color[] colors = new Color[imageSize * imageSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = backgroundColor;
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        // Draw the character onto the texture
        DrawCharacterOnTexture(texture, inputCharacter.ToString(), foregroundColor);
        
        return texture;
    }

    private void DrawCharacterOnTexture(Texture2D texture, string text, Color textColor)
    {
        // Create a temporary GameObject with TextMesh to render the text
        GameObject tempObj = new GameObject("TempTextRenderer");
        TextMesh textMesh = tempObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 100; // 固定字体大小，后面会进行缩放
        textMesh.color = textColor;
        textMesh.font = customFont;
        textMesh.alignment = TextAlignment.Center; // 强制居中对齐
        textMesh.anchor = TextAnchor.MiddleCenter; // 锚点为中心
        textMesh.characterSize = 0.5f; // 调整字符大小
        
        // 添加Mesh渲染器组件并设置
        MeshRenderer renderer = tempObj.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("GUI/Text Shader"));
        renderer.material.mainTexture = customFont.material.mainTexture;
        
        // 创建一个空对象作为父对象，以便于居中
        GameObject container = new GameObject("TextContainer");
        tempObj.transform.parent = container.transform;
        
        // 创建临时相机
        GameObject cameraObj = new GameObject("TempCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.backgroundColor = backgroundColor;
        camera.clearFlags = backgroundColor.a < 1 ? CameraClearFlags.Depth : CameraClearFlags.SolidColor; // 当alpha小于1时使用Depth模式以支持透明
        camera.orthographic = true;
        camera.orthographicSize = 0.6f; // 正交大小足够放下字符
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 10f;
        
        // 放置相机和文本
        container.transform.position = Vector3.zero;
        cameraObj.transform.position = new Vector3(0, 0, -5); // 相机位置
        cameraObj.transform.LookAt(container.transform.position);
        
        // 通过缩放调整文本大小来匹配所需的fontSize
        float scaleFactor = fontSize / 100.0f;
        tempObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        
        // 创建目标渲染纹理，确保支持透明通道
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = renderTexture;
        
        // 如果需要透明背景，先清除渲染纹理
        if(backgroundColor.a < 1)
        {
            RenderTexture rt = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            RenderTexture.active = rt;
        }
        
        camera.Render();
        
        // 复制渲染纹理到目标纹理
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        
        // 清理
        RenderTexture.active = null;
        camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        Object.DestroyImmediate(container); // 销毁容器会同时销毁子对象
        Object.DestroyImmediate(cameraObj);
    }

    private TextAlignment ConvertTextAnchorToTextAlignment(TextAnchor anchor)
    {
        switch(anchor)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                return TextAlignment.Left;
            
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                return TextAlignment.Right;
            
            default:
                return TextAlignment.Center;
        }
    }

    private void GenerateAndSaveImage()
    {
        // 直接调用静态方法生成并保存图片
        string filePath = CharacterImageGenerator.GenerateAndSaveCharacterImage(
            inputCharacter,
            savePath,
            fileName,
            imageSize,
            backgroundColor,
            foregroundColor,
            customFont,
            fontSize
        );
        
        if (!string.IsNullOrEmpty(filePath))
        {
            EditorUtility.DisplayDialog("Image Generated", "Image saved to:\n" + filePath, "OK");
            
            // 选择并显示生成的图片
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    }

    // Add this new method to sanitize filename
    private string SanitizeForFileName(string input)
    {
        // These characters are not allowed in Windows file names
        char[] invalidChars = Path.GetInvalidFileNameChars();
        
        // Replace invalid characters with underscore
        foreach (char invalidChar in invalidChars)
        {
            input = input.Replace(invalidChar, '_');
        }
        
        return input;
    }
}

/// <summary>
/// 静态类，提供从任何脚本生成字符图片的功能
/// </summary>
public static class CharacterImageGenerator
{
    /// <summary>
    /// 生成字符图片并返回Texture2D对象
    /// </summary>
    /// <param name="character">要绘制的字符</param>
    /// <param name="imageSize">图片尺寸</param>
    /// <param name="backgroundColor">背景颜色</param>
    /// <param name="foregroundColor">文字颜色</param>
    /// <param name="font">字体，如果为null则使用默认字体</param>
    /// <param name="fontSize">字体大小</param>
    /// <returns>生成的图片纹理</returns>
    public static Texture2D GenerateCharacterTexture(char character, int imageSize = 256, Color? backgroundColor = null, 
                                                    Color? foregroundColor = null, Font font = null, int fontSize = 128)
    {
        // 设置默认值
        backgroundColor = backgroundColor ?? Color.white;
        foregroundColor = foregroundColor ?? Color.black;
        
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        }
        
        // 创建纹理
        Texture2D texture = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);
        
        // 填充背景色
        Color[] colors = new Color[imageSize * imageSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = backgroundColor.Value;
        }
        texture.SetPixels(colors);
        texture.Apply();
        
        // 绘制字符
        DrawCharacterOnTexture(texture, character.ToString(), foregroundColor.Value, font, fontSize, backgroundColor.Value);
        
        return texture;
    }
    
    /// <summary>
    /// 生成字符图片并保存到指定路径
    /// </summary>
    /// <param name="character">要绘制的字符</param>
    /// <param name="savePath">保存路径</param>
    /// <param name="fileName">文件名前缀</param>
    /// <param name="imageSize">图片尺寸</param>
    /// <param name="backgroundColor">背景颜色</param>
    /// <param name="foregroundColor">文字颜色</param>
    /// <param name="font">字体，如果为null则使用默认字体</param>
    /// <param name="fontSize">字体大小</param>
    /// <returns>生成的图片的完整路径</returns>
    public static string GenerateAndSaveCharacterImage(char character, string savePath = "Assets/GeneratedImages", 
                                                      string fileName = "CharImage", int imageSize = 256, 
                                                      Color? backgroundColor = null, Color? foregroundColor = null, 
                                                      Font font = null, int fontSize = 128)
    {
#if UNITY_EDITOR
        // 创建目录
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
        
        // 生成纹理
        Texture2D texture = GenerateCharacterTexture(character, imageSize, backgroundColor, foregroundColor, font, fontSize);
        
        // 转换为PNG
        byte[] bytes = texture.EncodeToPNG();
        
        // 处理文件名中的非法字符
        string safeChar = SanitizeForFileName(character.ToString());
        
        // 创建唯一文件名
        string uniqueFileName = string.Format("{0}_{1}_{2}.png", fileName, safeChar, System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        string filePath = Path.Combine(savePath, uniqueFileName);
        
        // 保存图片
        File.WriteAllBytes(filePath, bytes);
        
        // 刷新AssetDatabase
        AssetDatabase.Refresh();
        
        // 配置导入设置
        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = imageSize;
            importer.filterMode = FilterMode.Point; // 设置过滤模式为Point
            importer.SaveAndReimport();
        }
        
        return filePath;
#else
        Debug.LogWarning("GenerateAndSaveCharacterImage is only available in the Unity Editor.");
        return null;
#endif
    }
    
    private static void DrawCharacterOnTexture(Texture2D texture, string text, Color textColor, Font font, int fontSize, Color backgroundColor)
    {
        // 创建临时的TextMesh对象
        GameObject tempObj = new GameObject("TempTextRenderer");
        TextMesh textMesh = tempObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 100; // 固定字体大小，后面会进行缩放
        textMesh.color = textColor;
        textMesh.font = font;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.5f;
        
        // 添加Mesh渲染器组件并设置
        MeshRenderer renderer = tempObj.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("GUI/Text Shader"));
        renderer.material.mainTexture = font.material.mainTexture;
        
        // 创建容器对象
        GameObject container = new GameObject("TextContainer");
        tempObj.transform.parent = container.transform;
        
        // 创建临时相机
        GameObject cameraObj = new GameObject("TempCamera");
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.backgroundColor = backgroundColor;
        camera.clearFlags = CameraClearFlags.SolidColor;  // 不使用透明
        camera.orthographic = true;
        camera.orthographicSize = 0.6f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 10f;
        
        // 放置相机和文本
        container.transform.position = Vector3.zero;
        cameraObj.transform.position = new Vector3(0, 0, -5);
        cameraObj.transform.LookAt(container.transform.position);
        
        // 缩放调整
        float scaleFactor = fontSize / 100.0f;
        tempObj.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        
        // 创建RenderTexture
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = renderTexture;
        camera.Render();
        
        // 复制渲染纹理到目标纹理
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();
        
        // 清理
        RenderTexture.active = null;
        camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTexture);
        Object.DestroyImmediate(container);
        Object.DestroyImmediate(cameraObj);
    }
    
    private static string SanitizeForFileName(string input)
    {
        // 处理Windows文件名不允许的字符
        char[] invalidChars = Path.GetInvalidFileNameChars();
        
        foreach (char invalidChar in invalidChars)
        {
            input = input.Replace(invalidChar, '_');
        }
        
        return input;
    }
} 