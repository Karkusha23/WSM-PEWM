using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static class for procedure stage generation

public static class FloorGenerator
{
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
        Boss, // Boss Room
        PreBoss, // Pre-boss room
        Item // Item room
    }

    // Struct for storing points on floor grid
    public struct FloorPoint
    {
        public int i { get; set; }
        public int j { get; set; }

        public FloorPoint(int row, int col)
        {
            i = row;
            j = col;
        }
    };

    // Class for floor grid
    public class FloorGrid
    {
        private RoomType[,] grid;

        public RoomType this[int i, int j]
        {
            get => grid[i, j];
            set => grid[i, j] = value;
        }

        public RoomType this[FloorPoint point]
        {
            get => grid[point.i, point.j];
            set => grid[point.i, point.j] = value;
        }

        public int rows { get => grid.GetLength(0); }

        public int cols { get => grid.GetLength(1); }

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
    }

    // Generate floor with given parameters
    public static FloorGrid GenerateFloor(int floorHeight, int floorWidth, int roomCount, int itemRoomCount, float bigRoomProbability)
    {
        floorGrid = new FloorGrid(floorHeight, floorWidth);
        freeSmallRoom = new PointList();
        freeBigRoom = new PointList();
        return floorGrid;
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

    // Curren floorGrid
    private static FloorGrid floorGrid;

    // Lists that store free spots for rooms
    private static PointList freeSmallRoom;
    private static PointList freeBigRoom;

    private static bool BuildRandomRoom(float bigRoomProbability)
    {
        if (Random.value <= bigRoomProbability && freeBigRoom.Count > 0)
        {
            int index = Random.Range(0, freeBigRoom.Count);
            int row = freeBigRoom[index].i;
            int col = freeBigRoom[index].j;

            CheckBigFreeRoom(row, col);
            removeFreeBigAround(row, col);

            floorGrid[row, col] = RoomType.BigCore;
            floorGrid[row + 1, col] = floorGrid[row, col + 1] = floorGrid[row + 1, col + 1] = RoomType.BigSubunit;

            freeBigRoom.RemoveByPoint(row, col);
        }
        else
        {
            if (freeSmallRoom.Count == 0)
            {
                return false;
            }

            int index = Random.Range(0, freeSmallRoom.Count);
            int row = freeSmallRoom[index].i;
            int col = freeSmallRoom[index].j;

            CheckSmallFreeRoom(row, col);
            RemoveFreeSmallAround(row, col);

            floorGrid[row, col] = RoomType.Small;

            freeSmallRoom.RemoveByPoint(row, col);
        }
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

    private static void removeFreeBigAround(int row, int col)
    {
        if (floorGrid[row, col] == 3)
        {
            freeSmallRoom.RemoveByPoint(row, col);
            floorGrid[row, col] = 2;
        }
        for (int i = 1; i < 4; ++i)
        {
            int curRow = row + i / 2;
            int curCol = col + i % 2;
            switch (floorGrid[curRow, curCol])
            {
                case 1:
                    freeSmallRoom.RemoveByPoint(curRow, curCol);
                    break;
                case 2:
                    freeBigRoom.RemoveByPoint(curRow, curCol);
                    break;
                case 3:
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
                    case 2:
                        freeBigRoom.RemoveByPoint(curRow, col - 1);
                        floorGrid[curRow, col - 1] = 0;
                        break;
                    case 3:
                        freeBigRoom.RemoveByPoint(curRow, col - 1);
                        floorGrid[curRow, col - 1] = 1;
                        break;
                }
            }
            if (row > 0)
            {
                switch (floorGrid[row - 1, col - 1])
                {
                    case 2:
                        freeBigRoom.RemoveByPoint(row - 1, col - 1);
                        floorGrid[row - 1, col - 1] = 0;
                        break;
                    case 3:
                        freeBigRoom.RemoveByPoint(row - 1, col - 1);
                        floorGrid[row - 1, col - 1] = 1;
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
                    case 2:
                        freeBigRoom.RemoveByPoint(row - 1, curCol);
                        floorGrid[row - 1, curCol] = 0;
                        break;
                    case 3:
                        freeBigRoom.RemoveByPoint(row - 1, curCol);
                        floorGrid[row - 1, curCol] = 1;
                        break;
                }
            }
        }
    }

    private static void buildItemRooms()
    {
        PointList freeItemRoom = new PointList();
        foreach (FloorPoint point in freeSmallRoom)
        {
            if (!(isInMiddle(point.i, point.j) || getDoorCount(point.i, point.j) != 1))
            {
                freeItemRoom.Add(point);
            }
        }
        int itemRoomCount = Random.Range(minItemRoomCount, maxItemRoomCount + 1);
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
            floorGrid[row, col] = 9;
        }
    }

    private static void buildBossRoom()
    {
        byte[,] tmpMatrix = new byte[floorGrid.rows + 6, floorGrid.cols + 6];
        for (int i = 0; i < floorGrid.rows; ++i)
        {
            for (int j = 0; j < floorGrid.cols; ++j)
            {
                tmpMatrix[i + 3, j + 3] = floorGrid[i, j];
            }
        }
        floorGrid = tmpMatrix;
        floorGrid.rows += 6;
        floorGrid.cols += 6;
        int tmpType = Random.Range(0, 4);
        int[] freeRoom = new int[Mathf.Max(floorGrid.rows, floorGrid.cols)];
        int freeRoomCount = 0;
        if (tmpType == 0)
        {
            int row = 0, col = 0;
            bool indicator = false;
            for (; row < floorGrid.rows; ++row)
            {
                for (; col < floorGrid.cols; ++col)
                {
                    if (floorGrid[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                col = 0;
            }
            for (; col < floorGrid.cols; ++col)
            {
                if (floorGrid[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = col;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorGrid[row - 1, tmpPos] = 8;
            floorGrid[row - 3, tmpPos] = 7;
            floorGrid[row - 3, tmpPos + 1] = floorGrid[row - 2, tmpPos] = floorGrid[row - 2, tmpPos + 1] = 6;
        }
        else if (tmpType == 1)
        {
            int row = 0, col = 0;
            bool indicator = false;
            for (; col < floorGrid.cols; ++col)
            {
                for (; row < floorGrid.rows; ++row)
                {
                    if (floorGrid[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                row = 0;
            }
            for (; row < floorGrid.rows; ++row)
            {
                if (floorGrid[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = row;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorGrid[tmpPos, col - 1] = 8;
            floorGrid[tmpPos, col - 3] = 7;
            floorGrid[tmpPos, col - 2] = floorGrid[tmpPos + 1, col - 3] = floorGrid[tmpPos + 1, col - 2] = 6;
        }
        else if (tmpType == 2)
        {
            int row = 0, col = floorGrid.cols - 1;
            bool indicator = false;
            for (; col >= 0; --col)
            {
                for (; row < floorGrid.rows; ++row)
                {
                    if (floorGrid[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                row = 0;
            }
            for (; row < floorGrid.rows; ++row)
            {
                if (floorGrid[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = row;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorGrid[tmpPos, col + 1] = 8;
            floorGrid[tmpPos, col + 2] = 7;
            floorGrid[tmpPos, col + 3] = floorGrid[tmpPos + 1, col + 2] = floorGrid[tmpPos + 1, col + 3] = 6;
        }
        else
        {
            int row = floorGrid.rows - 1, col = 0;
            bool indicator = false;
            for (; row >= 0; --row)
            {
                for (; col < floorGrid.cols; ++col)
                {
                    if (floorGrid[row, col] >= 4)
                    {
                        indicator = true;
                        break;
                    }
                }
                if (indicator)
                {
                    break;
                }
                col = 0;
            }
            for (; col < floorGrid.cols; ++col)
            {
                if (floorGrid[row, col] >= 4)
                {
                    freeRoom[freeRoomCount++] = col;
                }
            }
            int tmpPos = freeRoom[Random.Range(0, freeRoomCount)];
            floorGrid[row + 1, tmpPos] = 8;
            floorGrid[row + 2, tmpPos] = 7;
            floorGrid[row + 2, tmpPos + 1] = floorGrid[row + 3, tmpPos] = floorGrid[row + 3, tmpPos + 1] = 6;
        }
    }
}
