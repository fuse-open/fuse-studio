#include <iostream>
#include <stdexcept>
#include <pwd.h>
#include <signal.h>

#include "logger.h"
#include "socketserver.h"
#include "singleproc.h"

using std::cerr;
using std::cout;
using std::endl;
using std::string;
using std::runtime_error;
using std::exception;
using std::chrono::seconds;

string home();
string log_name(int argc, const char* const argv[]);
string socket_name();
string socket_name(int argc, const char* const argv[]);
void set_signal_handler();
void signaled(int signal);

bool terminate = false;

int main(int argc, char* argv[])
{
	try
	{
		if (!acquire_lock(socket_name() + ".pid", seconds(2)))
		{
			cout << "A log server is already running" << endl;
			return 1;
		}
		if (argc == 1)
		{
			//Using production settings
		}
		else if (argc == 3)
		{
			cout << "WARNING: Using settings meant for testing only!" << endl;
		}
		else
		{
			throw runtime_error("This command takes no parameters"); //None that you should know about, anyway
		}
		set_signal_handler();
		const off_t max_file_size(500000);
		const unsigned int backup_count(5);
		Logger logger(log_name(argc, argv), max_file_size, backup_count);
		SocketServer server(socket_name(argc, argv), logger);
		while(!terminate)
		{
			server.receive();
		}
	}
	catch (const runtime_error& e)
	{
		cerr << "ERROR: " << e.what() << endl;
		return 2;
	}
	catch (const exception& e)
	{
		cerr << "ERROR: An unknown exception occured" << endl;
		return 2;
	}
	cout << "Logserver stopped gracefully" << endl;
}

string home()
{
	const char* homedir;

	if ((homedir = getenv("HOME")) == NULL) {
		homedir = getpwuid(getuid())->pw_dir;
	}
	return string(homedir);
}

string log_name(int argc, const char* const argv[])
{
	if (argc == 3)
	{
		return argv[2];
	}
	else
	{
		return home() + "/.fuse/logs/fuse.log";
	}
}

string socket_name()
{
	return home() + "/.fuse/logserver";
}

string socket_name(int argc, const char* const argv[])
{
	if (argc == 3)
	{
		return argv[1];
	}
	else
	{
		return socket_name();
	}
}

void set_signal_handler()
{
	struct sigaction call_signaled;
	call_signaled.sa_handler = signaled;
	sigemptyset(&call_signaled.sa_mask);
	call_signaled.sa_flags = 0;
	if (sigaction(SIGINT, &call_signaled, 0) != 0)
	{
		throw runtime_error(string("Could not set signal handler: ") + strerror(errno));
	}
}

void signaled(int)
{
	terminate = true;
}
