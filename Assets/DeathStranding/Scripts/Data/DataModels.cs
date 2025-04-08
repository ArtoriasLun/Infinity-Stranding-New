using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALUNGAMES
{
    // 地形类型枚举
    public enum TerrainType
    {
        Empty,          // 空地 ' '
        Road,           // 平地 '.'
        Grass,          // 草地 '*'
        Mountain,       // 山脉 '^'
        Water,          // 河流 '~'
        CityWall,       // 城墙 '#'
        CityGate,       // 城门 '|'
        BuildingWall,   // 建筑墙 '#'
        BuildingGate,   // 建筑门 '|'
        TaskPoint,      // 任务点 '■'
        DeliveryPoint,  // 交付点 '□'
        RestPoint,      // 休息点 '+'
        Tree,           // 小树 't'
        LargeTree,      // 大树 'T'
        Bar,            // 酒吧 'B'
        Yard,           // 仓库 'Y'
        Hotel,          // 旅馆 'H'
        Exchange,
        Player = -1        // 交易所 'E'
    }

    // 方向枚举
    public enum Direction
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft
    }

    // 建筑类
    [Serializable]
    public class Building
    {
        public string Type;
        public string Name;
        public Vector2Int LocalPosition;
        public int Width;
        public int Height;
        public string[,] Layout;
        public Dictionary<string, List<Vector2Int>> SpecialPoints = new Dictionary<string, List<Vector2Int>>();

        public Building(string type, Vector2Int localPosition)
        {
            Type = type;
            LocalPosition = localPosition;
            Name = type.ToUpper(); // 默认名称为类型的大写形式
            SpecialPoints = new Dictionary<string, List<Vector2Int>>();
        }
    }

    // 城市类
    [Serializable]
    public class City
    {
        public string Name;
        public Vector2Int Position;
        public List<Building> Buildings = new List<Building>();
        public int Size;
        public char Symbol; // 用于在地图上显示的符号

        public City(string name, Vector2Int position, int size)
        {
            Name = name;
            Position = position;
            Size = size;
            Symbol = '?'; // 默认符号
        }
    }

    // 任务类
    [Serializable]
    public class Task
    {
        public string Description;
        public Vector2Int Source;
        public Vector2Int Destination;
        public int CargoAmount;
        public float Reward;

        public Task(string description, Vector2Int source, Vector2Int destination, int cargoAmount, float reward)
        {
            Description = description;
            Source = source;
            Destination = destination;
            CargoAmount = cargoAmount;
            Reward = reward;
        }
    }
}