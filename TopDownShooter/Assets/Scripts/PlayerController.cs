using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    Vector3 velocity;
    Rigidbody playerRB;

    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }



    public void Move(Vector3 _moveVelocity)
    {
        velocity = _moveVelocity;

    }

    void FixedUpdate()
    {
        playerRB.MovePosition(playerRB.position + velocity * Time.fixedDeltaTime);
    }
}

