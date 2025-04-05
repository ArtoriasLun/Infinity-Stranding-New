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

    // 在类中添加新的字段来跟踪选中的项目
    private string selectedMarketItem = null;
    private string selectedInventoryItem = null;

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

        // 注册市场按钮
        root.Q<Button>("market-buy-1").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) BuyGood(selectedMarketItem, 1);
        };
        
        root.Q<Button>("market-buy-10").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) BuyGood(selectedMarketItem, 10);
        };
        
        root.Q<Button>("market-buy-100").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) BuyGood(selectedMarketItem, 100);
        };
        
        root.Q<Button>("market-buy-max").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) {
                var marketItem = gameState.planets[gameState.currentPlanet].market[selectedMarketItem];
                int maxAfford = Mathf.FloorToInt(gameState.credits / marketItem.price);
                int maxSpace = gameState.cargoCapacity - gameState.cargo.Values.Sum(item => item.quantity);
                int maxAmount = Mathf.Min(marketItem.quantity, maxAfford, maxSpace);
                BuyGood(selectedMarketItem, maxAmount);
            }
        };
        
        // 注册库存按钮
        root.Q<Button>("inventory-sell-1").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem)) SellGood(selectedInventoryItem, 1);
        };
        
        root.Q<Button>("inventory-sell-10").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem)) SellGood(selectedInventoryItem, 10);
        };
        
        root.Q<Button>("inventory-sell-100").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem)) SellGood(selectedInventoryItem, 100);
        };
        
        root.Q<Button>("inventory-sell-all").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem) && gameState.cargo.ContainsKey(selectedInventoryItem)) {
                SellGood(selectedInventoryItem, gameState.cargo[selectedInventoryItem].quantity);
            }
        };
        
        // 为表格容器添加键盘导航支持
        marketItemsElement.RegisterCallback<KeyDownEvent>(OnMarketKeyDown);
        inventoryItemsElement.RegisterCallback<KeyDownEvent>(OnInventoryKeyDown);
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
        
        var market = gameState.planets[gameState.currentPlanet].market;
        bool hasTradableItems = false;
        
        foreach (var marketPair in market)
        {
            var goodId = marketPair.Key;
            var marketItem = marketPair.Value;
            
            if (goodId != "fuel") // 燃料特殊处理
            {
                hasTradableItems = true;
                var row = CreateSelectableRow(
                    goodId, 
                    GetGoodDisplayName(goodId), 
                    $"{marketItem.price}", 
                    $"{marketItem.quantity}"
                );
                
                marketItemsElement.Add(row);
            }
        }
        
        // 如果没有可交易的商品，显示提示
        if (!hasTradableItems) 
        {
            var emptyRow = new Label("这个星球没有可交易的商品");
            emptyRow.AddToClassList("empty-message");
            marketItemsElement.Add(emptyRow);
            selectedMarketItem = null;
            return;
        }
        
        // 如果之前有选中的商品，尝试重新选择它
        if (!string.IsNullOrEmpty(selectedMarketItem) && market.ContainsKey(selectedMarketItem))
        {
            SelectMarketRow(selectedMarketItem);
        }
        // 否则选择第一个商品（如果有）
        else if (marketItemsElement.childCount > 0)
        {
            var firstRow = marketItemsElement.Q(className: "selectable-row");
            if (firstRow != null)
            {
                SelectMarketRow(firstRow.userData as string);
            }
        }
    }

    private void UpdateInventory()
    {
        inventoryItemsElement.Clear();
        
        if (gameState.cargo.Count == 0)
        {
            var emptyRow = new Label("货舱是空的");
            emptyRow.AddToClassList("empty-message");
            inventoryItemsElement.Add(emptyRow);
            selectedInventoryItem = null;
            return;
        }
        
        foreach (var cargoPair in gameState.cargo)
        {
            var goodId = cargoPair.Key;
            var cargoItem = cargoPair.Value;
            
            var row = CreateSelectableRow(
                goodId,
                GetGoodDisplayName(goodId),
                $"{cargoItem.quantity}",
                $"{cargoItem.buyPrice}"
            );
            
            inventoryItemsElement.Add(row);
        }
        
        // 如果之前有选中的商品，尝试重新选择它
        if (!string.IsNullOrEmpty(selectedInventoryItem) && gameState.cargo.ContainsKey(selectedInventoryItem))
        {
            SelectInventoryRow(selectedInventoryItem);
        }
        // 否则选择第一个商品（如果有）
        else if (inventoryItemsElement.childCount > 0)
        {
            var firstRow = inventoryItemsElement.Q(className: "selectable-row");
            if (firstRow != null)
            {
                SelectInventoryRow(firstRow.userData as string);
            }
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
                travelButton.style.borderLeftWidth = 0;
                travelButton.style.borderRightWidth = 0;
                travelButton.style.borderTopWidth = 0;
                travelButton.style.borderBottomWidth = 0;
                
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

    // 为键盘导航添加事件处理
    private void OnMarketKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow)
        {
            var rows = marketItemsElement.Query(className: "selectable-row").ToList();
            if (rows.Count == 0) return;
            
            int selectedIndex = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].ClassListContains("selected"))
                {
                    selectedIndex = i;
                    break;
                }
            }
            
            // 计算新的选择索引
            if (evt.keyCode == KeyCode.UpArrow)
                selectedIndex = (selectedIndex <= 0) ? rows.Count - 1 : selectedIndex - 1;
            else // Down arrow
                selectedIndex = (selectedIndex >= rows.Count - 1) ? 0 : selectedIndex + 1;
            
            // 设置新选择的商品
            SelectMarketRow(rows[selectedIndex].userData as string);
        }
    }

    private void OnInventoryKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow)
        {
            var rows = inventoryItemsElement.Query(className: "selectable-row").ToList();
            if (rows.Count == 0) return;
            
            int selectedIndex = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].ClassListContains("selected"))
                {
                    selectedIndex = i;
                    break;
                }
            }
            
            // 计算新的选择索引
            if (evt.keyCode == KeyCode.UpArrow)
                selectedIndex = (selectedIndex <= 0) ? rows.Count - 1 : selectedIndex - 1;
            else // Down arrow
                selectedIndex = (selectedIndex >= rows.Count - 1) ? 0 : selectedIndex + 1;
            
            // 设置新选择的商品
            SelectInventoryRow(rows[selectedIndex].userData as string);
        }
    }

    // 创建可选择的行
    private VisualElement CreateSelectableRow(string goodId, params string[] cells)
    {
        var row = new VisualElement();
        row.AddToClassList("selectable-row");
        row.userData = goodId; // 存储商品ID以便于选择
        row.style.borderLeftWidth = 0;
        row.style.borderRightWidth = 0;
        row.style.borderTopWidth = 0;
        row.style.borderBottomWidth = 0;
        
        // 创建选择指示器单元格
        var selectTd = new VisualElement();
        selectTd.AddToClassList("td");
        selectTd.AddToClassList("col-select");
        selectTd.style.width = 56;  // 设置宽度为56px，与表头保持一致
        selectTd.style.borderLeftWidth = 0;
        selectTd.style.borderRightWidth = 0;
        selectTd.style.borderTopWidth = 0;
        selectTd.style.borderBottomWidth = 0;
        
        // 添加方块指示器
        var marker = new VisualElement();
        marker.AddToClassList("selection-marker");
        marker.style.borderLeftWidth = 0;
        marker.style.borderRightWidth = 0;
        marker.style.borderTopWidth = 0;
        marker.style.borderBottomWidth = 0;
        selectTd.Add(marker);
        
        row.Add(selectTd);
        
        // 为每个单元格创建内容
        for (int i = 0; i < cells.Length; i++)
        {
            var td = new VisualElement();
            td.AddToClassList("td");
            td.style.borderLeftWidth = 0;
            td.style.borderRightWidth = 0;
            td.style.borderTopWidth = 0;
            td.style.borderBottomWidth = 0;
            
            // 添加对应的列样式类
            if (i == 0)
                td.AddToClassList("col-item");
            else if (i == 1)
                td.AddToClassList("col-price");
            else if (i == 2)
                td.AddToClassList("col-quantity");
            
            var label = new Label(cells[i]);
            label.style.overflow = Overflow.Hidden;
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.borderLeftWidth = 0;
            label.style.borderRightWidth = 0;
            label.style.borderTopWidth = 0;
            label.style.borderBottomWidth = 0;
            
            td.Add(label);
            row.Add(td);
        }
        
        // 添加点击事件，只改变选择状态
        row.RegisterCallback<ClickEvent>(evt => {
            if (row.parent == marketItemsElement) {
                SelectMarketRow(goodId);
                ClearInventorySelection();
            }
            else if (row.parent == inventoryItemsElement) {
                SelectInventoryRow(goodId);
                ClearMarketSelection();
            }
        });
        
        return row;
    }

    // 清除市场选择
    private void ClearMarketSelection()
    {
        foreach (var row in marketItemsElement.Query(className: "selectable-row").ToList())
        {
            row.RemoveFromClassList("selected");
        }
        selectedMarketItem = null;
    }

    // 清除库存选择
    private void ClearInventorySelection()
    {
        foreach (var row in inventoryItemsElement.Query(className: "selectable-row").ToList())
        {
            row.RemoveFromClassList("selected");
        }
        selectedInventoryItem = null;
    }

    // 选择市场行
    private void SelectMarketRow(string goodId)
    {
        // 清除之前的选择
        foreach (var row in marketItemsElement.Query(className: "selectable-row").ToList())
        {
            row.RemoveFromClassList("selected");
        }
        
        // 选择新行
        foreach (var row in marketItemsElement.Query(className: "selectable-row").ToList())
        {
            if ((string)row.userData == goodId)
            {
                row.AddToClassList("selected");
                row.Focus(); // 给选中的行增加焦点
                
                // 让选中效果更明显
                Debug.Log($"选中了市场商品: {GetGoodDisplayName(goodId)}");
                break;
            }
        }
        
        selectedMarketItem = goodId;
        
        // 确保表格容器获得焦点，这样:focus选择器才能生效
        marketItemsElement.Focus();
    }

    // 选择库存行
    private void SelectInventoryRow(string goodId)
    {
        // 清除之前的选择
        foreach (var row in inventoryItemsElement.Query(className: "selectable-row").ToList())
        {
            row.RemoveFromClassList("selected");
        }
        
        // 选择新行
        foreach (var row in inventoryItemsElement.Query(className: "selectable-row").ToList())
        {
            if ((string)row.userData == goodId)
            {
                row.AddToClassList("selected");
                row.Focus(); // 给选中的行增加焦点
                
                // 让选中效果更明显
                Debug.Log($"选中了库存商品: {GetGoodDisplayName(goodId)}");
                break;
            }
        }
        
        selectedInventoryItem = goodId;
        
        // 确保表格容器获得焦点，这样:focus选择器才能生效
        inventoryItemsElement.Focus();
    }

    // 获取商品的显示名称
    private string GetGoodDisplayName(string goodId)
    {
        switch (goodId)
        {
            case "food": return "食物";
            case "water": return "水";
            case "ore": return "矿石";
            case "fuel": return "燃料";
            default: return goodId;
        }
    }
} 