using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Server
{
    public class ChatServer
    {
        List<ChatSocket> clientList = new List<ChatSocket>();

        public static void Main(String[] args)
        {
            ChatServer chatServer = new ChatServer();
            chatServer.run();
        }

        public void run()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, ChatSetting.port);

            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            newsock.Bind(ipep);
            newsock.Listen(10);

            while (true)
            {
                Socket socket = newsock.Accept();
                Console.WriteLine("接受一個新連線!");
                ChatSocket client = new ChatSocket(socket);
                try
                {
                    clientList.Add(client);
                    client.newListener(processMsgComeIn);
                }
                catch
                {
                }
//                clientList.Remove(client);
            }
            //	  newsock.Close();
        }

        public byte[] processMsgComeIn(byte[] msg)
        {
            Packet packet = new Packet(msg);
            switch (packet.getCommand())
            {
                case Packet.Commands.ReportName:
                    string name = packet.getReportNameData();
                    Console.WriteLine("收到使用者：" + name);
                    break;
                case Packet.Commands.TextMessage:
                    int chatroomIndex = packet.getChatroomIndex();
                    packet.changeChatroomIndex(2);
                    break;
            }

            
            return new byte[1024];
        }

        public void broadCast(byte[] msg)
        {
            Console.WriteLine("廣播訊息給 " + msg+" 線上使用者共"+clientList.Count+"個人!");
            foreach (ChatSocket client in clientList)
            {
				if (!client.isDead) {
					Console.WriteLine("Send to "+client.remoteEndPoint.ToString()+":"+msg);
					client.send(msg);
				}
            }
        }
    }
}
