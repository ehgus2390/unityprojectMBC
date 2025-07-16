using System;
using System.Collections.Generic;

// 이 클래스는 인벤토리에 저장될 아이템 하나의 데이터를 정의합니다.
// Firestore와 데이터를 주고받기 위해 직렬화(Serializable) 가능해야 합니다.
[Serializable]
public class Item
{
    public string id;       // 아이템 고유 ID (예: "sword_001", "potion_red")
    public string name;     // 아이템 이름 (예: "강철 검", "빨간 포션")
    public int count;       // 아이템 개수
    public string description; // 아이템 설명

    // Firestore는 객체를 직접 저장하지 못하므로, Dictionary 형태로 변환하는 함수가 필요합니다.
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "id", id },
            { "name", name },
            { "count", count },
            { "description", description }
        };
    }

    // Dictionary 형태에서 Item 객체로 변환하는 정적 함수입니다.
    public static Item FromDictionary(Dictionary<string, object> dict)
    {
        return new Item
        {
            id = dict["id"].ToString(),
            name = dict["name"].ToString(),
            // Firestore에서 숫자는 Int64(long)으로 취급될 수 있으므로 변환해줍니다.
            count = Convert.ToInt32(dict["count"]),
            description = dict["description"].ToString()
        };
    }
}
