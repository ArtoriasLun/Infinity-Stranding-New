using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameState
{
    public string currentPlanet = "earth";
    public int fuel = 100;
    public int maxFuel = 100;
    public int credits = 1000;
    public int cargoCapacity = 10;
    public Dictionary<string, CargoItem> cargo = new Dictionary<string, CargoItem>();
    public Dictionary<string, Planet> planets = new Dictionary<string, Planet>();
    public Dictionary<string, Good> goods = new Dictionary<string, Good>();

    public GameState()
    {
        // 初始化星球
        planets = new Dictionary<string, Planet>
        {
            ["earth"] = new Planet
            {
                name = "地球",
                x = 3, y = 3,
                market = new Dictionary<string, MarketItem>
                {
                    ["food"] = new MarketItem { price = 10, quantity = 100 },
                    ["water"] = new MarketItem { price = 5, quantity = 200 },
                    ["ore"] = new MarketItem { price = 50, quantity = 30 },
                    ["fuel"] = new MarketItem { price = 8, quantity = 1000 }
                }
            },
            ["mars"] = new Planet
            {
                name = "火星",
                x = 8, y = 3,
                market = new Dictionary<string, MarketItem>
                {
                    ["food"] = new MarketItem { price = 30, quantity = 50 },
                    ["water"] = new MarketItem { price = 40, quantity = 30 },
                    ["ore"] = new MarketItem { price = 30, quantity = 100 },
                    ["fuel"] = new MarketItem { price = 12, quantity = 800 }
                }
            },
            ["venus"] = new Planet
            {
                name = "金星",
                x = 3, y = 8,
                market = new Dictionary<string, MarketItem>
                {
                    ["food"] = new MarketItem { price = 20, quantity = 80 },
                    ["water"] = new MarketItem { price = 60, quantity = 10 },
                    ["ore"] = new MarketItem { price = 40, quantity = 70 },
                    ["fuel"] = new MarketItem { price = 15, quantity = 500 }
                }
            },
            ["jupiter"] = new Planet
            {
                name = "木星",
                x = 10, y = 10,
                market = new Dictionary<string, MarketItem>
                {
                    ["food"] = new MarketItem { price = 50, quantity = 20 },
                    ["water"] = new MarketItem { price = 80, quantity = 5 },
                    ["ore"] = new MarketItem { price = 20, quantity = 150 },
                    ["fuel"] = new MarketItem { price = 5, quantity = 2000 }
                }
            }
        };

        // 初始化商品
        goods = new Dictionary<string, Good>
        {
            ["food"] = new Good { name = "食物" },
            ["water"] = new Good { name = "水" },
            ["ore"] = new Good { name = "矿石" },
            ["fuel"] = new Good { name = "燃料" }
        };
    }
}

[Serializable]
public class Planet
{
    public string name;
    public int x, y;
    public Dictionary<string, MarketItem> market = new Dictionary<string, MarketItem>();
}

[Serializable]
public class MarketItem
{
    public int price;
    public int quantity;
}

[Serializable]
public class Good
{
    public string name;
}

[Serializable]
public class CargoItem
{
    public int quantity;
    public int buyPrice;
} 