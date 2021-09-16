﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Cosmos.Network;
using kcp;

namespace Cosmos
{
    //================================================
    //1、ServerChannel启动后，接收并维护remote进入的连接;
    //2、当有请求进入并成功建立连接时，触发onConnected，分发参数分别为
    //NetworkChannelKey以及建立连接的conv;
    //3、当请求断开连接，触发onDisconnected，分发NetworkChannelKey以及
    //断开连接的conv;
    //4、已连接对象发来数据时，触发onReceiveData，分发NetworkChannelKey
    //以及发送来数据的conv;
    //================================================
    /// <summary>
    /// / KCP服务端通道；
    /// </summary>
    public class KCPServerChannel : INetworkChannel
    {
        string ip;
        ushort port;
        KcpServerService kcpServerService;
        Action<int> onConnected;
        Action<int> onDisconnected;
        Action<int, byte[]> onReceiveData;

        public event Action<int> OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        public event Action<int> OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }
        public event Action<int, byte[]> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }

        public bool IsConnect { get { return kcpServerService.Server.IsActive(); } }
        public NetworkChannelKey NetworkChannelKey { get; private set; }
        public KCPServerChannel(string channelName, string ip, ushort port)
        {
            NetworkChannelKey = new NetworkChannelKey(channelName, $"{ip}:{port}");
            KCPLog.Info = (s) => Utility.Debug.LogInfo(s);
            KCPLog.Warning = (s) => Utility.Debug.LogInfo(s, MessageColor.YELLOW);
            KCPLog.Error = (s) => Utility.Debug.LogError(s);
            this.ip = ip;
            this.port = port;
        }
        /// <summary>
        /// 服务端启动服务器；
        /// </summary>
        public void Connect()
        {
            kcpServerService = new KcpServerService();
            kcpServerService.Port = port;
            kcpServerService.ServiceSetup();
            kcpServerService.ServiceUnpause();

            kcpServerService.OnServerDataReceived += OnReceiveDataHandler;
            kcpServerService.OnServerDisconnected += OnDisconnectedHandler;
            kcpServerService.OnServerConnected += OnConnectedHandler;
            kcpServerService.ServiceConnect();
        }
        public void AbortChannel()
        {
            kcpServerService?.ServicePause();
            kcpServerService.OnServerDataReceived -= OnReceiveDataHandler;
            kcpServerService.OnServerDisconnected -= OnDisconnectedHandler;
            kcpServerService.OnServerConnected -= OnConnectedHandler;
            kcpServerService?.ServerServiceStop();
        }
        public void TickRefresh()
        {
            kcpServerService?.ServiceTick();
        }
        /// <summary>
        /// 与已经连接的connectionId断开连接；
        /// </summary>
        /// <param name="connectionId">连接Id</param>
        public void Disconnect(int connectionId)
        {
            kcpServerService?.ServiceDisconnect(connectionId);
        }
        public void SendMessage(NetworkReliableType reliableType, byte[] data, int connectionId)
        {
            var segment = new ArraySegment<byte>(data);
            var byteType = (byte)reliableType;
            kcpServerService?.ServiceSend((KcpChannel)byteType, segment, connectionId);
        }
        void OnDisconnectedHandler(int conv)
        {
            onDisconnected?.Invoke(conv);
        }
        void OnConnectedHandler(int conv)
        {
            onConnected?.Invoke(conv);
        }
        void OnReceiveDataHandler(int conv, ArraySegment<byte> arrSeg, int Channel)
        {
            var rcvLen = arrSeg.Count;
            var rcvData = new byte[rcvLen];
            Array.Copy(arrSeg.Array, 1, rcvData, 0, rcvLen);
            onReceiveData?.Invoke(conv, rcvData);
        }
    }
}