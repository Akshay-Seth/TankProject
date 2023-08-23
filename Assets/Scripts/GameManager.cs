using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UIElements;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private NetworkObject _localPlayer;

    void Singleton()
    {
        if (Instance == null && Instance != this)
        {
            Destroy(Instance);
        }

        Instance = this;
    }


    Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();
    Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();

    [SerializeField] private Transform[] StartPositions;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text _scoreUI;
    [SerializeField] private TMP_InputField _playerNameField;

    [SerializeField] private GameObject _endgameScreeen;
    [SerializeField] private TMP_Text _endGameMessage;

    public NetworkVariable<short> _state = new NetworkVariable<short>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server

    );
    private void Awake()
    {
        Singleton();

        if (IsServer)
        {
            _state.Value = 0;
        }
    }
    public void SetLocalPlayer(NetworkObject localPlayer)
    {
        _localPlayer = localPlayer;

        if (_playerNameField.text.Length > 0)
        {
            _localPlayer.GetComponent<PlayerInfo>().SetName(_playerNameField.text);
        }
        else
        {
            _localPlayer.GetComponent<PlayerInfo>().SetName($"player-{_localPlayer.OwnerClientId}");
        }

        _playerNameField.gameObject.SetActive(false);
    }

    //Will be called by player info of each player in game

    public void OnplayerJoined(NetworkObject playerObject)
    {
        //Assign Start position
        playerObject.transform.position = StartPositions[(int)playerObject.OwnerClientId].position;
        playerScores.Add(playerObject.OwnerClientId, 0);
    }

    public void StartGame()
    {
        _state.Value = 1;
        ShowScoreUI();

    }

    public void SetPlayerName(NetworkObject playerObject, string name)
    {
        if (playerNames.ContainsKey(playerObject.OwnerClientId))
        {
            playerNames[playerObject.OwnerClientId] = name;
        }
        else
        {
            playerNames.Add(playerObject.OwnerClientId, name);
        }
    }

    //If the bullet hits, we will call the server to do this

    public void AddScore(ulong playerId)
    {
        if (IsServer)
        {
            playerScores[playerId]++;
            ShowScoreUI();
            CheckWinner(playerId);
        }
    }
    public void ShowScoreUI()
    {
        _scoreUI.text = "";
        PlayerScores _scores = new PlayerScores();

        _scores._scores = new List<ScoreInfo>();

        foreach (var item in playerScores)
        {
            ScoreInfo temp = new ScoreInfo();
            temp.score = item.Value;
            temp.Id = item.Key;
            temp.Name = playerNames[item.Key];
            _scores._scores.Add(temp);

            _scoreUI.text += $"{item.Key} {playerNames[item.Key]} : {item.Value}\n";
        }

        //UPDATE CLIENT 
        UpdateClientScoreClientRPC(JsonUtility.ToJson(_scores));
    }


    [ClientRpc] public void UpdateClientScoreClientRPC(string scoreInfo)
    {
        PlayerScores _scores = JsonUtility.FromJson<PlayerScores>(scoreInfo);
        _scoreUI.text = "";

        foreach (var item in _scores._scores) {
            
            _scoreUI.text += $"{item.Id} {item.Name} : {item.score}\n";
        }
    }
    void CheckWinner(ulong PlayerId)
    {
        if (playerScores[PlayerId] >= 10)
        {
            //End Game
            EndGame(PlayerId);
        }
    }

    void EndGame(ulong WinnerID)
    {
        if(IsServer)
        {
            //Show Winning UI for the server
            _endgameScreeen.SetActive(true);
            if(WinnerID == NetworkManager.LocalClientId)
            {
                _endGameMessage.text = "Winner!";
            }
            else
            {
                _endGameMessage.text = $"You lost\nThe Winner is {playerNames[WinnerID]}";
            }

            ScoreInfo temp = new ScoreInfo();
            temp.score = playerScores[WinnerID];
            temp.Id = WinnerID;
            temp.Name = playerNames[WinnerID];

            ShowGameEndUIClientRPC(JsonUtility.ToJson(temp));
        }
    }

    [ClientRpc] public void ShowGameEndUIClientRPC( string winnerinfo)
    {
        _endgameScreeen.SetActive(true);
        ScoreInfo info = JsonUtility.FromJson<ScoreInfo>(winnerinfo);

        if(info.Id == NetworkManager.LocalClientId) {
            _endGameMessage.text = "Winner!";
        }
        else
        {
            _endGameMessage.text = $"You lost /n The Winner is {info.Name}";
        }
    }

    public void ResetPlayerPosition(NetworkObject playerObject, ulong PlayerId)
    {
        playerObject.transform.position = StartPositions[(int)PlayerId].position;
    }
}
[System.Serializable]
public class PlayerScores
{
    public List<ScoreInfo> _scores;
}

[System.Serializable]
public class ScoreInfo
{
    public ulong Id;
    public string Name;
    public int score;
}