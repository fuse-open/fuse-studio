#ifndef SOCKETSERVER_H
#define SOCKETSERVER_H
#include "logger.h"

class SocketServer
{
public:
	SocketServer(std::string name, Logger& logger);
	void receive();

private:
	void validate_name() const;
	void create_socket();
	void bind_socket();
	void set_timeout(std::chrono::milliseconds timeout);

	const std::string name;
	int fd;
	Logger& logger;
	static const std::size_t buffer_size = 100000;
	char buffer[buffer_size];
};
#endif
