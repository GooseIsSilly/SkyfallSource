# Quick Shop UI Setup - Minimal Steps

Since I can't directly create Unity prefabs, here are the **absolute minimum steps** you need to create the shop UI:

## Option 1: Create UI Manually (15-20 minutes)

### Step 1: Create UIShopView Prefab (5 min)
1. Open Menu scene
2. Create UI → Panel, rename to "UIShopView"
3. Add these components to UIShopView:
   - Canvas (Render Mode: Screen Space - Camera)
   - Canvas Group
   - GraphicRaycaster
   - **UI Shop View** script
4. Make it full screen (RectTransform: Anchor to stretch-stretch, all offsets 0)
5. Inside UIShopView, create:
   - **Background** (UI → Image, dark semi-transparent)
   - **Content** panel (800x600, centered)
     - Inside Content:
       - **TitleText** (TextMeshPro): "AGENT SHOP"
       - **CloudCoinsText** (TextMeshPro): "CloudCoins: 0"
       - **ScrollView** (UI → Scroll View)
         - Find child: Viewport → Content
         - Add **UI List** component to Content
         - Add **Vertical Layout Group** to Content
6. Link in UIShopView component:
   - Shop Items List → Viewport/Content
   - Cloud Coins Text → CloudCoinsText
7. Save as prefab to `Assets/TPSBR/UI/Prefabs/MenuViews/UIShopView.prefab`
8. Delete from scene

### Step 2: Create UIShopItem Widget (5 min)
1. Create UI → Panel, rename to "UIShopItem" (720x120)
2. Add **Horizontal Layout Group** (Padding: 10, Spacing: 15)
3. Inside UIShopItem, create in order:
   - **AgentIcon** (UI → Image, 100x100)
   - **InfoContainer** (Empty GameObject)
     - Add Vertical Layout Group
     - Inside InfoContainer:
       - **AgentName** (TextMeshPro)
       - **CostText** (TextMeshPro)
   - **PurchaseButton** (UI → Button, change to UIButton, 150x60)
     - Inside: **ButtonText** (TextMeshPro): "BUY"
4. Add **UI Shop Item** component to root
5. Link all fields in component
6. Save as prefab to `Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab`
7. Delete from scene

### Step 3: Link Everything (2 min)
1. Open UIShopView prefab
2. Select Viewport → Content (with UI List)
3. Set Item Instance → UIShopItem prefab
4. Save

### Step 4: Add Shop Button (3 min)
1. In Menu scene, find the main menu UI
2. Duplicate any existing button (like Settings)
3. Rename to "ShopButton", change text to "SHOP"
4. Find the GameObject with **UIMainMenuView** component
5. In Inspector, drag your new button to the "Shop Button" field
6. Save scene

### Step 5: Configure Prices (2 min)
1. Find **AgentSettings** asset (search in Project)
2. Set prices:
   - Soldier → Cloud Coin Cost: 0
   - Marine → Cloud Coin Cost: 500
3. Save

### Done! Test it:
- Play the game
- Click "SHOP" button
- Marine should be locked, Soldier owned

---

## Option 2: Ask Me to Guide You Step-by-Step

Since I can't create the prefabs directly, I can:
1. Walk you through each step with screenshots/explanations
2. Provide exact values for every field
3. Help troubleshoot if something doesn't work

Just let me know which GameObject you're working on and I'll guide you!

---

## Option 3: Use Existing Prefabs as Templates

You can also duplicate and modify existing UI prefabs:
1. **For UIShopView**: Duplicate `UIAgentSelectionView.prefab` as a starting point
2. **For UIShopItem**: Duplicate any existing UI widget as a starting point
3. Modify components and scripts as needed

---

## Testing Without Full UI Setup

You can test the shop system logic without the UI by:
1. Adding `ShopSystemDebugHelper` component to any GameObject in the scene
2. Using the context menu (right-click component) to:
   - Add CloudCoins
   - Log owned agents
   - Check current balance
3. Testing agent ownership in code:
```csharp
var playerData = Global.PlayerService.PlayerData;
Debug.Log($"Owns Marine: {playerData.ShopSystem.OwnsAgent("Marine")}");
Debug.Log($"CloudCoins: {playerData.CoinSystem.CloudCoins}");
```

---

## Need Help?

Let me know:
- **Which step you're on** and I'll provide detailed guidance
- **If you get stuck** and I'll help troubleshoot
- **If you want me to create a Unity Editor script** that can automate some of this setup
