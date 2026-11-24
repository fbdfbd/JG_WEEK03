public static class LobbyUiText
{
    public static string ProfileRequired => GameLanguage.Text("프로필 필요");
    public static string Owned => GameLanguage.Text("보유");
    public static string Equipped => GameLanguage.Text("장착 중");
    public static string CanBuy => GameLanguage.Text("구매 가능");
    public static string NeedMoreGold => GameLanguage.Text("골드 부족");
    public static string Buy => GameLanguage.Text("구매");
    public static string Use => GameLanguage.Text("사용");
    public static string Equip => GameLanguage.Text("장착");
    public static string SelectEquipment => GameLanguage.Text("장비 선택");
    public static string SelectItem => GameLanguage.Text("아이템 선택");
    public static string NeedMoreCost => GameLanguage.Text("코스트 부족");
    public static string SelectTile => GameLanguage.Text("타일 선택");
    public static string CancelTargeting => GameLanguage.Text("선택 취소");
    public static string StaminaFull => GameLanguage.Text("스태미나 가득 참");
    public static string NoItem => GameLanguage.Text("아이템 없음");

    public static string BuyWithGold(int price)
    {
        return GameLanguage.Format("구매 {0}G", price);
    }

    public static string Price(int price)
    {
        return $"{price}G";
    }

    public static string OwnedCount(int count)
    {
        return GameLanguage.Format("보유 x{0}", count);
    }

    public static string Count(int count)
    {
        return $"x{count}";
    }

    public static string Cost(int cost)
    {
        return GameLanguage.Format("코스트 {0}", cost);
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
