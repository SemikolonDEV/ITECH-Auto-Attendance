#!/bin/bash

cp ~/projects/Rose-Linode/env/ITECH-Auto-Attendance/appsettings.json .
docker compose down && docker compose up -d && docker ps