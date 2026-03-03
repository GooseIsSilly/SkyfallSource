# Shop UI Setup Guide

## Step-by-Step Instructions to Create the Shop UI

### Part 1: Create UIShopView Prefab

1. **Open the Menu scene**
   - File → Open Scene → `Assets/TPSBR/Scenes/Menu.unity`

2. **Create the Shop View GameObject**
   - In Hierarchy, right-click → UI → Panel
   - Rename it to "UIShopView"
   - In Inspector:
     - Add Component → `Canvas` (set Render Mode to "Screen Space - Camera")
     - Add Component → `Canvas Group`
     - Add Component → `GraphicRaycaster`
     - Add Component → `UI Shop View` (our custom script)

3. **Setup the Root Transform**
   - RectTransform should stretch to fill screen:
     - Anchor Presets: Click the square in the top left, then hold Alt+Shift and click the bottom-right preset (stretch-stretch)
     - Left, Right, Top, Bottom: all set to 0

4. **Create Background**
   - Right-click UIShopView → UI → Image
   - Rename to "Background"
   - Set Color to semi-transparent black (R:0.1, G:0.1, B:0.1, A:0.9)
   - Stretch to fill parent (same anchor preset as above)

5. **Create Content Panel**
   - Right-click UIShopView → UI → Panel
   - Rename to "Content"
   - RectTransform:
     - Anchor: Center-Center
     - Width: 800
     - Height: 600
     - Position X: 0, Y: 0

6. **Create Title Text**
   - Right-click Content → UI → Text - TextMeshPro
   - Rename to "TitleText"
   - Set Text to "AGENT SHOP"
   - RectTransform:
     - Anchor: Top-Center
     - Width: 760 (Content width - 40)
     - Height: 50
     - Pos X: 0, Pos Y: -25
   - TextMeshPro settings:
     - Font Size: 48
     - Alignment: Center
     - Font Style: Bold

7. **Create CloudCoins Display**
   - Right-click Content → UI → Text - TextMeshPro
   - Rename to "CloudCoinsText"
   - Set Text to "CloudCoins: 0"
   - RectTransform:
     - Anchor: Top-Center
     - Width: 760
     - Height: 40
     - Pos X: 0, Pos Y: -80
   - TextMeshPro settings:
     - Font Size: 36
     - Alignment: Center
     - Font Style: Bold

8. **Create Shop Items List Container**
   - Right-click Content → Create Empty
   - Rename to "ShopItemsListContainer"
   - RectTransform:
     - Anchor: Stretch-Stretch (relative to Content)
     - Left: 20, Right: 20
     - Top: 140, Bottom: 20
   - Add Component → `Vertical Layout Group`:
     - Padding: 10 on all sides
     - Spacing: 20
     - Child Controls Size: Check Width and Height
     - Child Force Expand: Uncheck both
   - Add Component → `Content Size Fitter`:
     - Vertical Fit: Preferred Size

9. **Create Scroll View (Optional but recommended)**
   - Actually, let's wrap the list in a scroll view:
   - Delete "ShopItemsListContainer" 
   - Right-click Content → UI → Scroll View
   - Rename to "ScrollView"
   - RectTransform:
     - Anchor: Stretch-Stretch (relative to Content)
     - Left: 20, Right: 20
     - Top: 140, Bottom: 20
   - Find the child "Viewport → Content":
     - This is where we'll put the items
     - Add Component → `UI List` (our custom component)
     - Add Component → `Vertical Layout Group`:
       - Spacing: 20
       - Child Controls Size: Check Width, uncheck Height
       - Child Force Expand: Uncheck both

10. **Configure UIShopView Component**
    - Select the root "UIShopView" GameObject
    - In the `UI Shop View` component:
      - Shop Items List: Drag the "Viewport → Content" GameObject here
      - Cloud Coins Text: Drag the "CloudCoinsText" GameObject here
      - Cloud Coins Format: Set to "CloudCoins: {0}"

11. **Save as Prefab**
    - Drag the "UIShopView" GameObject from Hierarchy into `Assets/TPSBR/UI/Prefabs/MenuViews/`
    - This creates the prefab
    - Delete the instance from the scene (it will be spawned at runtime)

### Part 2: Create UIShopItem Widget Prefab

1. **Create a new UI Panel**
   - In Hierarchy, right-click → UI → Panel
   - Rename to "UIShopItem"
   - RectTransform:
     - Width: 720
     - Height: 120

2. **Setup Layout**
   - Add Component → `Horizontal Layout Group`:
     - Padding: 10 on all sides
     - Spacing: 15
     - Child Alignment: Middle Left
     - Child Controls Size: Check Height only
     - Child Force Expand: Check Height only

3. **Create Agent Icon**
   - Right-click UIShopItem → UI → Image
   - Rename to "AgentIcon"
   - RectTransform:
     - Width: 100
     - Height: 100
   - Add Component → `Layout Element`:
     - Min Width: 100
     - Min Height: 100
     - Preferred Width: 100
     - Preferred Height: 100

