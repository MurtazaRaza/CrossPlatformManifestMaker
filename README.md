Cross platform manifest maker for Unreal's ChunkDownloader plugin to read.

This console application is useful for generating platform specific BuildManifest file that can be part of a build pipeline. 

Compile the code using Rider or Visual studio or any .Net compiler. 

How to generate manifest: (inside command prompt or terminal where executable file is kept)

[exe file name] "[path to previous manifest file or null]"
"[directory where pak files are stored]"
"[Build Version]"
"[Platform]"
"[quality]"
"[pak file name that has been updated]"
"[pak file name that has been updated]"
[followed by names of however many pak files that were updated] 

For running on mac, dotnetcore needs to be installed. 
To run prefix dotnet followed by .dll path followed by the other parameters listed above.
[dotnet][dll file name] "[path to previous manifest file or null]"
...
...

The code was made keeping in mind specific requirements, but, it can very easily be editted to, let's say, not include quality settings.
