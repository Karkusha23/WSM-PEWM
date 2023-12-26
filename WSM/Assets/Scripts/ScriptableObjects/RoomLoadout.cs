using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomLoadout : ScriptableObject
{
    [System.Serializable]
    public struct LoadoutUnit
    {
        public GameObject prefab;
        public int row;
        public int col;
    }

    public List<LoadoutUnit> loadout;
}
