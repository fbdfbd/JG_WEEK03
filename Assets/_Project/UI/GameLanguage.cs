using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Two languages only. Serialized Korean text remains the fallback and source of truth.
public static class GameLanguage
{
    private const string PreferenceKey = "GameLanguage.English";
    private static bool? english;
    public static bool IsEnglish => english ?? (english = PlayerPrefs.GetInt(PreferenceKey, 1) == 1).Value;

    private static readonly Dictionary<string, string> Ui = new Dictionary<string, string>
    {
        { "시작", "Start" }, { "종료", "Exit" }, { "설정", "Settings" }, { "닫기", "Close" },
        { "가방", "Inventory" }, { "훈련장", "Training" }, { "상점", "Shop" },
        { "장비", "Equipment" }, { "소모품", "Items" }, { "버리기", "Discard" },
        { "프로필 필요", "Profile required" }, { "보유", "Owned" }, { "장착 중", "Equipped" },
        { "구매 가능", "Available" }, { "골드 부족", "Not enough gold" }, { "구매", "Buy" },
        { "사용", "Use" }, { "장착", "Equip" }, { "장비 선택", "Select equipment" },
        { "아이템 선택", "Select an item" }, { "코스트 부족", "Not enough capacity" },
        { "타일 선택", "Select a tile" }, { "선택 취소", "Cancel selection" },
        { "스태미나 가득 참", "Stamina is full" }, { "아이템 없음", "No items" },
        { "구매 {0}G", "Buy {0}G" }, { "보유 x{0}", "Owned x{0}" }, { "코스트 {0}", "Cost {0}" },
        { "색깔 장비는 1개만 장착 가능", "Only one color can be equipped" },
        { "다음 날", "Next day" }, { "다음날", "Next day" }, { "{0}일차", "Day {0}" },
        { "타일 {0}", "Tile {0}" }, { "플레이어 영토", "Your territory" },
        { "적 영토", "Enemy territory" }, { "중립 영토", "Neutral territory" },
        { "플레이어 점령지", "Your territory" }, { "사건 타일", "Event" }, { "보스 타일", "Boss" },
        { "적 점령 예정", "Enemy attack incoming" }, { "플레이어 시작지", "Your base" },
        { "적 시작지", "Enemy base" }, { "일반 상태", "Normal" },
        { "사건 진입", "Enter event" }, { "도전하기", "Challenge" },
        { "공격하기", "Attack" }, { "방어하기", "Defend" },
        { "인접한 중립 지역입니다. 도전에 성공하면 플레이어 영토가 됩니다.", "A neighboring neutral tile. Complete the challenge to claim it." },
        { "인접한 적 영토입니다. 전투에서 이기면 해당 지역을 점령합니다.", "A neighboring enemy tile. Win the battle to capture it." },
        { "적의 점령이 예고된 지역입니다. 이번 턴 안에 방어해야 합니다.", "An enemy attack is coming. Defend this tile before the day ends." },
        { "이미 확보한 지역입니다.", "You already control this tile." },
        { "현재는 진입 조건을 만족하지 않습니다.", "You cannot enter this tile yet." },
        { "승리", "Victory" }, { "패배", "Defeat" }, { "무승부", "Draw" },
        { "적 시작지를 점령했습니다.", "You captured the enemy base." },
        { "10일차 종료\n플레이어 {0}칸 / 적 {1}칸", "Day 10 complete\nYour tiles: {0} / Enemy tiles: {1}" },
        { "이동", "Move" }, { "직선 이동", "Straight movement" },
        { "특수 공격", "Special attack" }, { "일반 공격", "Basic attack" }, { "나가기", "Leave" },
        { "특수게이지 5 소모", "Costs 5 special gauge" },
        { "특수게이지 15 소모\n데미지 10의 공격", "Costs 15 special gauge\nDeals 10 damage" },
        { "데미지 1의 공격\n타격 성공 시 \n특수게이지 1 회복", "Deals 1 damage\nRestores 1 special gauge\non hit" }
    };

