using UnityEngine;
using Verse;
using VerseText = Verse.Text;
using UnityText = UnityEngine.UI.Text;

namespace TeleCore.UI;

public static class TWidgets
{
    public static class Text
    {
        public static void TextFieldNumeric<T>(Rect rect, ref T value, float min = 0, float max = float.MaxValue) where T : unmanaged
        {
            var ctrlName = $"TextField{rect.y:F0}{rect.x:F0}";
            var buffer = UICache.GetBuffer(ctrlName);
            
            using var focus = new GUIFocus(rect, ctrlName);
            
            var fieldValue = TextField(rect, buffer);
            
            if (GUI.GetNameOfFocusedControl() != ctrlName)
            {
                Widgets.ResolveParseNow<T>(buffer, ref value, ref buffer, min, max, true);
                return;
            }
        
            if (fieldValue != buffer && Widgets.IsPartiallyOrFullyTypedNumber<T>(ref value, fieldValue, min, max))
            {
                buffer = fieldValue;
                if (fieldValue.IsFullyTypedNumber<T>())
                {
                    Widgets.ResolveParseNow<T>(fieldValue, ref value, ref buffer, min, max, false);
                }
            }
        }
        
        public static string TextField(Rect rect, string? text)
        {
            text ??= "";
        
            using var context = new GUIStyleContext(GUI.skin.textField, true);
            var result = GUI.TextField(rect, text, context.Style);
        
            return result;
        }

    }

    public struct UIMaterial
    {
        public Texture2D Texture { get; }
        public Material Material { get; }
    }
    
    public static class Image
    {
        public static void DrawTextureWithSlider(Rect rect, UIMaterial material, ref float sliderVal, float min = 0, float max = 1)
        {
            Widgets.DrawTextureFitted(rect, material.Texture, 1, mat:material.Material);
            sliderVal = GUI.HorizontalSlider(rect, sliderVal, min, max, null, GUI.skin.horizontalSliderThumb);
        }
    }
    
    #region Grouping

    public static void BeginGroup(Rect position)
    {
        BeginGroup(position, GUIContent.none, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, string text)
    {
        BeginGroup(position, text, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, Texture image)
    {
        BeginGroup(position, image, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, GUIContent content)
    {
        BeginGroup(position, content, GUIStyle.none);
    }

    public static void BeginGroup(Rect position, GUIStyle style)
    {
        BeginGroup(position, GUIContent.none, style);
    }

    public static void BeginGroup(Rect position, string text, GUIStyle style)
    {
        GUI.BeginGroup(position, text, style);
        UnityGUIBugsFixer.Notify_BeginGroup();
    }

    public static void BeginGroup(Rect position, Texture image, GUIStyle style)
    {
        GUI.BeginGroup(position, image, style);
        UnityGUIBugsFixer.Notify_BeginGroup();
    }

    public static void BeginGroup(Rect position, GUIContent content, GUIStyle style)
    {
        GUI.BeginGroup(position, content, style);
        UnityGUIBugsFixer.Notify_BeginGroup();
    }
    
    public static void EndGroup()
    {
        GUI.EndGroup();
        UnityGUIBugsFixer.Notify_EndGroup();
    }
    
    #endregion
}