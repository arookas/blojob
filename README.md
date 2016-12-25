
## blojob v.0.3.1

### Summary

_blojob_ is a simple, to-the-point BLO toolset for various GameCube games.
It is designed to be as accurate as possible, but comes without the ability to edit.
It is written in C#, utilizes OpenTK's GameWindow class for rendering, and supports all of the BLO elements from the BLO1 and BLO0 formats:

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

> **Note:** You might need to fill in any unresolved-reference errors by supplying your IDE with the paths to the dependencies listed above.

### Usage

Once built, there will be a several executables:

|Name|Description|
|----|-----------|
|blojob|The primary shared library. Contains the BLO loading, saving, rendering, and manipulation code.|
|pablo|Simple BLO viewer, accurately displaying any given BLO file of the supported formats.|
|joblo|Dedicated BLO converter. Useful for upgrading "compact" BLOs to BLO1, as well as converting BLO1 to XML and back for basic editing.|

#### pablo

The viewer, _pablo_, has a very simple command-line interface allowing drag&#8209;and&#8209;drop usage, as well as direct command&#8209;prompt usage for more configuration.
The parameters are as follows:

```
pablo <input-file> [<format> [<search-path> [...]]]
```

|Parameter|Description|
|---------|-----------|
|&lt;input&#8209;file&gt;|The path to the BLO file to view. This is the only required parameter, allowing drag&#8209;and&#8209;drop with default configuration. May be a relative or absolute path.|
|&lt;format&gt;|Allows you to specify the format of the input BLO file. By default, this is assumed to be the BLO1 format. The possible values are: _compact_, _blo1_, _xml_.|
|&lt;search&#8209;path&gt;|Adds a fallback global search path to the resource finder. This is used to find texture, fonts, palettes, and other files referenced from within the BLO file. You may specify any number of search paths.|

Once the BLO is loaded, you may use various keys to toggle certain rendering flags:

|Key|Action|
|---|------|
|p|Allows panes to be rendered as white quads.|
|v|Shows all BLO elements, even ones which are set to be hidden by default.|

#### joblo

The converter, _joblo_, does not support drag&#8209;and&#8209;drop usage. It takes at least four parameters as follows:

```
joblo <input-file> <input-format> <output-file> <output-format> [<search-path> [...]]
```

|Parameter|Description|
|---------|-----------|
|&lt;input&#8209;file&gt;|The path to the BLO file to convert. May be a relative or absolute path. Must not be the same as &lt;output&#8209;file&gt;.|
|&lt;input&#8209;format&gt;|Specifies the format of the input file. The possible values are: _compact_, _blo1_, _xml_.|
|&lt;output&#8209;file&gt;|The path to which to save the converted BLO. May be relative or absolute. Must not be the same as &lt;input&#8209;file&gt;|
|&lt;output&#8209;format&gt;|Specifies the format to which to convert the input file. The possible values are: _compact_, _blo1_, _xml_.|
|&lt;search&#8209;path&gt;|Adds a fallback global search path to the resource finder. This is used to find texture, fonts, palettes, and other files referenced from within the BLO file. You may specify any number of search paths.|

> **Note:** If both &lt;input&#8209;format&gt; and &lt;output&#8209;format&gt; are the same value, joblo performs a basic file-copy operation with no conversion performed.
