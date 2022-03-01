using Models;
using System;
using System.Net.NetworkInformation;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace ISeeYou
{
    class Program
    {
        static Server server; // сервер
        static Thread listenThread; // потока для прослушивания

        static void Main(string[] args)
        {
            try
            {
                //Console.Write("Введите IP адрес, на котором желаете открыть сервер (Пример: 192.168.0.108): ");
                //string host = Console.ReadLine();

                //if (!RightIp(host))
                //{
                //    Console.ReadLine();
                //    return;
                //}

                string host = "127.0.0.1"; // host и номер порта проверять 

                Console.Write("Введите номер порта: ");
                int port = int.Parse(Console.ReadLine());
                //if (!PortEnabled(host, port))
                if (!CheckPortAvailable(port))
                {
                    Console.ReadLine();
                    return;
                }


                server = new Server(host,port);
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        private static bool RightIp(string ip)
        {
            try
            {
                Console.WriteLine("Загрузка...");
                IPAddress testIP = IPAddress.Parse(ip);
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(testIP);

                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine("Соединение установлено.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Не удалось установить соединение, проверьте введённые данные.");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.Write("При попытке установить соединение что-то пошло не так. Проверьте введённые данные.");
                return false;
            }
        }

        private static bool PortEnabled(string ip, int port)
        {
            try
            {
                Console.WriteLine("Обработка...");
                IPAddress testIP = IPAddress.Parse(ip);
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(testIP);

                using (TcpClient Scan = new TcpClient())
                {
                    try
                    {
                        //foreach(string str in System.IO.Ports.SerialPort.GetPortNames())
                        //{
                        //    if (int.Parse(str) == port)
                        //    {
                        //        System.IO.Ports.SerialPort Port;
                        //        Port = new System.IO.Ports.SerialPort(str);
                        //        Port.Open();
                        //        if (Port.IsOpen)
                        //        {
                        //            Console.WriteLine("Порт открыт и не задействован ни одним приложением или устройством.");
                        //            Port.Close();
                        //            return true;
                        //        }
                        //        else
                        //        {
                        //            Console.WriteLine("Порт уже используется другим приложением или устройством.");
                        //            Port.Close();
                        //            return false;
                        //        }
                        //    }
                        //}

                        Scan.Connect(reply.Address.ToString(), port);
                        Console.WriteLine($"[{port}] | PORT IS OPEN");
                        //Console.WriteLine($"[{port}] | PORT DOES NOT ACTIVATED.");
                        //Console.WriteLine($"ACTIVATION COMPLEATED.");
                        return true;
                    }
                    catch
                    {
                        Console.WriteLine($"[{port}] | PORT IS CLOSED");
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                Console.Write("Возникла ошибка при подкючении к порту. Проверьте введённые данные.");
                return false;
            }
        }

        private static bool CheckPortAvailable(int port)
        {
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = iPGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcp in tcpConnInfoArray)
            {
                if (tcp.LocalEndPoint.Port == port)
                {
                    Console.WriteLine("Данный порт недоступен. Возможно он уже используется.");
                    return false;
                }
            }
            Console.WriteLine($"[{port}] | PORT DOES NOT ACTIVATED OR IS NOT USED.");
            Console.WriteLine($"PORT ACTIVATION COMPLETED.");
            return true;
        }
    }
}
