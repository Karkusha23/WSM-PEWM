using System.Collections.Generic;
using UnityEngine;

// Static class for procedure stage generation

public static class FloorGenerator
{
    // Probability of rerolling room while generating if room has more than 1 door
    public const float smallRoomRerollProbability = 0.7f;

    // Room types for floor grid
    public enum RoomType : byte
    {
        None, // No room, can not build there
        FreeSmall, // No room, free only for small room
        FreeBig, // No room, free only for big room
        FreeSmallBig, // No room, free for small and big room
        Small, // Small room built
        BigCore, // Big room core cell (top left)
        BigSubunit, // Other big room cells
        PreBoss, // Pre-boss room
        Boss, // Boss Room
        Item // Item room
    }

    // Struct for storing points on floor grid
    public class FloorPoint : Point 
    {
        public FloorPoint(int row, int col) : base(row, col) { }
    };

    // Class for floor grid
    public class FloorGrid : Grid<RoomType>
    {
        public RoomType center
        {
            get => grid[rows / 2, cols / 2];
            set => grid[rows / 2, cols / 2] = value;
        }

        public FloorGrid(int floorHeight, int floorWidth)
        {
            grid = new RoomType[floorHeight, floorWidth];

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    grid[i, j] = RoomType.None;
                }
            }
        }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    result += (byte)grid[i, j];
                }
                result += '\n';
            }
            return result;
        }

        // Extends floor matrix by cellCount cells in all 4 directions
        public void extend(int cellCount)
        {
            RoomType[,] tmp = new RoomType[rows + cellCount * 2, cols + cellCount * 2];

            for (int i = 0; i < rows + cellCount * 2; ++i)
            {
                for (int j = 0; j < cols + cellCount * 2; ++j)
                {
                    tmp[i, j] = RoomType.None;
                }
            }

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    tmp[i + cellCount, j + cellCount] = grid[i, j];
                }
            }

            grid = tmp;
        }

        public bool isRoomOccupied(int row, int col)
        {
            return hasPoint(row, col) && IsRoomOccupied(grid[row, col]);
        }

        public bool isRoomSmall(int row, int col)
        {
            return hasPoint(row, col) && IsRoomSmall(grid[row, col]);
        }

        public bool isRoomBig(int row, int col)
        {
            return hasPoint(row, col) && IsRoomBig(grid[row, col]);
        }
    }

    // Generate floor with given parameters
    public static FloorGrid GenerateFloor(int floorHeight, int floorWidth, int roomCount, int itemRoomCount, float bigRoomProbability)
    {
        floorGrid = new FloorGrid(floorHeight, floorWidth);
        freeSmallRoom = new PointList();
        freeBigRoom = new PointList();

        floorGrid.center = RoomType.Small;
        CheckSmallFreeRoom(floorGrid.rows / 2, floorGrid.cols / 2);

        for (int i = 1; i < roomCount; ++i)
        {
            if (!BuildRandomRoom(bigRoomProbability))
            {
                break;
            }
        }

        BuildItemRooms(itemRoomCount);

        BuildBossRoom();

        FloorGrid result = floorGrid;

        floorGrid = null;
        freeSmallRoom = null;
        freeBigRoom = null;

        return result;
    }

    // If given room is already occupied
    public static bool IsRoomOccupied(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.None:
            case RoomType.FreeSmall:
            case RoomType.FreeBig:
            case RoomType.FreeSmallBig:
                return false;
            default:
                break;
        }

        return true;
    }

    // If room is small room
    public static bool IsRoomSmall(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.Small:
            case RoomType.PreBoss:
            case RoomType.Item:
                return true;
            default:
                break;
        }

        return false;
    }

    public static bool IsRoomBig(RoomType roomType)
    {
        switch (roomType)
        {
            case RoomType.BigCore:
            case RoomType.Boss:
                return true;
            default:
                break;
        }

        return false;
    }

    // Curren floorGrid
    private static FloorGrid floorGrid;

    // Lists that store free spots for rooms
    private static PointList freeSmallRoom;
    private static PointList freeBigRoom;

    private class PointList : List<FloorPoint>
    {
        // Remove point by coordinates
        public bool RemoveByPoint(int row, int col)
        {
            int index = IndexOf(new FloorPoint(row, col));
            if (index < 0)
            {
                return false;
            }
            RemoveAt(index);
            return true;
        }
    };

    private enum FloorPart : byte { North, South, West, East }

    private struct BossPoint
    {
        public FloorPoint point;
        public FloorPart floorPart;

        public BossPoint(FloorPoint point, FloorPart floorPart)
        {
            this.point = point;
            this.floorPart = floorPart;
        }
    }

    private static bool BuildRandomRoom(float bigRoomProbability)
    {
        if (Random.value <= bigRoomProbability && freeBigRoom.Count > 0)
        {
            return BuildBigRoom();
        }

        return BuildSmallRoom();
    }

    private static bool BuildSmallRoom()
    {
        if (freeSmallRoom.Count == 0)
        {
            return false;
        }

        int index = Random.Range(0, freeSmallRoom.Count);

        if (GetSmallDoorCount(freeSmallRoom[index]) > 1 && freeSmallRoom.Exists(x => GetSmallDoorCount(x) == 1))
        {
            int iterations = 10;
            while (GetSmallDoorCount(freeSmallRoom[index]) > 1)
            {
                index = Random.Range(0, freeSmallRoom.Count);
                --iterations;
                if (iterations < 0 || Random.value > smallRoomRerollProbability)
                {
                    break;
                }
            }
        }

        int row = freeSmallRoom[index].i;
        int col = freeSmallRoom[index].j;

        CheckSmallFreeRoom(row, col);
        RemoveFreeSmallAround(row, col);

        floorGrid[row, col] = RoomType.Small;

        freeSmallRoom.RemoveByPoint(row, col);

        return true;
    }

    private static bool BuildBigRoom()
    {
        int index = Random.Range(0, freeBigRoom.Count);
        int row = freeBigRoom[index].i;
        int col = freeBigRoom[index].j;

        CheckBigFreeRoom(row, col);
        RemoveFreeBigAround(row, col);

        floorGrid[row, col] = RoomType.BigCore;
        floorGrid[row + 1, col] = floorGrid[row, col + 1] = floorGrid[row + 1, col + 1] = RoomType.BigSubunit;

        freeBigRoom.RemoveByPoint(row, col);

        return true;
    }

    private static void CheckSmallFreeRoom(int row, int col)
    {
        if (row > 0)
        {
            CheckCell(row - 1, col);
        }
        if (col > 0)
        {
            CheckCell(row, col - 1);
        }
        if (row < floorGrid.rows - 1)
        {
            CheckCell(row + 1, col);
        }
        if (col < floorGrid.cols - 1)
        {
            CheckCell(row, col + 1);
        }
        if (row > 1)
        {
            if (col > 0)
            {
                CheckBigCell(row - 2, col - 1);
            }
            if (col < floorGrid.cols - 1)
            {
                CheckBigCell(row - 2, col);
            }
        }
        if (col > 1)
        {
            if (row > 0)
            {
                CheckBigCell(row - 1, col - 2);
            }
            if (row < floorGrid.rows - 1)
            {
                CheckBigCell(row, col - 2);
            }
        }
        if (row < floorGrid.rows - 2)
        {
            if (col > 0)
            {
                CheckBigCell(row + 1, col - 1);
            }
            if (col < floorGrid.cols - 1)
            {
                CheckBigCell(row + 1, col);
            }
        }
        if (col < floorGrid.cols - 2)
        {
            if (row > 0)
            {
                CheckBigCell(row - 1, col + 1);
            }
            if (row < floorGrid.rows - 1)
            {
                CheckBigCell(row, col + 1);
            }
        }
    }

    private static void CheckBigFreeRoom(int row, int col)
    {
        if (row > 0)
        {
            CheckCell(row - 1, col);
            CheckCell(row - 1, col + 1);
        }
        if (col > 0)
        {
            CheckCell(row, col - 1);
            CheckCell(row + 1, col - 1);
        }
        if (row < floorGrid.rows - 2)
        {
            CheckCell(row + 2, col);
            CheckCell(row + 2, col + 1);
        }
        if (col < floorGrid.cols - 2)
        {
            CheckCell(row, col + 2);
            CheckCell(row + 1, col + 2);
        }
        if (row > 1)
        {
            CheckBigCell(row - 2, col);
            if (col > 0)
            {
                CheckBigCell(row - 2, col - 1);
            }
            if (col < floorGrid.cols - 1)
            {
                CheckBigCell(row - 2, col + 1);
            }
        }
        if (col > 1)
        {
            CheckBigCell(row, col - 2);
            if (row > 0)
            {
                CheckBigCell(row - 1, col - 2);
            }
            if (row < floorGrid.rows - 1)
            {
                CheckBigCell(row + 1, col - 2);
            }
        }
        if (row < floorGrid.rows - 2)
        {
            CheckBigCell(row + 2, col);
            if (col > 0)
            {
                CheckBigCell(row + 2, col - 1);
            }
            if (col < floorGrid.cols - 1)
            {
                CheckBigCell(row + 2, col + 1);
            }
        }
        if (col < floorGrid.cols - 2)
        {
            CheckBigCell(row, col + 2);
            if (row > 0)
            {
                CheckBigCell(row - 1, col + 2);
            }
            if (row < floorGrid.rows - 1)
            {
                CheckBigCell(row + 1, col + 2);
            }
        }
    }

    private static void CheckCell(int row, int col)
    {
        if (floorGrid[row, col] == RoomType.None || floorGrid[row, col] == RoomType.FreeBig)
        {
            floorGrid[row, col] = floorGrid[row, col] == RoomType.None ? RoomType.FreeSmall : RoomType.FreeSmallBig;
            freeSmallRoom.Add(new FloorPoint(row, col));
        }
    }

    private static void CheckBigCell(int row, int col)
    {
        if ((floorGrid[row, col] == RoomType.None || floorGrid[row, col] == RoomType.FreeSmall) && row < floorGrid.rows - 2 && col < floorGrid.cols - 2)
        {
            bool indicator = true;
            for (int i = 1; i < 4; ++i)
            {
                if (IsRoomOccupied(floorGrid[row + i / 2, col + i % 2]))
                {
                    indicator = false;
                    break;
                }
            }
            if (indicator)
            {
                floorGrid[row, col] = floorGrid[row, col] == RoomType.None ? RoomType.FreeBig : RoomType.FreeSmallBig;
                freeBigRoom.Add(new FloorPoint(row, col));
            }
        }
    }

    private static void RemoveFreeSmallAround(int row, int col)
    {
        RemoveFreeBigRoom(row, col);

        if (row > 0 && col > 0)
        {
            RemoveFreeBigRoom(row - 1, col - 1);
        }
        if (row > 0)
        {
            RemoveFreeBigRoom(row - 1, col);
        }
        if (col > 0)
        {
            RemoveFreeBigRoom(row, col - 1);
        }
    }

    private static void RemoveFreeBigRoom(int row, int col)
    {
        if (floorGrid[row, col] == RoomType.FreeBig || floorGrid[row, col] == RoomType.FreeSmallBig)
        {
            freeBigRoom.RemoveByPoint(row, col);
            floorGrid[row, col] = floorGrid[row, col] == RoomType.FreeBig ? RoomType.None : RoomType.FreeSmall;
        }
    }

    private static void RemoveFreeBigAround(int row, int col)
    {
        if (floorGrid[row, col] == RoomType.FreeSmallBig)
        {
            freeSmallRoom.RemoveByPoint(row, col);
            floorGrid[row, col] = RoomType.FreeBig;
        }
        for (int i = 1; i < 4; ++i)
        {
            int curRow = row + i / 2;
            int curCol = col + i % 2;
            switch (floorGrid[curRow, curCol])
            {
                case RoomType.FreeSmall:
                    freeSmallRoom.RemoveByPoint(curRow, curCol);
                    break;
                case RoomType.FreeBig:
                    freeBigRoom.RemoveByPoint(curRow, curCol);
                    break;
                case RoomType.FreeSmallBig:
                    freeSmallRoom.RemoveByPoint(curRow, curCol);
                    freeBigRoom.RemoveByPoint(curRow, curCol);
                    break;
            }
        }
        if (col > 0)
        {
            for (int curRow = row; curRow <= row + 1; ++curRow)
            {
                switch (floorGrid[curRow, col - 1])
                {
                    case RoomType.FreeBig:
                        freeBigRoom.RemoveByPoint(curRow, col - 1);
                        floorGrid[curRow, col - 1] = RoomType.None;
                        break;
                    case RoomType.FreeSmallBig:
                        freeBigRoom.RemoveByPoint(curRow, col - 1);
                        floorGrid[curRow, col - 1] = RoomType.FreeSmall;
                        break;
                }
            }
            if (row > 0)
            {
                switch (floorGrid[row - 1, col - 1])
                {
                    case RoomType.FreeBig:
                        freeBigRoom.RemoveByPoint(row - 1, col - 1);
                        floorGrid[row - 1, col - 1] = RoomType.None;
                        break;
                    case RoomType.FreeSmallBig:
                        freeBigRoom.RemoveByPoint(row - 1, col - 1);
                        floorGrid[row - 1, col - 1] = RoomType.FreeSmall;
                        break;
                }
            }
        }
        if (row > 0)
        {
            for (int curCol = col; curCol <= col + 1; ++curCol)
            {
                switch (floorGrid[row - 1, curCol])
                {
                    case RoomType.FreeBig:
                        freeBigRoom.RemoveByPoint(row - 1, curCol);
                        floorGrid[row - 1, curCol] = RoomType.None;
                        break;
                    case RoomType.FreeSmallBig:
                        freeBigRoom.RemoveByPoint(row - 1, curCol);
                        floorGrid[row - 1, curCol] = RoomType.FreeSmall;
                        break;
                }
            }
        }
    }

    private static void BuildItemRooms(int itemRoomCount)
    {
        PointList freeItemRoom = new PointList();

        foreach (FloorPoint point in freeSmallRoom)
        {
            if (!(IsInCenter(point) || GetSmallDoorCount(point) != 1))
            {
                freeItemRoom.Add(point);
            }
        }

        for (int i = 0; i < itemRoomCount; ++i)
        {
            if (freeItemRoom.Count == 0)
            {
                return;
            }
            int index = Random.Range(0, freeItemRoom.Count);
            int row = freeItemRoom[index].i;
            int col = freeItemRoom[index].j;
            for (int curRow = row - 1; curRow <= row + 1; ++curRow)
            {
                for (int curCol = col - 1; curCol <= col + 1; ++curCol)
                {
                    freeItemRoom.RemoveByPoint(curRow, curCol);
                }
            }
            floorGrid[row, col] = RoomType.Item;
        }
    }

    private static void BuildBossRoom()
    {
        floorGrid.extend(3);

        var freeBossRoom = GetFreeBossRoom();

        if (freeBossRoom.Count == 0)
        {
            return;
        }

        var freeBossRoomNoItem = freeBossRoom.FindAll(x => floorGrid[x.point] != RoomType.Item);

        if (freeBossRoomNoItem.Count > 0)
        {
            BuildBossRoomByPoint(freeBossRoomNoItem[Random.Range(0, freeBossRoomNoItem.Count)]);
        }
        else
        {
            BuildBossRoomByPoint(freeBossRoom[Random.Range(0, freeBossRoom.Count)]);
        }
    }

    private static List<BossPoint> GetFreeBossRoom()
    {
        var freeBossRoom = new List<BossPoint>();

        for (int i = 3; i < floorGrid.rows; ++i)
        {
            bool hasRooms = false;

            for (int j = 0; j < floorGrid.cols; ++j)
            {
                if (floorGrid.isRoomOccupied(i, j))
                {
                    hasRooms = true;
                    freeBossRoom.Add(new BossPoint(new FloorPoint(i, j), FloorPart.North));
                }
            }

            if (hasRooms)
            {
                break;
            }
        }

        for (int i = floorGrid.rows - 4; i >= 0; --i)
        {
            bool hasRooms = false;

            for (int j = 0; j < floorGrid.cols; ++j)
            {
                if (floorGrid.isRoomOccupied(i, j))
                {
                    hasRooms = true;
                    freeBossRoom.Add(new BossPoint(new FloorPoint(i, j), FloorPart.South));
                }
            }

            if (hasRooms)
            {
                break;
            }
        }

        for (int j = 3; j < floorGrid.cols; ++j)
        {
            bool hasRooms = false;

            for (int i = 0; i < floorGrid.rows; ++i)
            {
                if (floorGrid.isRoomOccupied(i, j))
                {
                    hasRooms = true;
                    freeBossRoom.Add(new BossPoint(new FloorPoint(i, j), FloorPart.West));
                }
            }

            if (hasRooms)
            {
                break;
            }
        }

        for (int j = floorGrid.cols - 4; j >= 0; --j)
        {
            bool hasRooms = false;

            for (int i = 0; i < floorGrid.rows; ++i)
            {
                if (floorGrid.isRoomOccupied(i, j))
                {
                    hasRooms = true;
                    freeBossRoom.Add(new BossPoint(new FloorPoint(i, j), FloorPart.East));
                }
            }

            if (hasRooms)
            {
                break;
            }
        }

        return freeBossRoom;
    }

    private static void BuildBossRoomByPoint(BossPoint point)
    {
        switch (point.floorPart)
        {
            case FloorPart.North:
                floorGrid[point.point.i - 1, point.point.j] = RoomType.PreBoss;
                floorGrid[point.point.i - 3, point.point.j] = RoomType.Boss;
                floorGrid[point.point.i - 3, point.point.j + 1] = floorGrid[point.point.i - 2, point.point.j] = floorGrid[point.point.i - 2, point.point.j + 1] = RoomType.BigSubunit;
                break;
            case FloorPart.South:
                floorGrid[point.point.i + 1, point.point.j] = RoomType.PreBoss;
                floorGrid[point.point.i + 2, point.point.j] = RoomType.Boss;
                floorGrid[point.point.i + 2, point.point.j + 1] = floorGrid[point.point.i + 3, point.point.j] = floorGrid[point.point.i + 3, point.point.j + 1] = RoomType.BigSubunit;
                break;
            case FloorPart.West:
                floorGrid[point.point.i, point.point.j - 1] = RoomType.PreBoss;
                floorGrid[point.point.i - 1, point.point.j - 3] = RoomType.Boss;
                floorGrid[point.point.i - 1, point.point.j - 2] = floorGrid[point.point.i, point.point.j - 3] = floorGrid[point.point.i, point.point.j - 2] = RoomType.BigSubunit;
                break;
            case FloorPart.East:
                floorGrid[point.point.i, point.point.j + 1] = RoomType.PreBoss;
                floorGrid[point.point.i - 1, point.point.j + 2] = RoomType.Boss;
                floorGrid[point.point.i - 1, point.point.j + 3] = floorGrid[point.point.i, point.point.j + 2] = floorGrid[point.point.i, point.point.j + 3] = RoomType.BigSubunit;
                break;
        }
    }

    private static bool IsInCenter(FloorPoint point)
    {
        return point.i >= floorGrid.rows / 2 - 1 && point.i <= floorGrid.rows / 2 + 1 && point.j >= floorGrid.cols / 2 - 1 && point.j <= floorGrid.cols / 2 + 1;
    }

    private static int GetSmallDoorCount(FloorPoint point)
    {
        int result = 0;
        if (point.i < floorGrid.rows - 1 && IsRoomOccupied(floorGrid[point.i + 1, point.j]))
        {
            ++result;
        }
        if (point.j < floorGrid.cols - 1 && IsRoomOccupied(floorGrid[point.i, point.j + 1]))
        {
            ++result;
        }
        if (point.i > 0 && IsRoomOccupied(floorGrid[point.i - 1, point.j]))
        {
            ++result;
        }
        if (point.j > 0 && IsRoomOccupied(floorGrid[point.i, point.j - 1]))
        {
            ++result;
        }
        return result;
    }
}
