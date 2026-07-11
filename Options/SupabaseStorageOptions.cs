namespace AppWebSistemaComandasDigital.Options
{
    public sealed class SupabaseStorageOptions
    {
        public const string SectionName = "SupabaseStorage";
        public string Url { get; set; } = string.Empty;
        public string ServiceRoleKey { get; set; } = string.Empty;
        public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
        public SupabaseStorageBuckets Buckets { get; set; } = new();
    }

    public sealed class SupabaseStorageBuckets
    {
        public string Platos { get; set; } = "platos";
        public string Logos { get; set; } = "logos";
        public string Usuarios { get; set; } = "usuarios";
    }
}
