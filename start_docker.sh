#!/bin/bash

# Author: Min Thu Khaing

CONTAINER_NAME="sql_server_dev"
DB_PASSWORD="Apple0097@"

# Check if a container with the given name exists (running or stopped)
if [ ! "$(docker ps -aq -f name=$CONTAINER_NAME)" ]; then
    # If it doesn't exist, create and start it.
    echo "Container '$CONTAINER_NAME' not found. Creating a new one..."
    docker run --name $CONTAINER_NAME \
        -e "ACCEPT_EULA=Y" \
        -e "MSSQL_SA_PASSWORD=$DB_PASSWORD" \
        -p 1433:1433 \
        -d mcr.microsoft.com/mssql/server:latest
    echo "Container '$CONTAINER_NAME' created and started."
else
    # If it exists, check if it's already running.
    if [ ! "$(docker ps -q -f name=$CONTAINER_NAME)" ]; then
        # If it's stopped, start it.
        echo "Container '$CONTAINER_NAME' exists but is stopped. Starting it..."
        docker start $CONTAINER_NAME
        echo "Container '$CONTAINER_NAME' started."
    else
        # If it's already running, do nothing.
        echo "Container '$CONTAINER_NAME' is already running."
    fi
fi