using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace snClient
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
            string text = CreateRandomString(100);
            Console.WriteLine("Client Started");
            clientSocket.Connect("192.168.1.5", 8888);
            byte[] eos = new byte[6] { 5, 6, 1, 100, 1, 123 };

            NetworkStream serverStream = clientSocket.GetStream();
            MemoryStream outstrm = new MemoryStream();
            outstrm.Write(System.Text.Encoding.ASCII.GetBytes(text), 0, text.Length);
            outstrm.Write(eos, 0, 6);
            Console.WriteLine("Sending:" + outstrm.Length);
            serverStream.Write(outstrm.ToArray(), 0, (int)outstrm.Length);
            serverStream.Flush();

            MemoryStream responseStrm = new MemoryStream();
            int bytesread = 0;
            byte[] last_bytes = new byte[6];
            byte[] bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];
            do
            {

                bytesread = serverStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                responseStrm.Write(bytesFrom, 0, bytesread);
                bytesFrom = new byte[(int)clientSocket.ReceiveBufferSize];

                responseStrm.Seek((int)responseStrm.Length - 6, SeekOrigin.Begin);
                responseStrm.Read(last_bytes, 0, 6);
                responseStrm.Seek(0, SeekOrigin.End);

            } while (
                    last_bytes[0] != 5   ||
                    last_bytes[1] != 6   ||
                    last_bytes[2] != 1   ||
                    last_bytes[3] != 100 ||
                    last_bytes[4] != 1   ||
                    last_bytes[5] != 123
                );

            string returndata = System.Text.Encoding.ASCII.GetString(responseStrm.ToArray());
            Console.WriteLine(returndata);
            Console.ReadLine();
        }
        private static string CreateRandomString(int length)
        {
            Random r = new Random();
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = (byte)r.Next(48, 122);
            }
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
