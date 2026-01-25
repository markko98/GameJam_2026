using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public static class Utils
{
    // public static T CloneJson<T>(this T source)
    // {
    //     if (ReferenceEquals(source, null))
    //         return default;
    //
    //     var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
    //     return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
    // }

    public static string FormatTimeWithText(float seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.TotalDays >= 1)
        {
            return FormatTimeWithDays(timeSpan);
        } 
        return timeSpan.Hours > 0 ? $"{timeSpan.Hours:D2}h:{timeSpan.Minutes:D2}m:{timeSpan.Seconds:D2}s" : $"{timeSpan.Minutes:D2}m:{timeSpan.Seconds:D2}s";
    }
    
    public static string FormatTime(float seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        if (timeSpan.TotalDays >= 1)
        {
            return FormatTimeWithDays(timeSpan);
        } 
        return timeSpan.Hours > 0 ? $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}" : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
    
    public static string FormatTimeWithMs(float seconds, bool withText = true)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        var hundredths = timeSpan.Milliseconds / 10;
        var text = withText ? "s" : "";
        return timeSpan.Minutes < 10
            ? $"{timeSpan.Minutes:D1}:{timeSpan.Seconds:D2},{hundredths:D2}{text}"
            : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2},{hundredths:D2}{text}";
    }
    
    private static string FormatTimeWithDays(TimeSpan timeSpan)
    {
        int days = (int)timeSpan.TotalDays;
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;

        if (hours >= 24)
        {
            days += hours / 24;
            hours %= 24;
        }

        return $"{days:00}d:{hours:00}h:{minutes:00}m:{seconds:00}s";
    }

    public static float GetRandomValueFromRange(this Vector2 range)
    {
        return UnityEngine.Random.Range(range.x, range.y);
    }
    public static int GetRandomValueFromRange(this Vector2Int range, bool inclusive = true)
    {
        return UnityEngine.Random.Range(range.x, inclusive ? range.y + 1 : range.y);
    }
    
    public static string GetFilePath(string dir, string fileName)
    {
        string relativePath = Path.Combine(dir, $"{fileName}.mp3");
        string fullPath = Path.Combine(Application.persistentDataPath, relativePath);

        string directoryPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return fullPath;
    }
    
    public static IEnumerable<string> SplitCamelCase(this string source)
    {
        const string pattern = @"[A-Z][a-z]*|[a-z]+|\d+";
        var matches = Regex.Matches(source, pattern);
        foreach (Match match in matches)
        {
            yield return match.Value;
        }
    }
    
    public static string GetSeparatedCamelCase(string source)
    {
        var strings = SplitCamelCase(source);
        return strings.Aggregate("", (current, str) => current + (str + " "));
    }
    
    public static List<T> Shuffle<T>(this List<T> elements)
    {
        for (var i = elements.Count - 1; i > 0; i--)
        {
            var j = UnityEngine.Random.Range(0, i + 1);
            (elements[i], elements[j]) = (elements[j], elements[i]);
        }

        return elements;
    }
    
    public static string XorString(string encryptionKey, string input)
    {
        StringBuilder sb = new StringBuilder();
        for(int i=0; i < input.Length; i++)
            sb.Append((char)(input[i] ^ encryptionKey[(i % encryptionKey.Length)]));
        String result = sb.ToString ();

        return result;
    }
    
    public static float RoundUpToStep(float value, float step)
    {
        if (step <= 0f) return value;
        return Mathf.Ceil(value / step) * step;
    }
    
    
    public static void RefreshLayoutNow(List<RectTransform> rectTransforms)
    {
        Canvas.ForceUpdateCanvases();
        foreach (var rectTransform in rectTransforms)
        {
            ForceRebuild(rectTransform);
        }
        Canvas.ForceUpdateCanvases(); // one more for good measure
    }
    
    public static void RefreshLayoutEndOfFrame(List<RectTransform> rectTransforms)
    {
        CoroutineExecutor.Run(_RefreshLayoutEndOfFrame(rectTransforms));
    }

    private static IEnumerator _RefreshLayoutEndOfFrame(List<RectTransform> rectTransforms)
    {
        yield return null;
        RefreshLayoutNow(rectTransforms);
    }
    
    private static void ForceRebuild(RectTransform rt)
    {
        if (!rt) return;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    public static bool IntToBool(int isShown)
    {
        return isShown == 1;
    }

    public static int BoolToInt(bool isShown)
    {
        return isShown ? 1 : 0;
    }

    public static int LayerMaskToIndex(LayerMask mask)
    {
        var v = mask.value;
        for (int i = 0; i < 32; i++)
            if ((v & (1 << i)) != 0)
                return i;
        return 0;
    }

    public static void CenterModelByBounds(Transform modelRoot, bool placeAtBottom = false)
    {
        if (!modelRoot) return;

        var renderers = modelRoot.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (renderers.Length == 0) return;

        var bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        Vector3 offset = Vector3.zero;
        var centerLocal = modelRoot.InverseTransformPoint(bounds.center);
        if (placeAtBottom == false)
        {
            offset = centerLocal;
        }
        else
        {
            var minLocal =
                modelRoot.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
            offset = new Vector3(centerLocal.x, minLocal.y, centerLocal.z);
        }

        modelRoot.localPosition -= offset;
    }
}