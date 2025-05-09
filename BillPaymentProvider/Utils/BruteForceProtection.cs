using System.Collections.Concurrent;
using System;

namespace BillPaymentProvider.Utils
{
    public class BruteForceProtection
    {
        private static readonly ConcurrentDictionary<string, (int Attempts, DateTime? LockoutUntil)> _loginAttempts = new();
        private const int MaxAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(10);

        public static bool IsLockedOut(string username)
        {
            if (_loginAttempts.TryGetValue(username, out var entry))
            {
                if (entry.LockoutUntil.HasValue && entry.LockoutUntil.Value > DateTime.UtcNow)
                    return true;
            }
            return false;
        }

        public static void RegisterFailedAttempt(string username)
        {
            _loginAttempts.AddOrUpdate(username,
                (1, null),
                (key, old) =>
                {
                    int attempts = old.Attempts + 1;
                    if (attempts >= MaxAttempts)
                        return (attempts, DateTime.UtcNow.Add(LockoutDuration));
                    return (attempts, null);
                });
        }

        public static void ResetAttempts(string username)
        {
            _loginAttempts.TryRemove(username, out _);
        }
    }
}
