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
        List<string> userList = new List<string>();
        //List<ChatroomInfo> chatroomList= new List<ChatroomInfo>();
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
            newsock.Listen(1000);

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

        public void processMsgComeIn(byte[] msg, ChatSocket socket)
        {
            Packet packet = new Packet(msg);
            switch (packet.getCommand())
            {
                case Packet.Commands.ReportName:
                    string name = packet.getReportNameData();
                    Console.WriteLine("收到使用者：" + name);
                    Console.WriteLine(socket.socket.RemoteEndPoint.ToString());
                    userList.Insert(clientList.IndexOf(socket), name);
                    sendUserList();
                    break;

                case Packet.Commands.ChatRequest:
                    //chatroomList.Index.Add(packet.getChatroomIndex()); 
                    packet.getChatRequestData();
                    break;

               // case Packet.Commands.LeaveChatroom:
                  //  break;
                
                case Packet.Commands.TextMessage:
                    int chatroomIndex = packet.getChatroomIndex();
                    //packet.changeChatroomIndex(2);
                    break;
                case Packet.Commands.LogOut:
                    Console.WriteLine(userList[clientList.IndexOf(socket)] + "登出");
                    userList.RemoveAt(clientList.IndexOf(socket));
                    clientList.Remove(socket);
                    socket.close();
                    break;
            }
        }

        public void sendUserList()
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                List<string> exceptList = new List<string>();
                for (int j = 0; j < userList.Count; j++)
                {
                    if (i != j)
                        exceptList.Add(userList[j]);

                }
                Packet packet = new Packet();
                packet.makePacketUpdateUserList(exceptList);
               
                byte[] exceptListByte = packet.getPacket();
                clientList[i].send(exceptListByte);
            }
        }
        public void broadCast(byte[] msg, ChatSocket socket)
        {
            Console.WriteLine("廣播訊息給 " + msg+" 線上使用者共"+clientList.Count+"個人!");
            foreach (ChatSocket client in clientList)
            {
				if (!client.isDead && !client.Equals(socket)) {
					Console.WriteLine("Send to "+client.remoteEndPoint.ToString()+":"+msg);
					client.send(msg);
				}
            }
        }
    }
}
