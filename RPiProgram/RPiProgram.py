import socket
import Client
import RPi.GPIO as GPIO

GPIO.setwarnings(False)
GPIO.setmode(GPIO.BOARD)
#Set all GPIO pins to output
pins = [3, 5, 7, 11, 13, 15, 19, 21, 23, 29, 31, 33, 35, 37, 8, 10, 12, 16, 18, 22, 24, 26, 32, 36, 38, 40]
pins.sort()
pins = tuple(pins)
GPIO.setup(pins, GPIO.OUT, initial = GPIO.LOW)

udp_port = 20253
host = socket.gethostname();
print("Hostname: " + str(host))

#Setup socket
udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp.bind(('', udp_port))

while True:
    data, addr = udp.recvfrom(10)
    data = data.decode('ascii')
    print("\nGot: '" + data + "' from " + str(addr))
    
    #If device wants to connect to us
    if 'rpi' == data:
        #Create a socket and send a port to form TCP over
        s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        s.bind(('', 0))
        udp.sendto(str(s.getsockname()[1]).encode('ascii'), addr)
        print("SENT TO: " + str(addr))
        c = Client.Client(s, str(addr[0]), pins)

GPIO.cleanup()