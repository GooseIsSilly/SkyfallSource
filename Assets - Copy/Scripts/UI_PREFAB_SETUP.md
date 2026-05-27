# UIShopItem Prefab Setup Guide

This guide shows you exactly how to create the UIShopItem prefab that displays each character in the shop.

## Required UI Structure

The UIShopItem prefab needs these components in this hierarchy:

```
UIShopItem (GameObject)
├─ Image (Background)
├─ HorizontalLayoutGroup
├─ UIShopItem (Script Component)
│
├─ AgentIcon (GameObject)
│  ├─ Image (character icon)
│  └─ LayoutElement
│
├─ InfoContainer (GameObject)
│  ├─ VerticalLayoutGroup
│  │
│  ├─ AgentName (GameObject)
│  │  └─ TextMeshProUGUI (character name)
│  │
│  └─ CostText (GameObject)
│     └─ TextMeshProUGUI (price)
│
└─ PurchaseButton (GameObject)
   ├─ Image (button background)
   ├─ UIButton (Script)
   ├─ LayoutElement
   │
   └─ ButtonText (GameObject)
      └─ TextMeshProUGUI ("BUY" text)
```

## Step-by-Step Creation

### Option 1: Use the Built-in Creator (Easiest!)

You already have an editor tool that creates this automatically:

```
1. Open Menu scene
2. Unity Menu → TPSBR → Create Shop UI
3. Click "Create UIShopItem Widget"
4. Configure the UIShopItem component (see below)
5. Save as prefab
```

### Option 2: Manual Creation

If you want to create it manually:

#### 1. Create Root GameObject

```
1. Hierarchy → Right-click → UI → Image
2. Rename to "UIShopItem"
3. Set RectTransform:
   - Width: 720
   - Height: 120
4. Set Image:
   - Color: Dark grey (R:0.2, G:0.2, B:0.2, A:0.8)
5. Add Component → HorizontalLayoutGroup:
   - Padding: 10 on all sides
   - Spacing: 15
   - Child Alignment: Middle Left
   - Child Control Height: ✓
   - Child Force Expand Height: ✓
```

#### 2. Create Icon

```
1. Right-click UIShopItem → UI → Image
2. Rename to "AgentIcon"
3. Add Component → LayoutElement:
   - Min Width: 100
   - Min Height: 100
   - Preferred Width: 100
   - Preferred Height: 100
4. Image Color: White
```

#### 3. Create Info Container

```
1. Right-click UIShopItem → Create Empty
2. Rename to "InfoContainer"
3. Add Component → VerticalLayoutGroup:
   - Spacing: 5
   - Child Alignment: Middle Left
   - Child Control Width: ✓
   - Child Control Height: ✓
4. Add Component → LayoutElement:
   - Flexible Width: 1
```

#### 4. Create Agent Name

```
1. Right-click InfoContainer → UI → Text - TextMeshPro
2. Rename to "AgentName"
3. Set TextMeshProUGUI:
   - Text: "Agent Name"
   - Font Size: 28
   - Font Style: Bold
   - Alignment: Left
```

#### 5. Create Cost Text

```
1. Right-click InfoContainer → UI → Text - TextMeshPro
2. Rename to "CostText"
3. Set TextMeshProUGUI:
   - Text: "500 CloudCoins"
   - Font Size: 24
   - Color: Gold (R:1, G:0.9, B:0, A:1)
   - Alignment: Left
```

#### 6. Create Purchase Button

```
1. Right-click UIShopItem → UI → Image
2. Rename to "PurchaseButton"
3. Set Image:
   - Color: Blue (R:0.1, G:0.5, B:0.8, A:1)
4. Add Component → UIButton (your project's button script)
5. Add Component → LayoutElement:
   - Min Width: 150
   - Min Height: 60
   - Preferred Width: 150
```

#### 7. Create Button Text

```
1. Right-click PurchaseButton → UI → Text - TextMeshPro
2. Rename to "ButtonText"
3. Set RectTransform:
   - Anchor: Stretch/Stretch (full fill)
   - Left: 0, Right: 0, Top: 0, Bottom: 0
4. Set TextMeshProUGUI:
   - Text: "BUY"
   - Font Size: 24
   - Font Style: Bold
   - Alignment: Center
```

#### 8. Add UIShopItem Component

```
1. Select root "UIShopItem" GameObject
2. Add Component → UIShopItem (your script)
3. Assign references (drag & drop):
   - Agent Icon → AgentIcon's Image
   - Agent Name → AgentName's TextMeshProUGUI
   - Cost Text → CostText's TextMeshProUGUI
   - Purchase Button → PurchaseButton's UIButton
   - Purchase Button Text → ButtonText's TextMeshProUGUI
   - Owned Indicator → (Optional, create if needed)
```

