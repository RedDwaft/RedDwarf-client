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
from array import *

class BytesArray(array):
    '''
    class BytesArray
    Extension of array representing a buffer of reddwarf messages
    '''
    def __init__(self,typecode,bytes=None):
        array.__init__(self,'c')
        '''
        Constructor
        '''
        self.readindex = 0
    
    
    def setPosition(self,pos):
        self.readindex = pos
    
    def position(self):
        return self.readindex
    
    """
    writeBytes(self,bytes)
    write iteratively an iterator of bytes
    """
    def writeBytes(self,bytes):
        self.extend(bytes)
    
    def writeShort(self,s):
        if(abs(s)>0xFFFF):
            raise Exception("Argument is not a short number")
        self.extend([chr( (s>>8)& 255),chr(s &255 )])
    
    def writeUTF(self,str):
        self.writeShort(len(str))
        self.writeBytes(str)
        
    def readByte(self):
        self.readindex += 1
        return self[self.readindex-1]
    
    def readShort(self):
        x = (ord(self.readByte()) << 8)
        y = ord(self.readByte())
        return x+y
    
    def readInt(self):
        x = self.readByte() << 24
        x += self.readByte() << 16
        x += self.readByte() << 8
        x += self.readByte()
        return x
    
    def readBytes(self,length):
        self.readindex += length
        return self[self.readindex-length:self.readindex]
    
    def readSgsString(self):
        msgsize = self.readShort()
        return self.readBytes(msgsize).tostring()
    
    def bytesAvailable(self):
        return len(self)- self.readindex
    
    def readRemainingBytes(self):
        return self.readBytes(self.bytesAvailable())
    
    def myFlush(self):
        pass
  
        