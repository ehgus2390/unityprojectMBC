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
        // UI �ʱ�ȭ
        mainMenuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);

        // ��ư �̺�Ʈ ����
        connectButton.onClick.AddListener(ConnectToPhoton);
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);

        // Photon ����
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        UpdateStatus("���� �޴�");
    }

    public void ConnectToPhoton()
    {
        if (isConnecting) return;

        isConnecting = true;
        UpdateStatus("Photon ������ ���� ��...");

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
        UpdateStatus("�� ���� ��...");
    }

    public void JoinRoom()
    {
        if (!PhotonNetwork.IsConnected) return;

        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            UpdateStatus("�� �̸��� �Է��ϼ���!");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
        UpdateStatus("�� ���� ��...");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // Photon �ݹ��
    public override void OnConnectedToMaster()
    {
        isConnecting = false;
        UpdateStatus("Photon ������ �����");
        mainMenuPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        UpdateStatus($"���� ������: {cause}");
        mainMenuPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        UpdateStatus($"�� '{PhotonNetwork.CurrentRoom.Name}'�� ������");
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);

        // �÷��̾� ����
        SpawnPlayer();

        UpdatePlayerCount();
    }

    public override void OnLeftRoom()
    {
        UpdateStatus("���� ����");
        gamePanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateStatus($"{newPlayer.NickName}���� ������");
        UpdatePlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateStatus($"{otherPlayer.NickName}���� ����");
        UpdatePlayerCount();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"�� ���� ����: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"�� ���� ����: {message}");
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        // ���� ����Ʈ ����
        Transform spawnPoint = GetRandomSpawnPoint();
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        // �÷��̾� ����
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        // ���� �÷��̾� ����
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
        // ī�޶� ����
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }

        // �÷��̾� ��Ʈ�ѷ� Ȱ��ȭ
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // ���� ��Ʈ�ѷ� Ȱ��ȭ
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
            playerCountText.text = $"�÷��̾�: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }
    }
}