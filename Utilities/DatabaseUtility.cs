using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text.Json;

namespace EmergencyManagement.Utilities
{
    public static class DatabaseUtility
    {
        //private readonly string _connectionString;

        //public DatabaseUtility(IConfiguration configuration)
        //{
        //    _connectionString = configuration.GetConnectionString("EMSConnection");
        //}
        //public static string HashPassword(string password)
        //{
        //    using (var deriveBytes = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA256))
        //    {
        //        var salt = deriveBytes.Salt;
        //        var key = deriveBytes.GetBytes(32); // 256-bit

        //        var result = new byte[48];
        //        Buffer.BlockCopy(salt, 0, result, 0, 16);
        //        Buffer.BlockCopy(key, 0, result, 16, 32);

        //        return Convert.ToBase64String(result);
        //    }
        //}

        public static Dictionary<string, object>? GetFirstSection(Dictionary<string, object> formData, string key)
            {
                if (formData.TryGetValue(key, out var section) && section is JsonElement jsonArray && jsonArray.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonArray.GetRawText())?.FirstOrDefault();
                }
                return null;
            }

        public static List<Dictionary<string, object>> GetSectionList(Dictionary<string, object> formData, string key)
        {
            if (formData.TryGetValue(key, out var section) && section is JsonElement jsonArray && jsonArray.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonArray.GetRawText()) ?? new();
            }
            return new();
        }

        public static int? ParseInt(object? value)
        {
            return int.TryParse(value?.ToString(), out var result) ? result : (int?)null;
        }

        public static DateTime? ParseDate(object? value)
        {
            if (DateTime.TryParse(value?.ToString(), out var result))
            {
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            }
            return null;
        }

        public static class DateTimeHelper
        {
            public static DateTime EnsureUtc(DateTime dateTime)
            {
                if (dateTime.Kind == DateTimeKind.Utc)
                    return dateTime;
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            public static DateTime? EnsureUtc(DateTime? dateTime)
            {
                if (dateTime == null)
                    return null;
                return EnsureUtc(dateTime.Value);
          //      return dateTime.Value.Kind == DateTimeKind.Utc
          //? dateTime
          //: DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
            }
        }
       public static IQueryable<T> WhereIf<T>(
       this IQueryable<T> query,
       bool condition,
       Expression<Func<T, bool>> predicate)
        {
            return condition ? query.Where(predicate) : query;
        }

    }
}
