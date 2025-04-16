public enum BuildingType
{
    Bar,        // 酒吧，暂时没有功能
    Restaurant, // 餐厅，暂时没有功能
    Exchange,   // 贸易商店，暂时没有功能
    Yard,       // 包裹坞，必定有交货点，随机可能有取货点
    Hotel       // 旅馆，必定有休息点
}

public enum SizeCategory
{
    Small,      // 5x5
    Medium,     // 7x7
    Large,      // 9x9
    LargeL      // 11x9
}

public enum SpecialPointType
{
    DeliveryPoint,  // 交货点 □
    PickupPoint,    // 取货点 ■
    RestPoint       // 休息点 +
} 