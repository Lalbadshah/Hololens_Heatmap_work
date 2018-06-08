import socket
import cv2
import global_parameters as GLOBAL
import utils
#from queue import Queue
from multiprocessing import Queue

MIN_IMG, MAX_IMG = 1, 3000
q = Queue(2)


class FrameSender(object):
    def __init__(self):
        self.sock = socket.socket(socket.AF_INET,
                             socket.SOCK_STREAM)
        self.sock.bind((GLOBAL.TCP_IP, GLOBAL.TCP_PORT))
        self.sock.listen(5)
        print('FrameSender: Init done')


    def spin(self):
        print('FrameSender: Spin starts')
        seq = MIN_IMG
        # cv2.namedWindow('FrameSender')
        while True:
            filename = 'Current_Map' + '.png'
            buf = utils.load_jpeg_file_to_buffer(filename)
            img = utils.convert_jpeg_buffer_to_mat(buf)
            len_buf = len(buf)
            packet = utils.to_bytes(len_buf, 4, 'big') + buf # Python 2.7 workaround
            #packet = len_buf.to_bytes(4, 'big') + buf # built in since Python >= 3
            print('trying to conect...')
            conn, addr = self.sock.accept()
            print('did we connect?...')
            if addr[0] == GLOBAL.HOLOLENS_IP:
                print('FrameSender: HoloLens connected')
                conn.send(packet)
                print('FrameSender: Image sent', seq)
                #cv2.imshow('FrameSender', img)
                #c = cv2.waitKey(1)
                # if c & 0xFF == 27 or chr(c & 0xff) == 'q':
                #     break
                seq = seq + 1 if seq != MAX_IMG else MIN_IMG
            else:
                raise Exception('FrameSender: Unknown sender', addr)
        print('FrameSender: Spin ends')


if __name__ == "__main__":
    '''
    >> python frame_sender.py
    '''
    frame_sender = FrameSender()
    frame_sender.spin()
