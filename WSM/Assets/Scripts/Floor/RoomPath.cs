using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using static RoomPath;

// Static class that implements pathfinding algorithms and structs that are being used in them

public static class RoomPath
{
    // Room size consts
    public const int roomTileWidthCount = 15;
    public const int roomTileHeightCount = 9;
    public const float tileSize = 1.2f;

    // Size of actor box collider divided by tile size
    public const float actorSize = 0.8f;

    // Maximum actor offset relative to room grid with which it still can fit through narrow paths
    public const float maxAllowedActorGridOffset = tileSize * (1.0f - actorSize) / 2.0f;

    // Deafult travel cost for normal tile
    public const byte defaultTravelCost = 1;

    // Room type
    public enum RoomType { None, SmallRoom, BigRoom }

    // Stores coordinates in room grid
    public class RoomPoint
    {
        public int i { get; set; }
        public int j { get; set; }

        public RoomPoint(int row, int col)
        {
            i = row;
            j = col;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            RoomPoint other = (RoomPoint)obj;
            return this.i == other.i && this.j == other.j;
        }

        public override int GetHashCode()
        {
            return (i << 8) | j;
        }

        public override string ToString()
        {
            return string.Format("RoomPoint({0}, {1})", i, j);
        }

        public static bool operator ==(RoomPoint point1, RoomPoint point2)
        {
            return point1.i == point2.i && point1.j == point2.j;
        }

        public static bool operator !=(RoomPoint point1, RoomPoint point2)
        {
            return !(point1 == point2);
        }

        public static float Distance(RoomPoint point1, RoomPoint point2)
        {
            return Mathf.Sqrt((point1.i - point2.i) * (point1.i - point2.i) + (point1.j - point2.j) * (point1.j - point2.j));
        }
    }

    // Return local coordinates of point on room grid
    public static Vector3 RoomPointToLocal(RoomPath.RoomPoint point)
    {
        return tileSize * new Vector3((point.j - roomTileWidthCount / 2), (roomTileHeightCount / 2 - point.i), 0.0f);
    }

    public static Vector3 RoomPointToLocal(int row, int col)
    {
        return tileSize * new Vector3((col - roomTileWidthCount / 2), (roomTileHeightCount / 2 - row), 0.0f);
    }

    // Returns room grid point from room local coordinates
    public static RoomPath.RoomPoint LocalToRoomPoint(Vector3 pos)
    {
        return new RoomPath.RoomPoint(roomTileHeightCount / 2 - Mathf.RoundToInt(pos.y / tileSize), roomTileWidthCount / 2 + Mathf.RoundToInt(pos.x / tileSize));
    }

    // Returns fractional room grid point from room local coordinates. x is row, y is col
    public static Vector2 LocalToFractionalRoomPoint(Vector3 pos)
    {
        return new Vector2(roomTileHeightCount / 2 - pos.y / tileSize, roomTileWidthCount / 2 + pos.x / tileSize);
    }

    // Class for storing room grid. 0 if tile is not travable, otherwise value is traveling cost for tile
    public class RoomGrid
    {
        private byte[,] grid;

        public byte this[int i, int j]
        {
            get => grid[i, j];
            set => grid[i, j] = value;
        }

        public byte this[RoomPoint point]
        {
            get => grid[point.i, point.j];
            set => grid[point.i, point.j] = value;
        }

        public int rows { get => grid.GetLength(0); }

        public int cols { get => grid.GetLength(1); }

        public RoomGrid(RoomType roomType)
        {
            if (roomType == RoomType.SmallRoom)
            {
                grid = new byte[roomTileHeightCount, roomTileWidthCount];
            }
            else if (roomType == RoomType.BigRoom)
            {
                grid = new byte[roomTileHeightCount * 2, roomTileWidthCount * 2];
            }
            else
            {
                return;
            }

            for (int i = 0; i < this.rows; ++i)
            {
                for (int j = 0; j < this.cols; ++j)
                {
                    grid[i, j] = defaultTravelCost;
                }
            }
        }

        // If point is inside of grid bounds
        public bool hasPoint(RoomPoint point)
        {
            return point.i >= 0 && point.i < this.rows && point.j >= 0 && point.j < this.cols;
        }

        public bool hasPoint(int i, int j)
        {
            return i >= 0 && i < this.rows && j >= 0 && j < this.cols;
        }
    }

