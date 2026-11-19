# Baybaylan project guidance

- Prefer authoring gameplay, UI, and VFX objects, component references, and tunable settings in Unity prefabs or scenes. Keep objects artists and designers need to tune visible in Edit Mode and expose their settings in the Inspector.
- Use runtime code for behavior that depends on game state, and instantiate authored prefabs when the number of instances depends on gameplay. Avoid building prefab hierarchies or attaching their components in code when they can be authored in the editor.
