/*
*  Copyright (c) 2008 Jonathan Wagner
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace DarkstarSharp
{
    public class SimpleClient : FilterListener
    {
        private TcpClient socketConnection = null;
        private NetworkStream outStream = null;

        private const int MAX_READ = 8192;
        private byte[] byteBuffer = new byte[MAX_READ];
        private Dictionary<BigInteger, ClientChannel> channels = new Dictionary<BigInteger, ClientChannel>();
        private Dictionary<BigInteger, ClientChannelListener> channelListeners = new Dictionary<BigInteger, ClientChannelListener>();
        private SimpleClientListener listener = null;

        private MessageFilter messageFilter;

        public SimpleClient(SimpleClientListener listener)
        {
            this.listener = listener;
            this.messageFilter = new MessageFilter( this );
        }

        public void login(string host, int port) 
        {
            PasswordAuthentication auth = listener.GetPasswordAuthentication();

            if (null != auth)
            {
                try
                {
                    socketConnection = new TcpClient();
                    socketConnection.BeginConnect(host, port, new AsyncCallback(OnConnect), auth);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else
            {
                throw new Exception("Password Authentication was null");
            }
        }

        public void Logout(bool force)
        {
            WriteMessage( MessageFilter.FormatLogoutRequest() );
        }

        public void WriteMessage(byte[] message)
        {
            MemoryStream m = new MemoryStream();
            m.Position = 0;
            BinaryWriter writer = new BinaryWriter(m);
            writer.Write(Converter.GetBigEndian((ushort)message.Length));
            writer.Write(message);
            writer.Flush();

            if (socketConnection.Connected)
            {
                NetworkStream stream = socketConnection.GetStream();
                byte[] payload = m.ToArray();
                outStream.BeginWrite(payload, 0, payload.Length, new AsyncCallback(OnWrite), null);
            }
        }

        void OnWrite(IAsyncResult r)
        {
            try { 
                outStream.EndWrite(r); 
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        public void OnConnect(IAsyncResult asr)
        {
            PasswordAuthentication auth = asr.AsyncState as PasswordAuthentication;
            outStream = socketConnection.GetStream();
            socketConnection.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
            WriteMessage(MessageFilter.FormatLoginRequest( auth.Username, auth.Password ));
        }

        public void OnRead(IAsyncResult asr)
        {
            try
            {
                int bytesRead = socketConnection.GetStream().EndRead(asr);
                if (bytesRead < 1)
                {
                    socketConnection.Close();
                    listener.Disconnected(true, "Connection was closed by the server");
                    return;
                }
                else
                {
                    messageFilter.Receive( byteBuffer, bytesRead );
                    socketConnection.GetStream().BeginRead(byteBuffer, 0, MAX_READ, new AsyncCallback(OnRead), null);
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void SessionSend(byte[] bytes) 
        {
            WriteMessage( MessageFilter.FormatSessionMessage(bytes) );
        }

        public void OnLoginSuccess(byte[] reconnectKey)
        {
            listener.LoggedIn(reconnectKey);
        }

        public void OnLoginFailure(string failureMessage)
        {
            listener.LoginFailed(failureMessage);
        
        }
        public void OnLogoutSuccess() 
        {
            listener.Disconnected(false, "Logout successful");
        }
        public void OnSessionMessage(byte[] payload) 
        {
            listener.ReceivedMessage(payload);
        }

        public void OnChannelJoin(string channelName, byte[] channelId) 
        {
            ClientChannel channel = new ClientChannel(channelName, channelId, this);
            BigInteger key = new BigInteger(channel.ChannelId);
            channels.Add(key, channel);
            ClientChannelListener cList = listener.JoinedChannel(channel);
            channelListeners.Add(key, cList);
        }

        public void OnChannelLeave(byte[] channelId) 
        { 
            BigInteger key = new BigInteger(channelId);
            channelListeners[key].LeftChannel(channels[key]);
            channels.Remove(key);
            channelListeners.Remove(key);
        }

        public void OnChannelMessage(byte[] channelId, byte[] payload) 
        {
            BigInteger key = new BigInteger(channelId);
            channelListeners[key].ReceivedMessage(channels[key], payload);
        }
    }
}
