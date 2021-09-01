using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectJob : MonoBehaviour
{
    public InputField address;
    public InputField cl_port;
    public Button client;

    public InputField se_port;
    public Button server;

    private void Awake()
    {
        client.onClick.AddListener(() =>
        {
            GameManager.instance.address = address.text;
            GameManager.instance.port = ushort.Parse(cl_port.text);
            GameManager.instance.job = Job.Client;
            SceneManager.LoadScene("Game");
        });
        server.onClick.AddListener(() =>
        {
            GameManager.instance.port = ushort.Parse(se_port.text);
            GameManager.instance.job = Job.Server;
            SceneManager.LoadScene("Game");
        });
    }
}
