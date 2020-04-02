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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DarkstarSharp
{
    public class MessageFilter
    {
        private MemoryStream memStream;
        private BinaryReader reader;
        private FilterListener listener;

        public MessageFilter( FilterListener listener )
        {
            this.listener = listener;
            memStream = new MemoryStream();
            reader = new BinaryReader(memStream);
        }

        public void Receive(byte[] bytes, int length )
        {
            memStream.Seek(0, SeekOrigin.End);
            memStream.Write(bytes, 0, length);
            //Reset to beginning
            memStream.Seek(0, SeekOrigin.Begin);
            while (  RemainingBytes() > 2 )
            {
                ushort messageLen = Converter.GetBigEndian(reader.ReadUInt16());
                if (RemainingBytes() >= messageLen)
                {
                    MemoryStream messageStream = new MemoryStream();
                    BinaryWriter messageWriter = new BinaryWriter(messageStream);
                    messageWriter.Write(reader.ReadBytes(messageLen));
                    messageStream.Seek(0, SeekOrigin.Begin);
                    //listener.ReceiveMessage(SgsMessage.parseRaw(messageStream));
                    parseRaw(messageStream);
                }
                else
                {
                    //Back up the position two bytes
                    memStream.Position = memStream.Position - 2;
                    break;
                }
            }
            //Create a new stream with any leftover bytes
            byte[] leftover = reader.ReadBytes((int)RemainingBytes());
            //Clear
            memStream.SetLength(0);
            memStream.Write(leftover, 0, leftover.Length);
        }

        private long RemainingBytes()
        {
            return memStream.Length - memStream.Position;
        }

        private void parseRaw(MemoryStream rawSgsMessage)
        {
            BinaryReader reader = new BinaryReader(rawSgsMessage);
            SimpleSgsProtocol.OpCode opCode =
                   (SimpleSgsProtocol.OpCode)(Enum.ToObject(typeof(SimpleSgsProtocol.OpCode), reader.ReadByte()));

            if( opCode == SimpleSgsProtocol.OpCode.LOGIN_SUCCESS ) {
                ParseLoginSuccess( rawSgsMessage );
            }
            else if ( opCode == SimpleSgsProtocol.OpCode.LOGIN_FAILURE ) {
                ParseLoginFailure( rawSgsMessage );
            }
            else if ( opCode == SimpleSgsProtocol.OpCode.SESSION_MESSAGE ) {
                ParseSessonMessage( rawSgsMessage );
            }
            else if ( opCode == SimpleSgsProtocol.OpCode.LOGOUT_SUCCESS ) {
                ParseLogoutSuccess( rawSgsMessage );
            }           
            else if ( opCode == SimpleSgsProtocol.OpCode.CHANNEL_JOIN ) {
                ParseChannelJoin( rawSgsMessage );
            }   
            else if ( opCode == SimpleSgsProtocol.OpCode.CHANNEL_LEAVE ) {
                ParseChannelLeave( rawSgsMessage );
            }   
            else if ( opCode == SimpleSgsProtocol.OpCode.CHANNEL_MESSAGE ) {
                ParseChannelMessage( rawSgsMessage );
            } 
            else 
            {
                throw new Exception("CRITICAL ERROR UNKNOWN OPCODE" + opCode );
            }      
        }

        /************************************************************************************
         *                                    P A R S E R S
         ************************************************************************************/

        private void ParseLoginSuccess(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);
            byte[] reconnectKey = reader.ReadBytes((int)(ms.Length - ms.Position));
            listener.OnLoginSuccess(reconnectKey);
        }

        private void ParseLoginFailure(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);
            string failureMessage = fromSgsString(reader.ReadBytes((int)(ms.Length - ms.Position)));
            listener.OnLoginFailure(failureMessage);
        }

        private void ParseLogoutSuccess(MemoryStream ms)
        {
            listener.OnLogoutSuccess();
        }

        private void ParseSessonMessage(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);
            listener.OnSessionMessage(reader.ReadBytes((int)(ms.Length - ms.Position)));
        }

        private void ParseChannelJoin(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);
            ushort nameLen = ReadShort(reader);
            byte[] nameBytes = reader.ReadBytes(nameLen);
            string name = Encoding.UTF8.GetString(nameBytes, 0, nameBytes.Length);
            byte[] idBytes = reader.ReadBytes((int)(ms.Length - ms.Position));
            listener.OnChannelJoin(name, idBytes);
        }

        private void ParseChannelLeave(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);
            listener.OnChannelLeave(reader.ReadBytes((int)(ms.Length - ms.Position)));
        }

        private void ParseChannelMessage(MemoryStream ms)
        {
            BinaryReader reader = new BinaryReader(ms);
            byte[] rawId = reader.ReadBytes(ReadShort(reader));
            byte[] payload = reader.ReadBytes((int)(ms.Length - ms.Position));
            listener.OnChannelMessage(rawId, payload);
        }

        /************************************************************************************
         *                                  F O R M A T T E R S
         ************************************************************************************/

        public static byte[] FormatLoginRequest(string username, string passwd)
        {
            MemoryStream m = new MemoryStream();
            m.Seek(0, SeekOrigin.Begin);
            BinaryWriter writer = new BinaryWriter(m);
            writer.Write((byte)SimpleSgsProtocol.OpCode.LOGIN_REQUEST);
            writer.Write(SimpleSgsProtocol.VERSION);
            writer.Write(toSgsString(username));
            writer.Write(toSgsString(passwd));
            return m.ToArray();
        }

        public static byte[] FormatLogoutRequest()
        {
            byte[] b = new byte[1];
            b[0] = (byte)SimpleSgsProtocol.OpCode.LOGOUT_REQUEST;
            return b;
        }

        public static byte[] FormatSessionMessage(byte[] payload)
        {
            MemoryStream m = new MemoryStream();
            m.Seek(0, SeekOrigin.Begin);
            BinaryWriter writer = new BinaryWriter(m);
            writer.Write((byte)SimpleSgsProtocol.OpCode.SESSION_MESSAGE);
            writer.Write(payload);
            return m.ToArray();
        }

        public static byte[] FormatChannelMessage(byte[] channelId, byte[] payload)
        {
            MemoryStream m = new MemoryStream();
            m.Seek(0, SeekOrigin.Begin);
            BinaryWriter writer = new BinaryWriter(m);
            writer.Write((byte)SimpleSgsProtocol.OpCode.CHANNEL_MESSAGE);
            WriteShort(writer, (ushort)channelId.Length);
            writer.Write(channelId);
            writer.Write(payload);
            return m.ToArray();
        }

        public static byte[] toSgsString(string s)
        {
            byte[] theString = Encoding.UTF8.GetBytes(s);
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(Converter.GetBigEndian((ushort)theString.Length));
            writer.Write(theString);
            return ms.ToArray();
        }

        public static String fromSgsString(byte[] b)
        {
            MemoryStream ms = new MemoryStream(b);
            BinaryReader reader = new BinaryReader(ms);
            ushort len = ReadShort(reader);
            byte[] bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static String ReadSgsString(BinaryReader reader)
        {
            ushort len = ReadShort(reader);
            byte[] bytes = reader.ReadBytes(len);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static void WriteSgsString(BinaryWriter writer, string s)
        {
            byte[] theString = Encoding.UTF8.GetBytes(s);
            writer.Write(Converter.GetBigEndian((ushort)theString.Length));
            writer.Write(theString);
        }

        public static ushort ReadShort(BinaryReader reader)
        {
            return Converter.GetBigEndian(reader.ReadUInt16());
        }

        public static void WriteShort(BinaryWriter writer, ushort s)
        {
            writer.Write(Converter.GetBigEndian(s));
        }

        public static int ReadInt(BinaryReader reader)
        {
            return Converter.GetBigEndian(reader.ReadInt32());
        }

        public static void WriteInt(BinaryWriter writer, int i)
        {
            writer.Write(Converter.GetBigEndian(i));
        }


        public static long ReadLong(BinaryReader reader)
        {
            return Converter.GetBigEndian(reader.ReadInt64());
        }

        public static void WriteLong(BinaryWriter writer, long l)
        {
            writer.Write(Converter.GetBigEndian(l));
        }

        public static void WriteFloat(BinaryWriter writer, float f)
        {
            writer.Write(Converter.GetBigEndian(f));
        }

        public static float ReadFloat(BinaryReader reader)
        {
            return (float)Converter.GetBigEndian(reader.ReadUInt32());
        }
    }
}
