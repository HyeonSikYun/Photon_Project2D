using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Cinemachine;

public class Player : MonoBehaviourPunCallbacks,IPunObservable
{
    public Rigidbody2D rigid;
    public Animator animator;
    public SpriteRenderer sprite;
    public PhotonView photon;
    public TextMeshProUGUI nickname;
    public Image healthImage;

    bool isGround;
    Vector3 curPos;

    void Awake()
    {
        nickname.text = photon.IsMine ? PhotonNetwork.NickName : photon.Owner.NickName;
        nickname.color=photon.IsMine?Color.green:Color.red;

        if(photon.IsMine)
        {
            var CM=GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }
    }

    void Update()
    {
        if (photon.IsMine)
        {
            float axis = Input.GetAxisRaw("Horizontal");
            rigid.velocity = new Vector2(4 * axis, rigid.velocity.y);

            if(axis!=0)
            {
                animator.SetBool("walk", true);
                photon.RPC("FlipXRPC", RpcTarget.AllBuffered, axis);
            }
            else
            {
                animator.SetBool("walk", false);
            }

            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));
            animator.SetBool("jump", !isGround);
            if (Input.GetKeyDown(KeyCode.UpArrow) && isGround) photon.RPC("JumpRPC", RpcTarget.All);


            if(Input.GetKeyDown(KeyCode.Space))
            {
                PhotonNetwork.Instantiate("fireball", transform.position + new Vector3(sprite.flipX ? -0.4f : 0.4f, -0.11f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, sprite.flipX ? -1 : 1);
                animator.SetTrigger("shot");
            }
        }

        else if((transform.position-curPos).sqrMagnitude>=100)
        {
            transform.position = curPos;
        }

        else
        {
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
        }
    }

    [PunRPC]
    void FlipXRPC(float axis) => sprite.flipX = axis == -1;

    [PunRPC]
    void JumpRPC()
    {
        rigid.velocity = Vector2.zero;
        rigid.AddForce(Vector2.up * 600);
    }

    public void Hit()
    {
        healthImage.fillAmount -= 0.1f;
        if(healthImage.fillAmount<=0)
        {
            GameObject.Find("Canvas").transform.Find("DeathPanel").gameObject.SetActive(true);
            photon.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(healthImage.fillAmount);
        }

        else
        {
            curPos=(Vector3)stream.ReceiveNext();
            healthImage.fillAmount=(float)stream.ReceiveNext();
        }
    }
}
