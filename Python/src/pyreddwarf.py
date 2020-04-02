'''
*    pyreddwarf simple python connector to RedDwarf game server
*    Copyright (C) 2010  Abdelkader Gouaich, gouaich@lirmm.fr
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <http://www.gnu.org/licenses/>.
'''

'''
Created on Aug 11, 2010

@author: gouaich
'''


from bytesarray import BytesArray
from reddwarfprotocol import *
import socket, asyncore

class RedDwarfSimpleClient(object):
    '''
    RedDwarfSimpleClient
    A simple client to connect to RedDwarf.
    host: host of the Reddwarf server.
    port: port of the Reddwarf server.
    '''
    
    def __init__(self, host, port, connect=True):
        '''
        Constructor
        '''
        self.host = host
        self.port = port
        self.reconnectKey = BytesArray('c')
        self.eventListeners = {}
        self.channels = {}
        self.channelIdByName = {}
        self._messageFilter = MessageFilter(self)
        self.socket = RedDwarfSocket(self,connect)
        self.handler = RedDwarfEventHandler(self)
        self.isLogged = False
        self.registerListener(self.handler.on_loginFailure,"on_loginFailure")
        self.registerListener(self.handler.on_loginSuccess,"on_loginSuccess")
        self.registerListener(self.handler.on_loginRedirect,"on_loginRedirect")
        self.registerListener(self.handler.on_loginSuccess,"on_loginSuccess")
        self.registerListener(self.handler.on_reconnectSuccess,"on_reconnectSuccess")
        self.registerListener(self.handler.on_reconnectFailure,"on_reconnectFailure")
        self.registerListener(self.handler.on_sessionMessage,"on_sessionMessage")
        self.registerListener(self.handler.on_channelJoin,"on_channelJoin")
        self.registerListener(self.handler.on_channelMessage,"on_channelMessage")
        self.registerListener(self.handler.on_channelLeave,"on_channelLeave")
        self.registerListener(self.handler.on_rawMessage,"on_rawMessage")
    """
    connect the socket to the server
    """
    def connect(self):
        self.socket.connect((self.host,self.port))
        self.socket.connected = True
    """
    register a call back function on a RedDwarf event
    """    
    def registerListener(self,listener,eventType):
        if ( self.eventListeners.has_key(eventType)):
            self.eventListeners[eventType].append(listener)
        else:
             self.eventListeners[eventType] = [listener]
    
    """
    Logs on the server with the given username and password
    """
    def login(self,username,password):
        buf = BytesArray('c')
        buf.append(chr(LOGIN_REQUEST_CODE))
        buf.append(chr(VERSION))
        buf.writeUTF(username)
        buf.writeUTF(password)
        self._writeBytesPrecededByLength(buf, self.socket)
    """
    Logout from the server
    """
    def logout(self,forceSocketClose):
        buf = BytesArray('c')
        buf.append(chr(LOGOUT_REQUEST_CODE))
        self._writeBytesPrecededByLength(buf, self.socket)
    """
    Send a session message to the server
    """
    def sessionSend(self,message):
        buf = BytesArray('c')
        buf.append(chr(SESSION_MESSAGE_CODE))
        buf.writeBytes(message)
        print "sessionSend"
        print buf
        self._writeBytesPrecededByLength(buf, self.socket)
    """
    send a message on a channel
    channel : the channel instance
    message: message to send
    """
    def channelSend(self,channel, message):
        buf = BytesArray('c')
        buf.append(chr(CHANNEL_MESSAGE_CODE))
        self._writeBytesPrecededByLength(channel.rawIdBytes(), buf)
        buf.writeBytes(message)
        self._writeBytesPrecededByLength(buf, self.socket)
    """
    get a channel handle using its unique Id
    """
    def getChannelWithID(self,id):
        self.channels[id]
        
    def _writeBytesPrecededByLength(self, bytes, buffer):
        str = bytes
        if (type(bytes) == BytesArray):
            str = bytes.tostring()
        
        buffer.writeShort(len(str))
        buffer.writeBytes(str)
        buffer.myFlush()
    
    def onData(self,msg):
            self._messageFilter.receive(msg)
    
    
    def dispatchRedDwarfEvent(self,e):
        listenerFunctionName = "on_" + e.eventType()
        print listenerFunctionName
        if self.eventListeners.has_key(listenerFunctionName):
            for l in self.eventListeners[listenerFunctionName]:
                l(e,self)
        else:
            print "no handler for event %s "%listenerFunctionName
    
    
    def onRawMessage(self, messageString):
        message = BytesArray('c',messageString)#todo create with a init
        command = ord(message.readByte())
        if (command == LOGIN_SUCCESS_CODE):
            e = RedDwarfEvent(LOGIN_SUCCESS_MSG)
            e.reconnectKey = message.readRemainingBytes()
            self.dispatchRedDwarfEvent(e)
        elif (command == LOGIN_FAILURE_CODE):
            e = RedDwarfEvent(LOGIN_FAILURE_MSG)
            e.failureMessage = message.readSgsString();
            self.dispatchRedDwarfEvent(e)
        
        elif (command == LOGIN_REDIRECT_CODE):
            newHost = message.readSgsString()
            newPort = message.readInt()
            e = RedDwarfEvent(LOGIN_REDIRECT_MSG)
            e.host = newHost
            e.port = newPort
            self.dispatchRedDwarfEvent(e)
    
        elif (command ==  RECONNECT_SUCCESS_CODE):
            e = RedDwarfEvent( RECONNECT_SUCCESS_MSG)
            e.reconnectKey = message.readRemainingBytes()
            self.dispatchRedDwarfEvent(e)
    
        elif (command ==  RECONNECT_FAILURE_CODE):
            e = RedDwarfEvent(RECONNECT_FAILURE_MSG)
            e.failureMessage = message.readSgsString()
            self.dispatchRedDwarfEvent(e)
        
        elif (command ==  SESSION_MESSAGE_CODE):
            e = RedDwarfEvent( SESSION_MESSAGE_MSG)
            e.message = message.readRemainingBytes()
            self.dispatchRedDwarfEvent(e)
    
        elif (command ==  LOGOUT_SUCCESS_CODE):
            e = RedDwarfEvent( LOGOUT_MSG)
            self.dispatchRedDwarfEvent(e)
     
        elif (command ==  CHANNEL_JOIN_CODE):
            channelName = message.readSgsString()
            channel = RedDwarfClientChannel(channelName, message.readRemainingBytes())
            print "Joining this channel %s"%channel.uniqueId()
            self.channels[channel.uniqueId()] = channel
            
            e = RedDwarfEvent( CHANNEL_JOIN_MSG)
            e.channel = channel
            self.dispatchRedDwarfEvent(e)
    
        elif (command ==  CHANNEL_MESSAGE_CODE):
            channel = self.channels[bytesToChannelId(message.readSgsString())]
            e = RedDwarfEvent( CHANNEL_MESSAGE_MSG)
            e.channel = channel
            e.message = message.readRemainingBytes()
            self.dispatchRedDwarfEvent(e)
    
        elif (command ==  CHANNEL_LEAVE_CODE):
            channel = self._channels.get(RedDwarfClientChannel.bytesToChannelId(message.readRemainingBytes()))
            if (channel):
                self._channels.remove(channel.uniqueId())
                e = RedDwarfEvent( CHANNEL_LEAVE_MSG)
                e.channel = channel
                self.dispatchRedDwarfEvent(e)
        else:
          raise Exception("Undefined protocol command: %c"%command)


        
