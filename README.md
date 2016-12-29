
## blojob v.0.3.1

### Summary

_blojob_ is a simple, to&#8209;the&#8209;point BLO toolset for various GameCube games.
At its core, it is an API for loading, saving, rendering, and manipulating BLO elements.
Tools are included to view and convert BLO files.
_blojob_ has support for the following:

- Supports all BLO elements (panes, pictures, windows, textboxes, screens)
- Able to load textures, palettes, and fonts in their native formats
- Text rendering (with support for all ESC codes)
- Gradient mapping, translucency, and multitexturing

The various formats supported by the library are as follows:

|Format|Load|Save|Description|
|------|----|----|-----------|
|compact|&#x2713;|&#x2713;|"Compact", or "BLO0", format used in Pikmin 1. Very limited and cannot set most properties.|
|blo1|&#x2713;|&#x2713;|The standard "BLO1" format used by Wind Waker, Super Mario Sunshine, and Luigi's Mansion. Can set almost all properties.|
|xml|&#x2713;|&#x2713;|A custom, XML-based text format, allowing for easier editing. See [here](xlo-specs.md) for documentation on this format. Can set all properties.|

### Compiling

To compile _blojob_, you'll need to have the following libraries compiled and/or installed:

- [arookas library](http://github.com/arookas/arookas)
- [OpenTK](https://github.com/opentk/opentk)

The repository contains a [premake5](https://premake.github.io/) [script](premake5.lua).
Simply run the script with premake5 and build the resulting solution.

> **Note:** You might need to fill in any unresolved-reference errors by supplying your IDE with the paths to the dependencies listed above.

### Usage

Once built, there will be a several executables:

|Name|Description|
|----|-----------|
|blojob|The primary shared library. Contains the BLO loading, saving, rendering, and manipulation code.|
|pablo|Simple BLO viewer, accurately displaying any given BLO file of the supported formats.|
|joblo|Dedicated BLO converter. Useful for upgrading "compact" BLOs to BLO1, as well as converting BLO1 to XML and back for basic editing.|

#### pablo

The viewer, _pablo_, has a very simple command-line interface allowing drag&#8209;and&#8209;drop usage, as well as direct command&#8209;line usage for more configuration.
It utilizes OpenTK's GameWindow class for rendering.
In drag&#8209;and&#8209;drop mode, you can load a single BLO1-format file.
The command-line parameters are as follows:

|Parameter|Description|
|---------|-----------|
|-input _file_ _format_|Specifies the input BLO file and format. This is the only required parameter. _file_ may be a relative or absolute path. _format_ is optional, defaulting to BLO1.|
|-search-paths _path_ ...|Adds a fallback global search path to the resource finder. This is used to find texture, fonts, palettes, and other files referenced from within the BLO file. You may specify any number of search paths.|
|-display-size _width_ _height_|Resizes the viewer display to the specified dimensions. Useful to crop the view to the actual game's dimensions. The BLO will be centered with no scaling. By default, the BLO's dimensions are used.|

Once the BLO is loaded, you may use various keys to toggle certain rendering flags:

|Key|Action|
|---|------|
|p|Allows panes to be rendered as white quads.|
|v|Shows all BLO elements, even ones which are set to be hidden by default.|

#### joblo

The converter, _joblo_, does not support drag&#8209;and&#8209;drop usage.
The command-line parameters are as follows:

|Parameter|Description|
|---------|-----------|
|-input _file_ _format_|The path and format of the BLO file to convert. _file_ may be a relative or absolute path and cannot not be the same as the output. _format_ is optional, defaulting to BLO1.|
|-output _file_ _format_|Specifies the format and path to which to convert the input file. _file_ may be a relative or absolute path and cannot not be the same as the input. _format_ is optional, default to BLO1.|
|-search-paths _path_ ...|Adds a fallback global search path to the resource finder. This is used to find texture, fonts, palettes, and other files referenced from within the BLO file. You may specify any number of search paths.|

> **Note:** If both the input and output formats are the same, _joblo_ performs a basic file-copy operation with no conversion performed.
