#ifndef LOGGER_H
#define LOGGER_H
#include <string>
#include <fstream>

class Logger
{
public:
	Logger(std::string filename, size_t max_file_size, unsigned int backup_count);

	void log(std::string content);
private:
	std::string filename;
	size_t max_file_size;
	unsigned int backup_count;
	std::ofstream logfile;

	void open();
	void possibly_rotate(size_t pending_content_length);
	void rotate() const;
	void rename_if_exists(const std::string& old_name, const std::string& new_name) const;
	bool exists(const std::string& name) const;
	struct stat stat_or_throw(const std::string& filename) const;
	void ensure_file_exists();
	void ensure_dir_exists();
};
#endif
