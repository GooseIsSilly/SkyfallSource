# 🎯 Fortnite-Style Circular Map Setup

## Goal
Create a circular map with purple background, showing zone circles ONLY after the first shrink warning.

---

## 📐 Step-by-Step Setup

### **Step 1: Create Circle Mask**

1. **Select** `/Canvas/MapPanel/MapDisplay` in Hierarchy
2. **Add Component** → UI → **Mask**
3. In **Mask** component:
   - ✅ **Uncheck "Show Mask Graphic"**
4. In **Image** component (on MapDisplay):
   - **Keep your map sprite** (MapImage.png)
   - This will now act as both the mask AND the display

### **Step 2: Make Map Circular**

We need to use a circular sprite for masking. Two options:

#### **Option A: Use Unity's Built-in Circle**
1. Select `MapDisplay`
2. **Duplicate** it (Ctrl+D) and rename duplicate to `MapMask`
3. Move `MapMask` as first child (above original MapDisplay)
4. On `MapMask`:
   - Image → Source Image → Search **"Circle"** or **"Knob"**
   - Select Unity's circle sprite
   - Color = White (full opacity)
   - Add Component → UI → **Mask**
   - Uncheck "Show Mask Graphic"
5. Move original `MapDisplay`, `PlayerMarker`, `POIMarkers`, `CurrentZoneCircle`, `NextZoneCircle` **inside** `MapMask`

#### **Option B: Change MapDisplay to Square (Simpler for Now)**
1. Select `MapDisplay`
2. In **Rect Transform**:
   - Set **Width** = **700**, **Height** = **700** (square)
   - Set **Anchors** to Center
3. In **Image**:
   - **Uncheck Preserve Aspect**
4. Add **Mask** component
5. Uncheck "Show Mask Graphic"

### **Step 3: Adjust MapPanel Background**

1. Select `/Canvas/MapPanel`
2. In **Image** component → **Color**:
   - Set to purple: **RGB(160, 100, 200)** with **Alpha = 255**
   - Or match your reference image color

### **Step 4: Style Zone Circles (Border Only)**

For each zone circle (CurrentZoneCircle and NextZoneCircle):

1. **Generate hollow circle sprite:**
   - Menu → Tools → Generate Circle Sprite
   - Texture Size: **512**
   - Border Thickness: **0.08**
   - **UNCHECK "Fill Circle"** (hollow!)
   - Generate

2. **Assign to circles:**
   - Select `CurrentZoneCircle`
   - Image → Source Image → Drag `ZoneCircle.png`
   - Color → White with **Alpha = 200**
   
   - Select `NextZoneCircle`
   - Image → Source Image → Drag `ZoneCircle.png`
   - Color → White with **Alpha = 180**

### **Step 5: Adjust Zone Colors in SimpleMapSystem**

1. Select `/MapSystem` in Hierarchy
2. In **Simple Map System** component:
   - **Current Zone Color**: White `(255, 255, 255, 200)`
   - **Next Zone Color**: Light Blue `(100, 200, 255, 180)`

---

## 🎨 Recommended Hierarchy

```
MapPanel (purple background, full screen)
└── MapMask (white circle sprite with Mask component)
    ├── MapDisplay (your map image)
    │   ├── PlayerMarker
    │   └── POIMarkers
    ├── CurrentZoneCircle (hollow white circle border)
    └── NextZoneCircle (hollow light circle border)
```

---

## ⚙️ Inspector Settings

### **MapPanel**
- Rect Transform: Stretch to full screen
- Image Color: Purple RGB(160, 100, 200, 255)

### **MapMask** (if using Option A)
- Rect Transform: 
  - Anchors: Center
  - Width: 700, Height: 700
- Image: 
  - Source: Unity Circle sprite
  - Color: White (255, 255, 255, 255)
- Mask: 
  - Show Mask Graphic: OFF

### **MapDisplay** (inside MapMask)
- Rect Transform:
  - Anchors: Stretch/Stretch
  - Left: 0, Top: 0, Right: 0, Bottom: 0
- Image:
  - Source: Your map image
  - Preserve Aspect: ON

### **CurrentZoneCircle**
- Image:
  - Source: ZoneCircle.png (hollow)
  - Color: White (255, 255, 255, 200)
- Rect Transform: Anchors Center

### **NextZoneCircle**
- Image:
  - Source: ZoneCircle.png (hollow)
  - Color: Light Blue (100, 200, 255, 180)
- Rect Transform: Anchors Center

---

## 🎮 How It Works

1. **Game starts**: Map shows, NO zone circles
2. **Zone becomes active**: Still no circles (waiting for announcement)
3. **First shrink announced**: 
   - ✅ CurrentZoneCircle appears (white border)
   - ✅ NextZoneCircle appears (light border)
4. **Zone shrinks**: Circles animate
5. **Next announcement**: Circles update for next zone
6. **Final zone**: Circles disappear when shrinking ends

---

## 🔧 Quick Settings

**In SimpleMapSystem Inspector:**
- ✅ Show Zones = **Checked**
- Current Zone Color = **White (255, 255, 255, 200)**
- Next Zone Color = **Light Blue (100, 200, 255, 180)**

**Zone Circle Sprites:**
- Must be **hollow** (transparent center, visible border only)
- Use Generate Circle Sprite tool with "Fill Circle" **UNCHECKED**

---

## 🎨 Visual Result

```
┌─────────────────────────────────┐
│                                 │
│    Purple Background            │
│                                 │
│       ╭───────────╮             │
│       │ Your Map  │             │
│       │ (Circular)│             │
│       │           │             │
│       │    ⭕     │  ← Zone border appears
│       │   📍     │   after warning
│       ╰───────────╯             │
│                                 │
└─────────────────────────────────┘
```

---

## 🐛 Troubleshooting

**Map not circular?**
- Make sure Mask component is added
- Use a proper circle sprite (Unity's built-in or generated)
- "Show Mask Graphic" must be OFF

**Zones showing immediately?**
- Check the code change was applied
- Zone only shows after `IsAnnounced = true`

**Zones look filled instead of borders?**
- Regenerate circle sprite with "Fill Circle" UNCHECKED
- Use hollow/border-only sprite

---

Try **Option B first** (simpler square mask), then move to circular mask later if needed!
