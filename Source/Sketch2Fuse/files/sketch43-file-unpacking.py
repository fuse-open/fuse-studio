#!/usr/bin/python
import json
import zipfile
import os
import shutil
from glob import glob

## TODO Handle command line arguments

def extractincurrentdir(file):
    zip_ref = zipfile.ZipFile(file) # create zipfile object
    zip_ref.extractall() # extract file to current dir
    zip_ref.close() # close file    

# make these input parameters?
test_file_directory = "Sketch43"
output_dir = "output"
update = False # Bad boy, bad boy, what U gonna do? What U gonna do when they come for U

if not update:
    os.mkdir(output_dir)

curr_dir = os.getcwd()

files = os.listdir(test_file_directory)

for file in files:
    basename = os.path.basename(file)
    name, ext = os.path.splitext(basename)
    if ext != '.sketch': continue # skipping .DS_Store in MacOS
    sketch_file_dir = os.path.join(output_dir, name)
    if update and os.path.isdir(sketch_file_dir):
        continue # if the output dirctory already exists and we just want to update, skip this

    print file
    
    os.mkdir(sketch_file_dir)
    shutil.copy(os.path.join(test_file_directory, file), sketch_file_dir)

    os.chdir(sketch_file_dir)
    
    # unzip sketch
    extractincurrentdir(file)
    
    # make json files pretty
    # find all json files
    json_files =  [y for x in os.walk('.') for y in glob(os.path.join(x[0], '*.json'))]

    for json_file in json_files:
        with open(json_file, 'r+') as file_handle:
            json_str = json.dumps(json.load(file_handle), sort_keys=True, indent=4)
            file_handle.seek(0)     # reset file to start position
            file_handle.write(json_str) # write formatted json
            file_handle.truncate()  # get rid of potential garbage at end of file
                
    os.chdir(curr_dir)
