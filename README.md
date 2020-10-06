# StudioSB

A model application for Super Smash Bros Ultimate

This application is a work-in-progress, so expect incomplete features and bugs.

[![Build status](https://ci.appveyor.com/api/projects/status/s6dbvi66y40q3tl2/branch/master?svg=true)](https://ci.appveyor.com/project/Ploaj/studiosb/branch/master) 

**[Download Latest](https://github.com/Ploaj/StudioSB/releases)**

[Request a Feature / Report bug](https://github.com/Ploaj/StudioSB/issues)

# Application Theme
The old dark teal theme can be set by changing the following values in `ApplicationSettings.cfg` in a text editor.
```javascript
ForegroundColor=#FFFFFFF0
MiddleColor=#FF5C7070
BackgroundColor=#FF3C5050
PoppedInColor=#FF203030
PoppedOutColor=#FF506060
ButtonColor=#FF506060
BGColor1=#FF2F4F4F
BGColor2=#FFA9A9A9
```

The current dark theme values.
```javascript
ForegroundColor=#FFFFFFF0
MiddleColor=#FF232323
BackgroundColor=#FF232323
PoppedInColor=#FF232323
PoppedOutColor=#FF232323
ButtonColor=#FF666666
BGColor1=#FF3C3C3C
BGColor2=#FF3C3C3C
```

## Building (Windows)
* Compile in Visual Studio 2017 on Windows
* Requires .NET Framework 4.7.2
* The recommended OpenGL version is 4.20
   * Version 3.30 or higher may still work
* Other platforms are not supported.

## Credits
Project:
* [OpenTK](https://github.com/opentk/opentk)
* Copyright (c) 2006 - 2014 Stefanos Apostolopoulos <stapostol@gmail.com>
* MIT/X11 License: https://github.com/opentk/opentk/blob/develop/License.txt

Project:
* [SFGraphics](https://github.com/ScanMountGoat/SFGraphics)
* Copyright (c) 2018 SMG
* MIT License: https://github.com/ScanMountGoat/SFGraphics/blob/master/LICENSE.txt

Project:
* [SSBHLib](https://github.com/Ploaj/SSBHLib)
* Copyright (c) 2019 Ploaj & SMG
