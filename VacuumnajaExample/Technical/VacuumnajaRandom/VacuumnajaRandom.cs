using UnityEngine;

namespace _Scripts.Technical.VacuumnajaRandom
{
    public static class VacuumnajaRandom
    {
        public static float GetNextFloat(float min, float max)
        {
            return Random.Range(min, max);
        }

        public static float GetNextFloat(float max)
        {
            return Random.Range(0, max);
        }

        public static float GetNextFloat()
        {
            return Random.Range(0, 1);
        }

        public static int GetNextInt(int min, int max)
        {
            return Random.Range(min, max);
        }

        public static int GetNextInt(int max)
        {
            return Random.Range(0, max);
        }

        public static int GetNextInt()
        {
            return Random.Range(0, int.MaxValue);
        }

        public static bool GetNextBool()
        {
            return Random.Range(0, 2) == 1;
        }

        public static Color GetNextHSVColor()
        {
            return Color.HSVToRGB(GetNextFloat(0, 1), GetNextFloat(0, 1), GetNextFloat(0, 1));
        }
    }
}