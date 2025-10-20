using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    MapManager mapManager;

    public float moveSpeed = 5f;
    public float movementThreshold = 0.01f;
    public bool showPath = false;

    public GameObject target;

    bool isMoving = false;

    private List<MapManager.Node> curPath;

    // Start is called before the first frame update
    void Start()
    {
        //在场景中寻找第一个含有mapmanager组件的物体，并赋值组件的引用
        mapManager = FindObjectOfType<MapManager>();

        StartCoroutine(WaitForMapAndMove());
    }

    IEnumerator WaitForMapAndMove()
    {
        while (mapManager == null || mapManager.grid == null)
        {
            yield return null;
        }
        if (target != null)
        {
            MoveToTarget(target.transform.position);
        }
    }

    void MoveToTarget(Vector2 targetPos)
    {
        if (isMoving) return; //正在寻路则不做处理

        List<MapManager.Node> path = mapManager.FindPath(transform.position, targetPos);

        if (path != null && path.Count > 0)
        {
            StartCoroutine(FollowPath(path));
            curPath = path;
        }
        else
        {
            Debug.Log("can't find the path");
        }
    }

    IEnumerator FollowPath(List<MapManager.Node> path)
    {
        isMoving = true;

        foreach (MapManager.Node node in path)
        {
            while (Vector2.Distance(transform.position, node.worldPosition) > movementThreshold)
            {
                transform.position = Vector2.MoveTowards(transform.position, node.worldPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isMoving = false;
    }



    private void OnDrawGizmos()
    {
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
        }
        if (mapManager != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.position, Vector2.one * mapManager.gridSize);
        }
        else
        {
            // 如果mapManager还是null,使用默认大小
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.position, Vector2.one);
        }
        if (showPath)
        {
            if(curPath  != null)
            {
                foreach(MapManager.Node node in curPath)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(node.worldPosition, mapManager.gridSize * Vector2.one);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
