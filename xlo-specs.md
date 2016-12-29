# XLO Specifications

## Preface

This document specifies the format of the XML BLO file (or "XLO").
Future changes and updates may be made to accomodate other versions of the BLO format.

## Declaration

### Encoding

The encoding should be a Unicode-based encoding, such as UTF&#8209;8 ot UTF&#8209;16.
This allows all possible characters in textbox elements to be properly encoded.

## Types

There are various common types of properties defined by the element types.
The format among all instances remains the same.

### Boolean

A [boolean](https://en.wikipedia.org/wiki/Boolean) property.
The allowed values are "true" and "false" (case-insensitive).
If not stated otherwise, the default value of a boolean property (if omitted or invalid) is "false".

### Integer

A simple integer (whole number) property.
The allowed range and default value is specified in the property description.

### String

A simple string property.
The text in the element is used directly as&#8209;is.
Multiline values should be avoided if possible.

### Enum

A property with a single named constant.
The constant is case-insensitive.
The default value and constants are specified in the property description.

### Flags

A property with a combination of named constants.
The constants are case-insensitive and separated by commas.
The default value and constants are specified in the property description.

### Rectangle

A 2D rectangle property.
The default value is specified in the property description.

You may specify a rectangle by its position and size:

|Property|Type|Description|
|--------|----|-----------|
|&lt;x&gt;|integer|X position of the rectangle.|
|&lt;y&gt;|integer|Y position of the rectangle.|
|&lt;width&gt;|integer|Width of the rectangle.|
|&lt;height&gt;|integer|Height of the rectangle.|

A rectangle may also be specified by its edges:

|Property|Type|Description|
|--------|----|-----------|
|&lt;left&gt;|integer|Left edge of the rectangle.|
|&lt;top&gt;|integer|Top edge of the rectangle.|
|&lt;right&gt;|integer|Right edge the rectangle.|
|&lt;bottom&gt;|integer|Bottom edge the rectangle.|

### Color

An RGBA color property.
The default value is specified in the property description.

If the property element contains a &lt;r&gt;, &lt;g&gt;, and &lt;b&gt; element, the color is specified as separate components. Each component is specified as an integer value clamped to the range of 0&#8209;255. The &lt;a&gt; element is optional and defaults to 255 (opaque).

Otherwise, the color is specified as a single positive hexadecimal integer. The number of digits in the hexadecimal integer change the format of the color value, ranging from 1&#8209;8 digits. Any number of digits outside of this range is considered invalid and the default color value is assigned to the property.

|Digits|Format|
|------|------|
|1|A4, white|
|2|A8, white|
|3|RGB4|
|4|RGBA4|
|5|RGB4A8|
|6|RGB8|
|7|RGB8A4|
|8|RGBA8|

### Gradient

A two-point gradient color property.
The default value is from transparent black (`#00000000`) to opaque white (`#FFFFFFFF`).
The source color is transformed component-wise between the from/to colors in the gradient as follows:

_C<sub>dst</sub>_ = _C<sub>from</sub>_ × (1 - _C<sub>src</sub>_) + _C<sub>to</sub>_ × _C<sub>src</sub>_

Gradient mapping is performed after multitexturing but before applying the vertex colors.

|Property|Type|Description|
|--------|----|-----------|
|&lt;from&gt;|color|Specifies the first point of the color, used where the source component is zero.|
|&lt;to&gt;|color|Specifies the last point of the color, used where the source component is 255.|

### Resource

A reference to an external resource.
The text of the property element defines the path and/or name of the resource, including any extensions.
The _scope_ attribute defines at what scope to begin searching for the resource.
The default value is a null reference (i.e. no resource).

|Scope|Description|
|-----|-----------|
|none|Null reference (i.e. no resource). Text may be empty. You may also simply omit the property element.|
|localdirectory|The resource is relative to certain directory in the XLO's containing directory. The specified directory depends on the property.|
|localarchive|The resource is relative to the XLO's containing directory.|
|global|The resource is not relative to the XLO's containing directory, and is looked for the global search paths.|

For example, to reference "example.bti" in the local-directory scope:

```xml
<texture scope="localdirectory">example.bti</texture>
```

## Elements

The root XML element of the document should be a single screen element.
Every BLO element corresponds to an XML element.
You may nest BLO elements to create a hierarchy.
BLO elements inherit properties from their parent panes, such as position, rotation, and alpha.

Any XML element that is inside a BLO element but is not itself another BLO element is considered a property of the containing BLO element.
Unknown BLO element types are inherently considered XML elements, and thus property elements.
Any unknown property and BLO elements should be silently ignored.

### Pane

A pane element is the base of all other element types.
It defines the position, dimensions, rotation, translucency, and other display properties of the element.

The following properties are specified as XML attributes:

|Property|Type|Description|
|--------|----|-----------|
|id|string|Name of the pane. Default is no name (empty string). Used to reference and find elements at runtime. If more than four characters are given, only the first four are used. Each must be ASCII characters in the 0&#8209;255 range.|
|visible|boolean|Whether the element is visible by default. Defaults to "true".|
|connect|boolean|Whether to connect the pane to its parent element. While this affects nothing for most element types, certain types of elements may use it.|

The following properties are specified as XML elements:

|Property|Type|Description|
|--------|----|-----------|
|&lt;rectangle&gt;|rectangle|Display rectangle of the pane. Default is an empty rectangle.|
|&lt;angle&gt;|integer|Rotation angle, in degrees clockwise. The value wraps around 0&#8209;360. Defaults to zero (no rotation).|
|&lt;anchor&gt;|enum|Sets the anchor, or origin, of the pane. The pane's rectangle will be positioned around this point. Defaults to the top-left corner.|
|&lt;cull-mode&gt;|enum|Sets the culling mode for the pane. Defaults to _none_ (neither side is culled).|
|&lt;alpha&gt;|integer|Trasnslucency of the pane, clamped to the range of 0&#8209;255. Defaults to 255 (opaque). An optional boolean attribute, _inherit_, may be specified to change whether the pane inherits the translucency of its parent pane (default is "true").|

The available anchors are as follows:

|Enum|Description|
|----|-----------|
|topleft|Top-left corner of the pane.|
|topmiddle|Top edge of the pane.|
|topright|Top-right corner of the pane.|
|centerleft|Left edge of the pane.|
|centermiddle|Center of the pane.|
|centerright|Right edge of the pane.|
|bottomleft|Bottom-left corner of the pane.|
|bottommiddle|Bottom edge of the pane.|
|bottomright|Bottom-right corner of the pane.|

The available culling modes are as follows:

|Enum|Description|
|----|-----------|
|none|Neither side of the pane is culled.|
|front|The front side of the pane is culled.|
|back|The back side of the pane is culled.|
|all|Both sides of the pane are culled.|

### Picture

A picture element is a textured quad.
It can display one or more textures.
Multitexturing is a runtime-only feature; the XLO allows only one texture to be specified.
Picture elements inherit from pane, so all pane properties also apply here.

The following properties are specified as XML elements:

|Property|Type|Description|
|--------|----|-----------|
|&lt;texture&gt;|resource, timg|The BTI texture of the element.|
|&lt;palette&gt;|resource, tlut|The external palette to attach the to above texture. If null, the texture's embedded palette is used.|
|&lt;binding&gt;|flags|Specifies which edges of the quad to which the corresponding edges of the texture should be bound, or "pegged".|
|&lt;mirror&gt;|flags|Specifies which axes of the texture to mirror. Default is _none_.|
|&lt;rotate-90&gt;|boolean|Specifies whether to rotate the texture 90 degrees clockwise.|
|&lt;wrap-s&gt;|enum|Specifies the wrapping mode for the horizontal texture coordinate _s_. If not _none_, it should match the wrapping of the texture. Default value is _none_.|
|&lt;wrap-t&gt;|enum|Specifies the wrapping mode for the vertical texture coordinate _t_. If not _none_, it should match the wrapping of the texture. Default value is _none_.|
|&lt;gradient&gt;|gradient|Specifies the gradient for the texture, enabling gradient mapping. Only useful for intensity-format textures.|
|&lt;colors&gt;||Specifies the vertex colors for each corner of the quad. Each defaults to opaque white.|

The available binding flags are as follows:

|Flag|Description|
|----|-----------|
|left|Bind the left edge of the texture to the left edge of the quad.|
|top|Bind the top edge of the texture to the top edge of the quad.|
|right|Bind the right edge of the texture to the right edge of the quad.|
|bottom|Bind the bottom edge of the texture to the bottom edge of the quad.|

The available mirroring flags are as follows:

|Flag|Description|
|----|-----------|
|none|Neither axes are mirrored.|
|x|The texture's horizontal axis is mirrored.|
|y|The texture's vertical axis is mirrored.|

The available wrapping modes are as follows:

|Enum|Description|
|----|-----------|
|None|Texture is not wrapped to fill the quad.|
|Clamp|Texture uses clamp wrapping to fill the quad.|
|Repeat|Texture is repeated to fill the quad.|
|Mirror|Texture is mirror-repeated to fill the quad.|

#### &lt;colors&gt;

|Property|Type|Description|
|--------|----|-----------|
|top-left|color|The color for the top-left corner of the picture. Defaults to opaque white.|
|top-right|color|The color for the top-right corner of the picture. Defaults to opaque white.|
|bottom-left|color|The color for the bottom-left corner of the picture. Defaults to opaque white.|
|bottom-right|color|The color for the bottom-right corner of the picture. Defaults to opaque white.|

### Window

A window element is a textured rectangle using border, corner, and content textures to fill.
Resizing the window keeps the border intact.
If the window is resized too small to fit its border textures, it will be skipped during rendering.
Scissoring can be enabled to clamp the content elements inside the window's bounds.
Window elements inherit from pane, so all pane properties also apply here.

The following properties are specified as XML elements:

|Property|Type|Description|
|--------|----|-----------|
|&lt;content&gt;||Defines the properties of the content rectangle, such as dimensions and texture.|
|&lt;palette&gt;|resource, tlut|Specifies the external palette to attach to the textures. If not specified, the texture's embedded palette is used.|
|&lt;corners&gt;||Defines the properties of each corner, such as texture, color, and mirroring.|
|&lt;gradient&gt;|gradient|Specifies the gradient for the textures, enabling gradient mapping. Only useful for intensity-format textures.|

#### &lt;content&gt;

|Property|Type|Description|
|--------|----|-----------|
|&lt;rectangle&gt;|rectangle|Specifies the dimensions of the content rectangle, relative to the window's rectangle.|
|&lt;texture&gt;|resource, timg|Specifies the optional BTI texture used to fill the content rectangle. |

#### &lt;corners&gt;

There are four corners to a window: _top-left_, _top-right_, _bottom-left_, and _bottom-right_.
Each should at least be given a texture; windows cannot render if at least one corner is untextured.

For each corner, the properties are as follows:

|Property|Type|Description|
|--------|----|-----------|
|&lt;texture&gt;|resource, timg|Specifies the BTI texture of this corner.|
|&lt;color&gt;|color|Specifies the blend color of the corner. Default is opaque white.|
|&lt;mirror&gt;|enum|Specifies the axes of the texture to mirror. Defaults to _none_.|

The available mirroring flags are as follows:

|Flag|Description|
|----|-----------|
|none|Neither axes are mirrored.|
|x|The texture's horizontal axis is mirrored.|
|y|The texture's vertical axis is mirrored.|

### Textbox

A textbox element is a rectangle inside which text is rendered.
Textbox elements inherit from pane, so all pane properties also apply here.

The following properties are specified as XML elements:

|Property|Type|Description|
|--------|----|-----------|
|&lt;font&gt;|resource, font|The BFN font resource to use for rendering the text.|
|&lt;text&gt;|string|The default text buffer for the textbox. May be overriden at runtime. |
|&lt;colors&gt;||Specifies the vertex colors for each corner of the quad. Each defaults to opaque white.|
|&lt;binding&gt;||Specifies the alignment for each axis of the text in the textbox.|
|&lt;typesetting&gt;||Specifies typesetting information specific to this textbox, such as scale, spacing, and leading.|
|&lt;gradient&gt;|gradient|Specifies the gradient for the glyph textures, enabling gradient mapping.|

#### &lt;colors&gt;

|Property|Type|Description|
|--------|----|-----------|
|top|color|The color for the top edge of the glyphs. Defaults to opaque white.|
|bottom|color|The color for the bottom edge of the glyphs. Defaults to opaque white.|

#### &lt;binding&gt;

|Property|Type|Description|
|--------|----|-----------|
|horizontal|enum|Specifies the horizontal binding of the text. Defaults to left-alignment.|
|vertical|enum|Specifies the vertical binding of the text. Defaults to left-alignment.|

The available horizontal bindings are as follows:

|Enum|Description|
|----|-----------|
|left|Align each line to the left edge of the textbox.|
|center|Center each line in the textbox.|
|right|Align each line to the right edge of the textbox.|

The available vertical bindings are as follows:

|Enum|Description|
|----|-----------|
|top|Align the text to the top edge of the textbox.|
|center|Center the text vertically in the textbox.|
|right|Align the text to the bottom edge of the textbox.|

#### &lt;typesetting&gt;

|Property|Type|Description|
|--------|----|-----------|
|&lt;spacing&gt;|integer|Specifies the amount of space to add between glyphs (after kerning is applied). Default value is zero.|
|&lt;leading&gt;|integer|Specifies the amount of space between lines. Default value is the font resource's leading, if loaded; otherwise, 20.|
|&lt;width&gt;|integer|Specifies the width by which to scale the glyphs. Default value is the font resource's base width, if loaded; otherwise, 20.|
|&lt;height&gt;|integer|Specifies the height by which to scale the glyphs. Default value is the font resource's base height, if loaded; otherwise, 20.|

### Screen

A screen element represents the root of any BLO hierarchy.
Unlike other BLO elements, the screen element does not inherit any properties.
This element type is supported only as the root element;
any screen elements found at a deeper generation should be silently ignored.

#### &lt;info&gt;

|Property|Type|Description|
|--------|----|-----------|
|&lt;width&gt;|integer|The width of the screen element.|
|&lt;height&gt;|integer|The height of the screen element.|
|&lt;tint&#8209;color&gt;|color|The color of the screen element.May be omitted; if so, defaults to transparent black.|

#### &lt;search&#8209;paths&gt;

|Property|Type|Description|
|--------|----|-----------|
|&lt;path&gt;|string|Specifies a resource search path for this XLO, used to find fonts, textures, palettes, and other referenced files. The only implicit search path is the XLO's containing directory. The path may be absolute or relative to the containing XLO. Any number of &lt;path&gt; elements may be specified.|
