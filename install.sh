#!/bin/bash

if [ -z "$1" ];
then
  echo ----
  echo No version specified! Please specify a valid version like 1.2.3 or 1.2.3-rc1!
  exit 1
fi

echo ----
echo Starting building version $1

echo ----
echo Cleaning up
rm -r ./artifacts
dotnet tool uninstall -g tomware.TestR

echo ----
echo Restore
dotnet restore src/testr.Cli

echo ----
echo Packaging with Version = $1
dotnet pack src/testr.Cli -c Release -p:PackageVersion=$1 -p:Version=$1 -o ./artifacts/

echo ----
echo Installing testr globally with Version = $1
dotnet tool install --global --add-source ./artifacts/ tomware.TestR

echo ----
echo Done