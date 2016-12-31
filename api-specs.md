## blojob API Documentation

The public _blojob_ API allows for creating, loading, saving, rendering, and manipulation of BLO elements from various formats.
It is inspired by the API seen in Nintendo's "JSystem" library, as well as classes from the various games utilizing it.
For rendering, it requires an active OpenGL contex to be initialized and made current on the same thread from which the _blojob_ API calls are made.
The API is not thread-safe; it is the user's responsibility to block racing conditions.

#### Name Conversion

BLO elements are referenced by names, which are internally an unsigned, 32-bit integer.
You may generate a name from a string and vice versa using the static conversion methods on the `bloPane` class:

```cs
public static uint convertStringToName(string str);
public static string convertNameToString(uint name);
```

The string should be no more than 4 characters; if longer, only the last 4 will be used.
Each character should be within the 0&#8209;255 range.

### Creating elements at runtime

If you do not want to load a BLO from a stream, but rather create them at runtime, the constructors of the various element classes allow you to do just this.

#### Pane

The `bloPane` class exposes the following constructors:

```cs
public bloPane(uint name, bloRectangle rectangle);
public bloPane(bloPane parentPane, uint name, bool visible, bloRectangle rectangle);
```

The element will be initialized with the quad specified by `rectangle`.
The `name` parameter allows the element to be referenced later via the `search` method.
`visible` sets the element's initial visibility.
`parentPane` allows the element to be a child of the specified element.
If not `null`, this makes the element's rectangle and alpha relative to the parent.

#### Picture

The `bloPicture` class exposes the following constructors:

```cs
public bloPicture(bloTexture texture);
public bloPicture(uint name, bloRectangle rectangle, bloTexture texture, bloPalette palette);
```

In any case, `texture` assigns a graphic to the element and must not be `null`.
In the first overload, the element's rectangle will be initialized to the size of the given texture; otherwise, `rectangle` is used.
The `name` parameter allows the element to be referenced later via the `search` method.
`palette` allows to override the palette in `texture`; otherwise, the texture's embedded palette is used.

#### Window

The `bloWindow` class exposes the following constructors:

```cs
public bloWindow(bloTexture texture, bloTextureBase tbase);
public bloWindow(bloTexture topLeft, bloTexture topRight, bloTexture bottomLeft, bloTexture bottomRight);
public bloWindow(uint name, bloRectangle rectangle, bloTexture texture, bloTextureBase tbase, bloPalette palette);
public bloWindow(uint name, bloRectangle rectangle, bloTexture topLeft, bloTexture topRight, bloTexture bottomLeft, bloTexture bottomRight, bloPalette palette);
```

By default, a window element is initialized with an empty rectangle;
you may override this by using an overload with the `rectangle` parameter.
The `name` parameter allows the element to be referenced later via the `search` method.
A window has a texture for each corner, specified by the `topLeft`, `topRight`, `bottomLeft`, and `bottomRight` parameters.
You may also use a single texture for each corner;
in this case, `tbase` specifies which corner `texture` represents.
The texture will be mirrored appropriately to fit the remaining corners.

#### Textbox

The `bloTextbox` class exposes the following constructors:

```cs
public bloTextbox(uint name, bloRectangle rectangle, bloFont font, string text, bloTextboxHBinding hbind, bloTextboxVBinding vbind);
```

The `rectangle` parameter specifies the textbox's rendering area.
Text will wrap and align based on this rectangle.
`font` specifies the font to use for text rendering and encoding.
Lines of text rendered by the element will be aligned on both axes according to `hbind` and `vbind`.

> **Note:** if `font` is `null`, an empty text buffer will be assigned to the textbox and no text will be rendered with `draw`.
> If a valid font is assigned later, you must also assign a new text buffer using the `bloFont.encode` method.

### Loading from and saving to streams

It is very easy to load and save BLO.
The loading and saving APIs utilize .NET streams, so you may load and save a BLO to any type of stream.

To load a BLO, you must call one of the static loading methods in the `bloScreen` class,
whereas the BLO-saving methods are instance methods on the `bloScreen` class:

```cs
public static bloScreen loadCompact(Stream stream);
public static bloScreen loadBlo1(Stream stream);
public static bloScreen loadXml(Stream stream);

public void saveCompact(Stream stream);
public void saveBlo1(Stream stream);
public void saveXml(Stream stream);
```

