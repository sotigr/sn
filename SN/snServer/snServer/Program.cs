using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace snServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TcpListener serverSocket = new TcpListener(IPAddress.Any,8888);
                int requestCount = 0;

                serverSocket.Start();
                Console.WriteLine(" >> Server Started");

                Console.WriteLine(" >> Accept connection from client");
                requestCount = 0;

                while (true)
                {
                    TcpClient cs = serverSocket.AcceptTcpClient();

                    new Thread((clientSocket) =>
                    {

                        requestCount = requestCount + 1;
                        NetworkStream networkStream = ((TcpClient)clientSocket).GetStream();
                        byte[] bytesFrom = new byte[(int)((TcpClient)clientSocket).ReceiveBufferSize];
                        MemoryStream requestStrm = new MemoryStream();
                        int bytesread = 0;
                        byte[] last_bytes = new byte[6];
                        do
                        {

                            bytesread = networkStream.Read(bytesFrom, 0, (int)((TcpClient)clientSocket).ReceiveBufferSize);
                            requestStrm.Write(bytesFrom, 0, bytesread);
                            bytesFrom = new byte[(int)((TcpClient)clientSocket).ReceiveBufferSize];

                            requestStrm.Seek((int)requestStrm.Length - 6, SeekOrigin.Begin);
                            requestStrm.Read(last_bytes, 0, 6);
                            requestStrm.Seek(0, SeekOrigin.End);

                        } while (
                            last_bytes[0] != 5   ||
                            last_bytes[1] != 6   ||
                            last_bytes[2] != 1   ||
                            last_bytes[3] != 100 ||
                            last_bytes[4] != 1   ||
                            last_bytes[5] != 123
                        );

                        string dataFromClient = System.Text.Encoding.ASCII.GetString(requestStrm.ToArray());
                        Console.WriteLine(" >> Data from client - " + dataFromClient);


                        string serverResponse = "Last Message from client" + dataFromClient;

                        Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        Console.WriteLine(" >> " + serverResponse);
                        Console.WriteLine("Received: " + requestStrm.Length);


                        ((TcpClient)clientSocket).Close();


                    }).Start(cs);
                }
                serverSocket.Stop();
                Console.WriteLine(" >> exit");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.ReadLine();
        }
    }


}

