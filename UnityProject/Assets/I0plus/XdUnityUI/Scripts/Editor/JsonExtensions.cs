using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    public static class JsonExtensions
    {
        public static string Get(this Dictionary<string, object> json, string key)
        {
            if (json == null || !json.ContainsKey(key)) return null;
            var value = json[key];
            return value as string;
        }

        public static bool? GetBool(this Dictionary<string, object> json, string key)
        {
            if (json == null || !json.ContainsKey(key)) return null;
            var value = json[key];
            switch (value)
            {
                case bool b:
                    return b;
                case string str:
                    return str != "null" && str != "false" && str != "0";
            }

            return true;
        }


        public static float? GetFloat(this Dictionary<string, object> json, string key)
        {
            if (json == null || !json.ContainsKey(key))
            {
                return null;
            }

            return (float) json[key];
        }

        public static int? GetInt(this Dictionary<string, object> json, string key)
        {
            if (json == null || !json.ContainsKey(key))
            {
                return null;
            }

            return (int) (float) json[key];
        }

        public static T Get<T>(this Dictionary<string, object> json, string key) where T : class
        {
            if (json == null || !json.ContainsKey(key))
            {
                return null;
            }

            return json[key] as T;
        }

        public static Dictionary<string, object> GetDic(this Dictionary<string, object> json, string key)
        {
            if (json == null || !json.ContainsKey(key))
            {
                return null;
            }

            return json[key] as Dictionary<string, object>;
        }

        public static Vector2? GetVector2(this Dictionary<string, object> json, string keyX, string keyY)
        {
            if (json == null || !json.ContainsKey(keyX) || !json.ContainsKey(keyY))
            {
                return null;
            }

            return new Vector2((float) json[keyX], (float) json[keyY]);
        }
    }
}