import cv2
from network import UDPReciever
import math


from socket import *
import numpy as np




def foo():
    while True:
        s = socket(AF_INET, SOCK_DGRAM)
        s.bind(addr)

        data, address = s.recvfrom(buf)
        f = open(data, 'wb')

        data, address = s.recvfrom(buf)

        try:
            while(data):
                f.write(data)
                s.settimeout(timeOut)
                data, address = s.recvfrom(buf)
        except timeout:
            f.close()
            s.close()
        image = cv2.imread(fName)
        return image

filename = "Current_Map2.png"

USock = UDPReciever('127.0.0.1',6502)

host = "127.0.0.1"
port = 4096
buf = 1024
addr = (host, port)
# fName = 'img.jpg'
timeOut = 0.05


count =0
oldimg =  cv2.imread(filename)
while True:
    data = USock.recv(1024)
    data = data.split(',')
    x=float(data[0])
    y=float(data[1])
    #data[2]=int(data[2])
    x = math.floor(x * 1280)
    y = math.floor(y * 720)

    if(x>1280):
        x=1280
    if(x<0):
        x=0
    if(y<0):
        y=0
    if(y>720):
        y=720
    print(x,y)

    if (count==100):
        count = 0
        try:
            img = foo()
            oldimg = img
        except :
            print('Whoops')
            pass
    oldimg = cv2.flip( oldimg, 0 )
    oldimg = cv2.circle(oldimg,(int(x),int(y)), 20, (255,0,0), -1)
    oldimg = cv2.flip( oldimg, 0 )
    cv2.imshow('Current Gaze Heatmap',oldimg)
    cv2.waitKey(1)
    count+=1
