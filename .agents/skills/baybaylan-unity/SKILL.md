---
name: baybaylan-unity
description: Project-specific Unity guidance for the Baybaylan repository only. Use for any Unity editor, package, scene, prefab, asset, build, or test work in this repository; never apply it to another project.
---

# Baybaylan Unity Guidance

- Use Unity `6000.0.48f1` with revision `170d2541580d` unless the user explicitly approves a coordinated team upgrade.
- Treat `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, and `Packages/packages-lock.json` as one compatibility set. Do not accept automatic editor or package upgrades in isolation.
- Preserve Unity `.meta` files with their assets. Before changing scenes, prefabs, or imported assets, inspect existing uncommitted changes and avoid overwriting teammates' work.
- Keep the project on Universal Render Pipeline `17.0.4` and the Unity 6.0 compatible 2D package set while the editor baseline remains `6000.0.48f1`.
- Verify package or project setting changes by opening the project or running Unity in batch mode with the pinned editor and checking the Editor log for compiler errors.
