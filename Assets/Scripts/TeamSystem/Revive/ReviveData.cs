using Fusion;
using UnityEngine;

namespace TPSBR
{
    public struct ReviveData : INetworkStruct
    {
        public bool IsDown;
        public TickTimer ReviveTimer;
        public PlayerRef RevivingPlayer;
        public TickTimer BleedOutTimer;

        public NetworkBool HasRevivingPlayer { get { return _flags.IsBitSet(0); } set { _flags.SetBit(0, value); } }

        private byte _flags;
    }

    public class ReviveSettings
    {
        public const float REVIVE_DURATION = 5.0f;
        public const float BLEED_OUT_DURATION = 30.0f;
        public const float REVIVE_INTERACTION_DISTANCE = 2.5f;
        public const float REVIVE_HEALTH_RESTORED = 30f;
    }
}
