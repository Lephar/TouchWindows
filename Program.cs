using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Touch
{
    static class Program
    {
        private const int LEFT_DOWN = 0x02;
        private const int LEFT_UP = 0x04;
        private const int RIGHT_DOWN = 0x08;
        private const int RIGHT_UP = 0x10;
        
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern void MouseEvent(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private static int ReadInt(NetworkStream stream)
        {
            int temp, index = 0;
            Byte[] data = new Byte[4];

            while (index < 4)
            {
                index += (temp = stream.Read(data, index, 4 - index));

                if (temp == 0)
                    return Int32.MinValue;
            }

            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);

            return BitConverter.ToInt32(data, 0);
        }

        static void Main()
        {
            int port = 10110;
            IPAddress ip = null, eth = null, wifi = null;

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                if (ni.Name.Equals("Ethernet"))
                    eth = ni.GetIPProperties().UnicastAddresses[1].Address;
                else if (ni.Name.Equals("Wi-Fi"))
                    wifi = ni.GetIPProperties().UnicastAddresses[1].Address;

            if (eth != null)
                ip = eth;
            else if (wifi != null)
                ip = wifi;
            else
                Application.Exit();

            TcpListener socket = new TcpListener(ip, port);
            TcpClient client = null;
            NetworkStream stream = null;
            int o, x, y, v;
            socket.Start();

            while (true)
            {
                if (client == null || stream == null)
                {
                    client = socket.AcceptTcpClient();
                    stream = client.GetStream();
                }

                o = ReadInt(stream);

                if (o == Int32.MinValue)
                {
                    stream = null;
                    client = null;
                    continue;
                }

                if (o == 0)
                {
                    x = ReadInt(stream);
                    y = ReadInt(stream);

                    if (x == Int32.MinValue || y == Int32.MinValue)
                    {
                        stream = null;
                        client = null;
                        continue;
                    }
                    
                    Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
                }

                if (o == 1)
                    MouseEvent(LEFT_DOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                if (o == 2)
                    MouseEvent(LEFT_UP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                if (o == 3)
                    MouseEvent(RIGHT_DOWN, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
                if (o == 4)
                    MouseEvent(RIGHT_UP, (uint)Cursor.Position.X, (uint)Cursor.Position.Y, 0, 0);
            }
        }
    }
}
