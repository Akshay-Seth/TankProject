using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDamage : NetworkBehaviour
{
   public void DealDamage()
    {
        Debug.Log($"{OwnerClientId} was shot");

        GameManager.Instance.ResetPlayerPosition(NetworkObject, OwnerClientId);
    }
}
