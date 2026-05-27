# Fortnite-Style Party System Setup Guide

Complete setup instructions for the party/friends system with character previews in the menu lobby.

---

## What's New

### 1. Team Mode Dropdown
- Solo/Duo/Squad selection in Create Session view
- Automatically sets team mode when creating a game

### 2. Party Menu Button
- Opens party management view from main menu
- Add friends and manage party before matchmaking
- Similar to Fortnite's party system

### 3. Character Previews in Lobby
- Shows all party members' character skins in the menu
- Characters displayed side-by-side like Fortnite
- Updates automatically when party changes

---

## Setup Instructions

### STEP 1: Add Team Mode Dropdown to Create Session View

1. **Open Scene:**
   - Open `Assets/TPSBR/Scenes/Menu.unity`

2. **Select UICreateSessionView:**
   - Navigate to `/MenuUI/UICreateSessionView`

3. **Add Dropdown:**
   - Duplicate the existing Gameplay dropdown
   - Rename it to `TeamModeDropdown`
   - Position it below the Gameplay dropdown

4. **Assign Reference:**
   - Select `/MenuUI/UICreateSessionView`
   - In the `UICreateSessionView` component
   - Assign `TeamModeDropdown` to the `Team Mode` field

**Result:** Players can now select Solo/Duo/Squad when creating a session!

---

### STEP 2: Add Party Button to Main Menu

1. **Locate Main Menu Buttons:**
   - In Menu scene, find `/MenuUI/UIMainMenuView`
   - You'll see: PlayButton, SettingsButton, CreditsButton, QuitButton

2. **Duplicate Settings Button:**
   - Right-click `SettingsButton` → Duplicate
   - Rename to `PartyButton`
   - Position it between Play and Settings

3. **Change Label:**
   - Expand `PartyButton` → Find `Label` text
   - Change text from "Settings" to "Party"

4. **Create Party View:**
   - Right-click `/MenuUI` → Create Empty
   - Name it `UIPartyView`
   - Add component `UIPartyView`

5. **Connect Button:**
   - Select `/MenuUI/UIMainMenuView`
   - Find the `UIMainMenuView` component script
   - Add Party button click handler (you may need to edit the script)

**Alternative:** Ask a developer to add the Party button code to `UIMainMenuView.cs`

---

### STEP 3: Setup Party View UI

#### Basic Structure:

```
UIPartyView
├── BlurBackground
├── ViewHeader
│   ├── Title (Text: "PARTY & FRIENDS")
│   └── BackButton
├── LeftPanel (Friends List)
│   ├── Title (Text: "FRIENDS")
│   ├── AddFriendButton
│   ├── FriendsContainer (auto-populated)
│   └── NoFriendsMessage
├── RightPanel (Current Party)
│   ├── Title (Text: "CURRENT PARTY")
│   ├── CreatePartyButton
│   ├── LeavePartyButton
│   ├── PartyContainer (auto-populated)
│   └── NoPartyMessage
└── AddFriendDialog
    ├── FriendIDInput
    ├── FriendNicknameInput
    ├── ConfirmButton
    └── CancelButton
```

#### Assign References in UIPartyView Component:

**Friends List:**
- `Friends Container` → LeftPanel/FriendsContainer transform
- `Add Friend Button` → LeftPanel/AddFriendButton
- `No Friends Message` → LeftPanel/NoFriendsMessage

**Party Section:**
- `Party Container` → RightPanel/PartyContainer transform
- `Create Party Button` → RightPanel/CreatePartyButton
- `Leave Party Button` → RightPanel/LeavePartyButton
- `No Party Message` → RightPanel/NoPartyMessage

**Add Friend Dialog:**
- `Add Friend Dialog` → AddFriendDialog GameObject
- `Friend ID Input` → AddFriendDialog/FriendIDInput
- `Friend Nickname Input` → AddFriendDialog/FriendNicknameInput
- `Confirm Add Friend Button` → AddFriendDialog/ConfirmButton
- `Cancel Add Friend Button` → AddFriendDialog/CancelButton

---

### STEP 4: Setup Character Previews in Lobby

1. **Add Character Preview Manager:**
   - In Menu scene, find `/MenuUI/UIMultiplayerView`
   - Add child GameObject: `PartyCharacterPreviews`
   - Add component `UIPartyCharacterPreviews`

2. **Create Preview Container:**
   - Create empty GameObject in the scene (outside UI)
   - Name it `CharacterPreviewContainer`
   - Position it where you want characters to appear (e.g., center-bottom of screen)

3. **Setup Camera (Optional):**
   - If you want dedicated character preview camera:
   - Duplicate `SceneCamera/Camera`
   - Name it `PreviewCamera`
   - Set Culling Mask to only show character layer
   - Assign to `Preview Camera` field

