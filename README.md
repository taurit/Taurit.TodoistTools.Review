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

### Version 2017-10-29
#### Added
* web app manifest for better mobile experience
* ability to eliminate task during review
* alow set priority
* R# team-shared settings file

#### Updated
* improved API mock
* target framework from 4.5.2 to 4.7.1
* external dependencies
#### Removed
* unused Modernizr dependency

### Version 2016-01-01
#### Added
* Proof of concept
