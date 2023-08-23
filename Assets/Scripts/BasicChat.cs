using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
public class BasicChat : NetworkBehaviour
{

    [SerializeField] private TMP_InputField _chatInput;
    [SerializeField] private TMP_Text _chatText;
    
    public void SendChat()
    {
        if(IsServer)
        {
            ChatClientRPC(NetworkManager.Singleton.LocalClientId + _chatInput.text);
        }
        else if(IsClient)
        {
            ChatServerRPC(NetworkManager.Singleton.LocalClientId + _chatInput.text);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC(string message)
    {
       
        if (!IsHost)
        {
            _chatText.text += "\n" + message;
        }

        ChatClientRPC(message);
    }

    [ClientRpc]
    public void ChatClientRPC(string message)
    {
        _chatText.text += "\n" + message;
    }
}
