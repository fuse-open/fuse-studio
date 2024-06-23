#!/bin/bash
set -e

SOCKET_NAME=/tmp/fuse-logserver.socket
LOG_FILE=/tmp/fuse-logserver.log
NOF_PROCESSES=40
CALLS_PER_PROCESS=1000
SLEEP=5

NOF_LINES=$((NOF_PROCESSES * CALLS_PER_PROCESS))

echo
echo -e "\033[0;35mRunning test with $NOF_PROCESSES processes sending $CALLS_PER_PROCESS calls each, for a total of $NOF_LINES. Sleeping $SLEEP ms. between each call\033[0m"
echo

make
make test-client

rm -f $LOG_FILE $SOCKET_NAME
echo "Starting logserver"
./fuse-logserver $SOCKET_NAME $LOG_FILE &
PID=$!
sleep 1 #TODO wait for socket to open
echo "Starting test client"
./test-client $SOCKET_NAME ohai $CALLS_PER_PROCESS $NOF_PROCESSES $SLEEP
echo "Killing logserver"
kill -2 $PID

LINES=$(grep ohai $LOG_FILE | wc -l | awk '{print $1}')
PROCESSES=$(sort $LOG_FILE | uniq | wc -l | awk '{print $1}')

if [ "$LINES" -ne $NOF_LINES ]; then
    echo
    echo -e "\033[0;31mERROR: Expeced $NOF_LINES lines, got $LINES\033[0m"
    echo
	exit 1
fi

if [ "$PROCESSES" -ne $NOF_PROCESSES ]; then
    echo
    echo -e "\033[0;31mERROR: Expeced $NOF_PROCESSES processes, got $PROCESSES\033[0m"
    echo
	exit 1
fi

echo
echo -e "\033[0;32mSUCCESS!\033[0m"
echo
