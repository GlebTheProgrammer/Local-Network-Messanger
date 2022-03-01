using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Client
    {
        protected internal string Id { get; private set; }  // Уникальный идентификатор пользователя в сети
        protected internal NetworkStream Stream { get; private set; }  // Для получения и отправки данных с помощью сокетов в .NET используется класс потоков NetworkStream
        string userName;
        TcpClient client;
        Server server; // объект сервера

        public Client(TcpClient tcpClient, Server server)
        {
            Id = Guid.NewGuid().ToString();  // Генерирем рандомный ID-шник с помощью библиотеки guid
            client = tcpClient;
            this.server = server;
            server.AddConnection(this);  // При создании нового объекта юзера, сразу же добавляем его в коллекцию подключений
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();  
                string message = GetMessage();  // получаем имя пользователя
                userName = message;

                // Эта проверка может работать некорректно, если что, if необходимо просто убрать, также и ниже


                //if (userName != "")
                //{
                //    message = userName + " присоединился к беседе.";
                //    server.BroadcastMessage(message, this.Id);  // посылаем сообщение о входе нового пользователя в чат всем подключенным пользователям
                //    Console.WriteLine(message);
                //}

                if (userName == "")
                    userName = "Охрана вашего соединения";

                message = userName + " присоединился к беседе.";
                server.BroadcastMessage(message, this.Id);  // посылаем сообщение о входе нового пользователя в чат всем подключенным пользователям
                Console.WriteLine(message);
                while (true)  // в бесконечном цикле получаем сообщения от клиента
                {
                    try
                    {
                        //if (userName != "")
                        //{
                        //    message = GetMessage();
                        //    message = string.Format($"{userName}: {message}");
                        //    Console.WriteLine(message);
                        //    server.BroadcastMessage(message, this.Id);
                        //}
                        message = GetMessage();
                        message = string.Format($"{userName}: {message}");
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        //if (userName != "")
                        //{
                        //    message = string.Format($"{userName}: покинул чат.");
                        //    Console.WriteLine(message);
                        //    server.BroadcastMessage(message, this.Id);
                        //    break;
                        //}
                        message = string.Format($"{userName}: покинул чат.");
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(this.Id);  // в случае выхода из цикла закрываем доступ со стороны вышедшего пользователя
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[128]; // буфер для получения данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения на стороне пользователя
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
