import subprocess
import os
import fnmatch
from os import path
import shutil

def glob_recursive(path, f):
    for root, dirnames, filenames in os.walk(path):
        for filename in fnmatch.filter(filenames, f):
            yield root + "/" + filename

def otool(s, basepath_filters):
    o = subprocess.Popen(['/usr/bin/otool', '-L', s], stdout=subprocess.PIPE)
    for l in o.stdout:
        if l[0] == '\t':
            lib = l.split(' ', 1)[0][1:]
            if (type(basepath_filters) is list and [x for x in basepath_filters if lib.startswith(x)]):
                yield lib

def get_all_req_dependencies(lib, source_base_paths):
    need = set([lib])
    done = set()

    while need:
        needed = set(need)
        need = set()
        for f in needed:
            need.update(otool(f, source_base_paths))
        done.update(needed)
        need.difference_update(done)
    return done

def fixup_all_dylib_references(base_path, prefix, source_base_paths):
    included_dylib_paths = {}
    for f in glob_recursive(base_path, "*.dylib"):
        rel = f[len(base_path):]
        included_dylib_paths[path.basename(rel)] = rel
        subprocess.check_call(['install_name_tool', '-id', prefix + rel, f])

    # Another time, but fixup references to all bundled dylib files
    for f in glob_recursive(base_path, "*.dylib"):
        print('Fixing dependency paths for ' + f)
        for ref in otool(f, source_base_paths):
            print('    processing dep ' + ref)
            if path.basename(ref) in included_dylib_paths:
                newPath = prefix + included_dylib_paths[path.basename(ref)]
                subprocess.check_call(['install_name_tool', '-change', ref, newPath, f])

def copy_lib_and_dependencies(from_path, to_path, with_prefix, base_paths):
    real_from_path = path.realpath(from_path)
    deps = get_all_req_dependencies(real_from_path, base_paths)
    lib_path = to_path

    for cur_lib in deps:
        cur_lib_path = path.join(lib_path, path.basename(cur_lib))
        shutil.copy(cur_lib, cur_lib_path)

    # Add symlink to specific library.
    if path.islink(from_path):
        os.symlink(path.basename(real_from_path), path.join(lib_path, path.basename(from_path)))

def add_rpath(exe_path, rpath):
    subprocess.check_call(['install_name_tool', '-add_rpath', rpath, exe_path])
