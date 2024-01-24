using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class RoomLayout : ScriptableObject
{
    public bool isRoomBig;
    public List<LayoutField> layout;

    [System.Serializable]
    public enum SymmetryType : byte
    {
        None, // No symmetry
        XAxis, // Symmetry on X axis (rows are symmetric)
        YAxis, // Symmetry on Y axis (cols are symmetric)
        XYAxis, // Symmetry on X and Y (2x)
        Central // Central symmetry (4x)
    }

    [System.Serializable]
    public struct Point
    {
        public int row;
        public int col;

        public Point(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    [System.Serializable]
    public struct LayoutField
    {
        public GameObject obj;
        public SymmetryType symmetryType;
        public List<Point> points;
    }

    public struct ObjectPoint
    {
        public GameObject obj;
        public Point point;

        public ObjectPoint(GameObject obj, Point point)
        {
            this.obj = obj;
            this.point = point;
        }

        public override bool Equals(object obj)
        {
            if (obj is null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            ObjectPoint other = (ObjectPoint)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return (point.row << 4) | point.col;
        }

        public static bool operator==(ObjectPoint obj1, ObjectPoint obj2)
        {
            return obj1.point.row == obj2.point.row && obj1.point.col == obj2.point.col;
        }

        public static bool operator !=(ObjectPoint obj1, ObjectPoint obj2)
        {
            return !(obj1 == obj2);
        }
    }

    public List<ObjectPoint> getProps()
    {
        return getObjs(false);
    }

    public List<ObjectPoint> getEnemies()
    {
        return getObjs(true);
    }

    private List<ObjectPoint> getObjs(bool getEnemies)
    {
        var objs = new HashSet<ObjectPoint>();

        foreach (var field in layout)
        {
            if (getEnemies ^ field.obj.CompareTag("Enemy"))
            {
                continue;
            }
            foreach (var point in field.points)
            {
                addObjsSymmetry(objs, field.obj, point, field.symmetryType);
            }
        }

        return objs.ToList();
    }

    private void addObjsSymmetry(HashSet<ObjectPoint> objs, GameObject obj, Point point, SymmetryType symmetryType)
    {
        objs.Add(new ObjectPoint(obj, point));
        switch (symmetryType)
        {
            case SymmetryType.XAxis:
                addObjsXSymmetry(objs, obj, point);
                break;
            case SymmetryType.YAxis:
                addObjsYSymmetry(objs, obj, point);
                break;
            case SymmetryType.XYAxis:
                addObjsXYSymmetry(objs, obj, point);
                break;
            case SymmetryType.Central:
                if (point.row == getRoomHeight() / 2 || point.col == getRoomWidth() / 2)
                {
                    addObjsMiddleSymmetry(objs, obj, point);
                }
                else
                {
                    addObjsXSymmetry(objs, obj, point);
                    addObjsYSymmetry(objs, obj, point);
                    addObjsXYSymmetry(objs, obj, point);
                }
                break;
        }
    }

    private int getRoomHeight()
    {
        return (isRoomBig ? RoomPath.bigRoomTileHeightCount : RoomPath.roomTileHeightCount);
    }

    private int getRoomWidth()
    {
        return (isRoomBig ? RoomPath.bigRoomTileWidthCount : RoomPath.roomTileWidthCount);
    }

    private void addObjsXSymmetry(HashSet<ObjectPoint> objs, GameObject obj, Point point)
    {
        objs.Add(new ObjectPoint(obj, new Point(getRoomHeight() - 1 - point.row, point.col)));
    }

    private void addObjsYSymmetry(HashSet<ObjectPoint> objs, GameObject obj, Point point)
    {
        objs.Add(new ObjectPoint(obj, new Point(point.row, getRoomWidth() - 1 - point.col)));
    }

    private void addObjsXYSymmetry(HashSet<ObjectPoint> objs, GameObject obj, Point point)
    {
        objs.Add(new ObjectPoint(obj, new Point(getRoomHeight() - 1 - point.row, getRoomWidth() - 1 - point.col)));
    }

    private void addObjsMiddleSymmetry(HashSet<ObjectPoint> objs, GameObject obj, Point point)
    {
        int centerDistance = (point.row == getRoomHeight() / 2) ? Mathf.Abs(getRoomWidth() / 2 - point.col) : Mathf.Abs(getRoomHeight() / 2 - point.row);

        if (centerDistance == 0)
        {
            return;
        }

        objs.Add(new ObjectPoint(obj, new Point(getRoomHeight() / 2 + centerDistance, getRoomWidth() / 2)));
        objs.Add(new ObjectPoint(obj, new Point(getRoomHeight() / 2 - centerDistance, getRoomWidth() / 2)));
        objs.Add(new ObjectPoint(obj, new Point(getRoomHeight() / 2, getRoomWidth() / 2 + centerDistance)));
        objs.Add(new ObjectPoint(obj, new Point(getRoomHeight() / 2, getRoomWidth() / 2 - centerDistance)));
    }
}
