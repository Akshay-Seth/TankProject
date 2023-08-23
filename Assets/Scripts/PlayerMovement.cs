 using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float playerSpeed = 10f;
    [SerializeField] private float playerTurnSpeed = 10f;

    private Rigidbody playerRB;
    private float horizontal;
    private float vertical;

    [SerializeField] private string horizontalInput;
    [SerializeField] private string verticalInput;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerRB = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (IsServer && IsLocalPlayer)
        {
            if (GameManager.Instance._state.Value == 1)
            {
                this.horizontal = Input.GetAxis(horizontalInput);
                this.vertical = Input.GetAxis(verticalInput);
            }
            else
            {
                this.horizontal = 0;
                this.vertical = 0;
            }

        }
        else if(IsClient && IsLocalPlayer)
        {
            Debug.Log("Calling RPC");
            MovementServerRPC(Input.GetAxis(horizontalInput), Input.GetAxis(verticalInput));
        }

    }

    [ServerRpc] private void MovementServerRPC(float horizontal, float vertical)
    {
        if(GameManager.Instance._state.Value == 1)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
        }
        else
        {
            this.horizontal = 0;
            this.vertical = 0;
        }
        
    }

    private void FixedUpdate()
    {
        playerRB.velocity = playerRB.transform.forward * playerSpeed * vertical;
        playerRB.rotation = Quaternion.Euler(transform.eulerAngles + transform.up * horizontal * playerTurnSpeed);
    }
}
