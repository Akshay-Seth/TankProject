using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Playershoot : NetworkBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootSpeed;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private KeyCode shootInput;
    private Rigidbody playerRb;

    public override void OnNetworkSpawn()
    {
        playerRb = GetComponent<Rigidbody>();
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (IsServer && IsLocalPlayer)
            {
                Shoot(OwnerClientId);
            }
            else if (IsClient && IsLocalPlayer)
            {
                //Ask the server to spawn a bullet and shoot!
                RequestShootServerRPC();
            }
        }
    }

    [ServerRpc]
    public void RequestShootServerRPC(ServerRpcParams serverRpcParams = default)
    {
        Shoot(serverRpcParams.Receive.SenderClientId);
    }
    private void Shoot(ulong ownerID)
    {
        GameObject tempBullet = Instantiate(bullet, shootPoint.position, shootPoint.rotation);
        tempBullet.GetComponent<NetworkObject>().Spawn();
        tempBullet.GetComponent<Bullet>().clientID = ownerID;
        tempBullet.GetComponent<Rigidbody>().AddForce(playerRb.velocity + tempBullet.transform.forward * shootSpeed, ForceMode.VelocityChange);
        Destroy(tempBullet, 2f);
    }
}
