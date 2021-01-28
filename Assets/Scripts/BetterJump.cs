using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterJump : MonoBehaviour
{
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2;
    private WallJumpRaycast my_WallJumpRaycast;
    public GameObject wallJumpObj;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        my_WallJumpRaycast = wallJumpObj.GetComponent<WallJumpRaycast>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.y < 0 && WallSliding() == false)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump") && WallSliding() == false)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        else if  (WallSliding())
        {
            rb.velocity = new Vector3(rb.velocity.x,0.2f, rb.velocity.z);
        }
    }

    private bool WallSliding()
    {
        if (my_WallJumpRaycast.slideAgainstWall == true && wallJumpObj.active == true)
        {
            return true;
        }
        return false;
    }
}
