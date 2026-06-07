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
        public static string IPAddress = "0.0.0.0";

        /// <summary>UDP port the Fusion server is listening on (default Fusion port).</summary>
        public static ushort Port = 7777;
    }
}
