import socket
import threading

devices = []
udp_port = 20253
tcp_port = udp_port + 1

def ConnectToPhone(ip):
	print("Connecting")
	
	tcp.settimeout(5)
	error = tcp.connect_ex((ip, tcp_port))
	if error:
		return
	
	phones.append(ip);
	print("Done!")


host = socket.gethostname();
print("Hostname: " + str(host))

udp = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
udp.bind(('', udp_port))

tcp = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
tcp.bind((host, tcp_port))

while True:
	data, addr = udp.recvfrom(1024)
	data = data.decode('ascii')
	print("Got: " + data + " from " + str(addr))
	
	if host == data:
		ConnectToPhone(addr[0])