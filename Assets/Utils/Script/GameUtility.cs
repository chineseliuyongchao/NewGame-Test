using System.Collections.Generic;
using UnityEngine;

namespace Utils.Script
{
    public class GameUtility
    {
        public static void AnalysisJsonConfigurationTable<T>(TextAsset textAsset, Dictionary<int, T> dictionary)
            where T : BaseJsonData
        {
            T[] tData = ParseJson<T>(textAsset.text);
            foreach (T t in tData)
            {
                dictionary.TryAdd(t.ID, t);
            }
        }

        public static T[] ParseJson<T>(string jsonString)
        {
            // 将JSON数据转换成指定类型的对象数组
            JsonWrapper<T> jsonWrapper = JsonUtility.FromJson<JsonWrapper<T>>(jsonString);
            return jsonWrapper?.Sheet1;
        }

        /// <summary>
        /// 补齐字符串的长度
        /// </summary>
        /// <returns></returns>
        public static string CompletionLength(string str, int len)
        {
            int padding = len - str.Length;
            string paddingSpaces = new string(' ', padding);
            return str + paddingSpaces;
        }
    }
}