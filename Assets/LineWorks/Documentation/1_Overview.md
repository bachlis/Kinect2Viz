
# Overview # {#mainpage}

[TOC]

# Introduction # {#intro}

%LineWorks is a vector graphics package designed to be powerful, flexible, and easy-to-use. %LineWorks was created specifically for Unity to enable high-performance vector graphics.  %LineWorks offers a robust feature set with shader assisted high performance and a simple and easy-to-learn Editor interface.  With %LineWorks you will be able to easily:

-	Create high-performance line renderers (similar to Vectrosity).
-	Create complex and editable vector artwork direct in the editor (similar to RageSpline).
-	Create resolution-independent GUIs (fully integrated with the Unity UI).
-	Create dynamic 3D paths, roads, fences, rivers, etc. (similar to MegaShapes).
-	Import your SVG vector artwork directly from Illustrator, Inkscape, etc. (similar to SVG Importer or Simply SVG).

## Features ## {#features}

-	In-editor tools for the creation of vector graphics without scripting.
-	Simple Scripting API for the the creation of dynamic vector graphics at runtime.
-	Fully integrated with the new Unity UI for creating fully resolution independent UI’s.
-	Mesh Modeling options for creating fully editable and interact-able 3D meshes.
-	Shader based anti-aliasing for smooth line edges without using MSAA or FXAA.
-	Shader based dynamic widths and gradients for high-performance dynamic lines.
-	Shader based rendering of dynamic 3D lines (lines always face the camera) with no CPU overhead.
-	Works with any material (%LineWorks material is required only for the shader base features and gradients).
-	A complete replacement for the built-in LineRenderer, TrailRenderer, and other 3rd-party vector solutions.
-	Support for SVG importing (shapes, styles, and transforms are fully supported).

## Demos ## {#demos}

