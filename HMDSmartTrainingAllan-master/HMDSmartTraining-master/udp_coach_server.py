import socket
import time
import cv2
import numpy as np
import sys
python_version = sys.version_info.major
if python_version == 3:
	import queue as Queue
else:
	import Queue

#UDP_IP = "10.189.171.80"
#UDP_IP = "192.168.1.3"
#UDP_IP = "192.168.1.4"
#UDP_IP = "192.168.1.2"
#UDP_IP = "10.194.82.65"
#UDP_IP = "10.194.3.44"
# Put the IP address of HoloLens Here
#UDP_IP = "10.194.23.166"
#UDP_IP = "192.168.1.4"
#UDP_IP = "127.0.0.1"
UDP_IP = "10.194.61.193"

UDP_PORT = 12345
Holo_ADDR = (UDP_IP, UDP_PORT)




SCALE = 1
'''
CV Frame in Python script:
Left upper corner (0, 0) ---------- Right upper corner (640, 0)
          |                                   |
          |                                   |
          |                                   |
          |                                   |
          |                                   |
          |                                   |
          |                                   |
Left lower corner (0, 360) -------- Right lower corner (640, 360)



BT300 Canvas:
Left upper corner (-640, 360) ---------- Right upper corner (640, 360)
          |                                   |
          |                                   |
          |                                   |
          |                                   |
          |                                   |
          |                                   |
          |                                   |
Left lower corner (-640, -360) ------- Right lower corner (640, -360)



'''


class SendMsg(object):
	def __init__(self, x=0.0, y=0.0, seq=-1, eventCode=0):
		self.x = x
		self.y = y
		self.seq = seq
		self.eventCode = eventCode
		self.message = str(x) + "," + str(y) + "," + str(seq) + "," + str(eventCode)
		self.raw_message = str.encode(self.message, encoding='ascii')




class CursorSender(object):
	def __init__(self):
		# UDP socket creation
		self.sock = socket.socket(socket.AF_INET, # Internet
		                     socket.SOCK_DGRAM) # UDP
		# Create a black image, a window and bind the function to window
		self.width, self.height = 1280//SCALE, 720//SCALE
		self.img = np.zeros((self.height, self.width, 3), np.uint8)
		self.img.fill(255)
		# Sequence number
		self.seq = 0
		cv2.namedWindow('image')
		cv2.setMouseCallback('image', self.draw_cursor)

		self.sendQueue = Queue.Queue(20)
		self.save = False



	def spin(self):
		# Main event loop
		while True:
			cv2.imshow('image', self.img)
			c = cv2.waitKey(1)
			if c & 0xFF == 27 or chr(c & 0xff) == 'q':
				break
			if chr(c & 0xff) == 's':
				self.save = True

			# handle sendQueue
			while not self.sendQueue.empty():
				sendMsg = self.sendQueue.get()
				sent = self.sock.sendto(sendMsg.raw_message, Holo_ADDR)
				if sent:
					print("UDPCursorClient: Message sent:", sendMsg.message)


	# mouse callback function
	def draw_cursor(self, event, x, y, flags, param):
		if event == cv2.EVENT_MOUSEMOVE:
			self.img.fill(255)
			cv2.circle(self.img,(x,y),10,(143,143,240),-1)
			sendMsg = SendMsg(x*SCALE - 640, -y*SCALE + 360, self.seq, 4 if self.save else 1)
			self.save = False
			self.sendQueue.put(sendMsg)
			self.seq += 1
		elif event == cv2.EVENT_LBUTTONDOWN:
			self.img.fill(255)
			cv2.circle(self.img,(x,y),10,(143,240,143),-1)
			sendMsg = SendMsg(x*SCALE - 640, -y*SCALE + 360, self.seq, 2)
			self.sendQueue.put(sendMsg)
			self.seq += 1
		elif event == cv2.EVENT_RBUTTONDOWN:
			self.img.fill(255)
			cv2.circle(self.img,(x,y),10,(240,143,143),-1)
			sendMsg = SendMsg(x*SCALE - 640, -y*SCALE + 360, self.seq, 3)
			self.sendQueue.put(sendMsg)
			self.seq += 1
		elif event == cv2.EVENT_MBUTTONDOWN:
			self.img.fill(255)
			cv2.circle(self.img,(x,y),10,(240,143,143),-1)
			sendMsg = SendMsg(x*SCALE - 640, -y*SCALE + 360, self.seq, 5)
			self.sendQueue.put(sendMsg)
			self.seq += 1





if __name__ == "__main__":
	sender = CursorSender()
	sender.spin()

