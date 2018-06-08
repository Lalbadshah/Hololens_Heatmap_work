import zmq
from msgpack import loads

def init_requester(ctx,port) :
    req = ctx.socket(zmq.REQ)
    req.connect('tcp://localhost:'+str(port))

    return req

def create_subscriber(ctx,req,topic) :
    req.send_string('SUB_PORT')
    sub_port = req.recv_string()

    sub = ctx.socket(zmq.SUB)
    sub.connect('tcp://localhost:'+str(sub_port))
    sub.setsockopt_string(zmq.SUBSCRIBE, topic)

    return sub

def get_data(subscriber) :
    topic = subscriber.recv_string()
    msg = subscriber.recv()
    msg = loads(msg, encoding='utf-8')

    return msg
