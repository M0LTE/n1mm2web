# n1mm2web
A small .NET Core 2.0 console app that listens for RadioInfo datagrams from N1MM and uploads them to a website in HTML format to be included in a page. Designed for GB2GP but readily adaptable.  
Being .NET Core this will be cross-platform.

## Building
```
dotnet publish n1mm2web -r linux-arm -o ..\pub\pi
```

## Deploying
* Copy the output of pub\pi to a Raspberry Pi
* Copy appsettings.json to /etc/n1mm2web.conf
* Edit n1mm2web.conf and supply FTP details

## Running
### At the console:
```
chmod +x n1mm2web
./n1mm2web

Logs go to wherever you configured, e.g. /var/log/n1mm2web.log

### As a service
tbc

Logs go to wherever you configured, e.g. /var/log/n1mm2web.log

## To do
* Maybe look at making the HTML output template-based, maybe using the Razor engine or something to generate an HTML fragment
* Or, welcome to 2018, allow a webhook to be defined!