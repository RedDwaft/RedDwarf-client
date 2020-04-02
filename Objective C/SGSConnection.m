//
//  SGSConnection.m
//  LuckyOnline
//
//  Created by Timothy Braun on 3/11/09.
//  Copyright 2009 Fellowship Village. All rights reserved.
//

#import "SGSConnection.h"
#import "SGSContext.h"
#import "SGSSession.h"
#import "SGSMessage.h"
#import "SGSProtocol.h"

#import <CFNetwork/CFNetwork.h>

#define SGS_CONNECTION_IMPL_IO_BUFSIZE	SGS_MSG_MAX_LENGTH

@interface SGSConnection (PrivateMethods)

- (void)openStreams;
- (void)closeStreams;
- (void)connectionClosed;
- (void)resetBuffers;
- (void)processOutgoingBytes;
- (BOOL)processIncomingBytes;

@end


@implementation SGSConnection

@synthesize socket;
@synthesize state;
@synthesize context;
@synthesize session;
@synthesize inBuf;
@synthesize outBuf;
@synthesize expectingDisconnect;
@synthesize inRedirect;

- (id)initWithContext:(SGSContext *)aContext {
	if(self = [super init]) {
		// Save reference to our context
		self.context = aContext;
		
		// Set some defaults
		expectingDisconnect = NO;
		inRedirect = NO;
		state = SGSConnectionStateDisconnected;
		session = [[SGSSession alloc] initWithConnection:self];
		
		// Create our io buffers
		inBuf = [[NSMutableData alloc] init];
		outBuf = [[NSMutableData alloc] init];
	}
	return self;
}

- (void)dealloc {
	[inBuf release];
	[outBuf release];
	[session release];
	[context release];
	
	[super dealloc];
}

- (void)disconnect {
	[self closeStreams];
	expectingDisconnect = NO;
	state = SGSConnectionStateDisconnected;
	
	if(inRedirect) {
		// Just reset the buffers if we are being redirected
		[self resetBuffers];
	} else {
		// Not redirecting so release the buffers and release
		// our references to the context and session
		[inBuf release];
		[outBuf release];
		[session release];
		//[context release];
		
		inBuf = nil;
		outBuf = nil;
		session = nil;
		//context = nil;
	}
}

- (void)loginWithUsername:(NSString *)username password:(NSString *)password {
	// Create the host ref
	CFStringRef hostname = CFStringCreateWithCString(kCFAllocatorDefault, [context.hostname UTF8String], kCFStringEncodingASCII);
	CFHostRef host = CFHostCreateWithName(kCFAllocatorDefault, hostname);
	
	// Pre buffer the login request
	[session loginWithLogin:username password:password];
	
	// Try and connect to the socket
	CFReadStreamRef readStream = NULL;
	CFWriteStreamRef writeStream = NULL;
	
	CFStreamCreatePairWithSocketToCFHost(kCFAllocatorDefault, host, context.port, &readStream, &writeStream);
	if(readStream && writeStream) {
		CFReadStreamSetProperty(readStream, kCFStreamPropertyShouldCloseNativeSocket, kCFBooleanTrue);
		CFWriteStreamSetProperty(writeStream, kCFStreamPropertyShouldCloseNativeSocket, kCFBooleanTrue);
		inputStream = (NSInputStream *)readStream;
		outputStream = (NSOutputStream *)writeStream;
		
		[self openStreams];
		
		self.state = SGSConnectionStateConnected;
	}
}

- (void)logout:(BOOL)force {
	if(force) {
		[self connectionClosed];
		return;
	}
	
	expectingDisconnect = YES;
	if(inRedirect) {
		return;
	}
	
	[session logout];
}

- (void)sendMessage:(SGSMessage *)msg {
	[outBuf appendBytes:[msg bytes] length:[msg length]];
	[self processOutgoingBytes];
}

#pragma mark NSStreamDelegate Impl

