# Auto-Generated Party System with 3D Character Previews 🎮

The Party View now **fully auto-generates** a beautiful UI with **3D character previews** at runtime! Zero setup required.

---

## ✨ **NEW: 3D Character Preview System**

### **What You Get:**

**Dedicated Preview Panel** (automatically created)
- "PARTY LINEUP" header with accent color
- Custom render texture (1024x512, 4x antialiasing)
- Isolated preview camera with professional lighting
- Dark themed background matching UI

**3D Character Models** (one for each party member)
- Auto-generated capsule characters with team colors
- Player nicknames floating above heads
- ★ Crown icon for party leader (golden)
- Smooth positioning based on party size
- Professional lighting setup (directional + fill light)

**Dynamic Updates**
- Automatically refreshes when party changes
- Shows local player when not in party
- Centers characters based on party size (1-6 players)
- Color-coded per player (blue, red, green, yellow, purple, pink)

---

## 🎨 **Preview Features**

### **Automatic Character Generation:**
- **Capsule models** - Clean, simple placeholder characters
- **Team colors** - Each player gets a unique vibrant color
- **Nicknames** - Floating text above each character
- **Leader crown** - Golden ★ icon for party leader
- **Custom prefabs** - Assign `Player Preview Prefab` for real character models

### **Professional Camera Setup:**
- **Perspective camera** - FOV 30° for cinematic look
- **Directional light** - White light from top-right
- **Fill light** - Blue-tinted point light for depth
- **Dark background** - Matches UI theme
- **UI layer isolation** - Renders separately from main scene

### **Smart Positioning:**
- **Solo:** Single character centered
- **Duo:** 2 characters side-by-side
- **Squad (3-4):** Evenly spaced across panel
- **Full party (5-6):** All characters visible and centered

---

## 🚀 **Setup (Still Super Easy!)**

### **STEP 1: Create Party View**
1. Right-click Canvas → Create Empty
2. Name it `UIPartyView`
3. Add component `UIPartyView`
4. **Done!** UI + 3D previews auto-generate ✨

### **STEP 2: Customize (Optional)**

Select `UIPartyView` in Inspector:

#### **Auto-Generation Settings**
- ✅ `Auto Generate UI` - Leave ON
- `Widget Height` = 70px
- `Widget Spacing` = 8px
- `Panel Width` = 450px

#### **Character Preview Settings** (NEW!)
- ✅ `Enable Character Previews` - Toggle 3D previews
- `Preview Height` = 250px - Height of preview panel
- `Player Preview Prefab` - (Optional) Your character model
- `Character Spacing` = 2.5 units - Space between characters
- `Character Rotation` = (0, 180, 0) - Facing direction

#### **Colors**
- `Background Color` - Main panel
- `Panel Color` - Friends/Party panels
- `Header Color` - Top header
- `Accent Color` - Titles and highlights

---

## 🎬 **How Character Previews Work**

### **Render Pipeline:**

1. **Separate 3D Scene**
   - Creates isolated camera in UI layer
   - Spawns character models in preview container
   - Renders to dedicated RenderTexture

2. **RawImage Display**
   - RenderTexture shown in preview panel
   - Updates in real-time
   - No performance impact on main game

3. **Auto Cleanup**
   - Releases RenderTexture on close
   - Destroys preview camera
   - Clears character models

### **Character Colors:**

The system uses vibrant team colors:
```
Player 1: Blue    (0.3, 0.6, 1.0)
Player 2: Red     (1.0, 0.4, 0.3)
Player 3: Green   (0.3, 1.0, 0.5)
Player 4: Yellow  (1.0, 0.8, 0.2)
Player 5: Purple  (0.8, 0.3, 1.0)
Player 6: Pink    (1.0, 0.5, 0.7)
```

---

## 🎮 **Using Custom Character Models**

Want to show real character models instead of capsules?

### **Option 1: Assign Prefab**
1. Create your character prefab
2. Select `UIPartyView` in Inspector
3. Assign to `Player Preview Prefab` field
4. Characters will use your model!

### **Option 2: Add CharacterPreview Component**
```csharp
public class MyCharacterPreview : CharacterPreview
{
    public override void SetCharacter(string userID, string agentID)
    {
        base.SetCharacter(userID, agentID);
        // Load and display your character model based on agentID
    }
}
```

---

## 📐 **Visual Layout (WITH 3D Previews)**

