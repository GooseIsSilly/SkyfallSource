namespace TPSBR
{
    /// <summary>
    /// Single source of truth for the dedicated game-server address.
    /// All session-creation views read from here so changing the IP/port
    /// only needs to happen in one place.
    /// </summary>
    public static class DedicatedServerSettings
    {
        /// <summary>IP address of the dedicated Fusion game server.</summary>
        public const string IPAddress = "198.50.250.196";

        /// <summary>UDP port the Fusion server is listening on (default Fusion port).</summary>
        public const ushort Port = 27016;
    }
}
