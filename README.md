Cross platform manifest maker for Unreal's ChunkDownloader plugin to read.

This console application is useful for generating platform specific BuildManifest file that can be part of a build pipeline. 

Compile the code using Rider or Visual studio or any .Net compiler. 

How to generate manifest: (inside command prompt or terminal where executable file is kept)

[exe file name] "[path to previous manifest file or null]"
"[directory where pak files are stored]"
"[Build Version]"
"[Platform]"
"[quality]"
"[boolean to upload all or not]"
"[pak file name that has been updated]"
"[pak file name that has been updated]"
[followed by names of however many pak files that were updated] 

eg:
[exe file name] "C:\Users\BuildManifest-Android.txt"
"C:\Users\Android\high"
"v1.0.0"
"Android"
"high"
"false"
"pakchunk1001_s1-Android_ASTC.pak"
"pakchunk1200_s4-Android_ASTC.pak"

For running on mac, dotnetcore needs to be installed. 
To run prefix dotnet followed by .dll path followed by the other parameters listed above.
[dotnet][dll file name] "[path to previous manifest file or null]"
...
...

The code was made keeping in mind specific requirements, but, it can very easily be editted to, let's say, not include quality settings.
