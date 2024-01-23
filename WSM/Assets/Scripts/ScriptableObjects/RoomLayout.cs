using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RoomLayout : ScriptableObject
{
    [System.Serializable]
    public struct Point
    {
        public int row;
        public int col;
    }

    [System.Serializable]
    public struct LayoutField
    {
        public GameObject obj;
        public List<Point> points;
    }

    public List<LayoutField> layout;
}