"""
Get a channel id from a buffer
"""
def bytesToChannelId(buf):
    result = 0
    shift = (len(buf) - 1) * 8
    for i in range(len(buf)):
        b = ord(buf[i])
        result += (b & 255) << shift;
        shift -= 8
    return result


class RedDwarfClientChannel:
    '''
    class RedDwarfClientChannel to handle a channel.
    name: name of the channel
    rawId: id of the channel
    '''
    
    def __init__(self,name,rawId):
        self._name=name
        self._rawIdBytes = rawId
        self._idNumber= bytesToChannelId(rawId)
        
    
    def rawIdBytes(self):
        return self._rawIdBytes
    
    def uniqueId(self):
        return self._idNumber


class MessageFilter:
    """
    class MessageFilter
    Filter of server messages
    """
    def __init__(self,client):
        self.client = client
        self.messageBuffer = BytesArray('c')
        
    def receive(self,msg):
        self.messageBuffer.writeBytes(msg)
        self.messageBuffer.setPosition(0)
        
        while (self.messageBuffer.bytesAvailable()>2):
            
            payLoadLen = self.messageBuffer.readShort()
            
            if(self.messageBuffer.bytesAvailable()>=payLoadLen):
                self.client.onRawMessage(self.messageBuffer.readBytes(payLoadLen))
            else:
                self.messageBuffer.setPosition(self.messageBuffer.position() - 2)
                break
            
        newbuffer = BytesArray('c')
        newbuffer.extend(self.messageBuffer.readRemainingBytes())
        self.messageBuffer = newbuffer



