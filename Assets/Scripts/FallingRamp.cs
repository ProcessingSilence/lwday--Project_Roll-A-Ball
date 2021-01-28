using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRamp : MonoBehaviour
{
    public Rigidbody RampThatFalls;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RampThatFalls.isKinematic = false;
            RampThatFalls.useGravity = true;
            RampThatFalls.AddTorque(-transform.right * 30);
        }
    }
}
