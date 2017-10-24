using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp1
{
    class Program
    {
        static object lockObj = new object();

        static void zad4()
        {
            ThreadPool.QueueUserWorkItem(Server);
            ThreadPool.QueueUserWorkItem(Client);
            ThreadPool.QueueUserWorkItem(Client);
            while (true) ;
        }
        static void zad5(int dlugosc, int fragment)
        {
            int[] tab = new int[dlugosc];
            Random rnd = new Random();
            for (int i=0;i<tab.Length;i++)
            {
                tab[i] = rnd.Next(0, 100);
                Console.WriteLine(tab[i]);
            }

            Console.ReadKey();
            ThreadPool.QueueUserWorkItem(sumuj, new object[] { tab });
            ThreadPool.QueueUserWorkItem(sumuj, new object[] { tab });
            ThreadPool.QueueUserWorkItem(sumuj, new object[] { tab });
            Thread.Sleep(5000);
            Console.WriteLine(tab[0]);
        }

        static void Main(string[] args)
        {
            zad5(100,100);

        }

        static void Client(Object stateInfo)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2048));
            while (true)
            {
                string wiadomosc = Console.ReadLine();
                byte[] message = new ASCIIEncoding().GetBytes(wiadomosc);
                client.GetStream().Write(message, 0, message.Length);
                writeConsoleMessage(wiadomosc, ConsoleColor.Green);
                if (wiadomosc == "END") break;
            }
            client.Close();
        }
        static void Server(Object stateInfo)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 2048);
            server.Start();
            while (true)
            {
                writeConsoleMessage("Waiting for a connection... ", ConsoleColor.Red);
                TcpClient c = server.AcceptTcpClient();
                writeConsoleMessage("Connected! ", ConsoleColor.Red);
                ThreadPool.QueueUserWorkItem(ClientServer, new object[] { c });
            }
        }
        static void ClientServer(Object stateInfo)
        {
            TcpClient c = (TcpClient)((object[])stateInfo)[0];
            while (true)
            {
                byte[] buffer = new byte[1024];
                c.GetStream().Read(buffer, 0, 1024);
                string result = Encoding.UTF8.GetString(buffer);
                result = result.Split((char)0)[0];
                writeConsoleMessage(("Otrzymałem wiadomość: " + result), ConsoleColor.Red);
                if (result == "END") break;
            }
        }

        static void writeConsoleMessage(string message, ConsoleColor color)
        {
            lock (lockObj)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
        static void sumuj(Object stateInfo)
        {
            while(true)
            lock (lockObj)
            {
                int[] tab = (int[])((object[])stateInfo)[0];
                if (tab[1] == -1) break;
                Console.WriteLine("{0}+{1}={2}",tab[0],tab[1],tab[0]+tab[1]);
                tab[0] += tab[1];
                for (int i=1;i<tab.Length-i;i++)
                {
                    tab[i] = tab[i + 1];
                    tab[i + 1] = -1;
                }
            }
        }
    }
}