# ✨ Simple Map System

A **super easy** Fortnite-style map system using a static image!

---

## 🚀 Quick Start (Choose One)

### Option 1: Use the Automatic Tools (EASIEST!)

1. **Take Map Screenshot**
   - Go to `Tools > Map Screenshot Tool`
   - Set camera position and size
   - Click "Take Screenshot"
   - Done! Image saved to `Assets/Textures/MapImage.png`

2. **Measure Your Map**
   - Go to `Tools > Map Measurement Helper`
   - Click corners of your map
   - Copy the values it gives you

3. **Follow setup instructions** in `MAP_SETUP_INSTRUCTIONS.md`

### Option 2: Manual Setup (5 Minutes)

Read `QUICK_START.md` for step-by-step instructions!

---

## 📁 Files

- `SimpleMapSystem.cs` - Main map system script
- `MapPOI.cs` - Add to objects you want shown on map
- `MAP_SETUP_INSTRUCTIONS.md` - Detailed setup guide
- `QUICK_START.md` - Fast setup guide
- `Editor/MapScreenshotTool.cs` - Auto screenshot tool
- `Editor/MapMeasurementHelper.cs` - Map measurement helper

---

## 🎮 How to Use

**Controls:**
- Press **M** to toggle map

**Add POIs:**
1. Select any GameObject
2. Add `MapPOI` component
3. Set color and name
4. Done!

---

## 🔧 Settings Explained

### SimpleMapSystem Inspector

**Map Image:**
- The screenshot/image of your map

**Map World Size:**
- How big is your playable area in Unity units?
- Example: If map goes from X=-50 to X=50, size is 100

**Map World Center:**
- The center point of your playable area
- Example: If map goes from X=-50 to X=50, center is 0

**UI References:**
- Just drag and drop from Hierarchy!

---

## 💡 Tips

✅ Use the **Map Screenshot Tool** for perfect screenshots!
✅ Use the **Map Measurement Helper** to get exact values!
✅ Higher screenshot resolution = better quality
✅ POI colors auto-apply to markers
✅ Works with multiplayer!

---

## ❓ Troubleshooting

**Map doesn't open:**
- Check if Map Panel is assigned
- Look for errors in Console

**Markers in wrong place:**
- Adjust Map World Size and Center
- Use Map Measurement Helper tool!

**Player marker doesn't move:**
- Make sure player has "Player" tag

**No POIs showing:**
- Check POI Marker Prefab is assigned
- Make sure POIs have MapPOI component

---

## 🎉 That's It!

This is the **simplest possible map system**. No complex cameras, no render textures, just an image with markers!

**Need help?** Check the other .md files for detailed instructions!
