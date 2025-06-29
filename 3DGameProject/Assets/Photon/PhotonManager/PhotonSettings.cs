using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonSettings : MonoBehaviourPunCallbacks
{
    [Header("Photon Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private string appId = "your-photon-app-id";
    [SerializeField] private bool autoConnect = true;
    [SerializeField] private bool useNameServer = true;

    [Header("Room Settings")]
    [SerializeField] private int maxPlayersPerRoom = 4;
    [SerializeField] private bool isVisible = true;
    [SerializeField] private bool isOpen = true;

    [Header("Player Settings")]
    [SerializeField] private string playerName = "Player";

    void Start()
    {
        // Photon 설정
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        // 플레이어 이름 설정
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName + Random.Range(1000, 9999);
        }

        // 자동 연결
        if (autoConnect)
        {
            ConnectToPhoton();
        }
    }

    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Already connected to Photon");
            return;
        }

        Debug.Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void CreateRoom(string roomName = null)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Not connected to Photon");
            return;
        }

        string roomNameToUse = string.IsNullOrEmpty(roomName) ?
            "Room_" + Random.Range(1000, 9999) : roomName;

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = isVisible,
            IsOpen = isOpen
        };

        PhotonNetwork.CreateRoom(roomNameToUse, roomOptions);
        Debug.Log($"Creating room: {roomNameToUse}");
    }

    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Not connected to Photon");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
        Debug.Log($"Joining room: {roomName}");
    }

    public void JoinRandomRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("Not connected to Photon");
            return;
        }

        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Joining random room");
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving room");
        }
    }

    // Photon 콜백들
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected from Photon: {cause}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}");
    }

    //public override void OnJoinRandomRoomFailed(short returnCode, string message)
    //{
    //    Debug.LogError($"Failed to join random room: {message}");
    //}

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player joined: {newPlayer.NickName}");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"Player left: {otherPlayer.NickName}");
    }

    // 공개 메서드들
    public bool IsConnected => PhotonNetwork.IsConnected;
    public bool IsInRoom => PhotonNetwork.InRoom;
    public string CurrentRoomName => PhotonNetwork.CurrentRoom?.Name;
    public int PlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
    public int MaxPlayers => PhotonNetwork.CurrentRoom?.MaxPlayers ?? 0;

    // 설정 변경 메서드들
    public void SetGameVersion(string version)
    {
        gameVersion = version;
        PhotonNetwork.GameVersion = gameVersion;
    }

    public void SetPlayerName(string name)
    {
        playerName = name;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName + Random.Range(1000, 9999);
        }
    }

    public void SetMaxPlayersPerRoom(int maxPlayers)
    {
        maxPlayersPerRoom = maxPlayers;
    }
}