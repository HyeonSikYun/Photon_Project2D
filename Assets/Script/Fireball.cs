using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Fireball : MonoBehaviourPunCallbacks
{
    public PhotonView photon;
    int dir;

    void Start() => Destroy(gameObject, 3.5f);

    void Update() => transform.Translate(Vector3.right * 7 * Time.deltaTime * dir);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag=="Ground")
        {
            photon.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }

        if(!photon.IsMine&&collision.tag=="Player"&&collision.GetComponent<PhotonView>().IsMine)
        {
            collision.GetComponent<Player>().Hit();
            photon.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DirRPC(int dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
