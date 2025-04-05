using UnityEngine;
using UnityEngine.UIElements;

public class SpaceTradeController : MonoBehaviour
{
    private VisualElement root;
    private VisualElement marketItemsElement;
    private VisualElement inventoryItemsElement;
    private VisualElement travelOptionsElement;
    private string selectedMarketItem;
    private string selectedInventoryItem;
    private GameState gameState;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        marketItemsElement = root.Q<VisualElement>("market-items");
        inventoryItemsElement = root.Q<VisualElement>("inventory-items");
        travelOptionsElement = root.Q<VisualElement>("travel-options");
        gameState = FindObjectOfType<GameState>();

        // 修改市场按钮
        root.Q<Button>("market-buy-1").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) BuyGood(selectedMarketItem, 1);
        };
        
        root.Q<Button>("market-buy-10").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) BuyGood(selectedMarketItem, 10);
        };
        
        root.Q<Button>("market-buy-100").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) BuyGood(selectedMarketItem, 100);
        };
        
        root.Q<Button>("market-buy-1000").clicked += () => {
            if (!string.IsNullOrEmpty(selectedMarketItem)) {
                var marketItem = gameState.planets[gameState.currentPlanet].market[selectedMarketItem];
                int maxAmount = Mathf.Min(marketItem.quantity, 1000);
                BuyGood(selectedMarketItem, maxAmount);
            }
        };
        
        // 修改库存按钮
        root.Q<Button>("inventory-sell-1").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem)) SellGood(selectedInventoryItem, 1);
        };
        
        root.Q<Button>("inventory-sell-10").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem)) SellGood(selectedInventoryItem, 10);
        };
        
        root.Q<Button>("inventory-sell-100").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem)) SellGood(selectedInventoryItem, 100);
        };
        
        root.Q<Button>("inventory-sell-1000").clicked += () => {
            if (!string.IsNullOrEmpty(selectedInventoryItem) && gameState.cargo.ContainsKey(selectedInventoryItem)) {
                int maxAmount = Mathf.Min(gameState.cargo[selectedInventoryItem].quantity, 1000);
                SellGood(selectedInventoryItem, maxAmount);
            }
        };
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
                travelButton.text = $"Travel to {GetPlanetDisplayName(planetId)} (Fuel: {distance})";
                travelButton.AddToClassList("button");
                
                travelOptionsElement.Add(travelButton);
            }
        }
    }

    private string GetGoodDisplayName(string goodId)
    {
        switch (goodId)
        {
            case "food": return "Food";
            case "water": return "Water";
            case "ore": return "Ore";
            case "fuel": return "Fuel";
            default: return goodId;
        }
    }

    private string GetPlanetDisplayName(string planetId)
    {
        switch (planetId)
        {
            case "earth": return "Earth";
            case "mars": return "Mars";
            case "venus": return "Venus";
            case "mercury": return "Mercury";
            case "jupiter": return "Jupiter";
            default: return planetId;
        }
    }

    private VisualElement CreateSelectableRow(string goodId, params string[] cells)
    {
        var row = new VisualElement();
        row.AddToClassList("selectable-row");
        row.userData = goodId;
        
        var selectTd = new VisualElement();
        selectTd.AddToClassList("td");
        selectTd.AddToClassList("col-select");
        
        var marker = new VisualElement();
        marker.AddToClassList("selection-marker");
        selectTd.Add(marker);
        
        row.Add(selectTd);
        
        for (int i = 0; i < cells.Length; i++)
        {
            var td = new VisualElement();
            td.AddToClassList("td");
            
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
            
            td.Add(label);
            row.Add(td);
        }
        
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

    private void SelectMarketRow(string goodId)
    {
        foreach (var row in marketItemsElement.Query(className: "selectable-row").ToList())
        {
            row.RemoveFromClassList("selected");
        }
        
        foreach (var row in marketItemsElement.Query(className: "selectable-row").ToList())
        {
            if ((string)row.userData == goodId)
            {
                row.AddToClassList("selected");
                row.Focus();
                Debug.Log($"Selected market item: {GetGoodDisplayName(goodId)}");
                break;
            }
        }
        
        selectedMarketItem = goodId;
        marketItemsElement.Focus();
    }

    private void SelectInventoryRow(string goodId)
    {
        foreach (var row in inventoryItemsElement.Query(className: "selectable-row").ToList())
        {
            row.RemoveFromClassList("selected");
        }
        
        foreach (var row in inventoryItemsElement.Query(className: "selectable-row").ToList())
        {
            if ((string)row.userData == goodId)
            {
                row.AddToClassList("selected");
                row.Focus();
                Debug.Log($"Selected inventory item: {GetGoodDisplayName(goodId)}");
                break;
            }
        }
        
        selectedInventoryItem = goodId;
        inventoryItemsElement.Focus();
    }

    private void ClearMarketSelection()
    {
        selectedMarketItem = null;
    }

    private void ClearInventorySelection()
    {
        selectedInventoryItem = null;
    }

    private void BuyGood(string goodId, int quantity)
    {
        // Implementation of BuyGood method
    }

    private void SellGood(string goodId, int quantity)
    {
        // Implementation of SellGood method
    }

    private void TravelTo(string planetId)
    {
        // Implementation of TravelTo method
    }

    private float CalculateDistance(Planet currentPlanet, Planet targetPlanet)
    {
        // Implementation of CalculateDistance method
        return 0f; // Placeholder return, actual implementation needed
    }
} 