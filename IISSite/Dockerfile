FROM microsoft/iis:windowsservercore-1803
SHELL ["powershell"]
RUN mkdir C:\site
ADD . /site
RUN Install-WindowsFeature NET-Framework-45-ASPNET ; \  
    Install-WindowsFeature Web-Asp-Net45
RUN Remove-WebSite -Name 'Default Web Site' 
RUN Install-WindowsFeature Web-Windows-Auth

#These are only needed for accessing the IIS admin UI
#RUN net user localadmin Qwerty123456 /add
#RUN net localgroup Administrators localadmin /add 
#RUN Install-WindowsFeature Web-Mgmt-Service
#RUN New-ItemProperty -Path HKLM:\software\microsoft\WebManagement\Server -Name EnableRemoteManagement -Value 1 -Force
#RUN Start-Service WMSVC
#These are only needed for accessing the IIS admin UI

#Enable Windows Authenticaiton on the site
RUN Import-module IISAdministration; \
    New-IISSite -Name "Site" -PhysicalPath C:\site -BindingInformation "*:80:" 

RUN C:\windows\system32\inetsrv\appcmd.exe add apppool /name:"gmsatest" /managedPipelineMode:"Integrated"
RUN C:\windows\system32\inetsrv\appcmd.exe set config /section:applicationPools "/[name='gmsatest'].processModel.identityType:LocalSystem" 
RUN C:\windows\system32\inetsrv\appcmd.exe add site /name:Site /id:1 /bindings:http://dceu-gmsa2:80 /physicalPath:C:\site
#RUN c:\windows\system32\inetsrv\appcmd.exe set site Site "/[path='/'].applicationPool:gmsatest"

# Create apps 
RUN C:\windows\system32\inetsrv\appcmd.exe add app /site.name:Site /path:/secure/forms /physicalPath:C:\sites\secure\forms
RUN C:\windows\system32\inetsrv\appcmd.exe add app /site.name:Site /path:/secure/iwa /physicalPath:C:\sites\secure\iwa

# Associate sites with App Pool
RUN C:\windows\system32\inetsrv\appcmd.exe set app "Site/secure/forms" /applicationPool:gmsatest
RUN C:\windows\system32\inetsrv\appcmd.exe set app "Site/secure/iwa" /applicationPool:gmsatest

# Set some stuff anonymous i guess i dont know its tired and late and im not fully following the diff between app and site and kittens
RUN C:\windows\system32\inetsrv\appcmd.exe set config "Site" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"True" /commit:apphost
RUN C:\windows\system32\inetsrv\appcmd.exe set config "Site/secure/iwa" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"True" /commit:apphost
RUN C:\windows\system32\inetsrv\appcmd.exe set config "Site" -section:system.webServer/security/authentication/anonymousAuthentication /enabled:"True" /commit:apphost

RUN C:\windows\system32\inetsrv\appcmd.exe set config "Site" -section:system.webServer/security/authentication/windowsAuthentication /enabled:"True" /commit:apphost


EXPOSE 80



