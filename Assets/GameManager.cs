using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public enum Job
{
    Server,
    Client
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Job job;

    public string address;
    public ushort port;

    private void Awake()
    {
        instance = this;
    }
}
