using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    MapManager mapManager;

    public float moveSpeed = 5f;
    public float movementThreshold = 0.01f;
    public float findWayInterval = 0.5f;
    public bool showPath = false;

    public enum findPathMethod { FindPathAStar, FindPathDijkstra, FindPathDijkstraWithHeap } ;

    public findPathMethod curFindPathMethod;

    public Rigidbody2D enemyRb;

    public GameObject target;

    private List<MapManager.Node> curPath;
    private float findWayTimer;
    private int pathNodeIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        //�ڳ�����Ѱ�ҵ�һ������mapmanager��������壬����ֵ���������
        mapManager = FindObjectOfType<MapManager>();
        enemyRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null || mapManager == null || mapManager.grid == null)
        {
            return;
        }
        findWayTimer += Time.deltaTime;
        if(findWayTimer > findWayInterval)
        {
            UpdatePath();
            findWayTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    void UpdatePath() //�ҵ���·����·����������������·���ͽڵ������������������
    {
        List<MapManager.Node> newPath = null;
        if (curFindPathMethod == findPathMethod.FindPathAStar)
            newPath = mapManager.FindPathAStar(transform.position, target.transform.position);
        else if (curFindPathMethod == findPathMethod.FindPathDijkstra)
            newPath = mapManager.FindPathDijkstra(transform.position, target.transform.position);
        else if (curFindPathMethod == findPathMethod.FindPathDijkstraWithHeap)
            newPath = mapManager.FindPathDijkstraWithHeap(transform.position, target.transform.position);


        if (newPath != null && newPath.Count > 0)
        {
            curPath = newPath;
            pathNodeIndex = 0;
        }
        else
        {
            curPath = null;
            Debug.Log("Can't find the path");
        }
    }

    void Move() //���·��û�л�������ֹͣ�ƶ�������Ŀ��ڵ��ƶ����ƶ����ڵ�λ�����������
    {
        if(curPath == null || curPath.Count <= pathNodeIndex)
        {
            return;
        }

        MapManager.Node targetNode = curPath[pathNodeIndex];

        Vector2 pos = Vector2.MoveTowards(enemyRb.position, targetNode.worldPosition, moveSpeed * Time.deltaTime);
        enemyRb.MovePosition(pos);

        if (Vector2.Distance(transform.position, targetNode.worldPosition) == 0)
        {
            pathNodeIndex++;
        }
    }

    private void OnDrawGizmos()
    {
        if (showPath && curPath != null)
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<MapManager>();
            }
            foreach (MapManager.Node node in curPath)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(node.worldPosition, mapManager.gridSize * Vector2.one);
            }
        }
    }
}
