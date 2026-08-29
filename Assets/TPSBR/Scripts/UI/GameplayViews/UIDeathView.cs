namespace TPSBR.UI
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using TMPro;

    public class UIDeathView : UIView
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private Transform _respawnGroup;
        [SerializeField]
        private TextMeshProUGUI _respawnTime;
        [SerializeField]
        private Transform _spectatorGroup;

        // UIView INTERAFCE

        protected override void OnOpen()
        {
            base.OnOpen();
            Refresh();
        }

        protected override void OnTick()
        {
            base.OnTick();
            Refresh();
        }

        private void Refresh()
        {
            if (Context.Runner == null || Context.Runner.Exists(Context.GameplayMode.Object) == false)
                return;

            var player = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
            var statistics = player != null ? player.Statistics : default;

            if (statistics.IsEliminated == false)
            {
                _respawnGroup.SetActive(true);
                _respawnTime.text = $"{statistics.RespawnTimer.RemainingTime(Context.Runner):F1} s";
                _spectatorGroup.SetActive(false);
            }
            else
            {
                _respawnGroup.SetActive(false);
                _spectatorGroup.SetActive(true);

                // FIX: Keyboard.current can be null (no keyboard device / not focused / certain platforms).
                // The previous unguarded access threw a NullReferenceException every tick this view was
                // open, which silently broke any UI ticked after this one in the same frame loop
                // (e.g. sniper scope / weapon HUD) -- causing the "UI freezes after death" bug.
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.xKey.wasPressedThisFrame == true)
                    {
                        Context.GameplayMode.ChangeSpectatorTarget(true);
                    }
                    else if (Keyboard.current.zKey.wasPressedThisFrame == true)
                    {
                        Context.GameplayMode.ChangeSpectatorTarget(false);
                    }
                }
            }
        }
    }
}