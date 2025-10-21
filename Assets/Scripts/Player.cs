using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public float movementSpeed = 3f;

    public Rigidbody2D playerRb;

    float XInput, YInput;
    float movingThreshold = 0.05f;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        XInput = Input.GetAxis("Horizontal");
        YInput = Input.GetAxis("Vertical");
    }

    private void FixedUpdate() //����������йصĲ�����Ҫ����fixedUpdate��
    {
        Move();
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(XInput, YInput, 0);
        if(moveDir.magnitude > movingThreshold)
        {
            playerRb.velocity = moveDir * movementSpeed;
        }
        else
        {
            playerRb.velocity = new Vector2(0, 0);
        }
    }
}
