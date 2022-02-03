using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public static ConnectToServer Instance;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(this);
        }
        else
        {
            PhotonNetwork.Disconnect();
            Instance = this;
        }
    }

    /*
    public void LoadScene(string scene)
    {
        LoadingText.text = "Loading";
        LoadingScreen.SetActive(true);
        StartCoroutine(SetSceneLoadProgress(SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single)));
    }
    public IEnumerator SetSceneLoadProgress(AsyncOperation sceneLoading)
    {
        while (!sceneLoading.isDone)
        {
            Bar.ChangeFill(Mathf.RoundToInt(sceneLoading.progress));
            yield return null;
        }

        LoadingScreen.SetActive(false);
    }
    */

    public void Connect()
    {
        PhotonNetwork.NickName = "Ghest";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("CreateJoinLobby");
    }

    public void DesktopQuit()
    {
        PhotonNetwork.LeaveLobby();
        Application.Quit();
    }
}
