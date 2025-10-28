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
        //在场景中寻找第一个含有mapmanager组件的物体，并赋值组件的引用
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

    void UpdatePath() //找到新路径，路径满足条件，更新路径和节点索引，否则输出报错
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

    void Move() //如果路径没有或走完则停止移动，否则朝目标节点移动，移动到节点位置则更新索引
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
