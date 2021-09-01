using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerTest : MonoBehaviour
{
    public ScrollRect clientList;

    [Space]
    [Space]
    public InputField portInput;

    [Space]
    [Space]
    public Button start;
    public Button stop;

    [Space]
    [Space]
    public Button broadcast;
    public Button kick;

    [Space]
    [Space]
    public InputField sendInput;
    public Button send;

    [Space]
    [Space]
    public Text selectSocket;

    [Space]
    [Space]
    [Space]
    [Space]
    public GameObject socket;

    Queue<int> initQueue;

    public ushort port;

    public string sendText;

    public int selectedSocket;

    public Server server;


    private void Awake()
    {
        server = new Server(4);
        initQueue = new Queue<int>();

        portInput.onValueChanged.AddListener(text =>
        {
            try
            {
                port = ushort.Parse(text);
            }
            catch (System.Exception e)
            {
                print(e.Message);
            }
        });
        sendInput.onValueChanged.AddListener(text =>
        {
            sendText = text;
        });
    }
    public void SelectSocket(GameObject obj)
    {
        selectSocket.text = obj.transform.GetChild(0).GetComponent<Text>().text;
        selectedSocket = int.Parse(selectSocket.text.Substring(6));
    }

    public void ServerStart()
    {
        server.SetPort(port);
        server.Start();

        server.OnStarted = () =>
        {
            print("열림");
        };
        server.OnAcceptComplete = (i) =>
        {
            print("입장하였습니다. " + i);

            initQueue.Enqueue(i);
        };
        server.OnReceiveComplete = (i) =>
        {
            print("받았습니다. " + i + " " + server.receiveTextArray[i]);
        };
        server.OnSendComplete = () =>
        {
            print("보냈습니다. " + selectedSocket);
        };
        server.OnClosed = () =>
        {
            print("껐어요.");
        };
    }

    public void Update()
    {
        if (initQueue.Count != 0)
        {
            // 조건해서 들어왔을때 만들도록.
            GameObject obj = GameObject.Instantiate(socket, clientList.content);
            obj.transform.localPosition = new Vector2(50, clientList.content.childCount * -150);
            clientList.content.sizeDelta = new Vector2(0, 150 * (clientList.content.childCount + 1));

            int idx = clientList.content.childCount - 1;

            obj.transform.GetChild(0).GetComponent<Text>().text = $"Socket{initQueue.Peek()}";
            obj.transform.GetChild(1).GetComponent<Text>().text = "";
            obj.GetComponent<Button>().onClick.AddListener(() => SelectSocket(obj));
            initQueue.Dequeue();
        }
    }

    public void Stop()
    {
        server.Close();
    }

    public void Send()
    {
        server.Send(selectedSocket, sendText);
    }

}
