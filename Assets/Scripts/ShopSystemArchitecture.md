# Shop System Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    SHOP SYSTEM ARCHITECTURE                  │
└─────────────────────────────────────────────────────────────┘

┌──────────────────┐        ┌──────────────────┐
│   CloudCoin      │        │   ShopSystem     │
│   System         │        │                  │
│                  │        │  - Owned Agents  │
│  - Balance       │◄───────┤  - Purchase      │
│  - Add/Remove    │        │  - Unlock        │
│  - Can Afford    │        │  - Initialize    │
└────────┬─────────┘        └────────┬─────────┘
         │                           │
         │         ┌─────────────────┘
         │         │
         ▼         ▼
    ┌────────────────────┐
    │    PlayerData      │
    │                    │
    │  + CoinSystem      │
    │  + ShopSystem      │
    │  + AgentID         │
    └──────────┬─────────┘
               │
               │ Accessed by
               │
      ┌────────┴─────────┐
      │                  │
      ▼                  ▼
┌─────────────┐    ┌─────────────────┐
│ UIShopView  │    │ UIAgentSelection│
│             │    │ View            │
│ - Show Shop │    │                 │
│ - Purchase  │    │ - Filter Owned  │
│ - Display $ │    │ - Select Agent  │
└─────────────┘    └─────────────────┘
```

## Data Flow

### 1. Player Creation
```
PlayerService.CreateNewPlayer()
    │
    └──> PlayerData Constructor
            │
            └──> ShopSystem.Initialize()
                    │
                    └──> Adds "Soldier" to owned agents
```

### 2. Shop View Opening
```
Main Menu
    │
    └──> User clicks "SHOP" button
            │
            └──> UIMainMenuView.OnShopButton()
                    │
                    └──> Opens UIShopView
                            │
                            └──> Populates UIList with UIShopItems
                                    │
                                    ├──> For each AgentSetup
                                    │       │
                                    │       └──> Creates UIShopItem
                                    │               │
                                    │               ├──> Shows Icon
                                    │               ├──> Shows Name
                                    │               ├──> Shows Price
                                    │               └──> Button (BUY/OWNED/SELECTED)
                                    │
                                    └──> Shows CloudCoins balance
```

### 3. Purchase Flow
```
User clicks "BUY" on locked agent
    │
    └──> UIShopItem.OnPurchaseButtonClicked()
            │
            └──> Calls onPurchaseCallback
                    │
                    └──> UIShopView.OnPurchaseAgent()
                            │
                            ├──> ShopSystem.TryUnlockAgent()
                            │       │
                            │       └──> CoinSystem.TryPurchase()
                            │               │
                            │               ├──> Check: CanAfford?
                            │               │       │
                            │               │       ├──> YES: Deduct coins
                            │               │       │       │
                            │               │       │       └──> Return true
                            │               │       │
                            │               │       └──> NO: Return false
                            │               │
                            │               └──> If true: Add to owned list
                            │
                            ├──> Play sound (success/fail)
                            │
                            └──> Refresh shop list
```

### 4. Agent Selection
```
User opens Agent Selection View
    │
    └──> UIAgentSelectionView.OnOpen()
            │
            └──> GetOwnedAgents()
                    │
                    ├──> Loop through all agents
                    │       │
                    │       └──> For each agent:
                    │               │
                    │               └──> ShopSystem.OwnsAgent(id)?
                    │                       │
                    │                       ├──> YES: Add to list
                    │                       └──> NO: Skip
                    │
                    └──> Display only owned agents
```

## File Structure

```
/Assets
├── /Scripts
│   ├── CloudCoinSystem.cs                      # Currency logic
│   ├── ShopSystem.cs                           # Ownership logic
│   ├── CloudCoinReward.cs                      # Test helper
│   ├── ShopSystemDebugHelper.cs                # Debug helper
│   ├── SHOP_SYSTEM_COMPLETE.md                 # Main guide (START HERE)
│   ├── ShopSystemIntegrationChecklist.md       # Setup checklist
│   ├── ShopSystemArchitecture.md               # This file
│   ├── ShopUISetupGuide.md                     # Detailed UI guide
│   └── QuickShopSetup.md                       # Quick guide
│
├── /Editor
│   ├── ShopUICreator.cs                        # UI creation tool
│   └── ShopSystemSetupHelper.cs                # Settings tool
│
└── /TPSBR
    ├── /Scripts
    │   ├── /Player
    │   │   ├── PlayerData.cs                   # ✏️ MODIFIED
    │   │   └── PlayerService.cs                # ✏️ MODIFIED
    │   │
    │   ├── /Settings
    │   │   └── AgentSettings.cs                # ✏️ MODIFIED
    │   │
    │   └── /UI
    │       ├── /MenuViews
    │       │   ├── UIMainMenuView.cs           # ✏️ MODIFIED
    │       │   ├── UIAgentSelectionView.cs     # ✏️ MODIFIED
    │       │   └── UIShopView.cs               # ✨ NEW
    │       │
    │       └── /Widgets
    │           └── UIShopItem.cs               # ✨ NEW
    │
    └── /UI/Prefabs
        ├── /MenuViews
        │   └── UIShopView.prefab               # ⚠️ NEEDS CREATION
        │
        └── /Widgets
            └── UIShopItem.prefab               # ⚠️ NEEDS CREATION
