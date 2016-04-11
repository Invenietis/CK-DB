$DebugPreference = 'Continue'
$InformationPreference = 'Continue'
$WarningPreference = 'Continue'
$ErrorActionPreference = 'Stop'

$CodeCakeBuilderScriptLocation = [System.IO.Path]::Combine( $PSScriptRoot, "Bootstrap.ps1" )
$CodeCakeBuilderExeLocation = [System.IO.Path]::Combine( $PSScriptRoot, "bin", "Release", "CodeCakeBuilder.exe" )

# Build CCB project
Invoke-Expression $CodeCakeBuilderScriptLocation

# Run CCB project
& $CodeCakeBuilderExeLocation -nointeraction
