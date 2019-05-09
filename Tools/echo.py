# Example of simple echo server
# www.solusipse.net

import socket

def listen():
    connection = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    connection.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    connection.bind(('0.0.0.0', 5555))
    connection.listen(10)
    print('Listening...')
    while True:
        current_connection, address = connection.accept()
        while True:
            data = current_connection.recv(1024)
            if not data:
                continue
            current_connection.send(data)
            print('Echoing data length: %d' % len(data))


if __name__ == "__main__":
    try:
        listen()
    except KeyboardInterrupt:
        pass
