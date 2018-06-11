using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Linq;
using HoloToolkit.Unity;
using System.Collections.Generic;

#if UNITY_EDITOR
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif
#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif

/// <summary>
/// UDPCommunication tries to receive UDP packets
/// The script can run as part of an UWP app running on HoloLens.
/// It can also run with Holographic Remoting on a remote system that is running the Unity editor.
/// </summary>
public class UDPCommunication : Singleton<UDPCommunication>
{
  
    public static readonly Queue<string> Messages = new Queue<string>();

    public int myPort = 12345;
    private bool running = true;

#if UNITY_EDITOR
    static UdpClient udp;
    Thread thread;
    static readonly object lockObject = new object();

    string returnData = "";
    bool processData = false;

    // Use this for initialization
    void Start()
    {

        udp = new UdpClient(myPort);
        thread = new Thread(new ThreadStart(ThreadMethod));
        thread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //OnUDPButton();
        if (processData)
        {
            lock (lockObject)
            {
                processData = false;

                Messages.Enqueue(returnData.ToString());

                Debug.Log("Received: " + returnData);
                returnData = "";
            }
        }
    }

    private void OnApplicationQuit()
    {
        udp.Close();
        thread.Abort();
    }

    private void ThreadMethod()
    {
        while (true)
        {
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            byte[] receiveBytes = udp.Receive(ref RemoteIpEndPoint);

            lock (lockObject)
            {
                returnData = Encoding.ASCII.GetString(receiveBytes);
                processData = true;
            }
        }
    }
#endif


#if !UNITY_EDITOR
    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    DatagramSocket socket;

    // use this for initialization
    async void Start()
    {

        Debug.Log("Waiting for a connection...");

        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;

        HostName IP = null;
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
            .SingleOrDefault(
                hn =>
                    hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                    == icp.NetworkAdapter.NetworkAdapterId);

            await socket.BindEndpointAsync(IP, myPort.ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        Debug.Log("exit start");
    }

    // Update is called once per frame
    void Update()
    {
        while (ExecuteOnMainThread.Count > 0 )
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        Debug.Log("GOT MESSAGE: ");
        //Read the message that was received from the UDP echo client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        StreamReader reader = new StreamReader(streamIn);
        string message = await reader.ReadLineAsync();

        Debug.Log("Received: " + message);

        if (ExecuteOnMainThread.Count <5)
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                Messages.Enqueue(message.ToString());
            });
        }
    }
#endif
    public void OnUDPButton()
    {
        if (!running)
        {
            running = true;
         //   initListenerThread();
        }
        else
        {
            running = false;
       //     udpListeningThread = null;
       //     receivingUdpClient = null;
        }
    }
}