```

## Class Relationships

```
┌─────────────────────────────────────────────────────────────┐
│                     CLASS DIAGRAM                            │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────┐
│  CloudCoinSystem     │
├──────────────────────┤
│ - _cloudCoins: int   │
│ + CloudCoins: int    │
│ + IsDirty: bool      │
├──────────────────────┤
│ + CanAfford(cost)    │
│ + TryPurchase(cost)  │
│ + ClearDirty()       │
└──────────────────────┘

┌──────────────────────────────────┐
│  ShopSystem                      │
├──────────────────────────────────┤
│ - _ownedSkinsList: List<string>  │
│ - _ownedSkins: HashSet<string>   │
│ + IsDirty: bool                  │
├──────────────────────────────────┤
│ + Initialize()                   │
│ + OwnsAgent(id): bool            │
│ + TryUnlockAgent(...)            │
│ + ClearDirty()                   │
└──────────────────────────────────┘

┌──────────────────────────────────┐
│  PlayerData                      │
├──────────────────────────────────┤
│ - _coinSystem: CloudCoinSystem   │
│ - _shopSystem: ShopSystem        │
│ + CoinSystem: CloudCoinSystem    │
│ + ShopSystem: ShopSystem         │
│ + AgentID: string                │
├──────────────────────────────────┤
│ + PlayerData(userID)             │
│ + ClearDirty()                   │
└──────────────────────────────────┘

┌──────────────────────────────────┐
│  AgentSetup                      │
├──────────────────────────────────┤
│ - _agentID: string               │
│ - _displayName: string           │
│ - _icon: Sprite                  │
│ - _cloudCoinCost: int            │ ✨ NEW
├──────────────────────────────────┤
│ + ID: string                     │
│ + DisplayName: string            │
│ + Icon: Sprite                   │
│ + CloudCoinCost: int             │ ✨ NEW
└──────────────────────────────────┘

┌──────────────────────────────────┐
│  UIShopView : UIView             │
├──────────────────────────────────┤
│ - _shopItemsList: UIList         │
│ - _cloudCoinsText: TMP_Text      │
│ - _cloudCoinsFormat: string      │
├──────────────────────────────────┤
│ + OnOpen()                       │
│ + OnClose()                      │
│ - OnPurchaseAgent(agent)         │
│ - UpdateCloudCoinsDisplay()      │
└──────────────────────────────────┘

┌──────────────────────────────────┐
│  UIShopItem : UIBehaviour        │
├──────────────────────────────────┤
│ - _agentIcon: Image              │
│ - _agentName: TMP_Text           │
│ - _costText: TMP_Text            │
│ - _purchaseButton: UIButton      │
│ - _purchaseButtonText: TMP_Text  │
├──────────────────────────────────┤
│ + SetData(agent, player, cb)    │
│ - OnPurchaseButtonClicked()      │
│ - UpdateButtonState()            │
└──────────────────────────────────┘
```

## UI Hierarchy

```
UIShopView (RectTransform, Canvas, UIShopView)
├── Background (Image)
│   └── Color: rgba(0.1, 0.1, 0.1, 0.9)
│
└── Content (RectTransform) [800x600]
    ├── TitleText (TextMeshProUGUI)
    │   └── Text: "AGENT SHOP"
    │
    ├── CloudCoinsText (TextMeshProUGUI)
    │   └── Text: "CloudCoins: {balance}"
    │
    └── ShopItemsList (RectTransform, UIList, VerticalLayoutGroup)
        └── [UIShopItem instances created at runtime]
```

```
UIShopItem (RectTransform, UIShopItem, HorizontalLayoutGroup)
├── AgentIcon (Image)
│   └── Size: 100x100
│
├── InfoContainer (RectTransform, VerticalLayoutGroup)
│   ├── AgentName (TextMeshProUGUI)
│   │   └── Text: "Marine"
│   │
│   └── CostText (TextMeshProUGUI)
│       └── Text: "500 CloudCoins"
│
└── PurchaseButton (RectTransform, UIButton)
    └── ButtonText (TextMeshProUGUI)
        └── Text: "BUY" | "OWNED" | "SELECTED"
```

## State Machine

### UIShopItem Button States

```
Agent State Logic:

┌──────────────┐
│ Is Owned?    │
└──────┬───────┘
       │
    ┌──┴──┐
    │     │
   YES   NO
    │     │
    │     ├──> Can Afford?
    │     │       │
    │     │    ┌──┴──┐
    │     │    │     │
    │     │   YES   NO
    │     │    │     │
    │     │    │     └──> Button: DISABLED
    │     │    │           Text: "BUY"
    │     │    │
    │     │    └──> Button: ENABLED
    │     │           Text: "BUY"
    │     │           OnClick: Purchase
    │     │
    │     └──> Is Selected?
    │             │
    │          ┌──┴──┐
    │          │     │
    │         YES   NO
    │          │     │
    │          │     └──> Button: ENABLED
    │          │            Text: "OWNED"
    │          │            OnClick: Select
    │          │
    │          └──> Button: ENABLED
    │                 Text: "SELECTED"
    │                 OnClick: (Already selected)
