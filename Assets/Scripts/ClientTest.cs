using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientTest : MonoBehaviour
{
    public ScrollRect clientList;

    [Space]
    [Space]
    public Button newClient;

    [Space]
    [Space]
    public InputField addressInput;
    public InputField portInput;

    [Space]
    [Space]
    public Button connect;

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


    public string address;
    public ushort port;

    public string sendText;

    public int selectedSocket;

    public List<Client> clients;

    private void Awake()
    {
        clients = new List<Client>();
        addressInput.onValueChanged.AddListener(text =>
        {
            address = text;
        });
        portInput.onValueChanged.AddListener(text =>
        {
            try
            {
                port = ushort.Parse(text);
            }
            catch(System.Exception e)
            {
                print(e.Message);
            }
        });
        sendInput.onValueChanged.AddListener(text =>
        {
            sendText = text;
        });
    }

    public void NewClient()
    {
        GameObject obj = GameObject.Instantiate(socket, clientList.content);
        obj.transform.localPosition = new Vector2(50, clientList.content.childCount * -150);
        clientList.content.sizeDelta = new Vector2(0, 150 * (clientList.content.childCount + 1));

        int idx = clientList.content.childCount - 1;

        obj.transform.GetChild(0).GetComponent<Text>().text = $"Socket{idx}";
        obj.transform.GetChild(1).GetComponent<Text>().text = "";
        obj.GetComponent<Button>().onClick.AddListener(()=>SelectSocket(obj));

        clients.Add(new Client());

        clients[idx].OnConnected = () => { print("연결 되었습니다."); };
        clients[idx].OnSendComplete = () => { print("전송했습니다."); };
        clients[idx].OnReceiveComplete = () =>
        {
            print("받았습니다. " + clients[idx].receiveText);
        };

    }

    public void SelectSocket(GameObject obj)
    {
        selectSocket.text = obj.transform.GetChild(0).GetComponent<Text>().text;
        selectedSocket = int.Parse(selectSocket.text.Substring(6));
        if (clients[selectedSocket].isConnected)
        {
        }
        else
        {
            connect.transform.GetChild(0).GetComponent<Text>().text = "Connect";
            connect.onClick.RemoveAllListeners();
            connect.onClick.AddListener(Connect);
        }
    }

    public void Connect()
    {
        clients[selectedSocket].SetAddress(address);
        clients[selectedSocket].SetPort(port);
        clients[selectedSocket].Connect();
    }

    public void Send()
    {
        clients[selectedSocket].Send(sendText);
    }
}
