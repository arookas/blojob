
## blojob v.0.1.0

### Summary

_blojob_ is a simple, to-the-point BLO viewer for various GameCube games. It is designed to be as accurate as possible, but comes without the ability to edit. It is written in C#, utilizes OpenTK's GameWindow class for rendering, and supports all of the BLO elements from the BLO1 and earlier formats:

- Panes
- Pictures
- Windows
- Textboxes (including fonts)
- Screens

### Compiling

To compile _blojob_, you'll need to have the following libraries compiled and/or installed:

- [arookas library](http://github.com/arookas/arookas)
- [OpenTK](https://github.com/opentk/opentk)

The repository contains a [premake5](https://premake.github.io/) [script](premake5.lua).
Simply run the script with premake5 and build the resulting solution.

> _**Note:** You might need to fill in any unresolved-reference errors by supplying your IDE with the paths to the depencies listed above._

### Usage

A very simple command-line interface is implemented that allows for drag-and-drop usage, as well as direct command-prompt usage for more configuration. The parameters are as follows:

```
blojob.exe <input-file> [<format> [<search-path> [...]]]
```
|Parameter|Description|
|---------|-----------|
|&lt;input-file&gt;|The path to the BLO file to view. This is the only required parameter, allowing drag-and-drop with default configuration. May be a relative or absolute path.|
|&lt;format&gt;|Allows you to specify the format of the input BLO file. By default, this is assumed to be the BLO1 format. The possible values are: _compact_, _blo1_.|
|&lt;search-path&gt;|Adds a fallback global search path to the resource finder. This is used to find texture, fonts, palettes, and other files referenced from within the BLO file. You may specify any number of search paths.|
