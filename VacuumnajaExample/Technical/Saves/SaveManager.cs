namespace _Scripts.Technical.Saves
{
    public static class SaveManager
    {
        public static void Save<T>(string tag, T value)
        {
            ES3.Save(tag, value);
        }

        public static T Get<T>(string tag)
        {
            return ES3.Load<T>(tag);
        }
        public static bool HasKey(string tag)
        {
            return ES3.KeyExists(tag);
        }
    }
}