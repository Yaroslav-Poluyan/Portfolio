using UnityEngine;

namespace _Scripts.Technical.Helpers
{
    public static class IPHelper
    {
        public static bool CheckForCorrectIP(string ip)
        {
            if (ip.Length <= 0)
                return false;
            if (ip.Length > 15)
                return false;
            if (ip.Contains(" "))
                return false;
            if (ip.Contains("  ")) // 2 spaces
                return false;
            return true;
        }
    }
}