XR Prototype Kit
Ted Brown
MIT License

XrProtoypeKit (XPK) is an Unity abstraction layer for platform-specific implementations of XR features, including:
* 6DOF Controllers
* Spatial Anchors
* World Mesh Reconstruction

It also includes useful, robust solutions for common needs, including:
* Persistent Content (bound to Spatial Anchors)
* Menus
* Finite State Machines
* File I/O
* ... and other useful utilities

At this point, only Magic Leap and Editor are implemented.

--- PLUGIN SETUP ---
If you want to use this as part of another Unity project, while maintaining the ability to push and pull changes, it's suggested that you follow these steps.
All of the given directory names are suggestions.
1. Create a directory for external Git projects (e.g. [ProjectName]/Submodules) (it should be a sibling of [ProjectName]/Assets)
2. Clone this repo into the Submodules directory
3. Create a Plugins directory in your Unity project (e.g. [ProjectName]/Assets/Plugins)
4. Make a symbolic link to Submodules/XrProtoypeKit/Assets/XrProtoypeKit/ in Plugins

--- PROJECT SETUP ---
Whether using this as a standalone project or a plugin, the following changes need to be in place.
1. Add a World, Body, and Content layer