    private static readonly Dictionary<string, string[]> Items = new Dictionary<string, string[]>
    {
        { "equip_input", new[] { "Unusual movement", "Changes how directional inputs are interpreted." } },
        { "equip_color_red", new[] { "Red", "Changes your color to red." } },
        { "equip_color_yellow", new[] { "Yellow", "Changes your color to yellow." } },
        { "equip_color_green", new[] { "Green", "Changes your color to green." } },
        { "equip_color_blue", new[] { "Blue", "Changes your color to blue." } },
        { "equip_color_purple", new[] { "Purple", "Changes your color to purple." } },
        { "equip_color_black", new[] { "Black", "Changes your color to black." } },
        { "equip_bossAttack", new[] { "Wider hit zone", "Extends the boss hit zone by one face on each side. Hits in the added zones deal 50% damage." } },
        { "equip_attackdelay", new[] { "Faster attacks", "Reduces the delay between basic attacks." } },
        { "equip_specialAttackCost", new[] { "Efficient special", "Reduces special gauge consumption." } },
        { "equip_dash", new[] { "Sliding movement", "Hold a movement key to keep moving." } },
        { "equip_movementDelay", new[] { "Faster movement", "Reduces movement delay." } },
        { "equip_maxHpAdd", new[] { "More health", "Increases maximum health." } },
        { "euip_lifesteal", new[] { "Life steal", "Land 15 hits without taking damage, then recover 1 HP every 7 hits. Resets when you take damage." } },
        { "equip_holdfire", new[] { "Auto fire", "Hold Z to repeat basic attacks." } },
        { "stamina_recover", new[] { "Stamina potion", "Restores 1 stamina." } },
        { "emergency_defense", new[] { "Territory defense", "Prevents the enemy from capturing a threatened tile." } }
    };

    // title, situation, first label/description, second label/description
    private static readonly Dictionary<string, string[]> Events = new Dictionary<string, string[]>
    {
        { "event_rest_site", new[] { "Rest stop", "You found a place to rest.", "Gather supplies", "Gain a stamina potion.", "Scout the area", "Gain gold." } },
        { "event_getgold", new[] { "Gold", "You spot some gold.", "Pick it up", "Pick up a small pile of gold.", "Pick it up", "Pick up a large pile of gold." } },
        { "event_abandoned_cache", new[] { "Abandoned cache", "You found an abandoned supply crate by the road.", "Take it safely", "Gain gold and immediately claim this tile.", "Investigate", "Enter battle and earn double rewards." } }
    };

    public static string Text(string korean)
    {
        return IsEnglish && korean != null && Ui.TryGetValue(korean, out string value) ? value : korean;
    }

    public static string Format(string korean, params object[] args) => string.Format(Text(korean), args);

    public static string ItemText(string id, bool description, string fallback)
    {
        return IsEnglish && id != null && Items.TryGetValue(id, out string[] values)
            ? values[description ? 1 : 0] : fallback;
    }

    public static string EventText(string id, int field, string fallback)
    {
        return IsEnglish && id != null && Events.TryGetValue(id, out string[] values)
            && field >= 0 && field < values.Length ? values[field] : fallback;
    }

    public static void SetEnglish(bool value)
    {
        english = value;
        PlayerPrefs.SetInt(PreferenceKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        english = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsEnglish) return;
        // Runs once per scene, including inactive panels; never scans UI every frame.
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                string translated = Text(label.text);
                if (translated == label.text) continue;
                label.text = translated;
                Fit(label);
            }
        }
    }

    public static void Fit(TMP_Text label)
    {
        if (label.enableAutoSizing) return;
        label.fontSizeMax = label.fontSize;
        label.fontSizeMin = Mathf.Min(label.fontSize, Mathf.Max(14f, label.fontSize * 0.65f));
        label.enableAutoSizing = true;
    }
}
