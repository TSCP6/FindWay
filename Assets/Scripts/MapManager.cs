using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class MapManager : MonoBehaviour
{
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float gridSize = 1f;

    public Vector2 gridOrigin = Vector2.zero;

    public LayerMask obstacleLayer ;

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

    // Start is called before the first frame update
    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight];

        for (int x=0; x<gridWidth; x++)
        {
            for(int y=0; y<gridHeight; y++)
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
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x) / gridSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOrigin.y) / gridSize);
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
        if(grid == null) return;

        for (int x = 0; x < gridWidth; x++) 
        {
            for(int y = 0; y < gridHeight; y++)
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
