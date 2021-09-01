using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Server
{
    ushort port;

    int bufferSize;

    Socket room;
    Thread[] threads;
    public string[] receiveTextArray;
    ClientSocket[] clientSockets;
    public bool[] clientConnected;
    bool isListening;

    Thread roomEnableThread;

    int clientCount;

    int roomSize;
    
    bool isClosed;


    public delegate void ServerEvent();
    public delegate void ClientCall(int idx);

    public ServerEvent OnStarted;
    public ClientCall OnAcceptComplete;
    public ServerEvent OnClosed;
    public ServerEvent OnSendComplete;
    public ClientCall OnReceiveComplete;


    public Server(int roomSize, int bufferSize = 1024)
    {
        this.roomSize = roomSize;
        this.bufferSize = bufferSize;
    }

    public void Start()
    {
        clientCount = 0;
        isClosed = false;
        isListening = false;

        room = null;
        threads = new Thread[roomSize];
        receiveTextArray = new string[roomSize];
        clientSockets = new ClientSocket[roomSize];
        clientConnected = new bool[roomSize];

        for (int i = 0; i < roomSize; i++)
        {
            clientSockets[i] = new ClientSocket(this, i);
            clientConnected[i] = false;
            threads[i] = new Thread(clientSockets[i].Accept);
        }

        if (OnStarted != null)
        {
            OnStarted();
        }

        roomEnableThread = new Thread(EnableRoom);
        roomEnableThread.Start();
    }

    public void SetroomSize(int roomSize)
    {
        this.roomSize = roomSize;
    }

    public void SetBufferSize(int bufferSize)
    {
        this.bufferSize = bufferSize;
    }

    public void SetPort(ushort port)
    {
        this.port = port;
    }

    void EnableRoom()
    {
        while(!isClosed)
        {
            for (int i = 0; i < roomSize; i++)
            {
                if (clientSockets[i].clientSocket != null)
                {
                    if (!clientSockets[i].clientSocket.Connected)
                    {
                        clientConnected[i] = false;
                        if (clientSockets[i].receiveThread.IsAlive)
                        {
                            clientSockets[i].receiveThread.Abort();
                        }
                        clientSockets[i].clientSocket.Close();
                        threads[i].Abort();
                        clientSockets[i] = new ClientSocket(this, i);
                        threads[i] = new Thread(clientSockets[i].Accept);
                        --clientCount;
                    }
                }
                if (!clientConnected[i] && !isListening)
                {
                    if (threads[i] != null)
                    {
                        if (!threads[i].IsAlive)
                        {
                            isListening = true;
                            threads[i].Start();
                        }
                    }
                }
            }
        }
    }

    public void Close()
    {
        isClosed = true;
        for (int i = 0; i < roomSize; i++)
        {
            if (clientSockets[i].receiveThread != null)
            {
                if (clientSockets[i].receiveThread.IsAlive)
                {
                    clientSockets[i].receiveThread.Abort();
                }
            }
            threads[i].Abort();
        }
        receiveTextArray = null;
        clientSockets = null;
        clientConnected = null;
        if (room != null)
        {
            room.Close();
        }
        if (OnClosed != null)
        {
            OnClosed();
        }
    }

    public void Send(int SocketIndex, string text)
    {
        clientSockets[SocketIndex].Send(text);
    }

    public class ClientSocket
    {
        Server server;

        public Socket clientSocket;
        public byte[] buffer;
        public int index;
        public Thread receiveThread;


        public ClientSocket(Server server, int index)
        {
            this.server = server;
            this.index = index;
            buffer = new byte[server.bufferSize];
        }

        public void Accept()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, server.port));
            socket.Listen(3);
            server.room = socket;
            System.IAsyncResult result = socket.BeginAccept(r => 
            {
                if (!r.CompletedSynchronously && !server.isClosed)
                {
                    if (server.OnAcceptComplete != null)
                    {
                        server.OnAcceptComplete(index);
                    }
                }
                else
                {
                    socket.Close();
                    server.room = null;
                    return;
                }
            }
            , null);
            clientSocket = socket.EndAccept(result);
            socket.Close();
            server.room = null;
            server.isListening = false;
            server.clientConnected[index] = true;
            ++server.clientCount;

            receiveThread = new Thread(Receive);
            receiveThread.Start();
        }

        public void Send(string sendText)
        {
            if (!clientSocket.Connected)
                return;

            byte[] buffer = Encoding.UTF8.GetBytes(sendText);

            try
            {
                System.IAsyncResult result = clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, r =>
                {
                    if (!r.CompletedSynchronously)
                    {
                        if (server.OnSendComplete != null)
                        {
                            server.OnSendComplete();
                        }
                    }
                }, null);
                clientSocket.EndSend(result);
            }
            catch (System.Exception e)
            {

            }
        }

        void Receive()
        {
            while (clientSocket.Connected)
            {
                try
                {
                    System.IAsyncResult result = clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, r =>
                    {
                        if (!r.CompletedSynchronously)
                        {
                            server.receiveTextArray[index] = Encoding.UTF8.GetString(buffer);
                            System.Array.Clear(buffer, 0, buffer.Length);
                            if (server.OnReceiveComplete != null)
                            {
                                server.OnReceiveComplete(index);
                            }
                            server.receiveTextArray[index] = string.Empty;
                        }
                    }, null);
                    clientSocket.EndReceive(result);
                }
                catch (System.Exception e)
                {

                }
            }
        }

    }
}