# CreateVersions.ps1

$git_version = (git describe --tags --long --match ?.?.? | Select-String -pattern '(?<major>[0-9]+)\.(?<minor>[0-9]+).(?<patch>[0-9]+)-(?<commitCount>[0-9]+)-(?<hash>[a-z0-9]+)').Matches[0].Groups

$majorVersion = $git_version['major']
$minorVersion = $git_version['minor']
$patchVersion = $git_version['patch']
$commitCount = $git_version['commitCount']
 
$version = [string]::Join('.', @(
	$majorVersion,
	$minorVersion,
	$patchVersion
))

$branch = '%build.branch.name%'.Split('/')[-1]

$hash = '%build.vcs.number%'
$shorthash = $hash.substring(0,7)

Write-Host "##teamcity[setParameter name='system.MajorVersion' value='$majorVersion']"
Write-Host "##teamcity[setParameter name='system.MinorVersion' value='$minorVersion']"
Write-Host "##teamcity[setParameter name='system.PatchVersion' value='$patchVersion']"
Write-Host "##teamcity[setParameter name='system.CommitCount' value='$commitCount']"
Write-Host "##teamcity[setParameter name='system.AssemblyVersion' value='$version.%build.counter%']"
Write-Host "##teamcity[buildNumber '$version-$branch.%build.counter%']"
Write-Host "##teamcity[setParameter name='system.ShortHash' value='$shorthash']"
