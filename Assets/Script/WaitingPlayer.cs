using UnityEngine;
using Photon.Pun;
using TMPro;

public class WaitingPlayer : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI nicknameText;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        // 닉네임 표시
        if (photonView.IsMine)
        {
            nicknameText.text = PhotonNetwork.NickName;
            nicknameText.color = Color.green;
        }
        else
        {
            nicknameText.text = photonView.Owner.NickName;
            nicknameText.color = Color.red;
        }
    }
}