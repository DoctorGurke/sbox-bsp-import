# sbox-bsp-import
 Editor Tool for decompiling .bsp files and importing their environment into a scene.
 The tool itself is supposed to be used to port an environment into scene. It expects you to have referenced content (models, materials, etc) ported or replaced ahead of time. Paths should remain the same.

 ### How to Use:
 Use the Bsp Import Menu in the Editor with an open scene, this will open a settings dialog. Select your desired import options and .bsp file, click Import and wait.

## Features
 - Uses the bsp tree to build the world's render mesh, instead of relying on brushes.
 - Rebuild Displacement Meshes with correct orientations.
 - Creates Game Objects for Brush Entities, Meshes and Displacements.
 - Try to create GameObjects & Apply Components for some base entities where appropriate.
 - Separate options to allow for importing only entities, only geometry, or skipping tools materials.

## Planned features:
 - More Entites; basic environment entities should be supported.
 - Clip materials and similar. These need to be reconstructed.
 - Vertex paint from displacements.

## Issues and bugs
 Please make issues for bsp files the tool can't decompile&import. Provide the bsp and game info if possible.

## Contribute
 Feel free to contribute via PRs. The decompiler is written in a way where each lump and alterations thereof can be dissected in one type. Check existing Lump types for examples.
 For bsp type reference: https://developer.valvesoftware.com/wiki/BSP_(Source)
