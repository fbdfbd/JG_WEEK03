public static class LobbyUiText
{
    public const string ProfileRequired = "프로필 필요";
    public const string Owned = "보유";
    public const string Equipped = "장착 중";
    public const string CanBuy = "구매 가능";
    public const string NeedMoreGold = "골드 부족";
    public const string Buy = "구매";
    public const string Use = "사용";
    public const string Equip = "장착";
    public const string SelectEquipment = "장비 선택";
    public const string SelectItem = "아이템 선택";
    public const string NeedMoreCost = "코스트 부족";
    public const string SelectTile = "타일 선택";
    public const string CancelTargeting = "선택 취소";
    public const string StaminaFull = "스태미나 가득 참";
    public const string NoItem = "아이템 없음";

    public static string BuyWithGold(int price)
    {
        return $"구매 {price}G";
    }

    public static string Price(int price)
    {
        return $"{price}G";
    }

    public static string OwnedCount(int count)
    {
        return $"보유 x{count}";
    }

    public static string Count(int count)
    {
        return $"x{count}";
    }

    public static string Cost(int cost)
    {
        return $"코스트 {cost}";
    }

    public static string CostProgress(int currentCost, int maxCost)
    {
        return $"{currentCost} / {maxCost}";
    }

    public static string Gold(int gold)
    {
        return gold.ToString();
    }
}
