using System;
using System.Text.RegularExpressions;
using TeleCore.Shared;
using UnityEngine;
using Verse;
using VerseText = Verse.Text;
using UnityText = UnityEngine.UI.Text;

namespace TeleCore.UI;

public static class TWidgets
{
    public static class Text
    {
        private static Regex HEXREGEX = new Regex("^#([0-9A-Fa-f]{0,6})$");
        
        public static string TextField(Rect rect, string text, int maxLength = -1, Regex? inputValidator = null)
        {
            var ctrlName = $"TeleTextField{rect.y:F0}{rect.x:F0}";
            using var focus = new GUIFocus(rect, ctrlName);
            
            using var context = new GUIStyleContext(GUI.skin.textField, true);
            var result = GUI.TextField(rect, text, maxLength, context.Style);
            if (inputValidator == null || inputValidator.IsMatch(result))
            {
                return result;
            }
            return text;
        }
        
        public static bool TextFieldHex(Rect rect, out string hexResult, string? initialValue = null, string? context = null)
        {
            var ctrlName = $"TextFieldHex{rect.y:F0}{rect.x:F0}";
            using var focus = new GUIFocus(rect, ctrlName);
            //GUIStyle style;
            
            var buffer = UICache.GetBuffer(ctrlName, initialValue ?? "FFFFFF", context);
            var inputRes = GUI.TextField(rect, "#" + buffer, maxLength: 7);
            var match = HEXREGEX.Match(inputRes);
            var result = buffer;
            if (match.Success)
            {
                 result = match.Groups[1].Value;
            }
            UICache.SetBuffer(ctrlName, result, context);
            hexResult = result;
            return hexResult != buffer;
        }

        private static string ReverseFormat(string formatted, string format)
        {
            string pattern = Regex.Escape(format).Replace(@"\{0\}", "(.*)");
            var match = Regex.Match(formatted, pattern);
            return match.Success ? match.Groups[1].Value : formatted;
        }
        
        public static bool TextFieldNumeric<T>(Rect rect, ref T value, float min = 0, float max = float.MaxValue, GUIStyle? style = null, string? context = null) where T : unmanaged
        {
            var ctrlName = $"TextField{rect.y:F0}{rect.x:F0}";
            var buffer = UICache.GetBuffer(ctrlName, value, context);
            
            using var focus = new GUIFocus(rect, ctrlName);
            if(style != null)
                GUI.skin.textField = style;
            
            var valPrev = value;
            var fieldValue = TextField(rect, buffer);
            
            if (GUI.GetNameOfFocusedControl() != ctrlName)
            {
                Widgets.ResolveParseNow(buffer, ref value, ref buffer, min, max, true);
                return ValueChanged<T>(value, valPrev);
            }
        
            if (fieldValue != buffer && Widgets.IsPartiallyOrFullyTypedNumber<T>(ref value, fieldValue, min, max))
            {
                buffer = fieldValue;
                if (fieldValue.IsFullyTypedNumber<T>() || buffer.Length == 0)
                {
                    Widgets.ResolveParseNow(fieldValue, ref value, ref buffer, min, max, false);
                }
                
                //Update buffer to potential value
                UICache.SetBuffer(ctrlName, buffer, context);
            }
            return ValueChanged<T>(value, valPrev);
        }
        
        public static bool ValueChanged<T>(Numeric<T> value, Numeric<T> value2) where T : unmanaged
        {
            var diff = MathG.Abs(value - value2);
            return diff > Numeric<T>.Epsilon;
        }
    }

    public struct UIMaterial
    {
        public Texture2D Texture { get; }
        public Material Material { get; }
        
        public UIMaterial(Texture2D texture, Material material)
        {
            Texture = texture;
            Material = material;
        }
    }
    
    public static class Image
    {
        public static bool DrawTextureWithSlider(Rect rect, UIMaterial material, ref float sliderVal, float min = 0, float max = 1, Color? sliderColor = null)
        {
            var id = GUIUtility.GetControlID(FocusType.Passive, rect);
            
            var previous = sliderVal;
            var mousePos = Event.current.mousePosition;

            var curEv = Event.current.GetTypeForControl(id);
            
            switch (curEv)
            {
                case EventType.Repaint:
                {
                    var sliderPosX = rect.x + rect.width * Mathf.InverseLerp(min, max, sliderVal);
                    var sliderRect = new Rect(sliderPosX-1, rect.y-1, 3, rect.height+2);//new Rect(sliderPosX-2, rect.y - 2, 6, rect.height + 4);
                    var selectionArea = rect.ExpandedBy(2);
                    var isFocusedOrHovered = GUIUtility.hotControl == id | selectionArea.Contains(mousePos);
                    var color = isFocusedOrHovered ? Color.white : Color.grey;
                    Widgets.DrawBoxSolid(rect.ExpandedBy(1), color);
                    Graphics.DrawTexture(rect, material.Texture, 0, 0, 0, 0, material.Material);
                    Widgets.DrawBoxSolid(sliderRect, Color.white);
                    //Widgets.DrawBoxSolidWithOutline(sliderRect, sliderColor ?? Color.white, Color.black);
                    break;
                }
                case EventType.MouseDown:
                {
                    if (rect.Contains (Event.current.mousePosition) && Event.current.button == 0)
                        GUIUtility.hotControl = id;
                    break;
                }
                case EventType.MouseUp:
                {
                    if (GUIUtility.hotControl == id)
                        GUIUtility.hotControl = 0;
                    break;
                }
                case EventType.MouseDrag:
                {
                    if (GUIUtility.hotControl == id)
                    {
                        var newPos = mousePos.x - rect.x;
                        var newSliderVal = Mathf.InverseLerp(0, rect.width, newPos);
                        sliderVal = Mathf.Lerp(min, max, newSliderVal);
                        GUI.changed = true;
                        Event.current.Use();
                    }
                    break;
                }
                
            }

            return Math.Abs(previous - sliderVal) > float.Epsilon;
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