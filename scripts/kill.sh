if [ "$OSTYPE" = msys ]; then
    taskkill //f //t //im unohost.exe || :
    taskkill //f //t //im fuse-tray.exe || :
    taskkill //f //t //im fuse-lang.exe || :
    taskkill //f //t //im fuse-studio.exe || :
    taskkill //f //t //im fuse-preview.exe || :
    taskkill //f //t //im fuse.exe || :
else
    killall UnoHost || :
    killall "fuse X (menu bar)" || :
    killall fuse-lang || :
    killall "fuse X" || :
    killall fuse-preview || :
    killall fuse || :
fi
