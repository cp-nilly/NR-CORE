#! /bin/bash

if [ -f WSERVER ]
  then
    mono --debug --gc=sgen ./wServer.exe
    exit 0
fi

mono --debug --gc=sgen ./server.exe