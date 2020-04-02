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

#RedDwarf SimpleProtocol
MAX_MESSAGE_LENGTH = 65535
MAX_PAYLOAD_LENGTH =  65532
VERSION =  0x05

LOGIN_REQUEST_CODE =  0x10
LOGIN_SUCCESS_CODE =  0x11
LOGIN_FAILURE_CODE =  0x12
LOGIN_REDIRECT_CODE =  0x13
RECONNECT_REQUEST_CODE =  0x20
RECONNECT_SUCCESS_CODE =  0x21
RECONNECT_FAILURE_CODE =  0x22
SESSION_MESSAGE_CODE = 0x30
LOGOUT_REQUEST_CODE =  0x40
LOGOUT_SUCCESS_CODE =  0x41
CHANNEL_JOIN_CODE =  0x50
CHANNEL_LEAVE_CODE =  0x51
CHANNEL_MESSAGE_CODE =  0x52


LOGIN_SUCCESS_MSG = "loginSuccess" 
LOGIN_FAILURE_MSG = "loginFailure" 
LOGIN_REDIRECT_MSG = "loginRedirect" 
RECONNECT_SUCCESS_MSG= "reconnectSuccess" 
RECONNECT_FAILURE_MSG= "reconnectFailure" 
SESSION_MESSAGE_MSG = "sessionMessage" 
LOGOUT_MSG = "logout" 
CHANNEL_JOIN_MSG = "channelJoin" 
CHANNEL_MESSAGE_MSG = "channelMessage" 
CHANNEL_LEAVE_MSG = "channelLeave" 
RAW_MESSAGE_MSG = "rawMessage"  