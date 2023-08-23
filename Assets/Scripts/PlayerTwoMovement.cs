using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTwoMovement : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 10f;
    [SerializeField] private float playerTurnSpeed = 10f;

    private Rigidbody playerRB;
    private float horizontal;
    private float vertical;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("HorizontalAlt");
        vertical = Input.GetAxis("VerticalAlt");

    }

    private void FixedUpdate()
    {
        playerRB.velocity = playerRB.transform.forward * playerSpeed * vertical;

        playerRB.rotation = Quaternion.Euler(transform.eulerAngles + transform.up * horizontal * playerTurnSpeed);
    }
}
