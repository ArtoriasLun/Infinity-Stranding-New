using UnityEngine;

[CreateAssetMenu(fileName = "ASCIIConfig", menuName = "😊/ASCIIConfig")]
public class ASCIIConfig : ScriptableObject
{
    [System.Serializable]
    public struct TerrainConfig
    {
        public string terrainName; // 地形名称（如 "Plains", "Mountains"）
        public char asciiChar;     // 对应的 ASCII 字符（如 '.' 或 '^'）
        public Sprite sprite;      // 对应的 Sprite
    }
    [System.Serializable]
    public struct CityConfig
    {
        public string cityName; // 城市名称 如citySymbolPool字符的英语
        public char asciiChar;     // 对应的 ASCII 字符citySymbolPool = { "α", "β", "γ", "δ", "ε", "ζ", "η", "θ", "ι", "κ" };
        public Sprite sprite;      // 对应的 Sprite
    }
    [System.Serializable]
    public struct BuildingConfig
    {
        public string buildingName; // 建筑名称（如 "House", "Shop"）
        public char asciiChar;     // 对应的 ASCII 字符（如 'H' 或 'S'）
        public Sprite sprite;      // 对应的 Sprite
    }
    [System.Serializable]
    public struct CharacterConfig
    {
        public string characterName; // 角色名称（如 "Player", "Enemy"）
        public char asciiChar;       // 对应的 ASCII 字符（如 '@' 或 'M'）
        public Sprite sprite;        // 对应的 Sprite
    }

    [System.Serializable]
    public struct ItemConfig
    {
        public string itemName;    // 物品名称（如 "Cargo", "DeliveryPoint"）
        public char asciiChar;     // 对应的 ASCII 字符（如 '*' 或 'S'）
        public Sprite sprite;      // 对应的 Sprite
    }

    public TerrainConfig[] terrains;   // 地形配置
    public CityConfig[] cities; // 城市配置
    public BuildingConfig[] buildings; // 建筑配置
    public CharacterConfig[] characters; // 角色配置
    public ItemConfig[] items;         // 物品配置
}
