using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;
using System;

public class PlayerInfo : NetworkBehaviour
{
    [SerializeField] private TMP_Text _txtPlayerName;

    //A variable to hold the name of the player
    public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
        new FixedString64Bytes("PlayerName"), 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );

   
    public void SetName(string name)
    {
        PlayerName.Value = new FixedString64Bytes(name);
    }
    public override void OnNetworkSpawn()
    {
        PlayerName.OnValueChanged += OnNameChanged;

        _txtPlayerName.SetText(PlayerName.Value.ToString());
        gameObject.name = "Player_" +PlayerName.Value.ToString();

        if (IsLocalPlayer)
        {
            GameManager.Instance.SetLocalPlayer(NetworkObject);
        }

        GameManager.Instance.OnplayerJoined(NetworkObject);
    }
    public override void OnNetworkDespawn()
    {
        PlayerName.OnValueChanged -= OnNameChanged;
    }

    private void OnNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        if (newValue != previousValue)
        {
            _txtPlayerName.SetText(newValue.Value);
            GameManager.Instance.SetPlayerName(NetworkObject, newValue.ToString());
        }
    }
}
