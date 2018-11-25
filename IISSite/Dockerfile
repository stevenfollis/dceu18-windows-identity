# escape=`

# ================================================================================================
# Builder Stage
# https://blogs.msdn.microsoft.com/dotnet/2018/06/13/using-net-and-docker-together-dockercon-2018-update/
# ================================================================================================

FROM microsoft/dotnet-framework:4.7.2-sdk as builder

WORKDIR C:\site

# Build Visual Studio Solution
# TODO: Optimize Nuget restoration into layers
COPY . C:\site

RUN nuget restore

RUN msbuild `
	/p:Configuration=Release `
	WinContainerTester.sln

# ================================================================================================
# Runtime Stage
# ================================================================================================

#FROM microsoft/aspnet:4.7.2-windowsservercore-1803
FROM microsoft/aspnet:4.7.2-windowsservercore-ltsc2016

EXPOSE 80

WORKDIR C:\site

# Setup Operating System
RUN Import-Module "WebAdministration"; `
	Import-Module "IISAdministration" ; `
	Import-Module "ServerManager"; `
	Install-WindowsFeature "Web-Scripting-Tools"; `
	Install-WindowsFeature "Web-Windows-Auth";

# Enable IIS Remote Administration
RUN net user localadmin Qwerty123456 /add; `
	net localgroup Administrators localadmin /add; `
	Install-WindowsFeature Web-Mgmt-Service; `
	New-ItemProperty -Path "HKLM:\software\microsoft\WebManagement\Server" -Name "EnableRemoteManagement" -Value 1 -Force;

# Setup IIS Server
# Configure AppPool to run as LocalSystem 
RUN New-WebAppPool "GMSA-AppPool"; 

# Start AppPool automatically (no "cold start")
#RUN Set-ItemProperty `
		#-Path "IIS:\AppPools\GMSA-AppPool" `
		#-Name "startMode" `
		#-Value "AlwaysRunning";
#
#RUN Set-ItemProperty `
		#-Path "IIS:\AppPools\GMSA-AppPool" `
		#-Name "processModel" `
		#-Value @{identitytype=0};
#

# Replace Default Web Site
# Configure for Anonymous Authentication

RUN Remove-WebSite -Name 'Default Web Site'; `

	New-WebSite `
		-Name "Site" `
		-Port 80 `
		-PhysicalPath "C:\site" `
		-ApplicationPool "GMSA-AppPool"; `

	Set-WebConfigurationProperty `
		-Location "Site" `
		-PSPath IIS:\ `
		-Filter "system.webServer/security/authentication/anonymousAuthentication" `
		-Name "enabled" `
		-Value $true;

# Copy compiled files from Builder Stage
COPY --from=builder C:\site\ c:\site\

# 1-Create Web Applications
# 2-Configure authentication
RUN New-WebApplication `
		-Name "Secure\Forms" `
		-Site "Site" `
		-PhysicalPath "C:\site\Secure\Forms" `
		-ApplicationPool "GMSA-AppPool"; `
	
	New-WebApplication `
		-Name "Secure\IWA" `
		-Site "Site" `
		-PhysicalPath "C:\site\Secure\IWA" `
		-ApplicationPool "GMSA-AppPool"; `

	Set-WebConfigurationProperty `
		-Location "Site/Secure/Forms" `
		-PSPath IIS:\ `
		-Filter "/system.webServer/security/authentication/anonymousAuthentication" `
		-Name "Enabled" `
		-Value $True; `
		
	Set-WebConfigurationProperty `
		-Location "Site/Secure/IWA" `
		-PSPath IIS:\ `
		-Filter "/system.webServer/security/authentication/windowsAuthentication" `
		-Name "Enabled" `
		-Value $True; `

	Set-WebConfigurationProperty `
		-Location "Site/Secure/IWA" `
		-PSPath IIS:\ `
		-Filter "/system.webServer/security/authentication/anonymousAuthentication" `
		-Name "Enabled" `
		-Value False;

# Check application health against anonymous endpoint
HEALTHCHECK CMD powershell -command `  
    try { `
     $response = iwr http://localhost:80 -UseBasicParsing; `
     if ($response.StatusCode -eq 200) { return 0} `
     else {return 1}; `
    } catch { return 1 }

# Start IIS Remote Management and monitor IIS
ENTRYPOINT	Start-Service WMSVC; `
			C:\ServiceMonitor.exe w3svc;
