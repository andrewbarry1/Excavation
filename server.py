# Note - please never actually use WebSockets for desktop applications like this

import sys, time, random
from twisted.python import log
from twisted.internet import reactor
from autobahn.twisted.websocket import WebSocketServerProtocol
from autobahn.twisted.websocket import WebSocketServerFactory


maxPlayers = 2

clients = {}
room_ids = []
rooms = {}
maps = {}

def send_map(client):
    for l in maps[client.room_id]:
        client.sendMessage('z'+l)
def gen_map(room_id): # do initial map generation
    maplines = []
    for x in range(0,30):
        maplines.append(gen_map_line(x))
    maps[room_id] = maplines


def gen_map_line(x): # generate line of map, x is depth
    line = ''
    prob_rock = min(100, int(((1.0/35.0)*pow(x,2))+10))
    prob_iron = min(30, int(((1/1000)*pow(x,2))+10))
    prob_diam = max(0, min(15, ((1/10000)*pow(x+215,2))-10))
    prob_mars = max(0, min(5, ((1/10000)*pow(x,2))-2))
    for x in range(0,31):
        c = ''
        if (random.randint(0,100) < prob_rock): c = '1'
        if (random.randint(0,100) < prob_iron): c = '2'
        if (random.randint(0,100) < prob_diam): c = '3'
        if (random.randint(0,100) < prob_mars): c = '4'
        if (c == ''): c = '0'
        line += c
    return line

class MyServerProtocol(WebSocketServerProtocol):

    def onMessage(self, payload, isBinary):
        if (isBinary):
            payload = payload.encode()
        if (payload[0] == 'd'): # drilled rock
            coords = payload[1:].split(',')
            line = maps[self.room_id][int(coords[1])]
            maps[self.room_id][int(coords[1])] = line[:int(coords[0])] + '9' + line[int(coords[0])+1:]
            for c in rooms[self.room_id]:
                if not(c == self.pid):
                    clients[c].sendMessage(payload)
        elif (payload[0] == 'z'): # request more lines
            for x in range(len(maps[self.room_id]),len(maps[self.room_id])+30):
                print("Generated new line")
                newline = gen_map_line(x)
                maps[self.room_id].append(newline)
                for c in rooms[self.room_id]:
                    clients[c].sendMessage('z'+newline)
        else: # simple echo message to other clients
            for c in rooms[self.room_id]:
                if not(c == self.pid):
                    clients[c].sendMessage(payload)

    def onOpen(self):
        self.pid = int(time.time())
        clients[self.pid] = self
        create_room = None
        for room_id in room_ids:
            if not(len(rooms[room_id]) == maxPlayers):
                create_room = room_id
                break
        if create_room is None: # no empty rooms - make one
            self.room_id = int(time.time())
            room_ids.append(self.room_id)
            rooms[self.room_id] = [self.pid,]
            gen_map(self.room_id)
        else: # found nonfull room to join
            self.room_id = create_room
            rooms[self.room_id].append(self.pid)
        self.sendMessage('o'+str(self.room_id))
        self.sendMessage('y'+str(self.pid))
        send_map(self)
        for c in rooms[self.room_id]:
            if not(c == self.pid):
                self.sendMessage('a'+str(c))
        self.sendMessage('s')

    def onClose(self, wasClean, code, reason):
        rooms[self.room_id].remove(self.pid)
        if (len(rooms[self.room_id]) == 0):
            del rooms[self.room_id]
            room_ids


if __name__ == '__main__':
    factory = WebSocketServerFactory("ws://127.0.0.1:9001",debug=True)
    factory.protocol = MyServerProtocol
    reactor.listenTCP(9001,factory)
    reactor.run()
