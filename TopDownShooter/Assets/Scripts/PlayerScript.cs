using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    public float speed;
    public PlayerController playerController;

	// Use this for initialization
	void Start () {
        playerController = gameObject.GetComponent<PlayerController>();
	}
	// Update is called once per frame
	void Update () {
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * speed;

        playerController.Move(moveVelocity);

        Ray ray;


	}

}
