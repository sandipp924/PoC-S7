PoC-S7
======

RabbitMQ server needs to be installed. After installing it will run as a service and by default it will listen on 5672 port. The projects in this repository are configured to connect to localhost:5672 for the mq broker. The broker address can be configured via settings.xml file in the solution's root folder.

NOTE: RabbitMQ service may not run 'correctly'. Even if windows service is running, you may need to execute the following (adjust the path based on your installation) in bash or powershell:
C:\Program Files (x86)\RabbitMQ Server\rabbitmq_server-3.3.4\sbin\rabbitmq-server.bat

To run change the startup projects for the solution to multiple projects by right-clicking the solution and selecting Properties menu item and then Common Properties in the left pane on the properties dialog.
* ApplicationServerConsoleShell
* WebServerConsoleShell

Above two projects are simple console apps that simply run the appropriate ServiceStack host implementations found in separate projects.

This will run two app hosts in two separate consoles and a browser window will also be opened to demonstrate the request/response pipeline.
