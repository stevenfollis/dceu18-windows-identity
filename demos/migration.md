# Migration of a legacy application to Windows Container including gMSA

PowerShell Script
```powershell
# Set image name to use
$IMAGE = 'dtr.winid.net/dceu/jobs:1'

# Build Docker Container
 docker build --tag $IMAGE ./Jobs


# Run Docker Container
docker run -d --name jobs --publish 80 $IMAGE
Start-Process "chrome.exe" "$(docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' jobs)"


# Generate Credential Spec file
# https://github.com/MicrosoftDocs/Virtualization-Documentation/blob/live/windows-server-container-tools/ServiceAccounts/CredentialSpec.psm1
Import-Module ./CredentialSpec
New-CredentialSpec -Name dceu-gmsa -AccountName dceu-gmsa -Domain $(Get-ADDomain -Current LocalComputer)
code F:\Docker\credentialspecs\dceu-gmsa.json


# Run Docker Container with Credential Spec
docker rm --force jobs
docker run -d --name jobs --publish 80 --hostname dceu-gmsa --security-opt "credentialspec=file://dceu-gmsa.json" $IMAGE
Start-Process "chrome.exe" "$(docker inspect --format '{{ .NetworkSettings.Networks.nat.IPAddress }}' jobs)"


# Push to Docker Trusted Registry (DTR)
docker push $IMAGE
```