The difference among them is the format of the BLO in the stream. If the BLO fails to load, the return value will be `null`; otherwise, it is the root `bloScreen` element.

### Rendering BLO elements

To draw a BLO, you simply call a `draw` method on the root `bloScreen` element.
You may also call `draw` on any other nested element to start rendering from that element. Child elements will automatically be rendered where appropriate depending on their visibility and other factors.

> **Note:** the rendering methods do not set up the GL viewport, projection, render state, etc.
This responsibility lies on the user.

#### Context

Various global state is stored in the `bloContext` class.
You may get or set the context used by rendering methods by using the static methods on the `bloContext` class:

```cs
public static bloContext getContext();
public static bloContext setContext(bloContext context);
```

The setter method returns the previous context instance (e.g. to restore it later).
You must create a context and set it as the current before calling any rendering API, as the default context is `null`.

#### Shaders

The rendering code uses shaders for all textured surfaces.
The shaders must be initialized at least once before calling any rendering API.
You may set the shaders to use by calling the following method:

```cs
public bool setShaders(glShader fragment, glShader vertex);
```

The fragment and vertex shaders should contain the following uniforms:

```glsl
uniform sampler2D texture[4];
uniform vec4 blendColorFactor, blendAlphaFactor;
uniform int transparency[4];
uniform int textureCount;
uniform vec4 fromColor;
uniform vec4 toColor;
```

|Uniform|Description|
|-------|-----------|
|texture|An array of 4 `sampler2D` for multitexturing. Only `textureCount` samplers are guaranteed to be valid.|
|blendColorFactor|The blend factors for the RGB components. These are pre-normalized, where `blendColorFactor[n]` represents the mix percentage between the previous color and `texture[n]`.|
|blendAlphaFactor|The blend factors for the alpha components. These are pre-normalized, where `blendAlphaFactor[n]` represents the mix percentage between the previous alpha and `texture[n]`.|
|transparency|An array of 4 `int`. These are boolean flags for each texture to specify if the sampled texel should ignore the alpha (non-zero) or not (zero). If the alpha is ignored, it should be set to one (opaque).|
|textureCount|Set to the number of textures. Guaranteed to be at least one. This can be ignored for elements that do not support multitexturing.|
|fromColor|The from-color in the gradient (r, g, b, a).|
|toColor|The to-color in the gradient (r, g, b, a).|

Gradient mapping, if supported by the element, should be applied if `fromColor` is not a zero vector or `toColor` is not a unit vector.
It should be done after multitexturing but before the vertex color is applied.

### Manipulation and interpolation

There are several classes to manipulate BLO elements at runtime.
You may also use the public methods on the various element classes themselves to change their state.

#### Multitexturing

The `bloPicture` class has several methods to create multitexture surfaces.
This feature is a runtime only feature, as BLO hierarchies loaded from streams can specify only a single texture per picture element.

A total of four texture "slots", or "channels", may be assigned to a single `bloPicture`.
The following methods allow inserting, changing, and removing texture slots:

```cs
public bool insert(bloTexture texture, int slot, double factor);
public bloTexture changeTexture(bloTexture texture, int slot);
public bool remove(int slot);
```

The `insert` method inserts `texture` at the specified slot.
`slot` must be in the range of [0, current slot count].
`factor` represents the blending factor for this new texture, in the range of zero to one.
The return value indicates if the texture was successfully inserted into the specified slot.
This can be `false` if any of the parameters are invalid or if all texture slots are taken.

The `changeTexture` method allows you to simply change a texture on an already existing slot.
`slot` must be in the range of [0, current slot count).

The `remove` method simply removes the slot at the specified index.
`slot` must be in the range of [0, current slot count).

To further customize the blending factors, and to set the color and alpha factors separately, the following methods exist:

```cs
public void setBlendFactor(double factor, int slot);
public void setBlendFactor(double colorFactor, double alphaFactor, int slot);
public void setBlendColorFactor(double factor, int slot);
public void setBlendAlphaFactor(double factor, int slot);

public void setBlendKonstColor();
public void setBlendKonstAlpha();
```

The first four methods change the blend factors for either color, alpha, or both.
`slot` must be in the range of [0, current slot count).

