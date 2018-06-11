using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

#if !UNITY_EDITOR && UNITY_METRO
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq;
#endif


public class FrameReceiver : MonoBehaviour {

    private static string TAG = "FrameReceiver";

    [Tooltip("The IPv4 Address of PC.")]
    public string serverIP;

    [Tooltip("The connection port to use.")]
    public int connectionPort = 11000;
    
    private float timeToDeferFailedConnections = 10.0f;

    private enum FrameReceiverStatus {
        ToDefer,
        Defered,
        WaitingForConnection,
        ToStart,
        Looping
    };
    private FrameReceiverStatus frameReceiverStatus = FrameReceiverStatus.ToStart;

    public Material frameMaterial;
    private Texture2D textureToLoad;
    public Texture2D textureDefault;


    public Image indicator;
    public Text renderFPS;
    public Text videoFPS;

    private static Queue<byte[]> qTextureData = new Queue<byte[]>();

    #region FPS Calculator
    private static Queue<long> qRenderTick = new Queue<long>();    
    private static Queue<long> qVideoTick = new Queue<long>();

    private static void RenderTick() {
        while (qRenderTick.Count > 49) {
            qRenderTick.Dequeue();
        }
        qRenderTick.Enqueue(DateTime.Now.Ticks);
    }

    private static float GetRenderDeltaTime() {
        if (qRenderTick.Count == 0) {
            return float.PositiveInfinity;
        }
        return (DateTime.Now.Ticks - qRenderTick.Peek()) / 500000.0f;
    }

    private static void VideoTick() {
        while (qVideoTick.Count > 49) {
            qVideoTick.Dequeue();
        }
        qVideoTick.Enqueue(DateTime.Now.Ticks);
    }

    private static float GetVideoDeltaTime() {
        if (qVideoTick.Count == 0) {
            return float.PositiveInfinity;
        }
        return (DateTime.Now.Ticks - qVideoTick.Peek()) / 500000.0f;
    }
    #endregion
    
    public void Start() {
        serverIP = serverIP.Trim();
        textureToLoad = new Texture2D(2, 2);
        frameMaterial.mainTexture = textureDefault;
    }


    public void Update() {
        switch (frameReceiverStatus) {
            case FrameReceiverStatus.ToStart:
                indicator.color = Color.red;
                RequestAndGetData();
                break;
            case FrameReceiverStatus.ToDefer:
                indicator.color = Color.red;
                frameReceiverStatus = FrameReceiverStatus.Defered;
                Invoke("RequestAndGetData", timeToDeferFailedConnections);
                break;
            case FrameReceiverStatus.Defered:
                break;
            case FrameReceiverStatus.WaitingForConnection:
                break;
            case FrameReceiverStatus.Looping:
                indicator.color = Color.green;
                frameMaterial.mainTexture = textureToLoad;
                break;
            default:
                break;
        }
        if (qTextureData.Count > 0) {
            textureToLoad.LoadImage(qTextureData.Dequeue());
            textureToLoad.Apply();
            VideoTick();
        }

        RenderTick();
        var renderDeltaTime = GetRenderDeltaTime();
        if (renderFPS != null) {
            renderFPS.text = string.Format("Render: {0:0.0} ms ({1:0.} fps)", renderDeltaTime, 1000.0f / renderDeltaTime);
        }
        var videoDeltaTime = GetVideoDeltaTime();
        if (videoFPS != null) {
            videoFPS.text = string.Format("Video:   {0:0.0} ms ({1:0.} fps)", videoDeltaTime, 1000.0f / videoDeltaTime);
        }
    }



#if !UNITY_EDITOR && UNITY_METRO

    private StreamSocket networkConnection;
    

    /// <summary>
    /// Connects to the server and requests data.
    /// </summary>
    private void ConnectListener() {
        if (frameReceiverStatus == FrameReceiverStatus.WaitingForConnection 
            || frameReceiverStatus == FrameReceiverStatus.ToDefer
            || frameReceiverStatus == FrameReceiverStatus.Looping) {
            Debug.Log(TAG + ": ConnectListener() not supported at current status");
            return;
        }
        
        frameReceiverStatus = FrameReceiverStatus.WaitingForConnection;
        Debug.Log(TAG + ": Connecting to " + serverIP);
        HostName networkHost = new HostName(serverIP);
        networkConnection = new StreamSocket();

        IAsyncAction outstandingAction = networkConnection.ConnectAsync(networkHost, connectionPort.ToString());
        AsyncActionCompletedHandler aach = new AsyncActionCompletedHandler(RcvNetworkConnectedHandler);
        outstandingAction.Completed = aach;
    }

