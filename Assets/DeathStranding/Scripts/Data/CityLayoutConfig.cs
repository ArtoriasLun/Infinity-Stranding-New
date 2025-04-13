using UnityEngine;
using System.Collections.Generic;

namespace ALUNGAMES
{
    [CreateAssetMenu(fileName = "CityLayoutConfig", menuName = "😊/City Layout Config")]
    public class CityLayoutConfig : ScriptableObject
    {
        [System.Serializable]
        public class CityLayout
        {
            public string layoutName;
            [TextArea(10, 30)]
            public string layoutData;

            // 将字符串布局数据转换为二维数组
            public string[,] GetLayoutArray()
            {
                string[] rows = layoutData.Split('\n');
                // 移除空行
                rows = System.Array.FindAll(rows, row => !string.IsNullOrWhiteSpace(row));
                
                if (rows.Length == 0) return new string[0, 0];

                int height = rows.Length;
                int width = rows[0].Length;
                string[,] layout = new string[height, width];

                for (int y = 0; y < height; y++)
                {
                    string row = rows[y].TrimEnd();
                    for (int x = 0; x < width && x < row.Length; x++)
                    {
                        layout[y, x] = row[x].ToString();
                    }
                }

                return layout;
            }
        }

        [SerializeField]
        private List<CityLayout> cityLayouts = new List<CityLayout>();

        public List<CityLayout> CityLayouts => cityLayouts;

        private void OnValidate()
        {
            // 验证所有布局的有效性
            foreach (var layout in cityLayouts)
            {
                if (string.IsNullOrEmpty(layout.layoutName))
                {
                    Debug.LogWarning("City layout name cannot be empty!");
                }

                if (string.IsNullOrEmpty(layout.layoutData))
                {
                    Debug.LogWarning($"Layout data is empty for {layout.layoutName}!");
                    continue;
                }

                // 验证布局数据的格式
                string[] rows = layout.layoutData.Split('\n');
                rows = System.Array.FindAll(rows, row => !string.IsNullOrWhiteSpace(row));
                
                if (rows.Length == 0)
                {
                    Debug.LogWarning($"Invalid layout data for {layout.layoutName}: No valid rows found!");
                    continue;
                }

                int expectedWidth = rows[0].Length;
                for (int i = 1; i < rows.Length; i++)
                {
                    if (rows[i].Length != expectedWidth)
                    {
                        Debug.LogWarning($"Invalid layout data for {layout.layoutName}: Row {i + 1} has inconsistent width!");
                    }
                }
            }
        }
    }
} 