After making changes to the blend factors, you must call the `setBlendKonstColor` and/or `setBlendKonstAlpha` functions to update the values for the shader.
These functions normalize the blend factors by calculating their sum and dividing each slot's factor by this sum.

#### ExPane

The `bloExPane` class linearly interpolates a BLO element overtime.
It can fade, translation, and resize the element.

To use it, first you must attach an element to it using one of the constructor overloads.

```cs
public bloExPane(bloPane pane);
public bloExPane(bloScreen screen, uint name);
```

You may assign a `bloPane` instance directly, or search for one in a `bloScreen` instance by name.
Once assigned, you may call any of the following methods to actually begin an interpolation:

```cs
public bool update();

public void setPaneOffset(int steps, int xTo, int yTo, int xFrom, int yFrom);
public void setPaneSize(int steps, int wTo, int hTo, int wFrom, int hFrom);
public void setPaneAlpha(int steps, int to, int from);
```

The `steps` parameter represents how many updates it will take to finish the interpolation.
Only one interpolation of each property (offset, size, alpha) may be set at a time, but any number of the properties may be interpolated simultaneously.

The to- and from- parameters specify the starting and ending values.
For position and size, they are relative to the pane's initial rectangle when assigned to the `bloExPane`.

The `update` method should be called every update frame in your program's loop, and returns `true` if all properties have finished interpolating.

#### BoundPane

The `bloBoundPane` class interpolates BLO elements in a three&#8209;point, non&#8209;linear curve.
You can position and resize an element, but not fade alpha, with this class.

Similar to `bloExPane`, you must first attach an element to it before using it by calling one of the constructor overloads:

```cs
public bloBoundPane(bloPane pane);
public bloBoundPane(bloScreen screen, uint name);
```

You may assign a `bloPane` instance directly, or search for one in a `bloScreen` instance by name.
Once assigned, you may call any of the following methods to actually begin an interpolation:

```cs
public virtual bool update();

public void setPanePosition(int steps, bloPoint top, bloPoint mid, bloPoint bot);
public void setPaneSize(int steps, bloPoint top, bloPoint mid, bloPoint bot);
```

The `steps` parameter represents how many updates it will take to finish the interpolation.
Only one interpolation of each property (position, size) may be set at a time, but any number of the properties may be interpolated simultaneously.

The `top,` `mid`, and `bot` parameters specify the 2D points in the curve.
The interpolation begins at `top` and ends at `bot`, meeting `mid` at the midpoint with an exponential curve.

The `update` method should be called every update frame in your program's loop, and returns `true` if all properties have finished interpolating.

#### BlendPane

The `bloBlendPane` class inherits from `bloBoundPaen`; as such, it also interpolates BLO elements in a three-point, non-linear curve.
On top of positioning and resizing an element, you can also blend between two textures over time with this class.

Similar to `bloExPane`, you must first attach an element to it before using it by calling one of the constructor overloads:

```cs
public bloBlendPane(bloPane pane);
public bloBlendPane(bloScreen screen, uint name);
```

You may assign a `bloPane` instance directly, or search for one in a `bloScreen` instance by name.
Once assigned, you may call any of the following methods to actually begin an interpolation:

```cs
public override bool update();

public void setPaneBlend(int steps, bloTexture to, bloTexture from);

// inherited from bloBoundPane
public void setPanePosition(int steps, bloPoint top, bloPoint mid, bloPoint bot);
public void setPaneSize(int steps, bloPoint top, bloPoint mid, bloPoint bot);
```

The `steps` parameter represents how many updates it will take to finish the interpolation.
Only one interpolation of each property (position, size, blend) may be set at a time, but any number of the properties may be interpolated simultaneously.

The `setPaneBlend` method blends a `bloPicture` between the textures `from` and `to` over `steps` updates.
`from` may be null; in which case, the blend occurs instantly and no interpolation occurs.
Note that this particular method is enabled only if the assigned element is derived from `bloPicture`.
The other methods work on all element types.

> **Note:** The attached `bloPicture` should already have a second texture slot inserted via its `insert` merthod.
This class does not implicitly add a second texture to the attached element.

The `update` method should be called every update frame in your program's loop, and returns `true` if all properties have finished interpolating.
