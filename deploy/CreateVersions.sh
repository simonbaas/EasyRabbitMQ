#!/bin/bash

version=`git describe --tags --long --match ?.?.?`
branch=`git branch | grep \* | cut -d ' ' -f2`
buildCounter=%build.counter%

if [[ $version =~ ^([0-9]+)\.([0-9]+)\.([0-9]+)-([0-9]+)-([a-z0-9]+)$ ]]
then
    majorVersion="${BASH_REMATCH[1]}"
    minorVersion="${BASH_REMATCH[2]}"
    patchVersion="${BASH_REMATCH[3]}"
    commitCount="${BASH_REMATCH[4]}"
    shortHash="${BASH_REMATCH[5]}"

    assemblyVersion="$majorVersion.$minorVersion.$patchVersion.$commitCount"

    echo "##teamcity[setParameter name='system.MajorVersion' value='$majorVersion']"
    echo "##teamcity[setParameter name='system.MinorVersion' value='$minorVersion']"
    echo "##teamcity[setParameter name='system.PatchVersion' value='$patchVersion']"
    echo "##teamcity[setParameter name='system.CommitCount' value='$commitCount']"
    echo "##teamcity[setParameter name='system.AssemblyVersion' value='$assemblyVersion']"
    echo "##teamcity[setParameter name='system.ShortHash' value='$shortHash']"
    echo "##teamcity[buildNumber '$assemblyVersion-$branch.$buildCounter']"
else
    echo "$version doesn't match regular expression"
    exit 1
fi