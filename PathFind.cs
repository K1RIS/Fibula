using UnityEngine;
using System.Collections.Generic;
using System;

public class PathFind : MonoBehaviour
{
    private class Node
    {
        public bool walkable;
        public Vector3 gridPosition;
        public int gridX;
        public int gridY;

        public int gCost;
        public int hCost;
        public Node parent;
        public int heapIndex;

        public Node(bool _walkable, Vector3 _gridPos, int _gridX, int _gridY)
        {
            walkable = _walkable;
            gridPosition = _gridPos;
            gridX = _gridX;
            gridY = _gridY;
        }

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }
    }

    private class Heap
    {
        Node[] nodes;
        int currentItemCount;

        public Heap(int maxHeapSize)
        {
            nodes = new Node[maxHeapSize];
        }

        public void Add(Node node)
        {
            node.heapIndex = currentItemCount;
            nodes[currentItemCount] = node;
            SortUp(node);
            currentItemCount++;
        }

        public Node RemoveFirst()
        {
            Node firstItem = nodes[0];
            currentItemCount--;
            nodes[0] = nodes[currentItemCount];
            nodes[0].heapIndex = 0;
            SortDown(nodes[0]);
            return firstItem;
        }

        public void UpdateItem(Node node)
        {
            SortUp(node);
        }

        public int Count
        {
            get
            {
                return currentItemCount;
            }
        }

        public bool Contains(Node node)
        {
            return Equals(nodes[node.heapIndex], node);
        }

        private void SortDown(Node node)
        {
            while (true)
            {
                int childIndexLeft = node.heapIndex * 2 + 1;
                int childIndexRight = node.heapIndex * 2 + 2;
                int swapIndex = 0;

                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;

                    if (childIndexRight < currentItemCount)
                    {
                        if (nodes[childIndexLeft].CompareTo(nodes[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (node.CompareTo(nodes[swapIndex]) < 0)
                    {
                        Swap(node, nodes[swapIndex]);
                    }
                    else
                    {
                        return;
                    }

                }
                else
                {
                    return;
                }

            }
        }

        void SortUp(Node node)
        {
            int parentIndex = (node.heapIndex - 1) / 2;

            while (true)
            {
                Node parentItem = nodes[parentIndex];
                if (node.CompareTo(parentItem) > 0)
                {
                    Swap(node, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (node.heapIndex - 1) / 2;
            }
        }

        void Swap(Node nodeA, Node nodeB)
        {
            nodes[nodeA.heapIndex] = nodeB;
            nodes[nodeB.heapIndex] = nodeA;
            int itemAIndex = nodeA.heapIndex;
            nodeA.heapIndex = nodeB.heapIndex;
            nodeB.heapIndex = itemAIndex;
        }
    }




    private Node[,] grid;
    private int gridSizeX, gridSizeY;
    private List<Node> path;

    private void Awake()
    {
        gridSizeX = gridSizeY = 23;
    }

    public Vector3 DetectWhereToStep(Vector3 target, bool withoutLastStep = false)
    {
        print("detecting");
        CreateGrid();
        try
        {
            FindPath(transform.position, target, withoutLastStep);
        }
        catch(IndexOutOfRangeException)
        {
            print("that's too far");
            return Vector3.zero;
        }
        try
        {
            return -(transform.position - path[0].gridPosition);
        }
        catch(NullReferenceException)
        {
            print("u can't go there");
            return Vector3.zero;            
        }   
    }

    //private void OnDrawGizmos()
    //{
    //    if (grid != null)
    //    {
    //        foreach (Node n in grid)
    //        {
    //            Gizmos.color = (n.walkable) ? Color.white : Color.red;
    //            //if (n.walkable)
    //            //     Gizmos.color = Color.white;

    //            if (path != null)
    //                if (path.Contains(n))
    //                    Gizmos.color = Color.black;

    //            Gizmos.DrawCube(n.gridPosition, Vector3.one * (1 - .1f));
    //        }
    //    }
    //}

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 gridPosition = transform.position - new Vector3(x - 11, 0, y - 11);
                bool walkable = !Physics.CheckSphere(gridPosition, 0.1f);
                grid[gridSizeX - x - 1, gridSizeY - y - 1] = new Node(walkable, gridPosition, gridSizeX - x - 1, gridSizeY - y - 1);
            }
        }
    }

    private void FindPath(Vector3 startPos, Vector3 targetPos, bool withoutLastStep)
    {
        Node startNode = GetNodeFromPosition(startPos);
        Node targetNode = GetNodeFromPosition(targetPos);

        Heap openSet = new Heap(gridSizeX * gridSizeY);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                path = RetracePath(startNode, targetNode, withoutLastStep);
                return;
            }

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    if (!(withoutLastStep && neighbour == targetNode))
                    {
                        closedSet.Add(neighbour);
                        continue;
                    }
                }

                int newCostToNeighbour;

                if (GetDistance(currentNode, neighbour) == 10)
                    newCostToNeighbour = currentNode.gCost + 10;
                else
                    newCostToNeighbour = currentNode.gCost + 21;

                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
        }
    }

    private Node GetNodeFromPosition(Vector3 worldPosition)
    {
        Vector3 difference = (worldPosition - transform.position);
        int x = (gridSizeX - 1) / 2 + (int)difference.x;
        int y = (gridSizeY - 1) / 2 + (int)difference.z;
        return grid[x, y];
    }

    private List<Node> RetracePath(Node startNode, Node endNode, bool withoutLastStep)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        if (withoutLastStep)
        {
            path.RemoveAt(0);
        }

        path.Reverse();
        return path;
    }

    private List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    neighbours.Add(grid[checkX, checkY]);
            }
        }
        return neighbours;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}