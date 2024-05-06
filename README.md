# sbox-bsp-import
 Editor Tool for decompiling bsp into scene MeshComponent.
 The tool itself is supposed to be used to port an environment into scene. It expects you to have referenced content (models, materials, etc) ported ahead of time. Paths should remain the same.

 ### How to Use:
 Use the Bsp Import Menu in the Editor with an open scene, select your bsp file from disk, wait.

## Features
 - Constructs primitive world geometry as editable MeshComponents into the scene
 - Creates Game Objects for prop and brush entities 

## Planned and Needed features:
 - Displacements; currently displacement faces are skipped entirely
 - Materials and uvs; Materials are possible but vertex uvs arent supported by the current PolygonMesh API. Currently skips toolsskybox.
 - More Entites; basic environment entities alongside probes and lights should probably be translated

## Issues and bugs
 Make issues for bsp's the tool can't decompile, provide the bsp and game info if possible.

## Contribute
 Feel free to contribute via PRs. The decompiler is written in a way where each lump and alterations thereof can be dissected in one type. Check existing Lump types for examples.
 For bsp type reference: https://developer.valvesoftware.com/wiki/BSP_(Source)