class RedDwarfEvent:
    """
    class RedDwarfEvent
    This class represents RedDwarf events
    eventType: event id
    """
    def __init__(self,eventType):
        self._eventType = eventType
        self.message=''
    
    def eventType(self):
        return self._eventType


class RedDwarfSocket(asyncore.dispatcher):
    """
    class RedDwarfSocket
    """
    def __init__(self,client, connect=True):
        asyncore.dispatcher.__init__(self)    
        if(connect):
            self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.buffer=[]
        self.client = client
            
    def writeByte(self,byte):
        self.writeBytes(ord(byte))
    
    def writeShort(self,n):
        self.writeBytes( "%c%c"%(chr( (n>>8)& 255),chr(n&255)))
    
    def writeUTF(self,str):
        self.writeShort(len(str))
        self.writeBytes(str)
        
    def writeBytes (self, str):
        self.buffer.append(str)
    
    def handle_connect(self):
        print "connected !"
        pass
    
    def handle_close(self):
        self.close()

    def handle_read(self):
        try:
            buff=self.recv(8192)
            self.client.onData(buff)
        except socket.error:
            return
    def writable(self):
        return len(self.buffer)
    def handle_write(self):
        l = self.buffer[0]
        del(self.buffer[0])
        sent = self.send(l)
        if(len(l[sent:])):
            self.buffer.insert(0,l[sent:])
    
    def myFlush(self):
        pass


#TODO remove this class/unused
class Panda3DSocket:
    def __init__(self,client,cManager, cWriter, timeout=6000):
        self.cManager = cManager
        self.cWriter = cWriter
        self.client= client
        print client.host
        print client.port
        self.conn = cManager.openTCPClientConnection(client.host,client.port,timeout)
        print self.conn
        self.buffer=[]    
        
    def writeByte(self,byte):
        self.writeBytes(ord(byte))
    
    def writeShort(self,n):
        self.writeBytes( "%c%c"%(chr( (n>>8)& 255),chr(n&255)))
    
    def writeUTF(self,str):
        self.writeShort(len(str))
        self.writeBytes(str)
        
    def writeBytes (self, str):
        self.buffer.append(str)
    
    def handle_connect(self):
        pass
    
    def handle_close(self):
        self.close()

    def handle_read(self):
        buff=self.recv(8192)
        self.client.onData(buff)
        
    def writable(self):
        print "writable %i"%len(self.buffer)
        return len(self.buffer)
    
    def handle_write(self):
        print "TODO handle_write: need to create a PyDatagram object"
        print self.buffer
        datagram = PyDatagram()
        msg=""
        for x in self.buffer:
            for y in x:
                print y
                datagram.addInt8(ord(y)) 
        print datagram           
        self.buffer = []
        if(self.conn):
            self.cWriter.send(datagram,self.conn)
        else:
            print "unable to write on the socket"
            
    def myFlush(self):
        self.handle_write()


class RedDwarfEventHandler:
    """
    class RedDwarfEventHandler
    Base class of event handlers
    """
    def __init__(self,client):
        self._client = client
    
    def on_loginFailure(self,event, client): 
        print "RedDwarfEventHandler: on_loginFailure"
        
    def on_loginSuccess(self,event, client):
        print "RedDwarfEventHandler: on_loginSuccess"
        client.isLogged = True
        
    def on_loginRedirect(self,event, client): 
        print "on_loginRedirect"
    
    def on_reconnectSuccess(self,event, client): 
        print "on_reconnectSuccess"
    
    def on_reconnectFailure(self,event, client): 
        print "on_reconnectFailure"
    
    def on_sessionMessage(self,event,client):
        print "on_sessionMessage"
        print "got this message: '%s'"%event.message.tostring()
    
    def on_channelJoin(self,event, client): 
        print "on_channelJoin"
        print event.channel._name
        print event.channel.uniqueId()
        print event.channel.rawIdBytes()
        client.channelIdByName[event.channel._name] = event.channel.uniqueId()
        
    def on_channelMessage(self,event, client):
        print "on_channelMessage"
    
    def on_channelLeave(self,event, client):
        print "on_channelLeave"
    
    def on_rawMessage(self,event, client):
        print "on_rawMessage"
    

        

