using UnityEngine;

namespace ALUNGAMES.Data
{
    public static class TileID
    {
        // 基础地形 (0-99)
        public const int EMPTY = 0;
        public const int GRASS = 1;
        public const int MOUNTAIN = 2;
        public const int WATER = 3;
        public const int ROAD = 4;
        public const int WALL = 5;
        public const int GATE = 6;
        public const int TREE = 7;
        public const int BUILDING = 8;

        // 城市相关 (100-199)
        public const int CITY_WALL = 100;
        public const int CITY_GATE = 101;
        public const int BUILDING_WALL = 102;
        public const int BUILDING_GATE = 103;

        // 特殊点 (200-299)
        public const int TASK_POINT = 200;
        public const int DELIVERY_POINT = 201;
        public const int REST_POINT = 202;

        // 自然物体 (300-399)
        public const int LARGE_TREE = 301;

        // 建筑类型 (400-499)
        public const int BAR = 400;
        public const int YARD = 401;
        public const int HOTEL = 402;
        public const int EXCHANGE = 403;

        // 获取ID的类别
        public static TileCategory GetCategory(int id)
        {
            if (id < 100) return TileCategory.Terrain;
            if (id < 200) return TileCategory.City;
            if (id < 300) return TileCategory.SpecialPoint;
            if (id < 400) return TileCategory.Nature;
            if (id < 500) return TileCategory.Building;
            return TileCategory.Unknown;
        }
    }

    // Tile类别枚举
    public enum TileCategory
    {
        Unknown,
        Terrain,    // 基础地形
        City,       // 城市相关
        SpecialPoint, // 特殊点
        Nature,     // 自然物体
        Building    // 建筑
    }
} 