import socket
import Client

udp_port = 20253
host = socket.gethostname();
print("Hostname: " + str(host))

#Setup socket
udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp.bind(('', udp_port))

while True:
    data, addr = udp.recvfrom(1024)
    data = data.decode('ascii')
    print("\nGot: " + data + " from " + str(addr))
    
    #If device wants to connect to us
    if 'rpi' == data:
        #Create a socket and send a port to form TCP over
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.bind(('', 0))
        udp.sendto(str(s.getsockname()[1]).encode('ascii'), addr)
        udp.sendto(str(s.getsockname()[1]).encode('ascii'), addr)
        c = Client.Client(s, str(addr[0]))