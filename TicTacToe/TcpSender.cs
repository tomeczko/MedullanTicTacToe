using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace TicTacToeClient
{
    class TcpSender
    {
        private TcpClient tcpClient;

        public string sendCommand( string _ip, int _port, string _message )
        {
            tcpClient = new System.Net.Sockets.TcpClient();
            try
            {
                tcpClient.Connect(_ip, _port);
                System.Text.UTF8Encoding encoder = new UTF8Encoding();
                NetworkStream stream = tcpClient.GetStream();
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(_message);
                stream.Write(data, 0, data.Length);

                // Now we have to parse incoming data
                string incomingString = "";
                byte[] readBuffer = new byte[1024];
                while (incomingString.Length < 4 || incomingString.Substring(incomingString.Length - 3, 3) != "EOL") // End of list
                {
                    int numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                    incomingString += System.Text.Encoding.UTF8.GetString(readBuffer, 0, numberOfBytesRead);
                }
                return incomingString;

            }
            catch (SocketException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {

            }
            return ""; // Return empty string if anything goes wrong...
        }
    }
}
