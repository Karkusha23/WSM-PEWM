using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanel : MonoBehaviour
{
    private Dictionary<ItemController.Item, Text> itemCountTexts;

    void Start()
    {
        itemCountTexts = new Dictionary<ItemController.Item, Text>();
        itemCountTexts.Add(ItemController.Item.Damage, transform.Find("ItemDamage").Find("DamageCount").GetComponent<Text>());
        itemCountTexts.Add(ItemController.Item.Tears, transform.Find("ItemTears").Find("TearsCount").GetComponent<Text>());
        itemCountTexts.Add(ItemController.Item.Speed, transform.Find("ItemSpeed").Find("SpeedCount").GetComponent<Text>());

        itemCountTexts[ItemController.Item.Damage].text = "x" + PlayerData.damageItemCount;
        itemCountTexts[ItemController.Item.Tears].text = "x" + PlayerData.tearsItemCount;
        itemCountTexts[ItemController.Item.Speed].text = "x" + PlayerData.speedItemCount;
    }

    public void updateCountValue(ItemController.Item item, int newCount)
    {
        itemCountTexts[item].text = "x" + newCount;
    }
}
