using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2Shoot : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootSpeed;
    [SerializeField] private Transform shootPoint;

    private Rigidbody playerRb;
    // Start is called before the first frame update
    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject tempbullet = Instantiate(bullet, shootPoint.position, shootPoint.rotation);

        tempbullet.GetComponent<Rigidbody>().AddForce(playerRb.velocity + tempbullet.transform.forward * shootSpeed, ForceMode.VelocityChange);

        Destroy(tempbullet, 5f);
    }
}
