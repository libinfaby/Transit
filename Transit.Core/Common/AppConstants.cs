namespace Transit.Core.Common
{
    public static class AppConstants
    {
        public const int DefaultServerPort = 45000;
        public const int DefaultClientPort = 45001; // Base port, might increment?
        public const int HeartbeatIntervalMs = 5000;
        public const int FileChunkSize = 64 * 1024; // 64KB
    }
}
