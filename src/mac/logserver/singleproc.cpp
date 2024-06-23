#include <iostream>
#include <stdexcept>
#include <thread>
#include <chrono>
#include <sys/types.h>
#include <unistd.h>
#include <pwd.h>
#include <stdio.h>
#include <signal.h>
#include <libproc.h>

using namespace std;
using namespace std::chrono;

namespace {
	bool is_running(pid_t pid);
	string get_path(int pid);
	bool atomically_create_lock_file(const string& lockfilename, int my_pid);
	int try_read_pid_from(const string& lockfilename);
	void unlink_if_exists(const string& lockfilename);
	void remove_lock_file_if_needed(const string& lockfilename, const string& path_to_this_program);
}

bool acquire_lock(const string& lockfilename, seconds timeout)
{
	pid_t my_pid = getpid();
	cout << "Trying to lock " << lockfilename << endl;

	auto start = high_resolution_clock::now();
	while (duration_cast<seconds>(high_resolution_clock::now()-start) < timeout)
	{
		if (atomically_create_lock_file(lockfilename, my_pid))
		{
			return true;
		}
		remove_lock_file_if_needed(lockfilename, get_path(my_pid));
		this_thread::sleep_for(chrono::milliseconds(100));
	}
	return false;
}

namespace {

	class no_such_process : public runtime_error
	{
	public:
		no_such_process(const string& what_arg) : runtime_error(what_arg){}
	};

	//Atomically tries to create a file lockfilename, and writes my_pid to it
	//Returns true if it could do that, false if the file already exits
	//In any other error case it throws runtime_error
	bool atomically_create_lock_file(const string& lockfilename, int my_pid)
	{
		FILE* fd = fopen(lockfilename.c_str(), "wx");
		if (fd != NULL)
		{
			if(fprintf(fd, "%d", my_pid) < 0)
			{
				fclose(fd);
				throw runtime_error("Could not write to file file '" + lockfilename + "': " + strerror(errno));
			}
			fclose(fd);
			cout << "Created lock file for process " << my_pid << endl;
			return true;
		}
		else if (errno == EEXIST)
		{
			return false;
		}
		throw runtime_error("Could not open file '" + lockfilename+ "': " + strerror(errno));
	}

	//Deletes lockfilename if it contains a pid that's not currently running,
	//a pid that belongs to another program than ourself, or if the file is corrupt.
	void remove_lock_file_if_needed(const string& lockfilename, const string& path_to_this_program)
	{
		int pid = try_read_pid_from(lockfilename);
		if (pid > 0)
		{
			try
			{
				if (!is_running(pid) || (get_path(pid) != path_to_this_program))
				{
					unlink_if_exists(lockfilename);
				}
			}
			catch(const no_such_process&)
			{
				//Process went away in the mean time
				unlink_if_exists(lockfilename);
			}
		}
		else if(pid == -2) //File is corrupt, delete it
		{
			cout << "Deleting corrupted lock file '" << lockfilename << "'" << endl;
			unlink_if_exists(lockfilename);
		}
	}

	//Tries to read a pid from the file lockfilename
	//Returns the pid if successfull, -1 if it can't open the file, -2 if it can't read the file
	int try_read_pid_from(const string& lockfilename)
	{
		FILE* fd2 = fopen(lockfilename.c_str(), "r");
		if (fd2 == NULL)
		{
			return -1;
		}
		int pid;
		if (fscanf(fd2, "%d", &pid) != 1)
		{
			fclose(fd2);
			return -2;
		}
		fclose(fd2);
		return pid;
	}

	//Checks if process pid is running.
	//Throws runtime_error if the process is running but can't be signaled, if it is owned by another user
	bool is_running(pid_t pid)
	{
		if (kill(pid,0) == 0)
		{
			return true;
		}
		else if (errno != ESRCH)
		{
			throw runtime_error("Could not signal existing process " + to_string(pid));
		}
		return false;
	}

	//Gets the full path for process pid
	//Throws no_such_process if is unable to do so, for instance if process pid no longer exists
	string get_path(int pid)
	{
		char pathbuf[PROC_PIDPATHINFO_MAXSIZE];
		if(proc_pidpath (pid, pathbuf, sizeof(pathbuf)) <0)
		{
			throw no_such_process("Could not get path of process " + to_string(pid) + ": " + strerror(errno));
		}
		return string(pathbuf);
	}

	//Unlinks lockfilename if it exists
	//If the file exists, but can't be unlinked, throws runtime_error
	void unlink_if_exists(const string& lockfilename)
	{
		cout << "Deleting '" << lockfilename << "'" << endl;
		if (unlink(lockfilename.c_str()) != 0 && errno != ENOENT)
		{
			throw new runtime_error("Couldn't delete '" + lockfilename + "': " + strerror(errno));
		}
	}
}

/* int main() */
/* { */
/*	   string lockfilename = socket_name() + ".pid"; */
/*	   bool got_it = acquire_lock(lockfilename); */
/*	   cout << "Lock acquired?" << got_it << endl; */
/*	   while (true); */
/* } */
