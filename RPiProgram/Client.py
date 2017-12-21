import threading
import socket
from time import sleep

class Client (object):
    """description of class"""

    sock = None
    ip = None
    done = False

    def __init__(self, sock, ip):
        self.sock = sock
        self.ip = ip

        #Handle communication on separate thread
        threading._start_new_thread(self.HandleConnection, ())

    def CloseConnection(self):
        self.sock.close()
        self.done = True
        print("Connection with " + str(self.ip) + " terminated normally.")

    def RefreshSocketStates(self):
        print("ASDASD")

    def HandleConnection(self):
    
        port = self.sock.getsockname()[1]
        self.sock.settimeout(5) #Wait 5 seconds before stopping
        print("Connecting to '" + self.ip + "'" + "on port: " + str(port))

        #Give time for other side to open port (max wait=2s)
        for i in range(0, 8):
            error = self.sock.connect_ex((self.ip, port))
            if not error or error != 111: break
            sleep(0.25)

        if error:
            print('Connection timed out or Error. Terminating. TCP Error: ' + str(error))
            self.sock.close()
            return
    
        print("Connection to '" + self.ip + "' established.")
        self.sock.settimeout(None)
        while not self.done:
            cmd = self.sock.recv(1024)

            cmd = cmd.decode('ascii')
            print("Received: " + cmd)
            
            if cmd == "0": self.CloseConnection()
            if cmd == "1": self.RefreshSocketStates()