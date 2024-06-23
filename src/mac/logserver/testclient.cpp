#include <string>
#include <vector>
#include <iostream>
#include <chrono>
#include <thread>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <sys/wait.h>

using namespace std;
using std::chrono::milliseconds;

int log_to(string socket, string content);
int log_to(string socket, string content, int nof_times, milliseconds sleep);

int main(int argc, char *argv[])
{
	if (argc != 6)
	{
		cerr << "USAGE : " << argv[0] << " <socket name> <string to log> <times to log per process> <number of processes> <sleep (ms) between log call>" << endl;
		return 1;
	}

	string socket_name(argv[1]);
	string content(argv[2]);
	int nof_times(atoi(argv[3]));
	int nof_procs(atoi(argv[4]));
	int sleep(atoi(argv[5]));

	if (nof_procs == 1)
	{
		return log_to(socket_name, content, nof_times, milliseconds(sleep));
	}

	std::vector<pid_t> pids(nof_procs);
	for(std::vector<pid_t>::size_type i = 0; i < pids.size(); i++)
	{
		pid_t pid = fork();
		if (pid > 0)
		{
			pids[i] = pid;
		}
		else if (pid == 0)
		{
			return log_to(socket_name, content, nof_times, milliseconds(sleep));
		}
		else
		{
			cerr << "Unable to fork!" << endl;
			return 1;
		}
	}

	bool failed = false;
	for(const auto& pid : pids)
	{
		int status;
		pid_t result = waitpid(pid, &status, 0);
		if (result == -1)
		{
			cerr << "Error waiting for '" << pid << "': Result '" << result << "', error '" << strerror(errno) << "'" << endl;
			failed = true;
		}
		if (!WIFEXITED(status))
		{
			cerr << "Error in pid '" << pid << "', it didn't stop normally" << endl;
			failed = true;
		}
		if (WEXITSTATUS(status))
		{
			cerr << "Error in pid '" << pid << "', exited with status '" << WEXITSTATUS(status) << "'" << endl;
			failed = true;
		}
	}
	return int(failed);
}

int log_to(string socket_name, string content, int nof_times, milliseconds sleep)
{
	int result = 0;
	for (int i = 0; i < nof_times; i++)
	{
		if (sleep > milliseconds(0))
		{
			this_thread::sleep_for(sleep);
		}
		if((result = log_to(socket_name, content)) != 0)
		{
			return result;
		}
	}
	return 0;
}

int log_to(string socket_name, string content)
{
	struct sockaddr_un name;

	/* Create socket on which to send. */
	int sock = socket(AF_UNIX, SOCK_DGRAM, 0);
	if (sock < 0)
	{
		perror("opening datagram socket");
		return 1;
	}

	/* Construct name of socket to send to. */
	name.sun_family = AF_UNIX;
	strcpy(name.sun_path, socket_name.c_str());

	/* Send message. */
	string message(to_string(getpid()) + " " + content);
	if (sendto(sock, message.c_str(), message.length(), 0, (const struct sockaddr *)&name, sizeof(struct sockaddr_un)) < 0)
	{
		perror("sending datagram message");
		return 1;
	}
	if (close(sock) <0 )
	{
		perror("closing socket");
		return 1;
	}
	return 0;
}
