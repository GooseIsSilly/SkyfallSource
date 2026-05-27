# Simple Map System - Setup Instructions

## Step 1: Get Your Map Image

### Option A: Take a Screenshot (Easiest)
1. In Unity, go to your Scene view
2. Position the camera looking DOWN at your entire map
3. Take a screenshot (Windows: Win+Shift+S, Mac: Cmd+Shift+4)
4. Save it as `MapImage.png` in `/Assets/Textures/` folder

### Option B: Use the Scene View Camera
1. In Scene view, position camera looking down at your map
2. Go to `GameObject > Camera` to create temp camera
3. Position it where Scene camera is
4. Rotate to look straight down (Rotation: X=90, Y=0, Z=0)
5. Take screenshot from Game view
6. Delete the temp camera
7. Save screenshot as `MapImage.png` in `/Assets/Textures/`

---

## Step 2: Create the Map UI

### A. Create the Panel Structure
1. **Right-click in Hierarchy** → `UI` → `Canvas` (if you don't have one)
2. **Right-click Canvas** → `UI` → `Panel`
   - Rename it to **"MapPanel"**
   - Set the color to semi-transparent black (R:0, G:0, B:0, A:200)

### B. Create Map Display
3. **Right-click MapPanel** → `UI` → `Image`
   - Rename to **"MapDisplay"**
   - Set **Width: 600**, **Height: 600** (or whatever size you want)
   - Drag your `MapImage.png` into the **Source Image** field

### C. Create Player Marker
4. **Right-click MapDisplay** → `UI` → `Image`
   - Rename to **"PlayerMarker"**
   - Set **Width: 20**, **Height: 20**
   - Change color to **Yellow** or **White**
   - Set Anchor to **center-center**

### D. Create POI Markers Container
5. **Right-click MapDisplay** → `Create Empty`
   - Rename to **"POIMarkers"**

---

## Step 3: Create POI Marker Prefab

1. **Right-click Hierarchy** → `UI` → `Image`
   - Rename to **"POIMarker"**
2. Set **Width: 15**, **Height: 15**
3. Change color to **Red**
4. Drag it from Hierarchy to `/Assets/Prefabs/` folder to make it a prefab
5. Delete the POIMarker from Hierarchy (keep the prefab)

---

## Step 4: Setup the Map System

1. **Create Empty GameObject** in scene
   - Rename to **"MapSystem"**
2. **Add Component** → `Simple Map System`
3. **Assign References:**
   - **Map Image**: Drag `MapImage.png` sprite
   - **Map World Size**: Set to your map size in Unity units (ex: X=100, Y=100)
   - **Map World Center**: Set to center of your map (ex: X=0, Y=0)
   - **Map Panel**: Drag the MapPanel GameObject
   - **Map Display**: Drag the MapDisplay Image
   - **Player Marker**: Drag the PlayerMarker RectTransform
   - **POI Marker Prefab**: Drag the POIMarker prefab
   - **POI Markers Container**: Drag the POIMarkers GameObject

---

## Step 5: Add POIs to Your Map

1. Select any GameObject you want to show on the map
2. **Add Component** → `Map POI`
3. Set the POI name and color

Repeat for all POIs you want!

---

## Step 6: Test!

1. **Press Play**
2. **Press M** to toggle the map
3. Your player marker should move as you move!

---

## How to Adjust Map Settings

### If markers are in wrong positions:
- Adjust **Map World Size** (bigger = markers closer together)
- Adjust **Map World Center** to match your map's center point

### Example:
- If your map goes from X=-50 to X=50 and Z=-50 to Z=50:
  - **Map World Size**: X=100, Y=100
  - **Map World Center**: X=0, Y=0

- If your map goes from X=0 to X=200 and Z=0 to Z=200:
  - **Map World Size**: X=200, Y=200
  - **Map World Center**: X=100, Y=100

---

## Troubleshooting

**Map doesn't show:**
- Make sure Map Panel is assigned
- Check that Map Image sprite is assigned

**Player marker doesn't move:**
- Make sure your player has the tag "Player"

**POIs don't show:**
- Make sure you added MapPOI component to objects
- Check POI Marker Prefab is assigned

**Press M and nothing happens:**
- Check console for errors
- Make sure Input System package is installed

---

## That's it! 🎉

Press **M** to toggle your map on/off!
