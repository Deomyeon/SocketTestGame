using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject p1;

    public GameObject p2;
    public GameObject p3;
    public GameObject p4;

    Server server;
    Client client;


    Queue<string>[] queues = new Queue<string>[3];

    private void Awake()
    {
        for (int i = 0; i < 3; i++)
        {
            queues[i] = new Queue<string>();
        }

        switch (GameManager.instance.job)
        {
            case Job.Client:

                client = new Client();
                client.SetAddress(GameManager.instance.address);
                client.SetPort(GameManager.instance.port);

                client.Connect();

                p1.SetActive(true);
                p2.SetActive(false);
                p3.SetActive(false);
                p4.SetActive(false);

                client.OnReceiveComplete = () =>
                {
                    Data receiveData = JsonUtility.FromJson<Data>(client.receiveText);
                    switch (receiveData.protocol)
                    {
                        case "Active":
                            ActiveData playerActive = JsonUtility.FromJson<ActiveData>(receiveData.data);
                            p1.SetActive(playerActive.p1Active);
                            p2.SetActive(playerActive.p2Active);
                            p3.SetActive(playerActive.p3Active);
                            p4.SetActive(playerActive.p4Active);
                            break;
                        case "Position":
                            PositionData playerPosition = JsonUtility.FromJson<PositionData>(receiveData.data);
                            p1.transform.position = playerPosition.p1Position;
                            p2.transform.position = playerPosition.p2Position;
                            p3.transform.position = playerPosition.p3Position;
                            p4.transform.position = playerPosition.p4Position;
                            break;
                        case "Rotation":
                            RotationData playerRotation = JsonUtility.FromJson<RotationData>(receiveData.data);
                            p1.transform.rotation = playerRotation.p1Rotation;
                            p2.transform.rotation = playerRotation.p2Rotation;
                            p3.transform.rotation = playerRotation.p3Rotation;
                            p4.transform.rotation = playerRotation.p4Rotation;
                            break;
                    }
                };

                client.OnConnectFailed = () =>
                {
                    client.Connect();
                };

                
                break;

            case Job.Server:
                server = new Server(3);
                server.SetPort(GameManager.instance.port);
                server.Start();

                p1.SetActive(true);
                p2.SetActive(false);
                p3.SetActive(false);
                p4.SetActive(false);

                server.OnReceiveComplete = id =>
                {
                    queues[id].Enqueue(server.receiveTextArray[id]);
                };

                server.OnAcceptComplete = id =>
                {
                    switch (id)
                    {
                        case 0:
                            p2.SetActive(true);
                            break;
                        case 1:
                            p3.SetActive(true);
                            break;
                        case 2:
                            p4.SetActive(true);
                            break;
                    }

                    Data sendData = new Data("Active", JsonUtility.ToJson(new ActiveData(p1.activeSelf, p2.activeSelf, p3.activeSelf, p4.activeSelf)));
                    for (int i = 0; i < 3; i++)
                    {
                        if (server.clientConnected[i])
                        {
                            server.Send(i, JsonUtility.ToJson(sendData));
                        }
                    }

                };

                break;
        }
    }

    private void Update()
    {
        switch (GameManager.instance.job)
        {
            case Job.Client:

                Vector3 movePos = Vector3.zero;

                if (Input.GetKey(KeyCode.A))
                {
                    movePos = (Vector3.left * 10);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    movePos = (Vector3.right * 10);
                }

                if (movePos != Vector3.zero)
                {
                    Data moveData = new Data("Move", JsonUtility.ToJson(new MoveData(movePos)));
                    client.Send(JsonUtility.ToJson(moveData));
                }

                break;
            case Job.Server:

                if (Input.GetKey(KeyCode.A))
                {
                    p1.GetComponent<Rigidbody>().velocity = (Vector3.left * 10);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    p1.GetComponent<Rigidbody>().velocity = (Vector3.right * 10);
                }

                for (int i = 0; i < 3; i++)
                {
                    if(!server.clientConnected[i])
                    {
                        switch (i)
                        {
                            case 0:
                                p2.SetActive(false);
                                break;
                            case 1:
                                p3.SetActive(false);
                                break;
                            case 2:
                                p4.SetActive(false);
                                break;
                        }
                    }

                    if (queues[i].Count != 0)
                    {
                        Data receiveData = JsonUtility.FromJson<Data>(queues[i].Dequeue());
                        switch (receiveData.protocol)
                        {
                            case "Move":
                                MoveData moveData = JsonUtility.FromJson<MoveData>(receiveData.data);
                                switch (i)
                                {
                                    case 0:
                                        p2.transform.GetComponent<Rigidbody>().velocity = moveData.movePos;
                                        break;
                                    case 1:
                                        p3.transform.GetComponent<Rigidbody>().velocity = moveData.movePos;
                                        break;
                                    case 2:
                                        p4.transform.GetComponent<Rigidbody>().velocity = moveData.movePos;
                                        break;
                                }
                                break;
                        }
                    }
                }

                Data sendData = new Data("Active", JsonUtility.ToJson(new ActiveData(p1.activeSelf, p2.activeSelf, p3.activeSelf, p4.activeSelf)));
                for (int i = 0; i < 3; i++)
                {
                    if (server.clientConnected[i])
                    {
                        server.Send(i, JsonUtility.ToJson(sendData));
                    }
                }

                sendData = new Data("Position", JsonUtility.ToJson(new PositionData(p1.transform.position, p2.transform.position, p3.transform.position, p4.transform.position)));
                for (int i = 0; i < 3; i++)
                {
                    if (server.clientConnected[i])
                    {
                        server.Send(i, JsonUtility.ToJson(sendData));
                    }
                }

                sendData = new Data("Rotation", JsonUtility.ToJson(new RotationData(p1.transform.rotation, p2.transform.rotation, p3.transform.rotation, p4.transform.rotation)));
                for (int i = 0; i < 3; i++)
                {
                    if (server.clientConnected[i])
                    {
                        server.Send(i, JsonUtility.ToJson(sendData));
                    }
                }

                break;
        }
    }
}
