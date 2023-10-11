New-Service -Name "TodoistReview" -BinaryPathName "d:\Projekty\Taurit.TodoistTools.Review\source\Taurit.TodoistTools.Review\bin\Release\published\Taurit.TodoistTools.Review.exe"

Set-Service -Name "TodoistReview" -StartupType Automatic
Start-Service -Name "TodoistReview"
Stop-Service -Name "TodoistReview"