    // Used to store waypoints of path
    public class Path : List<RoomPoint>
    {
        public float GetPathLength()
        {
            float result = 0;
            for (int i = 0; i < this.Count - 1; ++i)
            {
                result += RoomPoint.Distance(this[i], this[i + 1]);
            }
            return result;
        }

        public void JoinPath(Path other)
        {
            if (other.Count == 0)
            {
                return;
            }
            if (this.Count > 0 && this[this.Count - 1] == other[0])
            {
                this.RemoveAt(this.Count - 1);
            }
            if (other.Count > 1 && other[1] == other[0])
            {
                other.RemoveAt(0);
            }
            this.AddRange(other);
            if (this.Count == 1)
            {
                this.Add(this[0]);
            }
        }

        // Smoothe path by deleting redundant waypoints
        public void Smoothe(RoomGrid roomGrid)
        {
            for (int i = 0; i < this.Count - 2; ++i)
            {
                while (i < this.Count - 2 && Raycast(this[i], this[i + 2], roomGrid, true))
                {
                    this.RemoveAt(i + 1);
                }
            }
        }
    }

    // Build shortest path from start to end using A* algorithm
    public static Path BuildPath(RoomPoint start, RoomPoint end, RoomGrid roomGrid)
    {
        if (start == end)
        {
            Path path = new Path();
            path.Add(start);
            path.Add(end);
            return path;
        }

        PathNode startNode = new PathNode(start);
        PathNode endNode = new PathNode(end);

        var openList = new SortedSet<PathNode>(new PathNode.PathNodeComparer());
        openList.Add(startNode);

        var closedSet = new HashSet<PathNode>();

        while (openList.Count > 0)
        {

            var curNode = openList.Min;
            openList.Remove(curNode);

            if (curNode == endNode)
            {
                return makePathFromNode(curNode, roomGrid);
            }

            closedSet.Add(curNode);

            int newFromStartToThis = curNode.fromStartToThis + roomGrid[curNode.point];

            var neighbors = findNeighbors(curNode, roomGrid);

            foreach (var neighbor in neighbors)
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                if (openList.TryGetValue(neighbor, out var openListNeighbor))
                {
                    if (newFromStartToThis < openListNeighbor.fromStartToThis)
                    {
                        openList.Remove(openListNeighbor);
                        openListNeighbor.fromStartToThis = newFromStartToThis;
                        openListNeighbor.fromThisToEnd = Mathf.RoundToInt(RoomPoint.Distance(openListNeighbor.point, end));
                        openListNeighbor.fullPath = newFromStartToThis + openListNeighbor.fromThisToEnd;
                        openListNeighbor.parent = curNode;
                        openList.Add(openListNeighbor);
                    }
                }
                else
                {
                    neighbor.fromStartToThis = newFromStartToThis;
                    neighbor.fromThisToEnd = Mathf.RoundToInt(RoomPoint.Distance(neighbor.point, end));
                    neighbor.fullPath = newFromStartToThis + neighbor.fromThisToEnd;
                    neighbor.parent = curNode;
                    openList.Add(neighbor);
                }
            }
        }

