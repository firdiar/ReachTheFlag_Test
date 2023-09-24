using Gtion.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager Instance;

    public bool IsGameRunning { get; private set; } = false;
    private bool isStartSquareDisabled;

    public NetworkVariable<SerializeableTime> EndSessionTime = new NetworkVariable<SerializeableTime>();
    public DateTime EndSessionTimeUtc { get; private set;}

    [SerializeField]
    private int sessionTime;
    public int SessionTime => sessionTime;

    [SerializeField]
    private GameObject startSquare;
    [SerializeField]
    private float flagDistance;
    [SerializeField]
    private Transform flags;

    [Header("UI")]
    [SerializeField]
    private StartGameUI startGameUI;
    [SerializeField]
    private GameplayUI gameplayUI;
    [SerializeField]
    private FinishGameUI finishUI;

    public int GameLevel { get; private set; } = 0;

    //This actually only for Server
    public int RequiredPlayerCount { get; private set; } = 0;
    public int Seed { get; private set; } = 0;
    public Dictionary<ulong,Transform> AllPlayers { get; private set; } = new Dictionary<ulong, Transform>(2);

    public System.Random Random { get; private set; }

    //End Game
    private Decision[] endGameDecision;
    public IReadOnlyList<Decision> EndGameDecision => endGameDecision;
    public EndGameState EndGameState { get; private set; }

    private void Start()
    {
        Instance = this;
        startGameUI.gameObject.SetActive(true);
        gameplayUI.gameObject.SetActive(false);
        finishUI.gameObject.SetActive(false);

        EndSessionTime.OnValueChanged += OnSessionTimeChanged;
    }

    private void OnSessionTimeChanged(SerializeableTime previousValue, SerializeableTime newValue)
    {
        EndSessionTimeUtc = newValue.ToDateTime();
        gameplayUI.Timer.StartTimer(EndSessionTimeUtc);

        //re-enable limit square
        double totalSecondLeft = GetSecondLeft();
        if ((sessionTime - totalSecondLeft) <= 4)
        {
            isStartSquareDisabled = false;
            startSquare.SetActive(true);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        Instance = null;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += JoinPlayer;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= JoinPlayer;
        }
    }

    public void RegisterPlayerObject(ulong clientId , Transform objectTrans) 
    {
        AllPlayers.Add(clientId, objectTrans);
    }

    public void InitGame(int player) 
    {
        RequiredPlayerCount = player;
        NetworkManager.Singleton.StartHost();
    }

    public void JoinGame()
    {
        RequiredPlayerCount = 2;
        NetworkManager.Singleton.StartClient();
    }

    private void JoinPlayer(ulong clientId)
    {
        if (RequiredPlayerCount == NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartGame();
        }
    }

    #region Start Game
    internal void NextGame()//this called only on server
    {
        if (IsGameRunning) return;

        ResetPositionClientRpc();
        StartGame();
    }

    internal void RetryGame()//this called only on server
    {
        if (IsGameRunning) return;

        ResetPositionClientRpc();
        StartGame(true); //restart game without changing seed
    }

    private void StartGame(bool isRetry = false) //this called only on server
    {
        if (!NetworkManager.Singleton.IsServer) return;

        EndSessionTime.Value = DateTime.UtcNow.AddSeconds(SessionTime);
        if (!isRetry)
        {
            Seed = UnityEngine.Random.Range(0, 10000);
        }        
        StartGameClientRpc(Seed, isRetry);
    }

    [ClientRpc]
    private void StartGameClientRpc(int seed, bool isRetry) 
    {
        if (!isRetry)
        {
            GameLevel++;
        }
        IsGameRunning = true;
        isStartSquareDisabled = false;
        startSquare.SetActive(true);

        Seed = seed;
        Random = new System.Random(seed);
        finishUI.gameObject.SetActive(false);
        gameplayUI.Initialize();
        
        GenerateProceduralMap();
    }

    [ClientRpc]
    private void ResetPositionClientRpc()
    {
        IsGameRunning = false;
        foreach (var player in AllPlayers)
        {
            player.Value.position = Vector3.zero;
        }
    }

    private void GenerateProceduralMap() 
    {
        //Spawn Flag
        var x = (Random.NextDouble() - 0.5)*2;
        var y = (Random.NextDouble() - 0.5) * 2;
        flags.position = new Vector2((float)x, (float)y).normalized * flagDistance; //distance consta
    }
    #endregion

    private void Update()
    {
        if (IsGameRunning)
        {
            double totalSecondLeft = GetSecondLeft();
            if (!isStartSquareDisabled && (sessionTime - totalSecondLeft) > 4)
            {
                isStartSquareDisabled = true;
                startSquare.SetActive(false);
            }
            
            if (totalSecondLeft <= 0)
            {
                FinishGame(1001);
            }
        }
    }

    private double GetSecondLeft() 
    {
        TimeSpan remainingTime = EndSessionTimeUtc - DateTime.UtcNow;
        return remainingTime.TotalSeconds;
    }

    #region Finish Game
    public void FinishGame(ulong ownerClientId)
    {
        if (!IsServer) return;

        if (ownerClientId == 1001)
        {
            if (RequiredPlayerCount > 1)
            {
                Debug.Log("Draw!");
                EndGameState = EndGameState.Draw;
            }
            else
            {
                Debug.Log("Lose!");
                EndGameState = EndGameState.Lose;
            }
        }
        else
        {
            Debug.Log("Win! : "+ownerClientId);
            EndGameState = EndGameState.Win;
        }

        GameOverClientRpc(ownerClientId, EndGameState);
    }

    [ClientRpc]
    public void GameOverClientRpc(ulong winner, EndGameState endGameState)
    {
        IsGameRunning = false;
        endGameDecision = new Decision[RequiredPlayerCount];
        EndGameState = endGameState;
        gameplayUI.gameObject.SetActive(false);
        finishUI.gameObject.SetActive(true);
        finishUI.Initialized(winner, endGameState);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetEndgameDecistionServerRPC(Decision decision , ServerRpcParams serverRpcParams = default)
    {
        var sender = serverRpcParams.Receive.SenderClientId;
        int index = (int)sender;
        endGameDecision[index] = decision;
        
        SetEndgameDecistionClientRPC(sender, decision);

        ContinueCheck();
    }

    [ClientRpc]
    private void SetEndgameDecistionClientRPC(ulong clientId , Decision decision)
    {
        int index = (int)clientId;
        endGameDecision[index] = decision;
        finishUI.UpdateView();
    }

    private void ContinueCheck()
    {
        bool isLeaving = false;
        bool isAllContinue = true;
        var PlayerDecision = GameplayManager.Instance.EndGameDecision;
        foreach (var item in PlayerDecision)
        {
            Decision decision = (Decision)item;
            if (decision != Decision.NextGame)
            {
                isAllContinue = false;
            }

            if (decision == Decision.LeaveGame)
            {
                isLeaving = true;
            }
        }

        if (isLeaving)
        {
            Debug.Log("We are not gonna continue play this game");
            CloseGameClientRPC();
            return;
        }

        if (isAllContinue && NetworkManager.Singleton.IsServer)
        {
            bool isSinglePlayerWin = RequiredPlayerCount == 1 && EndGameState == EndGameState.Win;
            bool isMultiplayerNotDraw = RequiredPlayerCount > 1 && EndGameState != EndGameState.Draw;
            if (isSinglePlayerWin || isMultiplayerNotDraw)
            {
                GameplayManager.Instance.NextGame();
            }
            else
            {
                GameplayManager.Instance.RetryGame();
            }
        }
    }

    [ClientRpc]
    private void CloseGameClientRPC() //this called when game is end
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}
