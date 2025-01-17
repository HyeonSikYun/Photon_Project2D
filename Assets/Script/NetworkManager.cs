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
    public TextMeshProUGUI countdownText; // 카운트다운 텍스트 추가

    // 대기실 캐릭터 스폰 위치
    public Transform waitingSpawnPoint1;  // Inspector에서 첫 번째 플레이어 대기 위치 지정
    public Transform waitingSpawnPoint2;  // Inspector에서 두 번째 플레이어 대기 위치 지정

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
        SpawnWaitingCharacter(); // 대기실 캐릭터 생성

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            StartCoroutine(StartGameWithDelay());
        }
    }

    // 대기실 캐릭터 생성
    void SpawnWaitingCharacter()
    {
        Transform spawnPoint = PhotonNetwork.CurrentRoom.PlayerCount == 1 ? waitingSpawnPoint1 : waitingSpawnPoint2;
        PhotonNetwork.Instantiate("WaitingPlayer", spawnPoint.position, Quaternion.identity); // WaitingPlayer는 대기실용 프리팹
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
        waitingText.text = $"대기 중인 플레이어: {PhotonNetwork.CurrentRoom.PlayerCount}/2";
    }

    IEnumerator StartGameWithDelay()
    {
        // 카운트다운 시작
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
        // 대기실 캐릭터 제거: 모든 클라이언트가 자신이 생성한 객체만 삭제
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("WaitingPlayer"))
        {
            PhotonView pv = obj.GetComponent<PhotonView>();

            // 자신이 생성한 객체만 삭제
            if (pv != null && pv.IsMine)
            {
                PhotonNetwork.Destroy(obj);
            }
        }

        // 모든 UI 비활성화
        lobbyPanel.SetActive(false);
        disconnectPanel.SetActive(false);
        respawnPanel.SetActive(false);
        countdownText.gameObject.SetActive(false);

        // 실제 게임 캐릭터 생성
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