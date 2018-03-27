# n1mm2web
A small .NET Core 2.0 console app that listens for RadioInfo datagrams from N1MM and uploads them to a website in HTML format to be included in a page. Designed for GB2GP but readily adaptable.  
Being .NET Core this will be cross-platform. Substitute platforms below as required.

## Installing on a Pi from binaries
```
# install a .NET Core dependency
apt-get -qqy install libunwind8

# fetch and unpack
cd /usr/local
wget https://github.com/M0LTE/n1mm2web/releases/download/1.0/n1mm2web-raspi.zip
unzip n1mm2web-raspi.zip

# put the template config file into /etc
mv n1mm2web/n1mm2web.json /etc/

# edit the configuration file
nano /etc/n1mm2web.json

# make the binary executable
chmod +x n1mm2web/n1mm2web

# install the service
echo "[Unit]
Description=n1mm2web
[Service]
WorkingDirectory=/usr/local/n1mm2web
ExecStart=/usr/local/n1mm2web/n1mm2web
Restart=always
RestartSec=10  
SyslogIdentifier=n1mm2web
User=root
Environment=\"\"
[Install]
WantedBy=multi-user.target" > /etc/systemd/system/n1mm2web.service

# make it start at boot
systemctl enable n1mm2web.service

# start it now
systemctl start n1mm2web.service

# check it's working
tail /var/log/n1mm2web.log
```

Logs go to wherever you configured, e.g. `/var/log/n1mm2web.log`

If this is on a Pi, you'll want to change the log directory, mount /var/log to a ramdisk, disable logging (blank logfile config setting), or use a high write-endurance card.


## Building from source
Install the .NET Core SDK following the instructions from your platform, then publish:

```
dotnet publish n1mm2web -r linux-arm -o ../pub/pi
```

Building and packaging should be possible on Windows, Linux and OS X and should not require Visual Studio etc, just the .NET Core SDK.

The output is a native Linux executable without dependencies other than those that appear in the output directory. i.e. .NET Core is packaged with the application.

## Deploying from source you built yourself
* Copy the output of pub/pi to a Raspberry Pi
* Copy the supplied n1mm2web.json to /etc/n1mm2web.json (or it can live next to the binary if you prefer)
* Edit n1mm2web.json and supply FTP details
* apt-get -qqy install libunwind8

## Running
### At the console:
```
chmod +x n1mm2web
./n1mm2web
```

Logs go to wherever you configured, e.g. `/var/log/n1mm2web.log`, as well as the console.

## To do
* Maybe look at making the HTML output template-based, maybe using the Razor engine or something to generate an HTML fragment
* Or, welcome to 2018, allow a webhook to be defined!