using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class MapManager : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float gridSize = 1f;

    public Vector2 gridOrigin = Vector2.zero;

    public LayerMask obstacleLayer;

    public Node[,] grid;

    public class Node
    {
        public Vector2Int position;
        public Vector2 worldPosition; //世界坐标
        public float gCost; //起点到当前点的代价
        public float hCost; //当前点到终点的估计代价
        public float fCost => gCost + hCost; //总代价
        public bool isObstacle; //是否是障碍
        public Node parent; //回溯路径
    }

    void Awake()
    {
        CreateGrid();
    }

    //-------------------初始化逻辑--------------------

    void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Node node = new Node();
                node.position = new Vector2Int(x, y); //表格坐标赋值

                float worldX = gridOrigin.x + gridSize * x + gridSize * 0.5f;
                float worldY = gridOrigin.y + gridSize * y + gridSize * 0.5f;
                node.worldPosition = new Vector2(worldX, worldY); //赋值结点的世界坐标

                node.isObstacle = Physics2D.OverlapCircle(node.worldPosition, gridSize * 0.4f, obstacleLayer);
                //OverLapCircle函数以worldPosition为圆心，以第二个参数为半径画圆，检测是否有layer的碰撞

                grid[x, y] = node;
            }
        }
    }

    public Vector2Int WorldToGrid(Vector2 worldPosition) //世界坐标转化为网格坐标
    {
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x - gridSize * 0.5f) / gridSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOrigin.y - gridSize * 0.5f) / gridSize);
        return new Vector2Int(Mathf.Clamp(x, 0, gridWidth - 1), Mathf.Clamp(y, 0, gridHeight - 1));
        //Clamp函数判断x是否在最小值0到最大值width之间，是则返回第一个参数
    }

    public Vector2 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector2(
            gridOrigin.x + gridPosition.x * gridSize + gridSize * 0.5f,
            gridOrigin.y + gridPosition.y * gridSize + gridSize * 0.5f);
    }

    public Node GetNode(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return grid[x, y];
        }
        return null;
    }

    public void OnDrawGizmos() //将表格画在scene中
    {
        if (grid == null) return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Node node = grid[x, y];
                if (node != null)
                {
                    Gizmos.color = node.isObstacle ? Color.black : Color.white;
                    Gizmos.DrawCube(node.worldPosition, Vector2.one * gridSize);

                    Gizmos.color = Color.gray;
                    Gizmos.DrawWireCube(node.worldPosition, Vector2.one * gridSize);
                }
            }
        }
    }

    //-------------------寻路算法函数部分-------------------
    public List<Node> FindPath(Vector2 startWorldPos, Vector2 endWorldPos) //寻路方法
                                                                           //1.转化坐标，获取始终点，并检查结点合法，创建开放列表和关闭哈希表，将起点添加开放表
                                                                           //2.循环开始，在所有开放表中找到f值最少的或者g值最小的，添加到关闭表，移除开放表
                                                                           //3.如果找到终点直接回溯，查找所有相邻结点，忽略障碍物或者已经在关闭列表邻居，
                                                                           //4.重新计算g值，为当前g值加上当前到邻居的距离，如果新的g值小于原有的值或者不在开放列表中，重新赋值g，h值，设置父节点，如果开放列表不包含，则添加进去
                                                                           //5.循环结束没有找到就返回空
    {
        Vector2Int startGrid = WorldToGrid(startWorldPos);
        Vector2Int endGrid = WorldToGrid(endWorldPos);

        Node startNode = GetNode(startGrid.x, startGrid.y);
        Node endNode = GetNode(endGrid.x, endGrid.y);

        if (startNode == null || endNode == null)
        {
            Debug.Log("Start or end point is null");
            return null;
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y].gCost = float.MaxValue;
                grid[x, y].hCost = 0;
                grid[x, y].parent = null;
            }
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closeList = new HashSet<Node>();

        startNode.gCost = 0; ;
        startNode.hCost = GetDistance(startNode, endNode);

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node curNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < curNode.fCost
                    || (openList[i].fCost == curNode.fCost && openList[i].hCost < curNode.hCost))
                {
                    curNode = openList[i];
                }
            }

            openList.Remove(curNode);
            closeList.Add(curNode);

            if (curNode == endNode) return RetracePath(startNode, endNode);

            foreach (Node neighor in GetNeighbors(curNode))
            {
                if (neighor.isObstacle || closeList.Contains(neighor)) continue;

                float newGCost = curNode.gCost + GetDistance(neighor, curNode);
                if (newGCost < neighor.gCost || !openList.Contains(neighor))
                {
                    neighor.gCost = newGCost;
                    neighor.hCost = GetDistance(neighor, endNode);
                    neighor.parent = curNode;

                    if (!openList.Contains(neighor))
                    {
                        openList.Add(neighor);
                    }
                }
            }
        }
        return null;
    }

    List<Node> RetracePath(Node startNode, Node endNode) //从终点回溯到起点
    {
        List<Node> path = new List<Node>();
        Node curNode = endNode;

        while (curNode != startNode)
        {
            path.Add(curNode);
            curNode = curNode.parent;
        }

        path.Reverse();
        return path;
    }

    List<Node> GetNeighbors(Node node) //获取邻居节点
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; //跳过自己
                int nearX = node.position.x + x;
                int nearY = node.position.y + y;
                Node neighbor = GetNode(nearX, nearY);
                if (neighbor != null)
                {
                    if (x != 0 && y != 0) //检测对角线防止穿墙
                    {
                        Node sideX = GetNode(node.position.x + x, node.position.y);
                        Node sideY = GetNode(node.position.x, node.position.y + y);

                        //跳过对角线邻居，这里也可以设置两边都是障碍时不可通过
                        if ((sideX != null && sideX.isObstacle) || (sideY != null && sideY.isObstacle)) continue;
                    }
                    neighbors.Add(neighbor);
                }
            }
        return neighbors;
    }

    float GetDistance(Node nodeA, Node nodeB) //两个结点之间的距离
    {
        int disX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int disY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        //对角线值为14，直线为10，对角线长度最多的为对应距离
        if (disX > disY) return 14 * disY + 10 * (disX - disY);
        return 14 * disX + 10 * (disY - disX);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
