using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Board_Burning.Model
{
    public class Scanner
    {
        #region 私有
        private static string jsonFile = System.Windows.Forms.Application.StartupPath + "\\Scanner\\";
        Socket ConnectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Socket temp;
        IPAddress ServerIPAddress;
        IPEndPoint ServerIPEndPoint;
        string ip;
        int port;
        int timeout;
        #endregion

        #region 暴露
        [Browsable(false), Category("属性"), Description("实体名")]
        public string name { get; set; }
        [Browsable(true), Category("参数"), Description("扫码器IP")]
        public string IP { get { return ip; } set { ip = value; OnAttributeChanged(new AttributeChangedEventArgs()); } }
        [Browsable(true), Category("参数"), Description("扫码器端口")]
        public int Port { get { return port; } set { port = value; OnAttributeChanged(new AttributeChangedEventArgs()); } }
        [Browsable(true), Category("参数"), Description("扫码器通讯超时")]
        public int Timeout
        {
            get
            {
                timeout = ConnectSocket.ReceiveTimeout;               
                return timeout;
            }
            set
            {
                timeout = value;
                ConnectSocket.SendTimeout = timeout;
                ConnectSocket.ReceiveTimeout = timeout;               
                OnAttributeChanged(new AttributeChangedEventArgs());
            }
        }
        #endregion

        public Scanner()
        {
            AttributeChanged += SerializeObject;
        }
        public Scanner(string name)
        {
            this.name = name;
            AttributeChanged += SerializeObject;
        }

        public bool Connected()
        {
            return ConnectSocket.Connected;
        }

        #region 同步通信
        public void Connect()
        {
            try
            {
                if (IP == null || Port == 0) return;
                ServerIPAddress = IPAddress.Parse(IP);
                ServerIPEndPoint = new IPEndPoint(ServerIPAddress, Port);
                if (ServerIPEndPoint != null)
                {
                    ConnectSocket.Connect(ServerIPEndPoint);
                }
            }
            catch
            {
            }

        }

        public void Send(Byte[] ClientData)
        {
            ConnectSocket.Send(ClientData, 0, ClientData.Length, SocketFlags.None);
        }

        public Byte[] Receive()
        {
            Byte[] ClientData = { };
            ConnectSocket.Receive(ClientData, 0, ClientData.Length, SocketFlags.None);
            return ClientData;
        }

        #region MyRegion
        public void ServerStart()
        {
            try
            {
                if (IP == null || Port == 0) return;
                ServerIPAddress = IPAddress.Parse(IP);
                ServerIPEndPoint = new IPEndPoint(ServerIPAddress, Port);
                if (ServerIPEndPoint != null)
                {
                    ListenSocket.Bind(ServerIPEndPoint);
                    ListenSocket.Listen(0);
                    //
                    Task.Run(new Action(() =>
                    {
                        while(true)
                        {
                            temp = ListenSocket.Accept();   //为新建立的连接创建新的Socket
                            while (temp != null && !temp.Poll(0, SelectMode.SelectRead))//是否连接成功
                            {
                                Thread.Sleep(24);
                            }
                            Thread.Sleep(24);
                        }
                    }));
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void ServerSend(Byte[] ClientData)
        {
            if (temp != null && !temp.Poll(0, SelectMode.SelectRead))//是否连接成功
            {
                temp.Send(ClientData, 0, ClientData.Length, SocketFlags.None);
            }
            else
            {

            }
        }

        public Byte[] ServerReceive()
        {
            Byte[] ClientData = new byte[1024];
            if (temp != null && !temp.Poll(0, SelectMode.SelectRead))//是否连接成功
            {
                temp.Receive(ClientData, 0, ClientData.Length, SocketFlags.None);
            }
            else
            {

            }
            return ClientData;
        }
        #endregion

        public void Disconnect()
        {
            if (ConnectSocket.Connected)
            {
                ConnectSocket?.Disconnect(true);
            }
        }
        #endregion

        #region 异步通信
        public void BeginConnect()
        {
            ServerIPAddress = IPAddress.Parse(IP);
            ServerIPEndPoint = new IPEndPoint(ServerIPAddress, Port);
            object[] ConnectState = { ConnectSocket };
            ConnectSocket.BeginConnect(ServerIPEndPoint, ConnectCallBack, ConnectState);
        }

        private static void ConnectCallBack(IAsyncResult AsyncResult)
        {
            object[] ConnectState = (object[])AsyncResult.AsyncState;
            Socket ConnectSocket = (Socket)ConnectState[0];
            ConnectSocket.EndConnect(AsyncResult);
        }

        public void BeginSend(Byte[] ClientData)
        {
            object[] SendState = { ConnectSocket, ClientData };
            ConnectSocket.BeginSend(ClientData, 0, ClientData.Length, SocketFlags.None, SendCallBack, SendState);
        }

        private static void SendCallBack(IAsyncResult AsyncResult)
        {
            object[] SendState = (object[])AsyncResult.AsyncState;
            Socket ConnectSocket = (Socket)SendState[0];
            byte[] ClientData = (byte[])SendState[1];
            ConnectSocket.EndSend(AsyncResult);
        }

        public Byte[] BeginReceive()
        {
            Byte[] ClientData = { };
            object[] ReceiveState = { ConnectSocket, ClientData };
            ConnectSocket.BeginReceive(ClientData, 0, ClientData.Length, SocketFlags.None, ReceiveCallBack, ReceiveState);
            return ClientData;
        }

 
        private static void ReceiveCallBack(IAsyncResult AsyncResult)
        {
            object[] ReceiveState = (object[])AsyncResult.AsyncState;
            Socket ConnectSocket = (Socket)ReceiveState[0];
            byte[] ClientData = (byte[])ReceiveState[1];
            ConnectSocket.EndReceive(AsyncResult);
        }

        public void BeginDisconnect()
        {
            object[] DisconnectState = { ConnectSocket };
            ConnectSocket?.BeginDisconnect(true, DisconnectCallBack, DisconnectState);
        }

        private static void DisconnectCallBack(IAsyncResult AsyncResult)
        {
            object[] DisconnectState = (object[])AsyncResult.AsyncState;
            Socket ConnectSocket = (Socket)DisconnectState[0];
            ConnectSocket.EndReceive(AsyncResult);
        }
        #endregion



        #region 私有方法

        #endregion

        #region 自定义事件

        #region 曝光属性值修改事件
        public class AttributeChangedEventArgs : EventArgs
        {
            public AttributeChangedEventArgs()
            {
            }
        }
        /// <summary>
        /// 属性值更改时
        /// </summary>
        public event EventHandler<AttributeChangedEventArgs> AttributeChanged;
        protected virtual void OnAttributeChanged(AttributeChangedEventArgs e)
        {
            AttributeChanged?.Invoke(this, e);
        }
        #endregion


        #endregion

        #region 事件处理方法
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SerializeObject(object sender, AttributeChangedEventArgs e)
        {
            if (Directory.Exists(jsonFile) == false) //如果不存在就创建文件夹
            {
                Directory.CreateDirectory(jsonFile);
            }
            File.WriteAllText(jsonFile + this.name + ".json", JsonConvert.SerializeObject((sender as Scanner)));
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Scanner DeserializeObject()
        {
            try
            {
                return JsonConvert.DeserializeObject<Scanner>(File.ReadAllText(jsonFile + this.name + ".json"));
            }
            catch (Exception e)
            {
                return new Scanner(this.name);
            }
        }
        #endregion

        #region 静态方法
        #endregion
    }
}
