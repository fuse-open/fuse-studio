#!/usr/bin/env python3
import sys
import random
import os
import time

help_text = """USAGE: __CMD__  <filename>

Writes a random app to <filename> every five seconds.
Can use ux:Classes or normal elements, by selecting which line to comment out
in the while loop at the end of the script.

If you want to watch that file, do `brew install fswatch` and then watch it:

fswatch <filename> | xargs -n1 cat
"""

def help():
    print(help_text.replace("__CMD__", sys.argv[0]))

if len(sys.argv) < 2:
    help()
    exit(1)

if sys.argv[1] == "-h":
    help()
    exit(0)

uxfilename=sys.argv[1]

def write_start(uxfile):
    uxfile.truncate()
    uxfile.write("<App>\n")
    uxfile.write("  <ClientPanel>\n")
    uxfile.write("    <StackPanel>\n")

def write_end(uxfile):
    uxfile.write("    </StackPanel>\n")
    uxfile.write("  </ClientPanel>\n")
    uxfile.write("</App>\n")

def write_ux_classes(uxfile):
    classes = []
    for i in range(1, random.randint(1,20)):
        classes.append("class" + str(random.randint(1,10000)))
    for c in classes:
        uxfile.write("      <Text Value=\"" + c + "\" ux:Class=\"" + c + "\"/>\n")
    for c in classes:
        uxfile.write("      <" + c + "/>\n")

def write_simple_elements(uxfile):
    for i in range(1, random.randint(1,20)):
        uxfile.write("      <Text Value=\"element " + str(random.randint(1,10000)) + "\" />\n")

while(True):
    print("Writing")
    with open(uxfilename, "w") as uxfile:
        write_start(uxfile)
        # write_ux_classes(uxfile)
        write_simple_elements(uxfile)
        write_end(uxfile)
    time.sleep(5)

