using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;
using TMPro;
using System;
using System.Threading.Tasks;

public class GameNetworkManager : MonoBehaviour
{
    [SerializeField] private int _maxConnections = 4;

    [Header("Connection to UI")]
    [SerializeField] private TMP_Text _statusTxt;
    [SerializeField] private GameObject _btnClient;
    [SerializeField] private GameObject _btnHost;
    [SerializeField] private TMP_Text _playerIDtext;
    [SerializeField] private TMP_InputField _joinCodeText;
    //[SerializeField] 


    private string _playerID;
    private bool _clientAuthenticated = false;
    private string _joincode;

    public async Task<RelayServerData> AllocateRelayServerAndCode(int MaxConnections, string region = null) 
    {
        Allocation allocation;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections, region);
        }
        catch (Exception e)
        {
            Debug.Log($"Relay allocation request failed - {e}");
            throw;
        }
        Debug.Log($"Server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"Server: {allocation.AllocationId}");

        try
        {
            _joincode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (Exception e)
        {
            Debug.Log($"Unable to creat a join code - {e}");
            throw;
        }

        return new RelayServerData(allocation, "dtls");
    }

    public void JoinHost()
    {
        NetworkManager.Singleton.StartHost();
        _statusTxt.text = "Joined as Host";
    }

    public void JoinClient()
    {
        NetworkManager.Singleton.StartClient();
        _statusTxt.text = "Joined as Client";
    }

    public void JoinServer()
    {
        NetworkManager.Singleton.StartServer();
        _statusTxt.text = "Joined as Server";
    }

}