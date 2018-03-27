# n1mm2web
A small .NET Core 2.0 console app that listens for RadioInfo datagrams from N1MM and uploads them to a website in HTML format to be included in a page. Designed for GB2GP but readily adaptable.  
Being .NET Core this will be cross-platform. Substitute platforms below as required.

## Building
Install the .NET Core SDK, then publish:

```
dotnet publish n1mm2web -r linux-arm -o ../pub/pi
```

Publishing should be possible on Windows, Linux and OS X.

The output is a native Linux executable without dependencies other than those that appear in the output directory.

## Deploying
* Copy the output of pub/pi to a Raspberry Pi
* Copy the supplied n1mm2web.json to /etc/n1mm2web.conf (or it can live next to the binary if you prefer)
* Edit n1mm2web.conf and supply FTP details
* apt-get -qqy install libunwind8

## Running
### At the console:
```
chmod +x n1mm2web
./n1mm2web
```

Logs go to wherever you configured, e.g. `/var/log/n1mm2web.log`

### As a service
```
cd /usr/local

wget https://github.com/M0LTE/n1mm2web/releases/download/1.0/n1mm2web-raspi.zip

unzip n1mm2web-raspi.zip

chmod +x n1mm2web/n1mm2web

echo "[Unit]
Description=n1mm2web
[Service]
WorkingDirectory=/usr/local/n1mm2web
ExecStart=/usr/local/n1mm2web/n1mm2web
Restart=always
RestartSec=10  # Restart service after 10 seconds if it crashes
SyslogIdentifier=n1mm2web
User=root
Environment=\"\"
[Install]
WantedBy=multi-user.target" > /etc/systemd/system/n1mm2web.service

systemctl enable n1mm2web.service

systemctl start n1mm2web.service
```

Logs go to wherever you configured, e.g. `/var/log/n1mm2web.log`

## To do
* Maybe look at making the HTML output template-based, maybe using the Razor engine or something to generate an HTML fragment
* Or, welcome to 2018, allow a webhook to be defined!