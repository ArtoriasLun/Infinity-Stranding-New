using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SpaceTradeController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    // UI元素引用
    private Label locationLabel;
    private Label fuelLabel;
    private Label creditsLabel;
    private Label cargoUsedLabel;
    private Label cargoCapacityLabel;
    private VisualElement mapElement;
    private VisualElement marketItemsElement;
    private VisualElement inventoryItemsElement;
    private VisualElement travelOptionsElement;
    private VisualElement gameOverElement;
    private Button resetGameButton;

    // 游戏状态
    private GameState gameState;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        
        // 获取UI元素引用
        locationLabel = root.Q<Label>("location");
        fuelLabel = root.Q<Label>("fuel");
        creditsLabel = root.Q<Label>("credits");
        cargoUsedLabel = root.Q<Label>("cargo-used");
        cargoCapacityLabel = root.Q<Label>("cargo-capacity");
        mapElement = root.Q<VisualElement>("map");
        marketItemsElement = root.Q<VisualElement>("market-items");
        inventoryItemsElement = root.Q<VisualElement>("inventory-items");
        travelOptionsElement = root.Q<VisualElement>("travel-options");
        gameOverElement = root.Q<VisualElement>("game-over");
        resetGameButton = root.Q<Button>("reset-game-button");

        // 初始化游戏
        resetGameButton.clicked += ResetGame;
        InitGame();
    }

    private void InitGame()
    {
        // 初始化游戏状态
        gameState = new GameState();
        UpdateAll();
    }

    private void UpdateAll()
    {
        UpdateMap();
        UpdateStatus();
        UpdateMarket();
        UpdateInventory();
        UpdateTravelOptions();
    }

    private void UpdateMap()
    {
        mapElement.Clear();
        
        for (int y = 0; y <= 12; y++)
        {
            string rowStr = "";
            
            for (int x = 0; x <= 12; x++)
            {
                bool foundPlanet = false;
                
                foreach (var planetPair in gameState.planets)
                {
                    var planetId = planetPair.Key;
                    var planet = planetPair.Value;
                    
                    if (planet.x == x && planet.y == y)
                    {
                        if (planetId == gameState.currentPlanet)
                        {
                            rowStr += "<color=#FF00FF>[X]</color>";
                        }
                        else
                        {
                            rowStr += "<color=#00FFFF>[O]</color>";
                        }
                        foundPlanet = true;
                        break;
                    }
                }
                
                if (!foundPlanet)
                {
                    rowStr += (x == 0 || y == 0 || x == 12 || y == 12) ? "+" : 
                            (x % 2 == 0 && y % 2 == 0) ? "." : " ";
                }
            }
            
            var rowLabel = new Label(rowStr);
            rowLabel.style.whiteSpace = WhiteSpace.NoWrap;
            mapElement.Add(rowLabel);
        }
    }

    private void UpdateStatus()
    {
        locationLabel.text = gameState.planets[gameState.currentPlanet].name;
        fuelLabel.text = gameState.fuel.ToString();
        creditsLabel.text = gameState.credits.ToString();
        
        int usedCargo = gameState.cargo.Values.Sum(item => item.quantity);
        cargoUsedLabel.text = usedCargo.ToString();
        cargoCapacityLabel.text = gameState.cargoCapacity.ToString();
        
        if (gameState.fuel < 30)
        {
            fuelLabel.AddToClassList(gameState.fuel < 0 ? "danger" : "warning");
        }
        else
        {
            fuelLabel.RemoveFromClassList("warning");
            fuelLabel.RemoveFromClassList("danger");
        }
        
        if (gameState.fuel < -10)
        {
            EndGame();
        }
    }

    private void UpdateMarket()
    {
        marketItemsElement.Clear();
        
        var planetMarket = gameState.planets[gameState.currentPlanet].market;
        
        foreach (var goodPair in planetMarket)
        {
            var goodId = goodPair.Key;
            var good = goodPair.Value;
            bool disabled = goodId == "fuel" && gameState.fuel >= gameState.maxFuel;
            
            var row = new VisualElement();
            row.AddToClassList("table-row");
            
            var nameCell = new VisualElement();
            nameCell.AddToClassList("td");
            nameCell.Add(new Label(gameState.goods[goodId].name));
            
            var priceCell = new VisualElement();
            priceCell.AddToClassList("td");
            priceCell.Add(new Label(good.price.ToString()));
            
            var quantityCell = new VisualElement();
            quantityCell.AddToClassList("td");
            quantityCell.Add(new Label(good.quantity.ToString()));
            
            var actionCell = new VisualElement();
            actionCell.AddToClassList("td");
            
            int[] quantities = { 1, 10, 100, 1000 };
            
            foreach (var qty in quantities)
            {
                var buyButton = new Button(() => BuyGood(goodId, qty));
                buyButton.text = $"买{qty}";
                buyButton.AddToClassList("button");
                
                if (disabled)
                {
                    buyButton.SetEnabled(false);
                }
                
                actionCell.Add(buyButton);
            }
            
            row.Add(nameCell);
            row.Add(priceCell);
            row.Add(quantityCell);
            row.Add(actionCell);
            
            marketItemsElement.Add(row);
        }
    }

    private void UpdateInventory()
    {
        inventoryItemsElement.Clear();
        
        foreach (var cargoPair in gameState.cargo)
        {
            var goodId = cargoPair.Key;
            var cargo = cargoPair.Value;
            
            var row = new VisualElement();
            row.AddToClassList("table-row");
            
            var nameCell = new VisualElement();
            nameCell.AddToClassList("td");
            nameCell.Add(new Label(gameState.goods[goodId].name));
            
            var quantityCell = new VisualElement();
            quantityCell.AddToClassList("td");
            quantityCell.Add(new Label(cargo.quantity.ToString()));
            
            var priceCell = new VisualElement();
            priceCell.AddToClassList("td");
            priceCell.Add(new Label(cargo.buyPrice.ToString()));
            
            var actionCell = new VisualElement();
            actionCell.AddToClassList("td");
            
            int[] quantities = { 1, 10, 100, 1000 };
            
            foreach (var qty in quantities)
            {
                var sellButton = new Button(() => SellGood(goodId, qty));
                sellButton.text = $"卖{qty}";
                sellButton.AddToClassList("button");
                actionCell.Add(sellButton);
            }
            
            row.Add(nameCell);
            row.Add(quantityCell);
            row.Add(priceCell);
            row.Add(actionCell);
            
            inventoryItemsElement.Add(row);
        }
    }

    private void UpdateTravelOptions()
    {
        travelOptionsElement.Clear();
        
        var currentPlanet = gameState.planets[gameState.currentPlanet];
        
        foreach (var planetPair in gameState.planets)
        {
            var planetId = planetPair.Key;
            var planet = planetPair.Value;
            
            if (planetId != gameState.currentPlanet)
            {
                var distance = Mathf.FloorToInt(CalculateDistance(currentPlanet, planet));
                
                var travelButton = new Button(() => TravelTo(planetId));
                travelButton.text = $"前往{planet.name} (燃料: {distance})";
                travelButton.AddToClassList("button");
                
                travelOptionsElement.Add(travelButton);
            }
        }
    }

    private float CalculateDistance(Planet planet1, Planet planet2)
    {
        var dx = planet1.x - planet2.x;
        var dy = planet1.y - planet2.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private void BuyGood(string goodId, int quantity)
    {
        var market = gameState.planets[gameState.currentPlanet].market[goodId];
        
        if (market.quantity < quantity)
        {
            Debug.Log("市场库存不足!");
            return;
        }

        var totalCost = market.price * quantity;
        
        if (gameState.credits < totalCost)
        {
            Debug.Log("资金不足!");
            return;
        }

        var currentCargo = gameState.cargo.Values.Sum(item => item.quantity);
        
        if (currentCargo + quantity > gameState.cargoCapacity && goodId != "fuel")
        {
            Debug.Log("货舱空间不足!");
            return;
        }

        // 特殊处理燃料购买
        if (goodId == "fuel")
        {
            var fuelSpace = gameState.maxFuel - gameState.fuel;
            var actualQty = Mathf.Min(quantity, fuelSpace);
            
            gameState.fuel += actualQty;
            market.quantity -= actualQty;
            gameState.credits -= market.price * actualQty;
        }
        else
        {
            gameState.credits -= totalCost;
            market.quantity -= quantity;
            
            if (!gameState.cargo.ContainsKey(goodId))
            {
                gameState.cargo[goodId] = new CargoItem { quantity = 0, buyPrice = market.price };
            }
            
            gameState.cargo[goodId].quantity += quantity;
        }

        UpdateAll();
    }

    private void SellGood(string goodId, int quantity)
    {
        if (!gameState.cargo.ContainsKey(goodId) || gameState.cargo[goodId].quantity < quantity)
        {
            Debug.Log("库存不足!");
            return;
        }
        
        var market = gameState.planets[gameState.currentPlanet].market[goodId];
        var totalIncome = market.price * quantity;
        
        gameState.credits += totalIncome;
        market.quantity += quantity;
        gameState.cargo[goodId].quantity -= quantity;

        if (gameState.cargo[goodId].quantity <= 0)
        {
            gameState.cargo.Remove(goodId);
        }
        
        UpdateAll();
    }

    private void TravelTo(string planetId)
    {
        var currentPlanet = gameState.planets[gameState.currentPlanet];
        var targetPlanet = gameState.planets[planetId];
        
        var distance = Mathf.FloorToInt(CalculateDistance(currentPlanet, targetPlanet));
        gameState.fuel -= distance;
        gameState.currentPlanet = planetId;
        
        UpdateAll();
    }

    private void EndGame()
    {
        gameOverElement.style.display = DisplayStyle.Flex;
    }

    private void ResetGame()
    {
        gameState.currentPlanet = "earth";
        gameState.fuel = 100;
        gameState.credits = 1000;
        gameState.cargo.Clear();
        
        // 重置所有星球市场
        foreach (var planet in gameState.planets.Values)
        {
            foreach (var item in planet.market.Values)
            {
                if (item.quantity < 100)
                {
                    item.quantity = 1000; // 简单重置
                }
            }
        }
        
        gameOverElement.style.display = DisplayStyle.None;
        UpdateAll();
    }
} 