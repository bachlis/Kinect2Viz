
# Editor Interface {#editortut}

[TOC]

Introduction {#intro}
============

%LineWorks primary method of interaction will be through the Unity Editor Inspector Window. This page will go over how to interface with LineWork through the Inspector and through the Scene View.

LW_Linework Component {#component}
===================

All %LineWorks elements will be managed by a [LW_Linework][] Component.  This is the only Component you need to interact with (There is a [LW_MaterialManager][] Component but it is auto-genereated and can be safely ignored). To begin, create an Empty GameObject and add the [LW_Linework][] Component. The [LW_Linework][] component will automatically manager the required rendererd, filters, materials, and colliders. The Component's inspector is divided into three (3) primary sections: [Component Properties][], [Graphic Properties][]], and [Style Properties][].

![LW_Linework Overview](screenshots/ss_01-01.png)

Component Properties {#componentprop}
--------------------

The component properties control the global properties for the [LW_Cavas][]. These properties will effect how all graphic elements are rendered unless an individual graphic overrides them.

### Canvas Properties {#canvasprop}

![Canvas Properties](screenshots/ss_02-02.png)

The canvas properties control the boundaries of the canvas and how that boudary should be scaled relative to the gameObject’s RectTransform. The LineWorks canvas is a similar concept to an images canvas in [Photoshop](https://helpx.adobe.com/photoshop/using/adjusting-crop-rotation-canvas.html) or the viewport and view box concept in the [SVG standard](http://tutorials.jenkov.com/svg/svg-viewport-view-box.html).

-	[View Box][] the Rect area for the display of LineWorks elements.
-	[Scale Mode][] a scale mode to control how LineWorks elements will be scaled.
-	[Set Rect To ViewBox][] Sets the RectTransform dimensions to match the ViewBox.
-	[Set Pivot To ViewBox][] Set the RectTransform pivot to match the ViewBox.
-	[Set ViewBox To Rect][] Sets the ViewBox to match the RectTransform.
-	[Set ViewBox to Bounds][] Sets the ViewBox to Fit the mesh bounds.

### Material Properties {#materialprop}

![Material Properties](screenshots/ss_02-03.png)

The material properties control the overall appearance for all graphic elements. In particular this is where shader-based features can be enabled. Many of these properties can be overridden at each individual style element.

-	[Sorting Layer ID][] the sorting layer on the MeshRenderer.
-	[Sorting Order][] the sorting order on the MeshRenderer.
-	[Vertex Color][] the vertex color.
-	[Custom Material][] the custom material.
-	[Main Texture][] the material texture.
-	[Blend Mode][] the shader blend mode.
-	[Feature Mode][] the shader feature mode.
-	[Stroke Draw Mode][] the shader stroke draw mode.
-	[Stroke Scale Mode][] the shader stroke scale mode.
-	[Join And Caps Mode][] the shader joins and caps mode.
-	[Gradients Mode][] the shader gradients mode.
-	[Anti-Aliasing Mode][] the shader anti-aliasing mode.

> [Main Texture][], [Blend Mode][], and [Feature Mode][] require [Custom Material][] to be empty.
> [Stroke Mode][], [Join Mode][], [Gradient Mode][], and [AntiAliasing Mode][] require [Feature Mode][] to be set to `Advanced`. If [Feature Mode][] is set to `Simple`, [Stroke Mode][], [Join Mode][], [Gradient Mode][], and [AntiAliasing Mode][] will be disabled.

### Level Of Detail Properties {#lodprop}

![LOD Properties](screenshots/ss_02-04.png)

The level of detail (LOD) properties control the overall level of detail for all graphic elements. These properties can be overridden at each individual style element.  There primary used is to control the balance between the total number of vertices and how segmented a curve will look.

-	[Segmentation][] the value for curve segmentation.
-	[Simplification][] the value for vertex simplification.

### Graphics Hierarchy {#graphicshierarchy}

![Graphics Hierarchy](screenshots/ss_03-05.png)

The graphic hierarchy is used to built a hierarchical tree o [LW_Graphic][] elements.  The hierarchy is comparable to the layers palette in [Illustrator](https://helpx.adobe.com/illustrator/using/layers.html).

Edit or Add to the hierarchy by clicking on the dropdown next to any [LW_Graphic] element (look for a dropdown that says "Menu"). Individual elements can be further edited by checking the checkbox next to each shape (i.e. selecting them).

The following [LW_Graphic][] elements are possible:

-	[LW_Circle][] defines a circle shape.
-	[LW_Ellipse][] defines an ellipse shape.
-	[LW_Rectangle][] defines a rectangle shape.
-	[LW_Line][] defines a line.
-	[LW_Polygon][] defines a regular polygon shape.
-	[LW_Star][] defines a star shape.
-	[LW_Polyline2D][] defines a two(2) dimensional polyline shape.
-	[LW_Polyline3D][] defines a three(3) dimensional polyline shape.
-	[LW_Path2D][] defines a two(2) dimensional bezier path shape.
-	[LW_Path3D][] defines a three(3) dimensional bezier path shape.
-	[LW_Group][] defines a group of shapes.

Graphic Properties {#graphicprop}
------------------

Graphic Properties show the properties of the currently selected [LW_Graphic][] in the Graphics Hierachy.

### General Graphic Properties {#generalprop}

![General Graphic Properties](screenshots/ss_03-02.png)

The general graphic properties are properties that are common to all [LW_Graphic][] elements.

-	[Name][] the element's name.
-	[Is Visible][] a value indicating whether this <see cref="LineWorks.LW_Element"/> is visible.
-	[Matrix][] the transformation matrix.
-	[Reverse Direction][] a value indicating whether the points of this <see cref="LineWorks.LW_Shape"/> should be iterated in reverse order.
-	[Editor Color][] a value indicating whether the points of this <see cref="LineWorks.LW_Shape"/> form a closed loop.

### Specific Shape Properties (Different for every LW_Shape type).

![Circle Properties](screenshots/ss_03-03.png)

The specific shape properties are difference for every shape and provide access to properties necessary to define the particular shape type.

See the [Scripting API][] for properties related to a specific shape type.

### Styles Collection

![General Style Properties](screenshots/ss_03-04.png)

Each [LW_Graphic][] element contains a collection of [LW_Style][] elements. Each [LW_Graphic][] element can have multiple style elements attached. This is how you control how the geometry defined by this [LW_Graphic][] is rendered. The collection is comparable to the "appearance" palette in [Illustrator](https://helpx.adobe.com/illustrator/using/appearance-attributes.html).

Edit or Add to the collection by clicking on the dropdown next to any [LW_Style][] element (look for a dropdown that says "Menu"). Individual elements can be further edited by checking the checkbox next to each shape (i.e. selecting them).

The following [LW_Style][] elements are possible:

-	[LW_Fill][] defines the appearance of how a shape is filled.
-	[LW_Stroke][] defines the appearance of how a line is drawn along the path of a shape.
-	[LW_Marker][] defines the placement of a provided [LW_Graphic][] element along the path of a shape.
-	[LW_Collider][] defines if a 2DCollider Component should be created for the shape.

> [LW_Style][]

Style Properties {#style}
----------------

Style Properties show the properties of the currently selected [LW_Style][]. 

### General Style Properties

![General Style Properties](screenshots/ss_03-06.png)

The general style properties are properties that are common to all [LW_Style][] elements.

-	[Name][] the element's name.
-	[Is Visible][] a value indicating whether this <see cref="LineWorks.LW_Element"/> is visible.
-	[Lateral Offset][] The lateral Offset from the 
-	[Vertical Offset][] The Vertical
-	[Segmentation Multiplier][] Multiplies the LineWorks.LW_Linework.segmentation by this value.
-	[Simplification Multiplier][] Multiplies the LineWorks.LW_Linework.simplification by this value.

### Paint Properties

![Paint Properties](screenshots/ss_03-07.png)

The paint properties control the appearance of [LW_Fill][] and [LW_Stroke][] style types.

-	[Custom Material Override][] the local custom Material.
-	[Main Texture Override][] the local Material texture.
-	[UV Tiling][] the mainTexture's UV tiling.
-	[UV Offset][] the mainTexture's UV offset.
-	[UV Mode][] how texture UV's are calculated.
-	[Opacity][] the global opacity.
-	[Paint Mode][] the method used to paint a style.
-	[Color][] the vertex color.
-	[Gradient Colors][] a list of Color Stops to control the color of a line along the length of the line.
-	[Gradient Transform][] the transformation matrix for the placement of the gradientStart and gradientEnd.
-	[Gradient Units][] how a gradient's position is defined.
-	[Gradient Start][] the start point for a linear gradient or the center point for a radial gradient.
-	[Gradient End][] the end point for a linear gradient or a point on the outer ring of a radial gradient.

### Specific Style Properties

![Specific Style Properties](screenshots/ss_03-08.png)

The specific style properties are difference for every style and provide access to properties necessary to control the appearance of a particular style type.

See the [Scripting API][] for properties related to a specific style type.

[View Box]: @ref LineWorks.LW_Linework.viewBox
[Scale Mode]: @ref LineWorks.LW_Linework.scaleMode
[Set Rect To ViewBox]: @ref LineWorks.LW_Linework.SetRectSizeToViewBox
[Set Pivot To ViewBox]: @ref LineWorks.LW_Linework.SetRectPivotToViewBox
[Set ViewBox To Rect]: @ref LineWorks.LW_Linework.SetViewBoxToRect
[Set ViewBox to Bounds]: @ref LineWorks.LW_Linework.SetViewBoxToBounds

[Sorting Layer ID]: @ref LineWorks.LW_Linework.sortingLayerId
[Sorting Order]: @ref LineWorks.LW_Linework.sortingOrder
[Vertex Color]: @ref LineWorks.LW_Linework.color
[Custom Material]: @ref LineWorks.LW_Linework.material
[Main Texture]: @ref LineWorks.LW_Linework.mainTexture
[Blend Mode]: @ref LineWorks.LW_Linework.blendMode
[Feature Mode]: @ref LineWorks.LW_Linework.featureMode
[Stroke Draw Mode]: @ref LineWorks.LW_Linework.strokeDrawMode
[Stroke Scale Mode]: @ref LineWorks.LW_Linework.strokeScaleMode
[Joins And Caps Mode]: @ref LineWorks.LW_Linework.joinsAndCapsMode
[Gradients Mode]: @ref LineWorks.LW_Linework.gradientsMode
[Anti-Aliasing Mode]: @ref LineWorks.LW_Linework.antiAliasingMode

[Segmentation]: @ref LineWorks.LW_Linework.segmentation
[Simplification]: @ref LineWorks.LW_Linework.simplification

[Name]: @ref LineWorks.LW_Element.name
[Is Visible]: @ref LineWorks.LW_Element.isVisible
[Matrix]: @ref LineWorks.LW_Graphic.matrix
[Reverse Direction]: @ref LineWorks.LW_Shape.reverseDirection
[Editor Color]: @ref LineWorks.LW_Shape.editorColor

[Lateral Offset]: @ref LineWorks.LW_Style.lateralOffset
[Vertical Offset]: @ref LineWorks.LW_Style.verticalOffset
[Segmentation Multiplier]: @ref LineWorks.LW_Style.segmentationMultiplier
[Simplification Multiplier]: @ref LineWorks.LW_Style.simplificationMultiplier

[Custom Material Override]: @ref LineWorks.LW_PaintStyle.material
[Main Texture Override]: @ref LineWorks.LW_PaintStyle.mainTexture
[UV Tiling]: @ref LineWorks.LW_PaintStyle.uvTiling
[UV Offset]: @ref LineWorks.LW_PaintStyle.uvOffset
[UV Mode]: @ref LineWorks.LW_PaintStyle.uvMode
[Opacity]: @ref LineWorks.LW_PaintStyle.opacity
[Paint Mode]: @ref LineWorks.LW_PaintStyle.paintMode
[Color]: @ref LineWorks.LW_PaintStyle.color
[Gradient Colors]: @ref LineWorks.LW_PaintStyle.gradientColors
[Gradient Transform]: @ref LineWorks.LW_PaintStyle.gradientTransform
[Gradient Units]: @ref LineWorks.LW_PaintStyle.gradientUnits
[Gradient Start]: @ref LineWorks.LW_PaintStyle.gradientStart
[Gradient End]: @ref LineWorks.LW_PaintStyle.gradientEnd

[Component Properties]: @ref componentprop
[Graphic Properties]: @ref graphic
[Style Properties]: @ref style

[Scripting API]: @ref scripttut

[LW_MaterialManager]: @ref LineWorks.LW_MaterialManager
[LW_Linework]: @ref LineWorks.LW_Linework

[LW_Graphic]: @ref LineWorks.LW_Graphic
[LW_Circle]: @ref LineWorks.LW_Circle
[LW_Ellipse]: @ref LineWorks.LW_Ellipse
[LW_Rectangle]: @ref LineWorks.LW_Rectangle
[LW_Line]: @ref LineWorks.LW_Line
[LW_Polygon]: @ref LineWorks.LW_Polygon
[LW_Star]: @ref LineWorks.LW_Star
[LW_Polyline2D]: @ref LineWorks.LW_Polyline2D
[LW_Polyline3D]: @ref LineWorks.LW_Polyline3D
[LW_Path2D]: @ref LineWorks.LW_Path2D
[LW_Path3D]: @ref LineWorks.LW_Path3D
[LW_Group]: @ref LineWorks.LW_Group

[LW_Style]: @ref LineWorks.LW_Style
[LW_Fill]: @ref LineWorks.LW_Fill
[LW_Stroke]: @ref LineWorks.LW_Stroke
[LW_Marker]: @ref LineWorks.LW_Marker
[LW_Collider]: @ref LineWorks.LW_Collider
[LW_Styles]: @ref LineWorks.LW_Styles

[LW_Element]: @ref LineWorks.LW_Element