- (void) stream:(NSStream*)stream handleEvent:(NSStreamEvent)eventCode {
	switch (eventCode) {
		case NSStreamEventOpenCompleted:
		{
			if(stream == outputStream) {
				// Output stream is connected
				// Update our state on this
				state = SGSConnectionStateConnected;
			}
			break;
		}
		case NSStreamEventHasBytesAvailable:
		{
			// read data from the buffer
			uint8_t buf[256];
			uint8_t *buffer;
			NSUInteger ilen = 0;
			if(![inputStream getBuffer:&buffer length:&ilen]) {
				NSInteger amount;
				while([inputStream hasBytesAvailable]) {
					amount = [inputStream read:buf maxLength:256];
					[inBuf appendBytes:buf length:amount];
					NSLog(@"Opcode: 0x%x", buf[2]);
				}
			} else {
				// We have a reference to the buffer
				// copy the buffer over to our input buffer and begin processing
				[inBuf appendBytes:buffer length:ilen];
			}
			do {} while([self processIncomingBytes]);
			break;
		}
		case NSStreamEventHasSpaceAvailable:
		{
			[self processOutgoingBytes];
			break;
		}
		case NSStreamEventEndEncountered:
		{
			[self connectionClosed];
			break;
		}
		case NSStreamEventErrorOccurred:
			[self connectionClosed];
			break;
		default:
			break;
	}
}

#pragma mark Private Methods

- (void)openStreams {
	inputStream.delegate = self;
	[inputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
	[inputStream open];
	outputStream.delegate = self;
	[outputStream scheduleInRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
	[outputStream open];
}

- (void)closeStreams {
	if(inputStream) {
		[inputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
		[inputStream release];
		inputStream = nil;
	}
	
	if(outputStream) {
		[outputStream removeFromRunLoop:[NSRunLoop currentRunLoop] forMode:NSDefaultRunLoopMode];
		[outputStream release];
		outputStream = nil;
	}
}

- (void)connectionClosed {
	if(inRedirect)
		return;
	
	[self disconnect];
	
	if([context.delegate respondsToSelector:@selector(sgsContext:disconnected:)]) {
		[context.delegate sgsContext:context disconnected:self];
	}
}

- (void)resetBuffers {
	[inBuf release];
	[outBuf release];
	inBuf = [[NSMutableData alloc] init];
	outBuf = [[NSMutableData alloc] init];
}

- (void)processOutgoingBytes {
	if(![outputStream hasSpaceAvailable]) {
		return;
	}
	
	unsigned olen = [outBuf length];
	if(0 < olen) {
		int writ = [outputStream write:[outBuf bytes] maxLength:olen];
		if(writ < olen) {
			memmove([outBuf mutableBytes], [outBuf mutableBytes] + writ, olen - writ);
			[outBuf setLength:olen - writ];
			return;
		}
		[outBuf setLength:0];
	}
}

- (BOOL)processIncomingBytes {
	// See if we have enough bytes to read the message length
	NSUInteger ilen = [inBuf length];
	if(ilen < SGS_MSG_LENGTH_OFFSET) {
		return NO;
	}
	
	// We have enough bytes, get the message length
	uint32_t mlen;
	[inBuf getBytes:&mlen length:SGS_MSG_LENGTH_OFFSET];
	mlen = ntohs(mlen);
	
	// Copy the bytes to the message buffer and clear them from the input buffer
	size_t len = mlen + SGS_MSG_LENGTH_OFFSET;
	NSMutableData *messageBuffer = [NSMutableData dataWithLength:len];
	memcpy([messageBuffer mutableBytes], [inBuf bytes], len);
	memmove([inBuf mutableBytes], [inBuf bytes] + len, [inBuf length] - len);
	[inBuf setLength:ilen - len];
	
	// Build the message with the message buffer
	SGSMessage *msg = [SGSMessage messageWithData:messageBuffer];
	[session receiveMessage:msg];
	
	return YES;
}

@end
