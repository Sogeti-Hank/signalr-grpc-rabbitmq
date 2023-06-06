docker-compose.yml to set up RabbitMQ if you need it

Make sure solution is set up for multiple start up projects: GrpcService, Producer and Web.

Think of Producer as a fake Orleans Silo creating data.

Here, RabbitMQ is isolated from the Web. SignalR initiates subscription to pipeline data via gRPC stream.
