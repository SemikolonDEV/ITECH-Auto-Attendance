#!/bin/bash

dockername="itech-auto-attendance";

# Stop current docker image
docker stop $(docker ps | awk '{split($2,image,":"); print $1, image[1]}' | awk -v image=$dockername '$2 == image {print $1}')

# Build & Start container
cp '~/projects/Rose-Linode/env/ITECH-Auto-Attendance/appsettings.json' .
docker build -t $dockername . && docker run -it -d $dockername && docker ps