#include <string>
#include <iostream>
#include <stdexcept>
#include <sys/socket.h>
#include <sys/un.h>

#include "socketserver.h"

using std::cout;
using std::cerr;
using std::endl;
using std::string;
using std::runtime_error;
using std::exception;

static sockaddr_un server_address(const string& socket_name);

SocketServer::SocketServer(string name, Logger& logger) : name(name), logger(logger)
{
	cout << "Starting logserver on '" << name << "'" << endl;
	validate_name();
	create_socket();
	bind_socket();
	set_timeout(std::chrono::milliseconds(100));
	cout << "Logserver successfully started" << endl;
}

void SocketServer::receive()
{
	socklen_t _;
	int bytes_received = recvfrom(fd, &buffer, buffer_size, 0, 0, &_);
	if (bytes_received == -1 )
	{
		if (errno == EINTR)
		{
			return;
		}
		if (errno != EWOULDBLOCK)
		{
			throw runtime_error(string("Couldn't recvfrom: ") + strerror(errno));
		}
	}
	else
	{
		logger.log(string(buffer, bytes_received));
	}
}

void SocketServer::validate_name() const
{
	socklen_t address_length = sizeof(struct sockaddr_un);

	if (name.length() + 1 > address_length)
	{
		throw runtime_error("Invalid socket name. '" + name + "' is longer than " + std::to_string(address_length -1) + " characters.");
	}
}

void SocketServer::create_socket()
{
	if ((fd = socket(AF_UNIX, SOCK_DGRAM, 0)) < 0)
	{
		throw runtime_error(string("Could not create socket: ") + strerror(errno));
	}
}

void SocketServer::bind_socket()
{
	cout << "Binding to socket '" << name << "'" << endl;
	sockaddr_un address = server_address(name);
	if (unlink(name.c_str()) < 0)
	{
		if (errno != ENOENT)
		{
			throw runtime_error("Couldn't unlink socket '" + name + "'");
		}
	}
	if (bind(fd, (const sockaddr *) &address, sizeof(address)) < 0)
	{
		close(fd);
		throw runtime_error("Couldn't bind socket '" + name + "': " + strerror(errno));
	}
}

void SocketServer::set_timeout(std::chrono::milliseconds timeout)
{
	const auto sec = std::chrono::duration_cast<std::chrono::seconds>(timeout);
	timeval tv;
	tv.tv_sec = sec.count();
	tv.tv_usec = std::chrono::duration_cast<std::chrono::microseconds>(timeout - sec).count();
	if (setsockopt(fd, SOL_SOCKET, SO_RCVTIMEO, (char *)&tv, sizeof(timeval)) < 0)
	{
		throw runtime_error("Couldn't set socket timeout");
	}
}

static sockaddr_un server_address(const string& socket_name)
{
	sockaddr_un address;
	memset(&address, 0, sizeof(address));
	address.sun_family = AF_UNIX;
	strcpy(address.sun_path, socket_name.c_str());
	return address;
}
