# Hosting the app as Windows Service

Here's a cheatsheet to host the app as a local Windows Service.

I don't need it deployed to the cloud, so this is how I currently host it (only when my Windows machine is on): 

```powershell
# Create the service
# TODO replace the your own path to exe file
New-Service -Name "TodoistReview" -BinaryPathName "d:\Projekty\Taurit.Toolkit.TodoistReview\source\Taurit.TodoistTools.Review\bin\Release\published\Taurit.TodoistTools.Review.exe"

Set-Service -Name "TodoistReview" -StartupType Automatic
Start-Service -Name "TodoistReview"
Stop-Service -Name "TodoistReview"

# unregister forever:
sc.exe delete "TodoistReview"
```