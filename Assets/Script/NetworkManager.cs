using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField nickNameInput;
    public GameObject disconnectPanel;
    public GameObject lobbyPanel;
    public GameObject respawnPanel;
    public TextMeshProUGUI waitingText;
    public TextMeshProUGUI countdownText; // ī��Ʈ�ٿ� �ؽ�Ʈ �߰�

    // ���� ĳ���� ���� ��ġ
    public Transform waitingSpawnPoint1;  // Inspector���� ù ��° �÷��̾� ��� ��ġ ����
    public Transform waitingSpawnPoint2;  // Inspector���� �� ��° �÷��̾� ��� ��ġ ����

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        disconnectPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        respawnPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
        PhotonNetwork.JoinOrCreateRoom("WaitingRoom", new RoomOptions { MaxPlayers = 2 }, null);
    }

    public override void OnJoinedRoom()
    {
        disconnectPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        UpdateWaitingText();
        SpawnWaitingCharacter(); // ���� ĳ���� ����

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartCoroutine(StartGameWithDelay());
        }
    }

    // ���� ĳ���� ����
    void SpawnWaitingCharacter()
    {
        Transform spawnPoint = PhotonNetwork.CurrentRoom.PlayerCount == 1 ? waitingSpawnPoint1 : waitingSpawnPoint2;
        PhotonNetwork.Instantiate("WaitingPlayer", spawnPoint.position, Quaternion.identity); // WaitingPlayer�� ���ǿ� ������
    }



    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        UpdateWaitingText();

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartCoroutine(StartGameWithDelay());
        }
    }

    private void UpdateWaitingText()
    {
        waitingText.text = $"��� ���� �÷��̾�: {PhotonNetwork.CurrentRoom.PlayerCount}/2";
    }

    IEnumerator StartGameWithDelay()
    {
        // ī��Ʈ�ٿ� ����
        countdownText.gameObject.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownText.text = "Game Start!";
        yield return new WaitForSeconds(1f);

        StartGame();
    }

    private void StartGame()
    {
        // ���� ĳ���� ����: ��� Ŭ���̾�Ʈ�� �ڽ��� ������ ��ü�� ����
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("WaitingPlayer"))
        {
            PhotonView pv = obj.GetComponent<PhotonView>();

            // �ڽ��� ������ ��ü�� ����
            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(obj);
            }
        }

        // ��� UI ��Ȱ��ȭ
        lobbyPanel.SetActive(false);
        disconnectPanel.SetActive(false);
        respawnPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);

        // ���� ���� ĳ���� ����
        Spawn();
    }



    public void Spawn()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-12f, 9f), 3, 0), Quaternion.identity);
        respawnPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        disconnectPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        respawnPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);
    }
}