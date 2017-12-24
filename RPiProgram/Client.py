import threading
import socket
import RPi.GPIO as GPIO
from time import sleep

class Client (object):
    """description of class"""

    sock = None
    ip = None
    done = False

    def __init__(self, sock, ip, pins):
        self.sock = sock
        self.ip = ip

        #Handle communication on separate thread
        threading._start_new_thread(self.HandleConnection, ())

    def __del__(self):
        self.sock.close() #Make sure to close the socket just in case

    def EstablishConnection(self):
        """Tries to connect to client and returns whether it was successful."""

        self.sock.settimeout(2)
        try:
            self.sock.listen(1)
            self.sock = self.sock.accept()[0] #Get the new socket to talk on
        except socket.error as err:
            print("Connection Error: " + str(err))
            return False

        return True

    def CloseConnection(self):
        self.sock.close()
        self.done = True
        print("Connection with " + str(self.ip) + " terminated normally.")

    def RefreshSocketStates(self):
        print("ASDASD")

    def SetCmd(self, cmd):
        pin = int(cmd[0:2])
        state = int(cmd[2])
        GPIO.output(pin, state)

    def GetCmd(self, cmd):
        pin = int(cmd)
        state = GPIO.input(pin)

        try:
            self.sock.send(str(state).encode('ascii'))
            print("SENT: " + str(state).encode('ascii'))
        except socket.error:
            done = True

    def HandleConnection(self):
        print("Waiting for '" + self.ip + "'" + " to connect...")
        if not self.EstablishConnection():
            self.sock.close()
            return
        print("Connection to '" + self.ip + "' established on port: " + str(self.sock.getsockname()[1]))
        
        self.sock.settimeout(None)
        while not self.done:
            cmd = self.sock.recv(1024)

            cmd = cmd.decode('ascii')
            print("Received: " + cmd)
            if cmd == "": pass
            elif cmd == "0": self.CloseConnection()
            elif cmd == "1": self.RefreshSocketStates()
            elif cmd[0] == "s": self.SetCmd(cmd[1:])
            elif cmd[0] == "g": self.GetCmd(cmd[1:])
