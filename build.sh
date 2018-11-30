#!/bin/bash
BASEDIR=$(dirname "$0")
items=("cremaconsole" "cremadev" "cremaserver")
releasePath="$BASEDIR/bin/Release"
deploymentPath="$BASEDIR/build"
solutionPath="$BASEDIR/crema.sln"

echo "Delete Release"
for item in ${items[@]}
do
    itemPath="$releasePath" 
    if [ -d "$itemPath" ]; then
        rm -rf $itemPath
    fi
done

if [ -d $deploymentPath ]; then
    rm -rf $deploymentPath
fi

echo "Restore"
msbuild $solutionPath -t:Restore -v:q

if [ ! $? = 0 ]; then
    echo "Restore failed."
    exit 1
fi

echo "Build"
msbuild $solutionPath -t:Rebuild -v:q -p:Configuration=Mono

if [ ! $? = 0 ]; then
    echo "Build failed."
    exit 1
fi

echo "Copy"
mkdir $deploymentPath

for item in ${items[@]}
do
    srcPath="$releasePath/$item"
    destPath="$deploymentPath/$item"
    mkdir $destPath
    rsync $srcPath $deploymentPath -v -q --recursive --exclude="*.pdb" --exclude="*.xml"
done
