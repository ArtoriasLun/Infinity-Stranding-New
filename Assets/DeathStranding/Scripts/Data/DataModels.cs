using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALUNGAMES
{
    // 地形类型枚举
    public enum TerrainType
    {
        Grass,
        Mountain,
        Water,
        Tree,
        LargeTree,  // 2x2大树
        Road,
        City
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

        public Building(string type, Vector2Int localPosition)
        {
            Type = type;
            LocalPosition = localPosition;
            Name = type.ToUpper(); // 默认名称为类型的大写形式
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