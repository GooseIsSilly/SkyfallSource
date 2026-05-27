╔══════════════════════════════════════════════════════════════════════════════╗
║                    LIVE EVENT SYSTEM - QUICK START                           ║
╚══════════════════════════════════════════════════════════════════════════════╝

AUTOMATED PREFAB CREATION (EASIEST):
=====================================

1. CREATE COUNTDOWN PREFAB
   Project Window → Right-Click → Create → Skyfall → Live Event Countdown Prefab
   ✓ Creates: /Assets/Prefabs/LiveEventCountdown.prefab
   
2. DRAG INTO SCENE
   ✓ Drag prefab to your map
   ✓ Position anywhere (high in sky, above landmarks, etc.)
   ✓ Yellow gizmo shows position in Scene view

3. CREATE EVENT DATA
   Project Window → Right-Click → Create → Skyfall → Live Event Data
   ✓ In Inspector, click "Set to 1 Minute from Now" (for testing)
   ✓ Assign your animation clip

4. SETUP SATELLITE
   Select your satellite → Right-Click → Skyfall → Setup Satellite Event Object
   ✓ Adds all needed components automatically
   ✓ Assign the event data in inspector

5. ADD MANAGER
   ✓ Create empty GameObject → Add LiveEventManager component
   ✓ Assign event data to array
   ✓ Ensure it's networked (spawned by Fusion)

DONE! Press Play and watch the countdown! 🚀


MENU SHORTCUTS:
===============

In Project Window:
  • Create → Skyfall → Live Event Data
  • Create → Skyfall → Live Event Countdown Prefab

In Hierarchy:
  • Skyfall → Create World Countdown Text
  • Skyfall → Setup Satellite Event Object (with satellite selected)


FEATURES:
=========
✓ Global UTC time sync - all servers stay in sync
✓ Countdown continues even offline
✓ Giant 3D floating text
✓ Auto billboard to camera
✓ Red text + pulsing in last minute (configurable)
✓ Event name display above countdown
✓ Animation triggering on event start
✓ Audio support
✓ Fully networked with Photon Fusion


TROUBLESHOOTING:
================

Text not visible?
  → Check Base Scale (try 10-20 for huge text)
  → Ensure GameObject is active
  → Position where camera can see it

Event already passed?
  → Click "Set to 1 Minute from Now" in Event Data inspector

Animation not playing?
  → Assign animation clip to Event Data
  → Verify Target Event matches on LiveEventAnimationTrigger


For full documentation, see:
/Pages/Live Event System Setup.md
