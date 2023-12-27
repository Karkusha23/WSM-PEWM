using System.Collections.Generic;
using static Room;

public static class RoomPath
{
    // Build shortest path from start to end using A* algorithm
    public static List<RoomPoint> buildPath(RoomPoint start, RoomPoint end, byte[,] roomGrid)
    {
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
                return makePathFromNode(curNode);
            }

            int newFromStartToThis = curNode.fromStartToThis + roomGrid[curNode.point.i, curNode.point.j];

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
                        openListNeighbor.fromThisToEnd = RoomPoint.Distance(openListNeighbor.point, end);
                        openListNeighbor.fullPath = newFromStartToThis + openListNeighbor.fromThisToEnd;
                        openListNeighbor.parent = curNode;
                        openList.Add(openListNeighbor);
                    }
                }
                else
                {
                    neighbor.fromStartToThis = newFromStartToThis;
                    neighbor.fromThisToEnd = RoomPoint.Distance(neighbor.point, end);
                    neighbor.fullPath = newFromStartToThis + neighbor.fromThisToEnd;
                    neighbor.parent = curNode;
                    openList.Add(neighbor);
                }
            }

            closedSet.Add(curNode);

        }

        return new List<RoomPoint>();
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
            parent = null;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            PathNode other = (PathNode)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            return (point.i << 8) | point.j;
        }

        public static bool operator ==(PathNode node1, PathNode node2)
        {
            return node1.point == node2.point;
        }

        public static bool operator !=(PathNode node1, PathNode node2)
        {
            return !(node1.point == node2.point);
        }

        public class PathNodeComparer : IComparer<PathNode>
        {
            public int Compare(PathNode node1, PathNode node2)
            {
                if (node1.fullPath < node2.fullPath)
                {
                    return -1;
                }
                if (node1.fullPath > node2.fullPath)
                {
                    return 1;
                }
                return 0;
            }
        }
    }

    // Makes path list from end node of path
    private static List<RoomPoint> makePathFromNode(PathNode node)
    {
        var path = new List<RoomPoint>();
        while (node != null)
        {
            path.Add(node.point);
            node = node.parent;
        }
        path.Reverse();
        return path;
    }

    // Returns list of nearest nodes that are avaliable for travel
    private static List<PathNode> findNeighbors(PathNode node, byte[,] roomGrid)
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
        if (row < roomTileHeightCount - 1 && roomGrid[row + 1, col] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row + 1, col)));
            hasDown = true;
        }
        if (col > 0 && roomGrid[row, col - 1] > 0)
        {
            neighbors.Add(new PathNode(new RoomPoint(row, col - 1)));
            hasLeft = true;
        }
        if (col < roomTileWidthCount - 1 && roomGrid[row, col + 1] > 0)
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
}
