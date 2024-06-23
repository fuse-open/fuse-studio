CC=g++
CFLAGS=-std=c++0x -Wall -Wextra -Weffc++ 
all: main.cpp socketserver.cpp logger.cpp singleproc.cpp
	$(CC) $(CFLAGS) main.cpp socketserver.cpp logger.cpp singleproc.cpp -o fuse-logserver

test-client: testclient.cpp
	$(CC) $(CFLAGS) testclient.cpp -o test-client