-	[WebPlayer](http://plp3d.com/LineWorks/Builds/WebPlayer/index.html "WebPlayer Demo")
-	[WebGL](http://plp3d.com/LineWorks/Builds/WebGL/index.html "WebGL Demo")

## Release Notes for 1.1.1 ## {#releasenotes}

-	Switched Third Party Library, LibTessDotNet, to version v1.0.55.
-	Added [optimize][] bool on LW_Canvas to optionally allow the simplification algorithm to run with a simplification value of 0.
-	Added Unity 5.4 support.
-	Optimized %LineWorks shader and split %LineWorks logic into cginc file so custom %LineWorks shaders can easily be created.
-	Removed pre-optimization of contour points as it was not always creating intended results.
-	Fixed bugs with how materials are pooled, shared, and rendered.
-	Switched styleData texture to use RGBAFloat format instead of RGBAHalf. Both have pros and cons but RGBAFloat seems to be more reliable.
-	Editor Inspector window remembers the state of foldouts. 
-	added [spaceWidthsEvenly][] and [spaceColorsEvenly][] to LW_Stroke. When true, these properties allow for the use of variable widths and gradients without supplying a percentage with the LW_WidthStop and LW_ColorStop, respectively.
-	Fixed a couple bugs with the SVG importer.

## Release Notes for 1.1.0 ## {#releasenotes}

-	Memory - Significantly reduced unnecessary memory allocations (There might still be room for some improvement).
-	Memory - Reduced the number of vertices needed for strokes without joins and textures by half.
-	Memory - Added [presizeVBO][] int to all LW_Style elements to pre-size the buffers to avoid allocations.
-	GUI - Added [useAccurateRaycasting][] boolean to LW_Canvas to toggle accurate testing for UI clicks on a %LineWorks Element.
-	API - Added Implicit conversions from Vector2 to/from LW_Point2D and Vector3 to/from LW_Point3D.
-	API - Added Methods to find the location at a percentage or a fixed length along a shape.
-	LOD - Override LOD on individual style element has been replaced with segmentation and simplification multipiers.
-	Shader - Override Material and it's subsiquent shader features on individual style elements were removed to reduce confusion. If you would like an individual style to have a different material or different shader features, create a material and set it on the individual style. Otherwise, shader features will only be set at the LW_Canvas level. 
-	Shader - Wording and the functionality of some of the Advanced Shader Features has changed.
-	Shader - Added a [strokeScaleMode][] property to better control how stroke widths are rendered.
-	Shader - Linework elements with a non-%LineWorks shader or the featureMode set to Simple will by-pass all shader calculations, including gradients. Previously, gradients were still handled by the shader.
-	Materials - Bug where occasionally, too many materials where being created has been fixed.
-	Materials - The singleton component, LW_MaterialManager, has been removed and replaced with a static LW_MaterialPool class. Any remaining LW_MaterialManager GameObjects can be safely removed.
-	ViewBox - Added some functions to help control the size and location of the viewBox in relation to the RectTransform.
-	Markers - Completely reworked Markers with several new features.
-	Strokes - Fixed bug with line failure when a line contains points in the same location.

[optimize]: @ref LineWorks.LW_Canvas.optimize
[spaceWidthsEvenly]: @ref LineWorks.LW_Stroke.spaceWidthsEvenly
[spaceColorsEvenly]: @ref LineWorks.LW_Stroke.spaceColorsEvenly
[presizeVBO]: @ref LineWorks.LW_Style.presizeVBO
[useAccurateRaycasting]: @ref LineWorks.LW_Canvas.useAccurateRaycasting
[strokeScaleMode]: @ref LineWorks.LW_Canvas.strokeScaleMode

* * *

# Installation # {#install}

After importing the %LineWorks package from the Asset Store a single folder named "%LineWorks" should be added to your project.  If %Lineworks should ever have to be removed or updated for a project, all of the files are located in this folder.  Within the "%LineWorks folder there are for following folders:

-	**Editor**: Home of all required editor scripts and gizmos. Users should not need to mess with anything in there.
-	**Examples**: Contains several examples of how %LineWorks might be utilized. Open the "Scenes" folder to load the examples. This folder is not required and can safely be deleted.
-	**Plugins**: Contains all of the object classes and helper classes required by %LineWorks.
-	**Shaders**: Contains the %LineWorks shader.

* * *

# Getting Started # {#getstarted}

Before beginning to use %LineWorks there are a fews that should be explained to help you understand how %LineWorks operates.

There are 3 primary ways to use %LineWorks: 
1.	[Editor Interface][]
2.	[Scripting API][]
3.	[SVG Importing][]

## Editor Interface ## {#ineditor}

The [LW_Canvas][] Component is the only %LineWorks component needed. Add it to an empty GameObject to create, control and edit all vector graphics.  To work in the Editor, just add a [LW_Canvas][] component to an empty GameObject (it will manager the required renderers, filters, and colliders). Add [LW_Graphic][] elements by clicking on the dropdown next to the default [LW_Group][] element (look for a dropdown that says "Menu"). With [LW_Graphic][] elements added, they can be edited by checking the checkbox next to each [LW_Graphic][] element (i.e. selecting them). Add [LW_Style][] elements to individual [LW_Graphic][] by using the dropdown next to the default [LW_Styles][] element on the selected [LW_Graphic][]. For a more detailed explanation, check out the [Editor Interface][] page.

## Scripting API ## {#inscript}

To work with the scripting API, just use the static "Create" methods to quickly instantiate a new %LineWorks element. To get started, create a [LW_Canvas][] component (a gameObject will be automatically created with the [LW_Canvas][] attached), a [LW_Graphic][] element, and a [LW_Style][] element. Then add the style elment to the graphic element and add the graphic element to the canvas. For a more detailed explanation, check out the [Scripting API][] page.


## SVG Importing ## {#import}

To work with Importing SVGs, just drag your SVG files to the project and they should automatically create a prefab ready to drag into your scene. If you delete the prefab or modify the SVG file you can reimport it by right-clicking the SVG asset and selecting "Reimport". For a more detailed explanation, check out the [SVG Importing][].

[Editor Interface]: @ref editortut
[Scripting API]: @ref scripttut
[SVG Importing]: @ref svgtut

[LW_Canvas]: @ref LineWorks.LW_Canvas
[LW_Group]: @ref LineWorks.LW_Group
[LW_Graphic]: @ref LineWorks.LW_Graphic
[LW_Styles]: @ref LineWorks.LW_Styles
[LW_Style]: @ref LineWorks.LW_Style

* * *

# Known Issues # {#issues}

-	SVG import does not cover the full SVG standard. in particular, filters, animations, clipping masks, text, images, pattern textures are not currently supported. We hope to add more compliance in the future.
-	Older iOS devices with the PowerVR SGX Hardware have a performance issue with how the default pixel shader calculates gradients.  %LineWorks can still be used on this older hardware but it is recommended to switch "gradientMode" to "Off".
-	Color gradients on iOS devices show color banding (not smoothly blended) due to a bug with the bilinear filtering on iOS.  We will be updating %LineWorks to work around this issue if the bug is not addressed.
-	%LineWorks does alot of work to try to share materials that have the same properties while respecting the sorting order of the graphics.  Occasionally during heavy editing especially when you have changed a property that effects the shader or it's sortingOrder, the wrong color or material will be applied to an element. Typically a quick save and reload of the scene will correct the issue.
-	Occasionally, when the "antiAliasingMode" is set to "On", the geometry has a very sharp/acute angle, and a fill is applied to the geometry, artifacts may occur when viewing the graphic at a very small scale.  This issue can be avoided by disabling "antiAliasingMode", avoiding acute angles, or avoiding small scale factors.
-	WebGL is producing the occasional issue. We're working on it.

* * *

# Toubleshooting # {#toubleshooting}

Should you have any questions, issues, or feedback about %LineWorks please feel free to contact <mike@plp3d.com>.