4. **Create Info Container**
   - Right-click UIShopItem → Create Empty
   - Rename to "InfoContainer"
   - Add Component → `Vertical Layout Group`:
     - Spacing: 5
     - Child Alignment: Middle Left
     - Child Controls Size: Check both
     - Child Force Expand: Uncheck both
   - Add Component → `Layout Element`:
     - Flexible Width: 1

5. **Create Agent Name Text**
   - Right-click InfoContainer → UI → Text - TextMeshPro
   - Rename to "AgentName"
   - Set Text to "Agent Name"
   - Font Size: 28
   - Font Style: Bold
   - Alignment: Left

6. **Create Cost Text**
   - Right-click InfoContainer → UI → Text - TextMeshPro
   - Rename to "CostText"
   - Set Text to "500 CloudCoins"
   - Font Size: 24
   - Color: Yellow/Gold (R:1, G:0.9, B:0)
   - Alignment: Left

7. **Create Purchase Button**
   - Right-click UIShopItem → UI → Button - TextMeshPro
   - Rename to "PurchaseButton"
   - Change the Button component to `UIButton` (click the gear icon → Edit Script or manually change)
   - RectTransform:
     - Width: 150
     - Height: 60
   - Add Component → `Layout Element`:
     - Min Width: 150
     - Preferred Width: 150

8. **Setup Button Text**
   - Select PurchaseButton → Text (TMP)
   - Rename to "ButtonText"
   - Set Text to "BUY"
   - Font Size: 24
   - Font Style: Bold
   - Alignment: Center

9. **Create Owned Indicator (Optional)**
   - Right-click UIShopItem → UI → Image
   - Rename to "OwnedIndicator"
   - Set it to be positioned in top-right corner with a checkmark sprite
   - Initially set Active to false

10. **Add UIShopItem Component**
    - Select the root "UIShopItem" GameObject
    - Add Component → `UI Shop Item`
    - Configure all the serialized fields:
      - Agent Icon: Drag AgentIcon
      - Agent Name: Drag AgentName
      - Cost Text: Drag CostText
      - Purchase Button: Drag PurchaseButton
      - Purchase Button Text: Drag PurchaseButton → Text (TMP)
      - Owned Indicator: Drag OwnedIndicator (if you created it)
      - Cost Format: "{0} CloudCoins"
      - Purchase Text: "BUY"
      - Owned Text: "OWNED"
      - Selected Text: "SELECTED"

11. **Save as Prefab**
    - Drag "UIShopItem" from Hierarchy into `Assets/TPSBR/UI/Prefabs/Widgets/`
    - Delete the instance from Hierarchy

12. **Link Item Prefab to Shop List**
    - Open the UIShopView prefab you created earlier
    - Select the "Viewport → Content" GameObject (the one with UIList component)
    - In the `UIList` component:
      - Item Instance: Drag the UIShopItem prefab here

### Part 3: Add Shop Button to Main Menu

1. **Open Menu Scene**
   - Make sure you have `Assets/TPSBR/Scenes/Menu.unity` open

2. **Find Main Menu UI**
   - In Hierarchy, look for the UI Canvas that contains the main menu buttons
   - Usually something like "UICanvas" or "Menu" containing buttons like "Play", "Settings", etc.

3. **Add Shop Button**
   - Duplicate an existing button (like the Settings button)
   - Rename it to "ShopButton"
   - Change the text to "SHOP" or "AGENT SHOP"
   - Position it near other menu buttons

4. **Update UIMainMenuView Script**
   - The script is at `/Assets/TPSBR/Scripts/UI/MenuViews/UIMainMenuView.cs`
   - I'll update it for you with a new shop button handler!

### Part 4: Configure Agent Prices

1. **Find AgentSettings**
   - In Project window, search for "AgentSettings"
   - Usually located in `Assets/TPSBR/Resources/Settings/Gameplay/AgentSettings.asset`

2. **Set Prices**
   - Select the AgentSettings asset
   - In Inspector, expand the Agents array
   - For each agent:
     - Soldier: Set "Cloud Coin Cost" to 0 (free/starter)
     - Marine: Set "Cloud Coin Cost" to 500 (or your preferred price)

3. **Save**
   - File → Save Project (Ctrl+S)

## Testing

1. **Enter Play Mode**
2. **Check CloudCoins**: Use the debug helper by adding `ShopSystemDebugHelper` component to any GameObject
3. **Open Shop**: Click the shop button from main menu
4. **Try Purchasing**: You should see Marine locked with a price, Soldier already owned
5. **Add Coins**: Exit play mode, use the debug helper to add coins, then test purchasing

## Troubleshooting

- **Shop doesn't open**: Make sure UIShopView prefab is in the correct folder and UIMainMenuView has the button linked
- **No items show**: Check that UIList has the UIShopItem prefab assigned
- **Compilation errors**: Make sure all scripts are saved and Unity has recompiled
- **Wrong prices**: Double-check AgentSettings asset configuration

## Quick Alternative: Automated Setup (Coming Next)

I'll create a script that can automatically set up basic shop UI for you!