```
╔════════════════════════════════════════════════════════════╗
║  PARTY & FRIENDS                                    ✕     ║
╠════════════════════════════════════════════════════════════╣
║                                                            ║
║  ┌──────────────────────────────────────────────────────┐ ║
║  │               PARTY LINEUP                           │ ║
║  ├──────────────────────────────────────────────────────┤ ║
║  │     ★                                                │ ║
║  │    👤         👤         👤         👤              │ ║
║  │   [You]     [Alice]    [Bob]    [Charlie]           │ ║
║  │  (Blue)     (Red)     (Green)   (Yellow)            │ ║
║  └──────────────────────────────────────────────────────┘ ║
║                                                            ║
║  ┌─────────────────┐    ┌──────────────────┐             ║
║  │ FRIENDS         │    │ CURRENT PARTY    │             ║
║  ├─────────────────┤    ├──────────────────┤             ║
║  │ + Add Friend    │    │ [Leave Party]    │             ║
║  ├─────────────────┤    ├──────────────────┤             ║
║  │ ┌─────────────┐ │    │ ┌──────────────┐ │             ║
║  │ │ Friend1  ● │ │    │ │ ★ You        │ │             ║
║  │ │ [Invite][X]│ │    │ │              │ │             ║
║  │ ├─────────────┤ │    │ ├──────────────┤ │             ║
║  │ │ Friend2  ○ │ │    │ │ Alice        │ │             ║
║  │ │ [Invite][X]│ │    │ │      [Kick]  │ │             ║
║  │ └─────────────┘ │    │ └──────────────┘ │             ║
║  └─────────────────┘    └──────────────────┘             ║
║                                                            ║
╚════════════════════════════════════════════════════════════╝
```

---

## 🎯 **Features Summary**

✅ **Fully automatic UI** - No prefabs, no manual setup
✅ **3D character previews** - Real-time rendered characters
✅ **Professional lighting** - Directional + fill light
✅ **Team colors** - Each player unique color
✅ **Leader crown** - Golden ★ for party leader
✅ **Dynamic updates** - Changes with party
✅ **Custom models support** - Use your own character prefabs
✅ **Performance optimized** - Separate render layer
✅ **Auto cleanup** - Resources released on close
✅ **Beautiful styling** - Fortnite-style dark theme

---

## 💡 **Pro Tips**

1. **No Setup** - Just add component, everything auto-generates!
2. **Toggle Previews** - Uncheck `Enable Character Previews` to disable
3. **Adjust Height** - Change `Preview Height` to resize panel
4. **Custom Models** - Assign `Player Preview Prefab` for real characters
5. **Spacing Control** - Adjust `Character Spacing` for tighter/wider lineup
6. **Camera Angle** - Modify `Character Rotation` for different poses

---

## 🔧 **Technical Details**

### **Render Setup:**
- **RenderTexture:** 1024x512, 4x MSAA, 24-bit depth
- **Camera:** Perspective, FOV 30°, UI layer only
- **Position:** (0, 1.2, -6) looking at characters
- **Lighting:** Directional (white) + Point (blue fill)

### **Character Hierarchy:**
```
PartyPreviewCamera
├── CharacterContainer
│   ├── Preview_Player1 (Capsule)
│   │   ├── Nickname (World Canvas)
│   │   └── Crown (World Canvas) [if leader]
│   ├── Preview_Player2 (Capsule)
│   └── ...
├── PreviewLight (Directional)
└── FillLight (Point)
```

### **Performance:**
- **Lightweight:** Simple capsule geometry
- **Isolated:** Renders on UI layer only
- **Efficient:** 512px height render texture
- **Cleanup:** Auto-released on view close

---

## 🎊 **What It Looks Like**

When you open the Party View, you'll see:

**Top Section:**
- Dark preview panel with "PARTY LINEUP" title
- 3D characters standing side-by-side
- Colored capsules (or your custom models)
- Nicknames floating above heads
- Golden star crown on party leader
- Professional lighting and shadows

**Bottom Section:**
- Friends list (left) - Add, invite, remove friends
- Party list (right) - Current party members
- All with beautiful dark theme and blue accents

**Everything auto-generates with zero setup!** 🚀

---

## 🐛 **Troubleshooting**

**Characters not showing?**
- Make sure `Enable Character Previews` is checked
- Check that `PartyLobbyManager.Instance` exists
- Verify UI layer exists in your project

**Preview looks dark?**
- Lights auto-generate, but check lighting settings
- Adjust camera position if needed
- Verify URP settings allow UI layer rendering

**Want to disable previews?**
- Uncheck `Enable Character Previews` in Inspector
- UI will still work perfectly without 3D previews

---

**Enjoy your beautiful auto-generated Party system with 3D character lineup!** 🎉✨

