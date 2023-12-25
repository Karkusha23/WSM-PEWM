using System.Collections.Generic;
using UnityEngine;

public static class PlayerData
{
    public static int health = 3;
    public static GameObject weaponSample = null;

    public static float damage = 1.0f;
    public static float reloadTime = 0.5f;
    public static float moveSpeed = 10.0f;

    public static Dictionary<Item.Items, int> itemCounts = null;
}
