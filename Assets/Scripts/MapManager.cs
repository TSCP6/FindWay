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
        public Vector2 worldPosition; //��������
        public float gCost; //��㵽��ǰ��Ĵ���
        public float hCost; //��ǰ�㵽�յ�Ĺ��ƴ���
        public float fCost => gCost + hCost; //�ܴ���
        public bool isObstacle; //�Ƿ����ϰ�
        public Node parent; //����·��
    }

    void Awake()
    {
        CreateGrid();
    }

    //-------------------��ʼ���߼�--------------------

    void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Node node = new Node();
                node.position = new Vector2Int(x, y); //������긳ֵ

                float worldX = gridOrigin.x + gridSize * x + gridSize * 0.5f;
                float worldY = gridOrigin.y + gridSize * y + gridSize * 0.5f;
                node.worldPosition = new Vector2(worldX, worldY); //��ֵ������������

                node.isObstacle = Physics2D.OverlapCircle(node.worldPosition, gridSize * 0.4f, obstacleLayer);
                //OverLapCircle������worldPositionΪԲ�ģ��Եڶ�������Ϊ�뾶��Բ������Ƿ���layer����ײ

                grid[x, y] = node;
            }
        }
    }

    public Vector2Int WorldToGrid(Vector2 worldPosition) //��������ת��Ϊ��������
    {
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x - gridSize * 0.5f) / gridSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOrigin.y - gridSize * 0.5f) / gridSize);
        return new Vector2Int(Mathf.Clamp(x, 0, gridWidth - 1), Mathf.Clamp(y, 0, gridHeight - 1));
        //Clamp�����ж�x�Ƿ�����Сֵ0�����ֵwidth֮�䣬���򷵻ص�һ������
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

    public void OnDrawGizmos() //�������scene��
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

    //-------------------Ѱ·�㷨��������-------------------
    public List<Node> FindPath(Vector2 startWorldPos, Vector2 endWorldPos) //Ѱ·����
                                                                           //1.ת�����꣬��ȡʼ�յ㣬�������Ϸ������������б�͹رչ�ϣ���������ӿ��ű�
                                                                           //2.ѭ����ʼ�������п��ű����ҵ�fֵ���ٵĻ���gֵ��С�ģ���ӵ��رձ��Ƴ����ű�
                                                                           //3.����ҵ��յ�ֱ�ӻ��ݣ������������ڽ�㣬�����ϰ�������Ѿ��ڹر��б��ھӣ�
                                                                           //4.���¼���gֵ��Ϊ��ǰgֵ���ϵ�ǰ���ھӵľ��룬����µ�gֵС��ԭ�е�ֵ���߲��ڿ����б��У����¸�ֵg��hֵ�����ø��ڵ㣬��������б�����������ӽ�ȥ
                                                                           //5.ѭ������û���ҵ��ͷ��ؿ�
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

    List<Node> RetracePath(Node startNode, Node endNode) //���յ���ݵ����
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

    List<Node> GetNeighbors(Node node) //��ȡ�ھӽڵ�
    {
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; //�����Լ�
                int nearX = node.position.x + x;
                int nearY = node.position.y + y;
                Node neighbor = GetNode(nearX, nearY);
                if (neighbor != null)
                {
                    if (x != 0 && y != 0) //���Խ��߷�ֹ��ǽ
                    {
                        Node sideX = GetNode(node.position.x + x, node.position.y);
                        Node sideY = GetNode(node.position.x, node.position.y + y);

                        //�����Խ����ھӣ�����Ҳ�����������߶����ϰ�ʱ����ͨ��
                        if ((sideX != null && sideX.isObstacle) || (sideY != null && sideY.isObstacle)) continue;
                    }
                    neighbors.Add(neighbor);
                }
            }
        return neighbors;
    }

    float GetDistance(Node nodeA, Node nodeB) //�������֮��ľ���
    {
        int disX = Mathf.Abs(nodeA.position.x - nodeB.position.x);
        int disY = Mathf.Abs(nodeA.position.y - nodeB.position.y);

        //�Խ���ֵΪ14��ֱ��Ϊ10���Խ��߳�������Ϊ��Ӧ����
        if (disX > disY) return 14 * disY + 10 * (disX - disY);
        return 14 * disX + 10 * (disY - disX);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