## UIShopItem Component Configuration

Once you have the UIShopItem component added, configure these fields:

### UI References (Drag & Drop)
```
Agent Icon:          [AgentIcon's Image component]
Agent Name:          [AgentName's TextMeshProUGUI]
Cost Text:           [CostText's TextMeshProUGUI]
Purchase Button:     [PurchaseButton's UIButton]
Purchase Button Text:[ButtonText's TextMeshProUGUI]
Owned Indicator:     [Optional GameObject to show when owned]
```

### Display Settings (Text Formats)
```
Cost Format:         "{0} CloudCoins"
Free Text:           "FREE"
Purchase Text:       "BUY"
Owned Text:          "OWNED"
Selected Text:       "SELECTED"
Can Afford Color:    White (R:1, G:1, B:1, A:1)
Cannot Afford Color: Red (R:1, G:0, B:0, A:1)
```

## Optional: Owned Indicator

To show a visual "OWNED" badge:

```
1. Right-click UIShopItem → UI → Image
2. Rename to "OwnedIndicator"
3. Set Image:
   - Sprite: CheckmarkIcon or badge
   - Color: Green
4. Position: Top-right corner
5. Initially set: Inactive (unchecked)
6. Assign to UIShopItem component's "Owned Indicator" field
```

## Saving as Prefab

Once your UIShopItem is complete:

```
1. Drag "UIShopItem" from Hierarchy to Project window
2. Save location: Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab
3. (or follow your project structure)
4. Confirm it's a prefab (blue cube icon)
```

## Linking to UIShopView

After creating the prefab:

```
1. Open Menu scene
2. Find UIShopView GameObject
3. Find the UIList component (usually on ShopItemsList child)
4. In UIList component:
   - Element Prefab → [Drag UIShopItem prefab here]
5. Save scene
```

## Testing the Prefab

```
1. Enter Play Mode
2. Open shop
3. Verify:
   - Each character appears as a UIShopItem
   - Icons display correctly
   - Names show up
   - Prices are visible
   - Buttons are clickable
   - Layout looks good
```

## Troubleshooting

### Items don't spawn
```
→ Check UIList has ElementPrefab assigned
→ Verify UIShopView has ShopDatabase assigned
```

### Layout is broken
```
→ Check HorizontalLayoutGroup settings on root
→ Check LayoutElements on children
→ Verify anchors are correct
```

### Icons don't show
```
→ Check AgentIcon Image component assigned
→ Verify CharacterData has icon sprites
```

### Buttons don't respond
```
→ Check UIButton component exists
→ Verify UIShopItem script is assigned
→ Check button listener is connected (auto in Awake)
```

### Text doesn't update
```
→ Check TextMeshProUGUI components assigned
→ Verify text format strings are correct
```

## Visual Preview

What it should look like:

```
┌────────────────────────────────────────────────────┐
│ ┌──────┐  Soldier                  ┌────────────┐ │
│ │      │  FREE                     │   OWNED    │ │
│ │ ICON │                           └────────────┘ │
│ └──────┘                                          │
└────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────┐
│ ┌──────┐  Marine                   ┌────────────┐ │
│ │      │  500 CloudCoins           │    BUY     │ │
│ │ ICON │  (white text)             └────────────┘ │
│ └──────┘                                          │
└────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────┐
│ ┌──────┐  Sniper                   ┌────────────┐ │
│ │      │  1000 CloudCoins          │    BUY     │ │
│ │ ICON │  (red - can't afford)     │ (greyed)   │ │
│ └──────┘                           └────────────┘ │
└────────────────────────────────────────────────────┘
```

## Customization Tips

### Make it fancier:
- Add border outlines
- Add glow effects on hover
- Animate button presses
- Add particle effects on purchase
- Show character stats/description

### Color themes:
```
Default (Dark):
- Background: Dark grey (0.2, 0.2, 0.2, 0.8)
- Button: Blue (0.1, 0.5, 0.8, 1)

Sci-Fi (Cyan):
- Background: Dark cyan (0, 0.2, 0.3, 0.9)
- Button: Bright cyan (0, 0.8, 1, 1)

Military (Green):
- Background: Dark green (0.1, 0.2, 0.1, 0.9)
- Button: Olive (0.4, 0.6, 0.2, 1)
```

### Layout variants:
```
Compact (100px height):
- Smaller icon (64x64)
- Single line text
- Smaller button

Large (150px height):
- Bigger icon (128x128)
- Add description text
- Add stats display

Grid (square cards):
- Change to VerticalLayoutGroup
- Icon on top
- Info below
- Button at bottom
```
