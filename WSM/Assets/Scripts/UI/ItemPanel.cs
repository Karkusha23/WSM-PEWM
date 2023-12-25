using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanel : MonoBehaviour
{
    private Dictionary<Item.Items, Text> itemCountTexts;

    void Start()
    {
        itemCountTexts = new Dictionary<Item.Items, Text>();
        itemCountTexts.Add(Item.Items.Damage, transform.Find("ItemDamage").Find("DamageCount").GetComponent<Text>());
        itemCountTexts.Add(Item.Items.Tears, transform.Find("ItemTears").Find("TearsCount").GetComponent<Text>());
        itemCountTexts.Add(Item.Items.Speed, transform.Find("ItemSpeed").Find("SpeedCount").GetComponent<Text>());

        if (PlayerData.itemCounts != null)
        {
            itemCountTexts[Item.Items.Damage].text = "x" + PlayerData.itemCounts[Item.Items.Damage];
            itemCountTexts[Item.Items.Tears].text = "x" + PlayerData.itemCounts[Item.Items.Tears];
            itemCountTexts[Item.Items.Speed].text = "x" + PlayerData.itemCounts[Item.Items.Speed];
        }
        else
        {
            itemCountTexts[Item.Items.Damage].text = "x0";
            itemCountTexts[Item.Items.Tears].text = "x0";
            itemCountTexts[Item.Items.Speed].text = "x0";
        }
    }

    public void updateCountValue(Item.Items item, int newCount)
    {
        itemCountTexts[item].text = "x" + newCount;
    }
}