4. **Configure Component:**
   ```
   Preview Container    → CharacterPreviewContainer transform
   Spacing             → 150 (distance between characters)
   Base Position       → (-200, -100, 5) adjust for your scene
   Rotation            → (0, 160, 0) characters face slightly towards camera
   Preview Camera      → (Optional) PreviewCamera
   Player Preview Prefab → (Optional) Your character prefab
   ```

5. **Position Characters:**
   - Test by creating a party
   - Adjust `Base Position` and `Spacing` values
   - Characters should appear side-by-side in view

---

## How It Works

### Creating a Party

1. Click "Party" button in main menu
2. Click "Create Party"  
3. You're now party leader (★ icon)
4. Invite friends using "Invite" button next to their name

### Adding Friends

1. In Party view, click "Add Friend"
2. Enter friend's User ID and Nickname
3. Click "Confirm"
4. Friend appears in your friends list

### Inviting to Party

1. Must be party leader or create party first
2. Click "Invite" next to online friend (green dot)
3. Friend receives invite (simulated for now)
4. When they accept, they appear in party list

### Character Previews

- Automatically shows when you open multiplayer lobby
- Displays your character if solo
- Shows all party members' characters side-by-side when in party
- Updates in real-time when party changes

### Team Mode Selection

1. Click "Play" → "Create Session"
2. Select map and gameplay type
3. Select team mode: Solo / Duo / Squad
4. Create session with selected team mode

---

## UI Widget Auto-Generation

All party member and friend widgets are **auto-generated at runtime**:

### Friend Widget includes:
- Friend nickname
- Online status indicator (green/gray dot)
- Invite button (green)
- Remove button (red)

### Party Member Widget includes:
- Member nickname  
- Leader icon (★) for party leader
- Kick button (only shown to leader, not for yourself)

### Character Preview includes:
- 3D character model (or placeholder capsule)
- Player nickname above character
- Auto-positioned based on party size

---

## Testing

### Test Party System:

1. **Test Friends:**
   ```
   - Add a friend (use any test User ID)
   - Friend appears in list
   - Remove friend
   - Friend disappears
   ```

2. **Test Party Creation:**
   ```
   - Create party
   - You become leader (★)
   - Invite friend
   - Leave party
   - Party disbanded
   ```

3. **Test Character Previews:**
   ```
   - Go to multiplayer lobby (solo)
   - Your character appears
   - Create party
   - Multiple characters appear side-by-side
   - Leave party
   - Back to single character
   ```

4. **Test Team Mode:**
   ```
   - Create Session
   - Select "Solo" - only 1 per team
   - Select "Duo" - 2 per team
   - Select "Squad" - 4 per team
   ```

---

## Quick Checklist

```
□ Team Mode dropdown added to Create Session view
□ Team Mode dropdown assigned in UICreateSessionView component
□ Party button added to main menu
□ Party button opens UIPartyView
□ UIPartyView created with left/right panels
□ Friends container assigned
□ Party container assigned
□ All buttons assigned (Add Friend, Create/Leave Party)
□ Add Friend dialog created and assigned
□ UIPartyCharacterPreviews added to multiplayer view
□ Character preview container created in scene
□ Preview positions configured
□ PartyLobbyManager exists in Menu scene
□ Test: Create party
□ Test: Add friend
□ Test: Character previews appear
□ Test: Select team mode (Solo/Duo/Squad)
```

---

## File Structure

```
/Assets/Scripts/TeamSystem/
├── UI/
│   ├── UIPartyView.cs                    # Main party/friends view
│   ├── UIFriendListWidget.cs             # Friend list item widget
│   ├── UIPartyMemberWidget.cs            # Party member widget
│   └── UIPartyCharacterPreviews.cs       # Character preview manager
├── PartyLobbyManager.cs                   # Backend party management
├── TeamData.cs                            # Party data structure
└── FriendsList.cs                         # Friends data structure
```

---

## Customization

### Change Colors:
- Edit widget creation code in UI scripts
- Modify background, button, and icon colors

### Change Layout:
- Adjust `widgetHeight` and `widgetSpacing` in UIPartyView
- Modify anchor positions in widget creation

### Character Spacing:
- Increase/decrease `Spacing` in UIPartyCharacterPreviews
- Adjust `Base Position` to move all characters

### Use Real Character Models:
- Create character prefab with `CharacterPreview` component
- Assign to `Player Preview Prefab` field
- Preview manager will use your prefab instead of placeholder

---

## Known Limitations

1. **Friends are simulated** - currently stored locally, not networked
2. **Party invites are simulated** - need to integrate with your networking
3. **Character previews** - uses placeholder capsules by default, assign real character prefabs
4. **Online status** - randomly simulated, integrate with actual player presence system

---

Need help with specific setup steps? Check the main Team System Setup Guide page!
