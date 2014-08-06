PoC-S7
======

RabbitMQ server needs to be installed. After installing it will run as a service and by default it will listen on 5672 port. The projects in this repository are configured to connect to localhost:5672 for the mq broker.

To run change the startup projects for the solution to multiple projects:
* ApplicationServicesHost
* WebServicesHost

This will run two app hosts in two separate consoles and a browser window will also be opened to demonstrate the request/response pipeline.
