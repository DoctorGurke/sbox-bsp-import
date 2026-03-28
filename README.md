# sbox-bsp-import
 Editor Tool for decompiling bsp into scene MeshComponent.
 The tool itself is supposed to be used to port an environment into scene. It expects you to have referenced content (models, materials, etc) ported ahead of time. Paths should remain the same.

 ### How to Use:
 Use the Bsp Import Menu in the Editor with an open scene, this will open a settings dialog. Select your desired import options and .bsp file, click Import and wait.

## Features
 - Creates Game Objects for Entities, world geometry and displacements.
 - Construct bsp world geometry and displacements.
 - Construct entities where appropriate, features only basic support right now.
 - Separate options to allow for importing only entities, only geometry, or skipping tools materials.

## Potential features:
 - Vertex paint from displacements.
 - More Entites; basic environment entities alongside probes and lights
 - Clip materials and similar. These need to be reconstructed.

## Issues and bugs
 Make issues for bsp's the tool can't decompile, provide the bsp and game info if possible.

## Contribute
 Feel free to contribute via PRs. The decompiler is written in a way where each lump and alterations thereof can be dissected in one type. Check existing Lump types for examples.
 For bsp type reference: https://developer.valvesoftware.com/wiki/BSP_(Source)
