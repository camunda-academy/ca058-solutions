# Nodejs Worker application for zeebe

## Description
This application is based on the zeebe nodejs gRPC client provided by Josh Wulf
and represents one of the possible solutions for the Camunda 8 training for developer exercises.
Only the overall solution is represented, not the deltas between the single exercises

## Requirements

https://nodejs.org/en/download/

## Usage
### Add the required libraries
* npm i zeebe-node
* npm i properties-reader

### Create application.properties 
* Create a folder config in the node app root
* Create application.properties file with the Zeebe connection configuration, and place it in the config folder

This file should look like this:

ZEEBE_CLUSTER_ID=234782734927
ZEEBE_CLIENT_ID=24234242353453453
ZEEBE_CLIENT_SECRET=842948294898012834989257758345
ZEEBE_REGION=bru-2

### Start the application
node workerApp.js

## Todo
Tests, add more workers for the send message tasks, review the code