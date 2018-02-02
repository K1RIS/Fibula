using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Photon.PunBehaviour
{
    public static GameManager instance;

    public static GameObject localPlayer;

    public InputField nickInput;

    void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        instance = this;

        PhotonNetwork.automaticallySyncScene = true;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("Fibula v1.0");
    }

    public void JoinGame()
    {
        RoomOptions ro = new RoomOptions()
        {
            MaxPlayers = 20
        };
        PhotonNetwork.JoinOrCreateRoom("Firstera", ro, null);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel("Rookgard");
        }
    }
    
    void OnLevelWasLoaded(int levelNumber)
    {
        if (!PhotonNetwork.inRoom) return;

        localPlayer = PhotonNetwork.Instantiate("Player", new Vector3(5.5f, 0.5f, 5.5f), Quaternion.identity, 0);
        localPlayer.GetComponent<Player>().photonView.RPC("changeNick", PhotonTargets.All, nickInput.text);
    }
}
