using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

public static class Serializer
{
    public static void SaveToPlayerPrefs<T>(string key, T data)
    {
        if (data == null)
        {
            Debug.Log("Data is null for key:" + key);
            return;
        }

        byte[] bytes = Serialize(data);
        string base64String = Convert.ToBase64String(bytes);
        PlayerPrefs.SetString(key, base64String);
    }

    public static T LoadFromPlayerPrefs<T>(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            string base64String = PlayerPrefs.GetString(key);

            if (String.IsNullOrEmpty(base64String))
            {
                if (typeof(T) == typeof(int))
                {
                    base64String = Convert.ToBase64String(Serialize(PlayerPrefs.GetInt(key)));
                }
                else if (typeof(T) == typeof(float))
                {
                    base64String = Convert.ToBase64String(Serialize(PlayerPrefs.GetFloat(key)));
                }
                else if (typeof(T) == typeof(string))
                {
                    base64String = Convert.ToBase64String(Serialize(PlayerPrefs.GetString(key)));
                }
                else
                {
                    // Should never happen
                    Debug.Log("Attempted to deserialize a non primitive type");
                }

                PlayerPrefs.SetString(key, base64String);
            }

            byte[] bytes = Convert.FromBase64String(base64String);
            T result = Deserialize<T>(bytes);
            AutoInitializeCollections(result);
            return result;
        }
        else
        {
            return default(T);
        }
    }

    public static void AutoInitializeCollections<T>(T obj)
    {
        if (obj == null) return;

        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            if (value != null) continue;

            var fieldType = field.FieldType;

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var newInstance = Activator.CreateInstance(fieldType);
                field.SetValue(obj, newInstance);
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var newInstance = Activator.CreateInstance(fieldType);
                field.SetValue(obj, newInstance);
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Queue<>))
            {
                var newInstance = Activator.CreateInstance(fieldType);
                field.SetValue(obj, newInstance);
            }
            else if (fieldType == typeof(string))
            {
                field.SetValue(obj, string.Empty);
            }
        }
    }

    private static byte[] Serialize<T>(T data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, data);
            return stream.ToArray();
        }
    }

    private static T Deserialize<T>(byte[] bytes)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream stream = new MemoryStream(bytes))
        {
            object obj = formatter.Deserialize(stream);
            return (T)obj;
        }
    }
}