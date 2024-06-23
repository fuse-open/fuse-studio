#include "logger.h"
#include <iostream>
#include <cstdio>
#include <sys/stat.h>

using namespace std;

Logger::Logger(string filename, size_t max_file_size, unsigned int backup_count) :
	filename(filename), max_file_size(max_file_size), backup_count(backup_count)
{
	cout << "Logging to '" << filename << "'" << endl;
	open();
}

void Logger::open()
{
	ensure_dir_exists();
	logfile.open(filename, ofstream::app);
}

void Logger::log(std::string content)
{
	ensure_file_exists();
	possibly_rotate(content.length());
	if (content.length() + 2 < max_file_size)
	{
		logfile << content << endl;
	}
	else
	{
		logfile.write(content.c_str(), max_file_size - 6);
		logfile << "(..)" << endl;
	}
}

void Logger::possibly_rotate(size_t pending_content_length)
{
	struct stat stat_buf = stat_or_throw(filename);
	if (stat_buf.st_size + pending_content_length > max_file_size)
	{
		logfile.close();
		rotate();
		open();
	}
}

struct stat Logger::stat_or_throw(const string& filename) const
{
	struct stat stat_buf;
	int rc = stat(filename.c_str(), &stat_buf);
	if (rc != 0)
	{
		throw runtime_error(string("Could not stat '" + filename + "': " + strerror(errno)));
	}
	return stat_buf;
}

void Logger::rotate() const
{
	for(int i = backup_count-1; i > 0; i--)
	{
		rename_if_exists(filename + "." + to_string(i), filename + "." + to_string(i + 1));
	}
	rename_if_exists(filename, filename + ".1");
}

void Logger::rename_if_exists(const std::string& old_name, const std::string& new_name) const
{
	if (exists(old_name))
	{
		if (rename(old_name.c_str(), new_name.c_str()) != 0)
		{
			throw runtime_error(string("Could not rename '" + old_name + "' to '" + new_name + "': " + strerror(errno)));
		}
	}
}

bool Logger::exists(const std::string& name) const
{
	struct stat stat_buf;
	int rc = stat(name.c_str(), &stat_buf);
	if (rc == 0)
	{
		return true;
	}
	if (errno == ENOENT)
	{
		return false;
	}
	throw runtime_error(string("Could not get stat of '" + name + "': " + strerror(errno)));
}

void Logger::ensure_file_exists()
{
	if (!exists(filename))
	{
		cout << "Log file '" << filename << "' went away, re-opening." << endl;
		try
		{
			logfile.close();
		}
		catch(...)
		{
		}
		open();
	}
}

void Logger::ensure_dir_exists()
{
	#if defined(WIN32) || defined(_WIN32)
	#define PATH_SEPARATOR "\\"
	#else
	#define PATH_SEPARATOR "/"
	#endif
	string logdir = filename.substr(0, filename.find_last_of(PATH_SEPARATOR));
	if (mkdir(logdir.c_str(), 0755) != 0)
	{
		if (errno != EEXIST)
		{
			throw runtime_error(string("Could not create '" + logdir + "': " + strerror(errno)));
		}
	}
}
