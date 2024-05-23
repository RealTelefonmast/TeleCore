using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class FontDef : Def
{
    public string fontPath = null;
    public int size = 12;
    public FontStyle style = FontStyle.Normal;

    public Font GetFont() => new Font(fontPath);
}