```

## Event Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    EVENT SEQUENCE                            │
└─────────────────────────────────────────────────────────────┘

1. Game Start
   └──> PlayerService.Initialize()
           └──> Load or Create PlayerData
                   └──> ShopSystem.Initialize()

2. Main Menu → Shop
   └──> UIMainMenuView.OnShopButton()
           └──> Open<UIShopView>()
                   └──> UIShopView.OnOpen()
                           ├──> Get all agents from AgentSettings
                           ├──> Create UIShopItem for each
                           ├──> UIShopItem.SetData(...)
                           └──> UpdateCloudCoinsDisplay()

3. Purchase Attempt
   └──> User clicks "BUY"
           └──> UIShopItem.OnPurchaseButtonClicked()
                   └──> _onPurchaseCallback.Invoke(agent)
                           └──> UIShopView.OnPurchaseAgent(agent)
                                   ├──> ShopSystem.TryUnlockAgent(...)
                                   │       └──> CoinSystem.TryPurchase(...)
                                   │               ├──> Deduct coins
                                   │               └──> Return true/false
                                   │
                                   ├──> If success:
                                   │       ├──> Play purchase sound
                                   │       ├──> Refresh shop list
                                   │       └──> Update coins display
                                   │
                                   └──> If failure:
                                           └──> Play error sound

4. Agent Selection
   └──> User clicks owned agent button
           └──> UIShopItem.OnPurchaseButtonClicked()
                   └──> PlayerData.AgentID = selectedAgent
                           └──> UpdateButtonState()
                                   └──> Previous: "OWNED"
                                        Current: "SELECTED"
```

## Persistence Flow

```
Data Save/Load:

Game State
    │
    ├──> On PlayerData change:
    │       └──> IsDirty = true
    │
    ├──> CloudCoinSystem change:
    │       └──> CoinSystem.IsDirty = true
    │
    ├──> ShopSystem change:
    │       └──> ShopSystem.IsDirty = true
    │
    └──> PlayerService.Save():
            ├──> Serialize PlayerData
            │       ├──> _cloudCoins (int)
            │       └──> _ownedSkinsList (List<string>)
            │
            └──> After save:
                    └──> PlayerData.ClearDirty()
                            ├──> CoinSystem.ClearDirty()
                            └──> ShopSystem.ClearDirty()

Load:
    PlayerService.LoadPlayer()
        └──> Deserialize PlayerData
                ├──> _cloudCoins restored
                ├──> _ownedSkinsList restored
                └──> ShopSystem.Initialize()
                        └──> Rebuild _ownedSkins HashSet
```

## Key Design Decisions

### 1. Why Two Collections in ShopSystem?
```
_ownedSkinsList: List<string>     // Serializable for saving
_ownedSkins: HashSet<string>      // Fast O(1) lookup at runtime

On Initialize():
    _ownedSkins.Clear()
    foreach skin in _ownedSkinsList:
        _ownedSkins.Add(skin)
```

### 2. Why Soldier is Always Owned?
```
In ShopSystem.Initialize():
    if (_ownedSkins.Count == 0)
    {
        _ownedSkins.Add("Soldier")
        _ownedSkinsList.Add("Soldier")
    }
```
This ensures every player always has at least one playable agent.

### 3. Why UIShopItem Uses Callback?
```
Separation of concerns:
    UIShopItem: Display logic only
    UIShopView: Business logic (purchasing, sounds, refresh)
    
OnPurchaseButtonClicked() → _onPurchaseCallback.Invoke()
                                    ↓
                          UIShopView.OnPurchaseAgent()
```

## Testing Hooks

### Debug Access Points

1. **Add Coins**
```csharp
Global.PlayerService.PlayerData.CoinSystem.CloudCoins += 1000;
```

2. **Check Ownership**
```csharp
bool owns = Global.PlayerService.PlayerData.ShopSystem.OwnsAgent("Marine");
```

3. **Reset Shop**
```csharp
var shop = Global.PlayerService.PlayerData.ShopSystem;
shop._ownedSkinsList.Clear();
shop.Initialize(); // Re-adds Soldier
```

4. **Force Purchase**
```csharp
var shop = Global.PlayerService.PlayerData.ShopSystem;
shop._ownedSkins.Add("Marine");
shop._ownedSkinsList.Add("Marine");
```

---

## Next Steps

1. ✅ Review this architecture
2. ✅ Read `SHOP_SYSTEM_COMPLETE.md` for setup
3. ✅ Follow Quick Start guide
4. ✅ Create UI prefabs
5. ✅ Test and enjoy!
