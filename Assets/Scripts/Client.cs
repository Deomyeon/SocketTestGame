using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Client
{
    Socket socket;

    IPAddress address;
    ushort port;


    public delegate void ClientEvent();

    public ClientEvent OnConnected;
    public ClientEvent OnConnectFailed;
    public ClientEvent OnSendComplete;
    public ClientEvent OnReceiveComplete;

    Thread receiveThread;

    int bufferSize;

    public string receiveText;

    byte[] buffer;

    public bool isConnected;


    public Client(int bufferSize = 1024)
    {
        this.bufferSize = bufferSize;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        buffer = new byte[bufferSize];
    }

    public void SetBufferSize(int bufferSize)
    {
        this.bufferSize = bufferSize;
        buffer = new byte[bufferSize];
    }

    public void SetAddress(string address)
    {
        this.address = IPAddress.Parse(address);
    }

    public void SetPort(ushort port)
    {
        this.port = port;
    }

    public void Connect()
    {
        if (socket.Connected)
            return;

        try
        {
            System.IAsyncResult result = socket.BeginConnect(new IPEndPoint(address, port), r =>
            {
                if (!r.CompletedSynchronously)
                {
                    isConnected = true;
                    if (OnConnected != null)
                    {
                        OnConnected();
                    }
                    receiveThread = new Thread(Receive);
                    receiveThread.Start();
                }
                else
                {
                    if (OnConnectFailed != null)
                    {
                        OnConnectFailed();
                    }
                }
            }, null);
            socket.EndConnect(result);
        }
        catch(System.Exception e)
        {
            
        }
    }

    public void Send(string sendText)
    {
        if (!socket.Connected)
            return;

        byte[] buffer = Encoding.UTF8.GetBytes(sendText);

        try
        {
            System.IAsyncResult result = socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, r =>
            {
                if(!r.CompletedSynchronously)
                {
                    if (OnSendComplete != null)
                    {
                        OnSendComplete();
                    }
                }
            }, null);
            socket.EndSend(result);
        }
        catch (System.Exception e)
        {

        }
    }

    void Receive()
    {
        while (socket.Connected)
        {
            try
            {
                System.IAsyncResult result = socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, r =>
                {
                    if (!r.CompletedSynchronously)
                    {
                        receiveText = Encoding.UTF8.GetString(buffer);
                        System.Array.Clear(buffer, 0, buffer.Length);
                        if (OnReceiveComplete != null)
                        {
                            OnReceiveComplete();
                        }
                        receiveText = string.Empty;
                    }
                }, null);
                socket.EndReceive(result);
            }
            catch (System.Exception e)
            {

            }
        }
    }
}
