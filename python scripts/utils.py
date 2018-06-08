import numpy as np
import cv2

def load_jpeg_file_to_buffer(filename):
    """
    Load jpg file to python bytes buffer

    :param filename: PATH/XXX.jpg
    :return: bytes buffer
    """
    file = open(filename, 'rb')
    buf = file.read()
    file.close()
    return buf


def load_jpeg_file_to_mat(filename, gray=False):
    if gray:
        return cv2.imread(filename, cv2.IMREAD_GRAYSCALE)
    else:
        return cv2.imread(filename, cv2.IMREAD_COLOR)



def convert_mat_to_jpeg_buffer(img):
    pass



def convert_jpeg_buffer_to_mat(buf, gray=False):
    if gray:
        return cv2.imdecode(np.frombuffer(buf, np.uint8), cv2.IMREAD_GRAYSCALE)
    else:
        return cv2.imdecode(np.frombuffer(buf, np.uint8), cv2.IMREAD_COLOR)



def int_from_bytes(array):
    if len(array) >= 4:
        return int.from_bytes(array[0:4], 'big')
    else:
        return int.from_bytes(array, 'big')


def convert_mat_gray_to_mat_color(gray):
    return cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR)


def to_bytes(n, length, endianess='big'):
    h = '%x' % n
    s = ('0'*(len(h) % 2) + h).zfill(length*2).decode('hex')
    return s if endianess == 'big' else s[::-1]
