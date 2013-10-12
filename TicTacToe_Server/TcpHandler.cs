using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TicTacToe_Server
{
    internal class TcpHandler
    {
        TcpListener listener;

        internal TcpHandler(int portNumber)
        {
            listener = new TcpListener(IPAddress.Any, portNumber); // Hardcoded port number
            listener.Start();
            startAccept();
        }

        static string GetString(byte[] bytes) // Helper function
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        private void startAccept()
        {
            listener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, listener);
        }

        // Process the client connection. 
        internal void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            startAccept();
            TcpClient client = listener.EndAcceptTcpClient(ar);
            using (NetworkStream ns = client.GetStream())
            {
                // Now we have to parse incoming data
                byte[] readBuffer = new byte[1024];
                int numberOfBytesRead = ns.Read(readBuffer, 0, readBuffer.Length);

                MessageReceivedEventArgs mrea = new MessageReceivedEventArgs(System.Text.Encoding.UTF8.GetString(readBuffer));
                MessageReceived(ref mrea);

                string response = mrea.returnMsg;
                Byte[] replyData = System.Text.Encoding.ASCII.GetBytes(response);
                ns.Write(replyData, 0, replyData.Length);
                ns.Flush();
                ns.Close();
            }

            client.Close();
        }

        public delegate void OnMessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
        public event OnMessageReceivedEventHandler OnMessageReceived;

        public class MessageReceivedEventArgs : EventArgs
        {
            private string _value;
            public string message
            {
                get { return _value; }
                set { _value = value; }
            }
            public string returnMsg
            {
                get { return _value; }
                set { _value = value; }
            }

            public MessageReceivedEventArgs(string value)
            {
                _value = value;
            }
        }

        protected virtual void MessageReceived(ref MessageReceivedEventArgs e)
        {
            if (OnMessageReceived != null)
                OnMessageReceived(this, e);
        }
    }
}
