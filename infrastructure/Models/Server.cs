using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace Models
{
    public class Server
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<Client> clients = new List<Client>(); // все подключённые к серверу пользователи
        IPAddress host;
        int port;

        public Server(string host, int port)
        {
            this.host = IPAddress.Parse(host);
            this.port = port;
        }

        // Добавляем пользователя в коллекццию подключений
        protected internal void AddConnection(Client client) 
        {
            clients.Add(client);
        }

        // Удаляем пользователя из коллекцции подключений
        public void RemoveConnection(string id)
        {
            Client client = clients.FirstOrDefault(c => c.Id == id);  // получаем клиента по его уникальному id
            if (client != null)              // Если нашли такого польователя - удаляем его из списка подключений
                clients.Remove(client);
        }

        // прослушивание входящих подключений
        public void Listen()
        {
            //---------------------------------------------------- Проблема --------------------------------------------------------
            try
            {
                //tcpListener = new TcpListener(IPAddress.Any, 139);
                //tcpListener = new TcpListener(IPAddress.Any, 8888);  // Тут указываем ip адрес и номер порта
                //tcpListener = new TcpListener(host, port);
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Client client = new Client(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(client.Process));  // Для обработки, на стороне каждого пользователя запускаем свой поток
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Была обнаружена ошибка: {ex.Message}");
                Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        public void BroadcastMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);  // Преобразуем string в набор байтов
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }

        // отключение всех клиентов
        public void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиентов
            }
            Environment.Exit(0); //завершение процесса
        }
    }
}
