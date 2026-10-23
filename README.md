# WorldForge

A 3D virtual robotics lab built in Unity, made for **Assignment 02 – Development of a Virtual Environment
Using Unity** (AIL7110, Introduction to Augmented Reality and Virtual Reality).

**Sahil Narkhede · B23CS1060**

The repository holds two Unity projects:

| Folder | What it is |
|---|---|
| [`worldforge-v2/`](worldforge-v2) | **Submitted version.** A focused robotics lab with a glass wall overlooking open green ground. |
| repository root (`Assets/`, `Packages/`, `ProjectSettings/`) | First, larger version: the lab plus an outdoor garden, sliding door, day/night cycle, holographic globe and more. |

Both are Unity 6.3 (6000.3.26f1) projects using the Universal Render Pipeline.

## Submitted version: `worldforge-v2`

A 20 m × 14 m lab with brick and concrete walls, tiled floor, six ceiling lights and a continuous glass
wall looking out over green ground. It is organised into zones:

- **Robot cell:** a 4-axis orange robot arm that picks up a crate and places it on another table.
- **Conveyor line:** an 8 m belt carrying crates between two housings.
- **Workstations:** two desks with monitors.
- **Workbench:** a crate, a ball and a toolbox you can pick up and throw.
- **Storage:** a shelf of boxes and a whiteboard.

| Assignment requirement | Where it is covered |
|---|---|
| At least five 3D objects | About 200 objects built from primitives at real-world scale: room, robot arm, conveyor, desks, chairs, shelf, workbench, signs, plants |
| Materials, colours, lighting, textures | URP physically based materials, 12 generated tileable textures (9 with normal maps), sun and sky, 6 shadowed ceiling spot lights, reflection probe, post-processing |
| Camera navigation | First-person controller with collisions, a third-person camera (V) and a guided camera tour (T) |
| Interaction | Robot arm pick-and-place, conveyor stop/start, light switch, pick up / rotate / throw objects |

**Deliverables**
- **Video:** [`worldforge-v2/Recordings/WorldForge_Tour.mp4`](worldforge-v2/Recordings/WorldForge_Tour.mp4) (2560×1440, 60 fps)
- **Report:** [`worldforge-v2/worldforge-report/`](worldforge-v2/worldforge-report), containing the PDF, the LaTeX source and its figures
- **Project files:** the `worldforge-v2/` folder

## Running a project

1. In Unity Hub, choose **Add → Add project from disk** and pick `worldforge-v2/` (or the repository root for the first version).
2. Open it with **Unity 6.3 (6000.3.26f1)**. Packages download automatically on the first open.
3. The `WorldForge` scene opens by itself. If it doesn't, use the menu **WorldForge → Open Scene**.
4. Press **Play** and click inside the Game view to capture the mouse.

### Controls

| Key | Action | Key | Action |
|---|---|---|---|
| W A S D / arrows | Move | E / left click | Interact, pick up, drop |
| Mouse | Look around | Right click | Throw held object |
| Shift | Sprint | R (hold) | Rotate held object |
| Space | Jump | V | First / third person |
| T | Guided camera tour | P | Screenshot |
| H | Hide help panel | U | Hide all UI |
| Esc | Release mouse | | |

The first version also uses **N** to switch between day and night.

### Editor menu (`WorldForge`)

- **Build Scene:** regenerates the scene, textures and materials.
- **Record Guided Tour (QHD 2560x1440 / Full HD):** plays the tour and records it with Unity Recorder into `Recordings/`.
  On Linux the output is WebM; convert it with `ffmpeg -i in.webm -c:v libx264 -crf 18 -pix_fmt yuv420p out.mp4`.
- **Build Linux Player / Build Windows Player:** creates a standalone build in `Build/`.

## Project layout

```
Assets/WorldForge/Scripts    runtime C#: player controller, interactions, HUD, guided tour
Assets/WorldForge/Editor     editor tools: scene builder, texture generator, tour recorder, builds
Assets/WorldForge/Materials, Textures, Settings
                             generated materials, textures, lighting and post-processing
Assets/Scenes/WorldForge.unity
Tools/make_signs.py          draws the sign, whiteboard and monitor textures (Python + Pillow);
                             run it from the project folder after editing, Unity only reads the PNGs
```

## Tools

Unity 6.3 LTS · Universal Render Pipeline 17.3 · Input System 1.20 · Unity Recorder 5.1.7 · C# ·
Python 3 + Pillow · FFmpeg · Git