    /// <summary>
    /// Requests data from the server and handles getting the data and firing
    /// the dataReadyEvent.
    /// </summary>
    private void RequestAndGetData() {
        ConnectListener();
    }

    /// <summary>
    /// When a connection to the server is established and we can start reading the data, this will be called.
    /// </summary>
    /// <param name="asyncInfo">Info about the connection.</param>
    /// <param name="status">Status of the connection</param>
    private void RcvNetworkConnectedHandler(IAsyncAction asyncInfo, AsyncStatus status) {
        try {
            // Status completed is successful.
            if (status == AsyncStatus.Completed) {
                DataReader networkDataReader;
                DataReaderLoadOperation drlo;

                // Since we are connected, we can read the data being sent to us.
                using (networkDataReader = new DataReader(networkConnection.InputStream)) {
                    // read four bytes to get the size.
                    
                    frameReceiverStatus = FrameReceiverStatus.Looping;

                    while (true) {

                        drlo = networkDataReader.LoadAsync(4);
                        while (drlo.Status == AsyncStatus.Started) {
                            // just waiting.
                        }

                        int dataSize = networkDataReader.ReadInt32();
                        if (dataSize < 0) {
                            Debug.Log(TAG + ": Super bad super big datasize");
                        }

                        // Need to allocate a new buffer with the dataSize.
                        byte[] mostRecentDataBuffer = new byte[dataSize];

                        // Read the data.
                        drlo = networkDataReader.LoadAsync((uint)dataSize);
                        while (drlo.Status == AsyncStatus.Started) {
                            // just waiting.
                        }
                        networkDataReader.ReadBytes(mostRecentDataBuffer);
                        Debug.Log(TAG + ": Image received with " + dataSize.ToString() + " bytes");
                        
                        qTextureData.Enqueue(mostRecentDataBuffer);
                        if (qTextureData.Count > 1) {
                            qTextureData.Dequeue();
                        }
                    }
                }
                networkConnection.Dispose();
                frameReceiverStatus = FrameReceiverStatus.ToStart;
            }
            else {
                Debug.Log(TAG + ": Failed to establish connection for rcv. Error Code:\n" + asyncInfo.ErrorCode);
                // In the failure case we'll requeue the data and wait before trying again.
                networkConnection.Dispose();
                frameReceiverStatus = FrameReceiverStatus.ToDefer;
            }

        }
        catch(Exception e) {
            networkConnection.Dispose();
            frameReceiverStatus = FrameReceiverStatus.ToDefer;
            Debug.Log(TAG + ": error caught, network disposed, waiting for new connection\n" + e);
        }
    }

    
    /// <summary>
    /// The change of IP address is only loaded when the connection is lost and reconnect again.
    /// </summary>
    private void ReadIPAddress() {
        try {
            using (Stream stream = OpenFileForRead(ApplicationData.Current.RoamingFolder.Path, "ipaddress.txt")) {
                // Read the file and deserialize the meshes.
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                serverIP = Encoding.ASCII.GetString(data);
                Debug.Log(TAG + ": ServerIP loaded, new IP address is " + serverIP);
            }
        }
        catch (Exception e) {
            Debug.Log(TAG + ": Error thrown when reading IP address.\n" + e);
        }
    }

    private static Stream OpenFileForRead(string folderName, string fileName) {
        Stream stream = null;
        Task task = new Task(
            async () => {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderName);
                StorageFile file = await folder.GetFileAsync(fileName);
                stream = await file.OpenStreamForReadAsync();
            });
        task.Start();
        task.Wait();
        return stream;
    }

#else
    /// <summary>
    /// Function place holder, not actually used
    /// </summary>
    private void RequestAndGetData() {
        ;
    }

#endif
}

