#pragma once
#include <string>

bool acquire_lock(const std::string& lockfilename, std::chrono::seconds timeout);
