using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumpDetection : MonoBehaviour
{
    public Transform Player;
    public bool touchingWall;
    public Vector3 offset;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Player)
            transform.position = new Vector3(Player.position.x + offset.x, Player.position.y + offset.y, Player.position.z + offset.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            touchingWall = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            touchingWall = false;
    }
}
