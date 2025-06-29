using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text playerCountText;

    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Game Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private int maxPlayersPerRoom = 4;

    private bool isConnecting = false;

    void Start()
    {
        // UI 초기화
        mainMenuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);

        // 버튼 이벤트 연결
        connectButton.onClick.AddListener(ConnectToPhoton);
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);

        // Photon 설정
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        UpdateStatus("메인 메뉴");
    }

    public void ConnectToPhoton()
    {
        if (isConnecting) return;

        isConnecting = true;
        UpdateStatus("Photon 서버에 연결 중...");

        if (PhotonNetwork.IsConnected)
        {
            OnConnectedToMaster();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnected) return;

        string roomName = string.IsNullOrEmpty(roomNameInput.text) ?
            "Room_" + Random.Range(1000, 9999) : roomNameInput.text;

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions);
        UpdateStatus("방 생성 중...");
    }

    public void JoinRoom()
    {
        if (!PhotonNetwork.IsConnected) return;

        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            UpdateStatus("방 이름을 입력하세요!");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
        UpdateStatus("방 참가 중...");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // Photon 콜백들
    public override void OnConnectedToMaster()
    {
        isConnecting = false;
        UpdateStatus("Photon 서버에 연결됨");
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        UpdateStatus($"연결 해제됨: {cause}");
        mainMenuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        UpdateStatus($"방 '{PhotonNetwork.CurrentRoom.Name}'에 참가함");
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);

        // 플레이어 스폰
        SpawnPlayer();

        UpdatePlayerCount();
    }

    public override void OnLeftRoom()
    {
        UpdateStatus("방을 나감");
        gamePanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateStatus($"{newPlayer.NickName}님이 참가함");
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateStatus($"{otherPlayer.NickName}님이 나감");
        UpdatePlayerCount();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"방 참가 실패: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"방 생성 실패: {message}");
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        // 스폰 포인트 선택
        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        // 플레이어 생성
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        // 로컬 플레이어 설정
        if (player.GetComponent<PhotonView>().IsMine)
        {
            SetupLocalPlayer(player);
        }
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    private void SetupLocalPlayer(GameObject player)
    {
        // 카메라 설정
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }

        // 플레이어 컨트롤러 활성화
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // 무기 컨트롤러 활성화
        WeaponController weaponController = player.GetComponent<WeaponController>();
        if (weaponController != null)
        {
            weaponController.enabled = true;
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[PhotonManager] {message}");
    }

    private void UpdatePlayerCount()
    {
        if (playerCountText != null && PhotonNetwork.CurrentRoom != null)
        {
            playerCountText.text = $"플레이어: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }
    }
}