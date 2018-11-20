#!/bin/bash
cd -- "$(dirname "$BASH_SOURCE")"

SUBLIME_PACKAGE_DIR="$HOME/Library/Application Support/Sublime Text 3/Installed Packages"
SUBLIME_PACKAGE_FILE="$SUBLIME_PACKAGE_DIR/Fuse.sublime-package"
FUSE_VERSION_FILE="$SUBLIME_PACKAGE_DIR/FuseVersion"
FUSE_CHECK_STATUS=false

function init
{	
	mkdir -p "$SUBLIME_PACKAGE_DIR"

	if [ "$1" == "-s" ] || [ "$1" == "-status" ]; then
		FUSE_CHECK_STATUS=true
	fi

	if [ -f "$SUBLIME_PACKAGE_FILE" ]; then
		check_pack_install
	else
		if [ $FUSE_CHECK_STATUS == true ]; then
			exit_on_no_plugin
		else
			install_or_update
		fi
	fi
}

function check_pack_install
{
	if [ $FUSE_CHECK_STATUS == true ]; then
		if ! [ -f "$SUBLIME_PACKAGE_FILE" ]; then
			exit_on_no_plugin
		fi
	fi

	if [ -f "$FUSE_VERSION_FILE" ]; then
		load_etag
	else
		set_default_etag
	fi

	if [ $FUSE_CHECK_STATUS == true ]; then
		check_for_update
	else
		install_or_update
	fi
}

# Etag file manipulation
function load_etag
{
	read -r LOCAL_ETAG < "$FUSE_VERSION_FILE"
	LOCAL_ETAG=$(echo $LOCAL_ETAG|tr -d '\n'|tr -d '\r')
}

function set_default_etag
{
	save_etag "None"
}

function save_etag
{
	echo $1 > "$FUSE_VERSION_FILE"
	LOCAL_ETAG=$1
}

# Update

function readHeaderLine
{
	local words=($1)
	shopt -s nocasematch
	if [[ "${words[0]}" == "Etag:" ]]; then
		is_etag=true
	else
		is_etag=false
	fi
	shopt -u nocasematch
	if [ $is_etag == true ]; then
		if [ "${words[1]}" != "$LOCAL_ETAG" ]; then
			if [ $2 == true ]; then
				save_package
				save_etag ${words[1]}
				exit_install_success
			else
				exit_update_available
			fi
		fi
	elif [ "${words[1]}" == "304" ]; then
		exit_no_update_available
	fi
}


function save_package
{
	mv temp_package "$SUBLIME_PACKAGE_FILE"
	if [[ $status -ne 0 ]]; then
        echo "Failed to copy package file! Is Sublime already running?" >&2
        exit_with_error
    fi
}

function install_or_update
{
	mkdir -p "$SUBLIME_PACKAGE_DIR"

	echo Installing sublime plugin...
	curl -s -L -o temp_package --header "If-None-Match: $LOCAL_ETAG" "https://raw.githubusercontent.com/fuse-open/Fuse.SublimePlugin/master/releases/FuseSublimePlugin_1_5_0.zip" --dump-header header_dump >/dev/null
	if [[ $status -ne 0 ]]; then
        echo "Download failed!" >&2
        exit_with_error
    fi

    while read -r line || [[ -n "$line" ]]; do
	    readHeaderLine "$line" true
	done < header_dump
}

function check_for_update
{
	echo "Checking for update"
	curl -s -L --header "If-None-Match: $LOCAL_ETAG" "https://raw.githubusercontent.com/fuse-open/Fuse.SublimePlugin/master/releases/FuseSublimePlugin_1_5_0.zip" --dump-header header_dump >/dev/null
	if [[ $status -ne 0 ]]; then
        echo "Check for update failed!" >&2
        exit_with_error
    fi

    while read -r line || [[ -n "$line" ]]; do
	   readHeaderLine "$line" false
	done < header_dump
}


# Exits and cleanup

function exit_install_success
{
	echo Successfully installed the sublime plugin
	# cleanup
	exit 0
}

function exit_on_no_plugin
{
	echo Sublime plugin has not been installed >&2
	cleanup
	exit 100
}

function exit_update_available
{
	echo A package update is available
	cleanup
	exit 200
}

function exit_no_update_available
{
	echo Package up to date
	cleanup
	exit 0
}

function exit_with_error
{
	echo Failed to install the Sublime plugin >&2
	cleanup
	exit 1
}

function cleanup
{
	if [ -f header_dump ]; then
		rm header_dump
	fi
	if [ -f temp_package ]; then
		rm temp_package
	fi
}

# start

init $@
