import socket
import threading
from time import sleep

udp_port = 20253

def HandleConnection(s, ip):
    
    port = s.getsockname()[1]
    s.settimeout(5) #Wait 5 seconds before stopping
    print("Connecting to '" + ip + "'" + "on port: " + str(port))

    #Give time for other side to open port (max wait=2s)
    for i in range(0, 8):
        error = s.connect_ex((ip, port))
        if not error or error != 111: break
        sleep(0.25)

    if error:
        print('Connection timed out or Error. Terminating. TCP Error: ' + str(error))
        s.close()
        return
    
    print("Connection to '" + ip + "' established.")
    while True:
        data = s.recv(1024)
        print('Msg: ' + data.decode('ascii'))
        s.close()
        print('Connection terminated.')
        return;

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

        #Handle communication on separate thread
        threading._start_new_thread(HandleConnection, (s, str(addr[0])))