        return new Path();
    }

    // Build path using local coordinates of start and end points
    public static Path BuildPath(Vector3 start, Vector3 end, RoomGrid roomGrid)
    {
        return BuildPath(LocalToRoomPoint(start), LocalToRoomPoint(end), roomGrid);
    }

    // Returns true if there's no obstacles between start and end
    // If considerTravelCost is true, points with travel cost greater than maximum of start and end points travel cost will be counted as untravable
    public static bool Raycast(RoomPoint start, RoomPoint end, RoomGrid roomGrid, bool considerTravelCost)
    {
        if (start == end)
        {
            return true;
        }

        int maxTravelCost = considerTravelCost ? Mathf.Max(roomGrid[start], roomGrid[end]) : 0;

        if (start.i == end.i)
        {
            if (start.j > end.j)
            {
                RoomPoint tmp = start;
                start = end;
                end = tmp;
            }
            for (int j = start.j; j <= end.j; ++j)
            {
                if (!IsPointTravable(start.i, j, roomGrid, maxTravelCost))
                {
                    return false;
                }
            }
            return true;
        }

        if (start.j == end.j)
        {
            if (start.i > end.i)
            {
                RoomPoint tmp = start;
                start = end;
                end = tmp;
            }
            for (int i = start.i; i <= end.i; ++i)
            {
                if (!IsPointTravable(i, start.j, roomGrid, maxTravelCost))
                {
                    return false;
                }
            }
            return true;
        }

        if (Mathf.Abs(end.i - start.i) == Mathf.Abs(end.j - start.j))
        {
            int rowDir = start.i < end.i ? 1 : -1;
            int colDir = start.j < end.j ? 1 : -1;
            int row = start.i;
            int col = start.j;
            while (row != end.i)
            {
                if (!IsPointTravable(row, col, roomGrid, maxTravelCost) || !IsPointTravable(row + rowDir, col, roomGrid, maxTravelCost) ||
                    !IsPointTravable(row, col + colDir, roomGrid, maxTravelCost))
                {
                    return false;
                }
                row += rowDir;
                col += colDir;
            }
            return IsPointTravable(end.i, end.j, roomGrid, maxTravelCost);
        }

        if (Mathf.Abs(end.i - start.i) < Mathf.Abs(end.j - start.j))
        {
            if (start.j > end.j)
            {
                RoomPoint tmp = start;
                start = end;
                end = tmp;
            }
            float halfRowDirection = (float)(end.i - start.i) / ((float)(end.j - start.j) * 2.0f);
            float row = (float)start.i;
            for (float j = start.j; j < end.j; j += 0.5f)
            {
                if (!IsNeighborhoodTravable(row, j, roomGrid, maxTravelCost))
                {
                    return false;
                }
                row += halfRowDirection;
            }
            if (!IsNeighborhoodTravable(row, end.j, roomGrid, maxTravelCost))
            {
                return false;
            }
            return true;
        }

        if (start.i > end.i)
        {
            RoomPoint tmp = start;
            start = end;
            end = tmp;
        }
        float halfColDirection = (float)(end.j - start.j) / ((float)(end.i - start.i) * 2.0f);
        float column = (float)start.j;
        for (float i = start.i; i < end.i; i += 0.5f)
        {
            int colInt = Mathf.RoundToInt(column);
            if (!IsNeighborhoodTravable(i, column, roomGrid, maxTravelCost))
            {
                return false;
            }
            column += halfColDirection;
        }
        if (!IsNeighborhoodTravable(end.i, column, roomGrid, maxTravelCost))
        {
            return false;
        }
        return true;
    }

    // Returns true, if there's no obstacles between start and end points. This overload of raycast takes into account fractional parts of local coordinates start and end points 
    // If considerTravelCost is true, points with travel cost greater than maximum of start and end points travel cost will be counted as untravable
    public static bool Raycast(Vector3 start, Vector3 end, RoomGrid roomGrid, bool considerTravelCost)
    {
        float distance = Vector3.Distance(start, end);

        if (distance <= tileSize)
        {
            return true;
        }

        RoomPoint startPoint = LocalToRoomPoint(start);
        RoomPoint endPoint = LocalToRoomPoint(end);

        if (!roomGrid.hasPoint(startPoint) || !roomGrid.hasPoint(endPoint))
        {
            return false;
        }

        int maxTravelCost = considerTravelCost ? Mathf.Max(roomGrid[startPoint], roomGrid[endPoint]) : 0;

        int checksCount = Mathf.Max(3, Mathf.CeilToInt(distance * 2.0f / tileSize));

        Vector2 startVec = LocalToFractionalRoomPoint(start);
        Vector2 endVec = LocalToFractionalRoomPoint(end);

        HashSet<RoomPoint> pointsToCheck = new HashSet<RoomPoint>();

        float row = startVec.x;
        float col = startVec.y;

        float rowDir = (endVec.x - startVec.x) / (float)checksCount;
        float colDir = (endVec.y - startVec.y) / (float)checksCount;

        for (int i = 0; i < checksCount; ++i)
        {
            int row1, row2 = -1;

            if (Mathf.Abs(row - Mathf.Round(row)) < (1 - actorSize) / 2.0f)
            {
                row1 = Mathf.RoundToInt(row);
            }
            else
            {
                row1 = Mathf.FloorToInt(row);
                row2 = Mathf.CeilToInt(row);
            }

            int col1, col2 = -1;

            if (Mathf.Abs(col - Mathf.Round(col)) < (1 - actorSize) / 2.0f)
            {
                col1 = Mathf.RoundToInt(col);
            }
            else
            {
                col1 = Mathf.FloorToInt(col);
                col2 = Mathf.CeilToInt(col);
            }

            pointsToCheck.Add(new RoomPoint(row1, col1));
            if (row2 >= 0)
            {
                pointsToCheck.Add(new RoomPoint(row2, col1));
            }
            if (col2 >= 0)
            {
                pointsToCheck.Add(new RoomPoint(row1, col2));
            }
            if (row2 >= 0 && col2 >= 0)
            {
                pointsToCheck.Add(new RoomPoint(row2, col2));
            }

            row += rowDir;
            col += colDir;
        }

        foreach (var point in pointsToCheck)
        {
            if (!IsPointTravable(point, roomGrid, maxTravelCost))
            {
                return false;
            }
        }

        return true;
    }

    // Returns random point that can be travelled to from start point and which satisfies condition minRadius <= Distance(start, end) <= maxRadius
    public static RoomPoint GetRandomTravablePointInRadius(RoomPoint start, RoomGrid roomGrid, float minRadius, float maxRadius)
    {
        if (maxRadius < 0 || minRadius > maxRadius)
        {
            return start;
        }

        var openList = new HashSet<RoomPoint>();
        var travablePoints = new HashSet<RoomPoint>();
        var neighbors = findNeighbors(start, roomGrid);
        foreach (var neighbor in neighbors)
        {
            if (RoomPoint.Distance(start, neighbor) <= maxRadius)
            {
                openList.Add(neighbor);
            }
        }

        while (openList.Count > 0)
        {
            var curPoint = openList.Last();
            openList.Remove(curPoint);

            travablePoints.Add(curPoint);

            neighbors = findNeighbors(curPoint, roomGrid);

            foreach (var neighbor in neighbors)
            {
                if (!travablePoints.Contains(neighbor) && !openList.Contains(neighbor) && RoomPoint.Distance(start, neighbor) <= maxRadius)
                {
                    openList.Add(neighbor);
                }
            }
        }

        List<RoomPoint> finalPoints = travablePoints.ToList();

        if (minRadius > 1)
        {
            for (int i = 0; i < finalPoints.Count; ++i)
            {
                while (i < finalPoints.Count && RoomPoint.Distance(start, finalPoints[i]) < minRadius)
                {
                    finalPoints.RemoveAt(i);
                }
            }
        }

        return finalPoints[Random.Range(0, finalPoints.Count)];
    }

    // Returns random point that can be travelled to from start point and which satisfies condition Distance(start, end) <= maxRadius
    public static RoomPoint GetRandomTravablePointInRadius(RoomPoint start, RoomGrid roomGrid, float maxRadius)
    {
        return GetRandomTravablePointInRadius(start, roomGrid, 0.0f, maxRadius);
    }

    // Returns local coordinates random point that can be travelled to from start point and which satisfies condition minRadius <= Distance(start, end) <= maxRadius
    public static Vector3 GetRandomTravablePointInRadius(Vector3 start, RoomGrid roomGrid, float minRadius, float maxRadius)
    {
        return RoomPointToLocal(GetRandomTravablePointInRadius(LocalToRoomPoint(start), roomGrid, minRadius, maxRadius));
    }

    // Returns local coordinates of point that can be travelled to from start point and which satisfies condition Distance(start, end) <= maxRadius
    public static Vector3 GetRandomTravablePointInRadius(Vector3 start, RoomGrid roomGrid, float maxRadius)
    {
        return GetRandomTravablePointInRadius(start, roomGrid, 0.0f, maxRadius);
    }

    // Builds path from start point to point from which sightPoint can be directly viewed 
    // Result point is chosen based on how far is it from start and how near is it to maxDistance
    public static Path BuildPathToPointWithSightOn(RoomPoint start, RoomPoint sightPoint, RoomGrid roomGrid, float maxDistance)
    {
        if (maxDistance < 1.0f)
        {
            return new Path();
        }

        var openList = new HashSet<RoomPoint>();
        var travablePoints = new HashSet<RoomPoint>();
        var neighbors = findNeighbors(sightPoint, roomGrid);
        foreach (var neighbor in neighbors)
        {
            if (RoomPoint.Distance(sightPoint, neighbor) <= maxDistance && Raycast(neighbor, sightPoint, roomGrid, false))
            {
                openList.Add(neighbor);
            }
        }

        while (openList.Count > 0)
        {
            var curPoint = openList.Last();
            openList.Remove(curPoint);

            travablePoints.Add(curPoint);

            neighbors = findNeighbors(curPoint, roomGrid);

            foreach (var neighbor in neighbors)
            {
                if (!travablePoints.Contains(neighbor) && !openList.Contains(neighbor) && RoomPoint.Distance(sightPoint, neighbor) <= maxDistance && Raycast(neighbor, sightPoint, roomGrid, false))
                {
                    openList.Add(neighbor);
                }
            }
        }

        if (travablePoints.Contains(start))
        {
            if (RoomPoint.Distance(start, sightPoint) >= maxDistance - 1.0f)
            {
                return BuildPath(start, start, roomGrid);
            }

            travablePoints.RemoveWhere(x => IsFacingSameDirection(start, sightPoint, x));
        }

        var finalPoints = travablePoints.ToList();
        finalPoints = finalPoints.OrderBy(x => -RoomPoint.Distance(x, sightPoint)).ToList();

        for (int i = 0; i < finalPoints.Count; ++i)
        {
            Path path = BuildPath(start, finalPoints[i], roomGrid);

            if (path.Count > 0)
            {
                return path;
            }
        }

        return new Path();
    }

    public static Path BuildPathToPointWithSightOn(Vector3 start, Vector3 sightPoint, RoomGrid roomGrid, float maxDistance)
    {
        return BuildPathToPointWithSightOn(LocalToRoomPoint(start), LocalToRoomPoint(sightPoint), roomGrid, maxDistance);
    }

    // Class used in A* algorithm
    private class PathNode
    {
        public RoomPoint point;

        // Distance from start point to current point
        public int fromStartToThis;

        // Approximate distance from current point to end point
        public int fromThisToEnd;

        // fromStartToThis + fromThisToEnd
        public int fullPath;

        public PathNode parent;

        public PathNode(RoomPoint point)
        {
            this.point = point;
            fromStartToThis = 0;
            fromThisToEnd = 0;
            fullPath = -1;
            parent = null;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            PathNode other = (PathNode)obj;
            return this.point == other.point;
        }

        public override int GetHashCode()
        {
            return point.GetHashCode();
        }

        public static bool operator ==(PathNode node1, PathNode node2)
        {
            if (node1 is null && node2 is null)
            {
                return true;
            }
            if (node1 is null || node2 is null)
            {
                return false;
            }
            return node1.point == node2.point;
        }

        public static bool operator !=(PathNode node1, PathNode node2)
        {
            return !(node1 == node2);
        }

        public class PathNodeComparer : IComparer<PathNode>
        {
            public int Compare(PathNode node1, PathNode node2)
            {
                if (node1.point == node2.point)
                {
                    return 0;
                }
                if (node1.fullPath < node2.fullPath)
                {
                    return -1;
                }
                if (node1.fullPath > node2.fullPath)
                {
                    return 1;
                }
                if (node1.GetHashCode() < node2.GetHashCode())
                {
                    return -1;
                }
                if (node1.GetHashCode() > node2.GetHashCode())
                {
                    return 1;
                }
                return 0;
            }
        }
    }

    // Makes path list from end node of path
    private static Path makePathFromNode(PathNode node, RoomGrid roomGrid)
    {
        var path = new Path();
        while (node != null)
        {
            path.Add(node.point);
            node = node.parent;
        }
        path.Reverse();
        if (path.Count == 1)
        {
            path.Add(path[0]);
        }
        path.Smoothe(roomGrid);
        return path;
    }

    // Returns list of nearest nodes that are avaliable for travel
    private static List<PathNode> findNeighbors(PathNode node, RoomGrid roomGrid)
    {
        var neighbors = new List<PathNode>();
        int row = node.point.i;
        int col = node.point.j;

        bool hasUp, hasDown, hasLeft, hasRight;
        hasUp = hasDown = hasLeft = hasRight = false;

        if (row > 0 && roomGrid[row - 1, col] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row - 1, col)));
            hasUp = true;
        }
        if (row < roomGrid.rows - 1 && roomGrid[row + 1, col] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row + 1, col)));
            hasDown = true;
        }
        if (col > 0 && roomGrid[row, col - 1] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row, col - 1)));
            hasLeft = true;
        }
        if (col < roomGrid.cols - 1 && roomGrid[row, col + 1] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row, col + 1)));
            hasRight = true;
        }

        if (hasUp && hasLeft && roomGrid[row - 1, col - 1] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row - 1, col - 1)));
        }
        if (hasUp && hasRight && roomGrid[row - 1, col + 1] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row - 1, col + 1)));
        }
        if (hasDown && hasLeft && roomGrid[row + 1, col - 1] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row + 1, col - 1)));
        }
        if (hasDown && hasRight && roomGrid[row + 1, col + 1] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row + 1, col + 1)));
        }

        return neighbors;
    }

    // Returns list of nearest nodes that are avaliable for travel
    private static List<RoomPoint> findNeighbors(RoomPoint point, RoomGrid roomGrid)
    {
        var neighbors = new List<RoomPoint>();
        int row = point.i;
        int col = point.j;

        bool hasUp, hasDown, hasLeft, hasRight;
        hasUp = hasDown = hasLeft = hasRight = false;

        if (row > 0 && roomGrid[row - 1, col] > 0)
        {
            neighbors.Add(new RoomPoint(row - 1, col));
            hasUp = true;
        }
        if (row < roomGrid.rows - 1 && roomGrid[row + 1, col] > 0)
        {
            neighbors.Add(new RoomPoint(row + 1, col));
            hasDown = true;
        }
        if (col > 0 && roomGrid[row, col - 1] > 0)
        {
            neighbors.Add(new RoomPoint(row, col - 1));
            hasLeft = true;
        }
        if (col < roomGrid.cols - 1 && roomGrid[row, col + 1] > 0)
        {
            neighbors.Add(new RoomPoint(row, col + 1));
            hasRight = true;
        }

        if (hasUp && hasLeft && roomGrid[row - 1, col - 1] > 0)
        {
            neighbors.Add(new RoomPoint(row - 1, col - 1));
        }
        if (hasUp && hasRight && roomGrid[row - 1, col + 1] > 0)
        {
            neighbors.Add(new RoomPoint(row - 1, col + 1));
        }
        if (hasDown && hasLeft && roomGrid[row + 1, col - 1] > 0)
        {
            neighbors.Add(new RoomPoint(row + 1, col - 1));
        }
        if (hasDown && hasRight && roomGrid[row + 1, col + 1] > 0)
        {
            neighbors.Add(new RoomPoint(row + 1, col + 1));
        }

        return neighbors;
    }

    private static bool IsNeighborhoodTravable(float row, float col, RoomGrid roomGrid, int maxTravelCost)
    {
        var rows = new List<int>();
        var cols = new List<int>();

        if (Mathf.Abs(row - Mathf.Round(row)) < (1 - actorSize) / 2.0f)
        {
            rows.Add(Mathf.RoundToInt(row));
        }
        else
        {
            rows.Add(Mathf.FloorToInt(row));
            rows.Add(Mathf.CeilToInt(row));
        }

        if (Mathf.Abs(col - Mathf.Round(col)) < (1 - actorSize) / 2.0f)
        {
            cols.Add(Mathf.RoundToInt(col));
        }
        else
        {
            cols.Add(Mathf.FloorToInt(col));
            cols.Add(Mathf.CeilToInt(col));
        }

        foreach (var i in rows)
        {
            foreach (var j in cols)
            {
                if (!IsPointTravable(i, j, roomGrid, maxTravelCost))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsPointTravable(RoomPoint point, RoomGrid roomGrid, int maxTravelCost)
    {
        return roomGrid.hasPoint(point) && roomGrid[point] > 0 && (maxTravelCost == 0 || !(roomGrid[point] > maxTravelCost));
    }

    private static bool IsPointTravable(int row, int col, RoomGrid roomGrid, int maxTravelCost)
    {
        return roomGrid.hasPoint(row, col) && roomGrid[row, col] > 0 && (maxTravelCost == 0 || !(roomGrid[row, col] > maxTravelCost));
    }

    private static bool IsFacingSameDirection(RoomPoint start, RoomPoint end1, RoomPoint end2)
    {
        Vector2 startVec = RoomPointToLocal(start);
        Vector2 endVec1 = RoomPointToLocal(end1);
        Vector2 endVec2 = RoomPointToLocal(end2);

        return Vector2.Dot(endVec1 - startVec, endVec2 - startVec) >= 0;
    }
}
