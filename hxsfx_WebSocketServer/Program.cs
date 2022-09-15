using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace hxsfx_WebSocketServer
{
    class Program
    {
        //存储所有连接的socket对象
        static Dictionary<string, IWebSocketConnection> ip_scoket_Dic = new Dictionary<string, IWebSocketConnection>();
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("请至少输入三个参数（使用协议、监听ip和监听端口）");
                return;
            }
            //使用协议
            var webSocketProtocol = args[0].Trim();// "wss";
            if (webSocketProtocol != "wss" && webSocketProtocol != "ws")
            {
                Console.WriteLine("使用协议仅可选择：wss或ws。");
                return;
            }
            //监听ip
            var ListenIP = args[1].Trim();// "0.0.0.0";124.222.218.191
            if (!Regex.IsMatch(ListenIP, @"^(\d{1,3}\.){3}\d{1,3}$"))
            {
                Console.WriteLine("请输入格式正确的IP。");
                return;
            }
            //监听端口
            var ListenPort = args[2].Trim();// "5678"; 
            if (!int.TryParse(ListenPort, out int listenPortInt) || listenPortInt < 0 || listenPortInt > 65535)
            {
                Console.WriteLine("请输入在0至65535的端口号。");
                return;
            }
            //证书文件所在地址（使用IIS版本的证书）
            var pfxFilePath = args.Length > 3 ? args[3].Trim() : "";
            //证书密码
            var pfxPassword = args.Length > 4 ? args[4].Trim() : "";
            if (webSocketProtocol == "wss" && (string.IsNullOrEmpty(pfxFilePath) || string.IsNullOrEmpty(pfxPassword)))
            {
                Console.WriteLine("未输入证书文件及证书密码，协议识别为：ws。");
                webSocketProtocol = "ws";
            }
            //组合监听地址
            var loaction = webSocketProtocol + "://" + ListenIP + ":" + ListenPort;
            var webSocketServer = new WebSocketServer(loaction);
            if (loaction.StartsWith("wss://"))
            {
                webSocketServer.Certificate = new X509Certificate2(pfxFilePath, pfxPassword
               , X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet
                );
                webSocketServer.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            }//当为安全链接时，将证书信息写入链接

            //开始侦听
            webSocketServer.Start(socket =>
            {
                var socketConnectionInfo = socket.ConnectionInfo;
                var clientId = socketConnectionInfo.ClientIpAddress + ":" + socketConnectionInfo.ClientPort;
                socket.OnOpen = () =>
                {
                    if (!ip_scoket_Dic.ContainsKey(clientId))
                    {
                        ip_scoket_Dic.Add(clientId, socket);
                    }
                    Console.WriteLine(CustomSend("服务端", $"[{clientId}]加入"));
                };
                socket.OnClose = () =>
                {
                    if (ip_scoket_Dic.ContainsKey(clientId))
                    {
                        ip_scoket_Dic.Remove(clientId);
                    }
                    Console.WriteLine(CustomSend("服务端", $"[{clientId}]离开"));
                };
                socket.OnMessage = message =>
                {
                    //将发送过来的json字符串进行解析
                    var msgModel = JsonConvert.DeserializeObject<MsgModel>(message);
                    Console.WriteLine(CustomSend(clientId, msgModel.msg, clientId));
                };
            });
            //出错后进行重启  
            webSocketServer.RestartAfterListenError = true;
            Console.WriteLine("【开始监听】" + loaction);
            //服务端发送消息给客户端
            do
            {
                Console.WriteLine(CustomSend("服务端", Console.ReadLine()));
            } while (true);
        }
        /// <summary>
        /// 自定义发送消息给链接中的客户端
        /// </summary>
        /// <param name="nickName">昵称</param>
        /// <param name="msg">发送消息</param>
        /// <param name="filterClientIds">过滤客户端（不发送消息给这些客户端）</param>
        /// <returns>返回发送的消息JSON字符串格式</returns>
        private static string CustomSend(string nickName, string msg, params string[] filterClientIds)
        {
            //消息
            var data = JsonConvert.SerializeObject(new MsgModel
            {
                nickName = nickName,
                msg = msg,
                date = DateTime.Now.ToString("yyyy年MM月dd日"),
                time = DateTime.Now.ToString("HH:mm:ss")
            });
            //将消息发送给链接客户端（不包含过滤客户端）
            ip_scoket_Dic.ToList().ForEach(s =>
            {
                if (!filterClientIds.Contains(s.Key))
                {
                    s.Value.Send(data);
                }
            });
            //将消息输入调试窗口
            Debug.WriteLine(data);
            //将消息写入文本日志
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            string filename = Path.Combine(logDir, DateTime.Now.ToString("yyyyMMdd") + ".txt");
            File.AppendAllLines(filename, new List<string>() { data });

            return data;
        }

    }
    
}
