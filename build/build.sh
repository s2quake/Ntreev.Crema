#!/bin/bash
items=("cremaconsole" "cremadev" "cremaserver")
releasePath="../bin/Release"
deploymentPath="./Release"

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
msbuild ../crema.sln -t:Restore -v:q

if [ ! $? = 0 ]; then
    echo "Restore failed."
    exit 1
fi

echo "Build"
msbuild ../crema.sln -t:Rebuild -v:q -p:Configuration=Mono

if [ ! $? = 0 ]; then
    echo "Build failed."
    exit 1
fi

echo "Copy"
mkdir $deploymentPath

for item in ${items[@]}
do
    srcPath="../bin/Release/$item"
    destPath="$deploymentPath/$item"
    mkdir $destPath
    rsync $srcPath $destPath -v -q --recursive --exclude="*.pdb" --exclude="*.xml"
done
#rsync ../bin/Release/cremaconsole/* ./Release/cremaconsole -v --recursive --exclude="*.pdb" --exclude="*.xml"