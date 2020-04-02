'''
Created on Sep 12, 2010

@author: gouaich
'''
from pyreddwarf import *

"Panda3D Tasks"
from direct.task import Task
import direct.directbase.DirectStart


readinglist = []
readinglist = []
writinglist = []
def tskReaderPolling(taskdata):
    for x in readinglist:
        if x.connected and x.readable():
            x.handle_read()
    return Task.cont
def tskWriterPolling(taskdata):
    for x in writinglist:
        if x.connected and x.writable():
            x.handle_write()
    return Task.cont

taskMgr.add(tskReaderPolling,"Poll the connection reader",-40)
taskMgr.add(tskWriterPolling,"Poll the connection writer",-40)

client = RedDwarfSimpleClient("127.0.0.1",1139)

writinglist.append(client.socket)
readinglist.append(client.socket)

client.connect()
client.login("test","test")

#running Panda3D loop
run()