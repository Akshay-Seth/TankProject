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
using Unity.VisualScripting;

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

    private async void Start()
    {
        await AuthenticatePlayer();
    }
    async Task AuthenticatePlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            _playerID = AuthenticationService.Instance.PlayerId;
            _clientAuthenticated = true;
            _playerIDtext.text = $"Client Authentication successful - {_playerID}";
        }
        catch (Exception e)
        {

            Debug.Log(e);
        }
    }

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

    public async Task<RelayServerData> JoinRelayServerWithCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log($"Relay allocation join request failed -{e}");
            throw;
        }

        Debug.Log($"Client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"Host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"Client: {allocation.AllocationId}");

        return new RelayServerData(allocation, "dtls");
    }

    IEnumerator ConfigureGetCodeAndJoinHost()
    {
        //run the task to get code and join as host
        var allocateAndGetCode = AllocateRelayServerAndCode(_maxConnections);
        while(!allocateAndGetCode.IsCompleted)
        {
            //Wait until we create the Allocation and get code
            yield return null;
        }
        if (allocateAndGetCode.IsFaulted)
        {
            Debug.LogError($"Cant start the server due to an exception {allocateAndGetCode.Exception.Message}");
        }

        var relayServerdate = allocateAndGetCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerdate);
        NetworkManager.Singleton.StartHost();

        _joinCodeText.gameObject.SetActive(true);
        _joinCodeText.text = _joincode;
        _statusTxt.text = "Join As Host";

    }

    IEnumerator ConfigureUseCodeAndJoinClient(string joinCode)
    {
        var joinAllocationFromCode = JoinRelayServerWithCode(joinCode);
        while (!joinAllocationFromCode.IsCompleted)
        {
            //Wait until we create the Allocation and get code
            yield return null;
        }
        if (joinAllocationFromCode.IsFaulted)
        {
            Debug.LogError($"Cant join the server due to an exception {joinAllocationFromCode.Exception.Message}");
        }

        var relayServerdate = joinAllocationFromCode.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerdate);
        NetworkManager.Singleton.StartClient();

        _statusTxt.text = "Join As Client";
    }
    public void JoinHost()
    {
        if (!_clientAuthenticated)
        {
            Debug.Log("client not Authenticated. Please try again");
            return;
        }

        StartCoroutine(ConfigureGetCodeAndJoinHost());

        _btnClient.gameObject.SetActive(false);
        _btnHost.gameObject.SetActive(false);
        _joinCodeText.gameObject.SetActive(false);
        //NetworkManager.Singleton.StartHost();
        //_statusTxt.text = "Joined as Host";
    }

    public void JoinClient()
    {
        if (!_clientAuthenticated)
        {
            Debug.Log("client not Authenticated. Please try again");
            return;
        }

        if(_joinCodeText.text.Length <= 0)
        {
            Debug.Log($"Please Enter a Proper join code");
            _statusTxt.text = "please enter a proper join code";
        }

        Debug.Log($"The join code is: {_joinCodeText.text}");
        StartCoroutine(ConfigureUseCodeAndJoinClient(_joinCodeText.text));


        _btnClient.gameObject.SetActive(false);
        _btnHost.gameObject.SetActive(false);
        _joinCodeText.gameObject.SetActive(false);

        //NetworkManager.Singleton.StartClient();
        //_statusTxt.text = "Joined as Client";
    }

    public void JoinServer()
    {
        if (!_clientAuthenticated)
        {
            Debug.Log("client not Authenticated. Please try again");
            return;
        } 
        NetworkManager.Singleton.StartServer();
        _statusTxt.text = "Joined as Server";
    }

}
