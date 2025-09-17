This repo is a quick .NET Console sample application to demonstrate how to determine if a given process is being run as an Elevated Administrator.   It takes the name of a process (as shown by the PowerShell Command "Get-Process") as a commandline argument.  

There are three results:
-NotElevated:  Code reliably determines that the target process is not running as elevated
-Elevated:   Code reliably determines that the target process is running as elevated.  This result can only be obtained if this code itself is being run with Elevated privilges.  
-AssumeElevated:   Code receives an Access Denied in making the determination.  If this console app is run as non-elevated and the target process is elevated, the code will throw an exception, which is expected in this case.  The assumption is then made that the target process is actually elevated, though this is not explicitly determined.
