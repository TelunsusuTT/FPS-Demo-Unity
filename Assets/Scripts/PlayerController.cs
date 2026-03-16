using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 3f;
    public float jumpForce = 5f;
    public float xSensitivity = 6;
    public float ySensitivity = 6;

    private float xRotation = 0;

    private Rigidbody rb;
    private Animator ani;
    private Vector3 velocity;
    private bool isJumped = false;

    [HideInInspector]
    public bool highSpeed = false;
    [HideInInspector]
    public bool isAiming = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ani = GetComponentInChildren<Animator>(); //动画在头部，需要找子物体
        //隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Mouse();
        HighSpeed();
        Move();
        Jump();
        IsAiming();
    }

    //鼠标旋转
    void Mouse()
    {
        float x = Input.GetAxis("Mouse X");
        float y = Input.GetAxis("Mouse Y");

        //上下旋转
        xRotation -= y * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80, 80);//向上下看最多80度
        ani.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);//头旋转通过动画组件找transform

        //左右旋转
        float yaw = x * xSensitivity;
        transform.Rotate(0f, yaw, 0f, Space.Self); //游戏物体自己旋转 Vector3.up是轴向 后面是转动多少
    }

    void Move()
    {
        //获取水平轴输入 A: -1; D: 1
        float horizontal = Input.GetAxis("Horizontal");

        //获取垂直
        float vertical = Input.GetAxis("Vertical");

        //计算运动方向
        Vector3 dir = (transform.forward*vertical + transform.right*horizontal).normalized;

        //速度
        velocity = dir * speed;
        velocity.y = rb.velocity.y;

        //移动动画
        ani.SetFloat("Movement", dir.magnitude);

    }

    void HighSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift) && IsGround())
        {
            highSpeed = true;
            speed = 5;
            ani.SetBool("Holstered", true);
        } else
        {
            highSpeed = false;
            speed = 3;
            ani.SetBool("Holstered", false);
        }
    }

    public void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && IsGround())
        {
            isJumped = true;
        } 
    }

    public bool IsGround()
    {
        RaycastHit hit;
        bool res = Physics.Raycast(transform.position + Vector3. up* 0.2f, -Vector3.up, out hit, 
            0.3f, LayerMask.GetMask("Ground"));
        return res;
    }

    public void IsAiming()
    {
        if (Input.GetMouseButton(1))
        {
            isAiming = true;
            ani.SetBool("Aim", true);
            float aim = ani.GetFloat("Aiming");
            ani.SetFloat("Aiming",Mathf.Lerp(aim, 1, 0.2f)); //利用插值获得平滑效果
        }
        else
        {
            isAiming = false;
            ani.SetBool("Aim", false);
            float aim = ani.GetFloat("Aiming");
            ani.SetFloat("Aiming", Mathf.Lerp(aim, 0, 0.2f));
        }
    }

    private void FixedUpdate()
    {
        if (isJumped)
        {
            isJumped = false;
            velocity.y = jumpForce;
        }
        rb.velocity = velocity;
        
    }
}
