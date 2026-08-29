# Simple Water Shader — Quick Start

## 1. One project setting

Open your URP Asset (Project Settings > Graphics, or Quality > the active render
pipeline asset) and enable **Depth Texture** under Rendering. That's the only
setting this shader needs.

## 2. Add water

**GameObject > 3D Object > Add Simple Water**

This drops a ready-to-use water plane into your scene at your Scene view camera's
position, with a material already assigned. Each water object gets its own material
(copied from the shared "Water Material Sample" template), so tweaking one pond
never affects another.

## 3. Adjust to taste

Select the water object and tune the material in the Inspector:

| Group | Properties |
|---|---|
| Waves | Wave Speed, Wave Strength, Wave Scale |
| Water Color | Shallow Color, Deep Color, Water Depth |
| Ripples | Normal Map, Normal Tiling, Normal Strength, Normal Speed |
| Reflection | Fresnel Power, Reflection Strength |
| Foam | Foam Color, Foam Distance, Foam Noise Texture, Foam Tiling, Foam Speed |

**Water Depth** = how many world units deep the water needs to be before it reads
as fully "deep" colored — raise it for a large lake, lower it for a small pond.

**Foam Distance** = how far the foam extends from the shoreline / a submerged object.

## Troubleshooting (short version)

- **Flat color, no gradient/foam** → Depth Texture isn't enabled (step 1).
- **Bright pink/magenta water** → shader failed to compile; check the Console.
- **No reflections** → add a Reflection Probe, or assign a skybox.
- **Menu item missing** → make sure the Editor script is inside a folder named
  exactly `Editor` somewhere under Assets.

Full documentation: `Simple Water Shader - Documentation.pdf`

## Support

- Email: ahmedhouididev@gmail.com
- Publisher page: assetstore.unity.com/publishers/105962
