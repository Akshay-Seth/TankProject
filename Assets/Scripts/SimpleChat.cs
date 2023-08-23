using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class SimpleChat : NetworkBehaviour
{
    [SerializeField] private TMP_InputField _chatInput;
    [SerializeField] private TMP_Text _chatText;


    public void SendChat()
    {
        if (IsServer) 
        {
            ChatClientRPC();//Calls ClientsS    
        }else if(IsClient)
        {
            ChatServerRPC();//Calls Server
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChatServerRPC()
    {
        _chatText.text = "A Client Says Hi";
    }

    [ClientRpc] 
    public void ChatClientRPC()
    {
        _chatText.text = "Server Says Hi";
    }

}
