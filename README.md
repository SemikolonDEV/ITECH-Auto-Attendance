<p align="center">
  <h1 align="center">Auto Attendance</h1>
  <p align="center">(Only relevant for ITECH students)<br/>An application that regularly inscribes school attendance</p>

# About
- Itech auto attendance is a program which automatically
  enroll attendance while you are still in bed. dont try to denied it...
- It could run on your main machine or on a server.
- The program will try to attendance daily at 9 am and on startup.
- Here is a full example (keep in mind that by default the browser window will not show)

https://user-images.githubusercontent.com/60587271/176958998-e385c5ad-0b81-4337-831e-0cb0e51ab8d3.mp4

## Installation with docker ⚙️(Recommended for server configuration)

1. Install docker
2. Pull and run selenium chrome standalone image (Required for the browser simulation)
   1. Pull the selenium image by running <code>docker pull selenium/standalone-chrome</code>
   2. Run the selenium image by running <code>docker run -d -p 4444:4444 --shm-size="1g" selenium/standalone-chrome:latest</code>
   3. You can check if selenium is running by opening the browser at <server-ip>:4444
3. Clone or download this git repo on your server
4. Configure your appsettings.json. [See here](#Configuration) how to configure settings.
5. Build an docker image by running <code>docker build -t itech-auto-attendance</code>
6. Run the build docker image by running <code>docker run -it -d itech-auto-attendance</code>


## Installation without docker ⚙️

#### Requirements
1. .Net 6 runtime installed
2. Latest version of chrome (needed for selenium)


#### Installation
1. Download the project build [here](https://github.com/SolomonRosemite/ITECH-Auto-Attendance/releases). (Or download and build the project yourself)
2. Configure your appsettings.json. [See here](#Configuration) how to configure settings.
3. Run the project (In the configuration you can set <code>HideWindow</code>
   to false to see if the program works as expected)

# Configuration

There are some required configurations to be made for this to work.

For editing and following along you can find the config in the appsettings.json.

### Properties
- (Required) Username and Password
  - Your itech login credentials.
  - This is needed for authenticating the user and to enroll attendance.


- (Required) AttendanceBlockName
  - The name of the current block.
  - This is needed for [selenium](https://www.selenium.dev/) to find and attendance for the right block
  - You can find the current block name by viewing the class course
    [or click here](https://moodle.itech-bs14.de/course/view.php?id=1570) and copying the name of the attendance
    link.

  See example screenshot below...
  <img src="https://github.com/SolomonRosemite/ITECH-Auto-Attendance/blob/4313e5f0406c8118badc5833f8ab0e152e9cd4f3/example.png?raw=true">

- CronExpression (defaults to: <code>"0 0 9 ? * *"</code>)
  - Defines when the program should try to enroll attendance. By default it runs every day on 9 am.


- HideWindow (defaults to: <code>true</code>)
  - Defines if the browser window should show. By default its hidden.


- RunOnlyOnce (defaults to: <code>false</code>)
  - Defines if the program should terminate after it had tried to enroll attendance regardless if it was successful or not.
    By default the program never terminates.


- RemoteDriverUrl
  - Specifies the remote web driver url. This will be required if you choose to run your program
    on the server. This url should look something like this <code>"http://<server-ip>:4444/wd/hub"</code>


- UseRemoteDriver (defaults to: <code>false</code>)
  - Defines if the program should use a remote web driver. This is also only relevant if you choose to run your program
    on the server. By default this is set to false.