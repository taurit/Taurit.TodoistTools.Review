## What is this app?

This repository contains small, home-made web application to help review queued tasks in [Todoist task manager](http://todoist.com) in a quick, systematic way.

I believe that reviewing task is a crucial part of GTD and that helps maintain clear view of what needs to be done.

Currently this app allows to:
* assign a label to a task, and
* change task priority.

Personally I use Todoist labels as a "context" or "environment", so keeping this information up to date allows for easy filtering of tasks based on my current whereabouts: home, supermarket, work, etc.

## Dependencies

Technically, the app is using:
* ASP.NET MVC,
* Knockout.js and therefore jQuery,
* Bootstrap,
* JSON.NET,
* RestSharp,

It's set up to compile with one click in most recent stable versions of Visual Studio Community IDE. It uses [Todoist API](https://developer.todoist.com/) to contact with the platform.

## Screenshot
![](documentation/todoist-review-app-screenshot-300-2017-10-29.png)

## Changelog

### Version 2018-10-14
* Added ability to estimate task duration (e.g. for stats)
* Sections are now hidden if they don't need review for clean UI
* Web app was migrated from the .NET Framework to the .NET Core
* Filter of tasks to review was updated to include all tasks that need review
* Added code to prevent accidental page refresh during review on mobile devices

### Version 2018-04-05
* Todoist API endpoint was updated because old version was shut down

### Version 2017-11-18
* "Priority" section was moved above the "labels" section for more natural review flow

### Version 2017-10-29
* added web app manifest for better mobile experience
* added ability to eliminate task during review
* added ability to set/update task priority
* added R# team-shared settings file
* improved API mock
* updated target framework from 4.5.2 to 4.7.1
* updated external dependencies
* removed unused Modernizr dependency

### Version 2016-01-01
* Proof of concept
