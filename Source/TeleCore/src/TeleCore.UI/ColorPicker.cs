using TeleCore.AssetLoader;
using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class ColorPicker
{
    private Material colorPickerMat;
    private Material hueMat;
    private Material satMat;
    private Material valMat;
    
    private Material redMat;
    private Material greenMat;
    private Material blueMat;
    
    //Current Color Working Values
    private Color _color;
    
    private int[] hsvVals = new int[3] ;
    private int[] colorVals = new int[3];

    public ColorPicker()
    {
        colorPickerMat = new Material(AssetBundleDB.LoadShader("ColorPickerGradient"));
        hueMat = new Material(AssetBundleDB.LoadShader("ColorPickerHue"));
        satMat = new Material(AssetBundleDB.LoadShader("ColorPickerHue"));
        valMat = new Material(AssetBundleDB.LoadShader("ColorPickerHue"));
        
        redMat = new Material(AssetBundleDB.LoadShader("ColorPickerRGB"));
        greenMat = new Material(AssetBundleDB.LoadShader("ColorPickerRGB"));
        blueMat = new Material(AssetBundleDB.LoadShader("ColorPickerRGB"));
        
        hueMat.SetInt("_Invert", 1);
        satMat.SetInt("_Invert", 1);
        valMat.SetInt("_Invert", 1);
        satMat.SetInt("_Mode", 1);
        valMat.SetInt("_Mode", 2);
        
        greenMat.SetInt("_Result", 1);
        blueMat.SetInt("_Result", 2);
    }
    
    public void Draw(Rect inRect)
    {
        //Colors
        var color = _color;
        var colorInt = new ColorInt(color);
        Color.RGBToHSV(color, out var h, out var s, out var v);
        
        colorVals[0] = colorInt.r;
        colorVals[1] = colorInt.g;
        colorVals[2] = colorInt.b;
        
        hsvVals[0] = (int) (h * 360);
        hsvVals[1] = (int) (s * 100);
        hsvVals[2] = (int) (v * 100);
        
        var colorSat = Color.HSVToRGB(h, 1, 1, true);
        
        //Rect stuff
        var parts = (inRect.height / 8) /2;
        var colorSize = parts * 8 + (5 + 10 + (5*7));
        var leftSide = inRect.LeftPartPixels(colorSize);
        var rightSide = new Rect(leftSide.xMax + 5, leftSide.y, inRect.width / 2, inRect.height);
        var colorPicker = leftSide.TopPartPixels(colorSize);

        //RectAggregator rectA = new RectAggregator(new Rect(rightSide.position, new Vector2(rightSide.width, 0)), this.GetHashCode());
        var div = new RectDivider(rightSide, this.GetHashCode(), new Vector2(5, 5));
        var HSV = div.NewRow(parts);
        var HSV_H = div.NewRow(parts);
        var HSV_S = div.NewRow(parts);
        var HSV_V = div.NewRow(parts, marginOverride:10);
        var textSize = Text.CalcSize("1000").x;
        
        var HSV_H_Label = HSV_H.NewCol(textSize/2);
        var HSV_S_Label = HSV_S.NewCol(textSize/2);
        var HSV_V_Label = HSV_V.NewCol(textSize/2);
        
        var HSV_H_Input = HSV_H.NewCol(textSize, HorizontalJustification.Right, 5);
        var HSV_S_Input = HSV_S.NewCol(textSize, HorizontalJustification.Right, 5);
        var HSV_V_Input = HSV_V.NewCol(textSize, HorizontalJustification.Right, 5);
        
        var RGB = div.NewRow(parts);
        var RGB_R = div.NewRow(parts);
        var RGB_G = div.NewRow(parts);
        var RGB_B = div.NewRow(parts);
        
        var RGB_R_Label = RGB_R.NewCol(textSize/2);
        var RGB_G_Label = RGB_G.NewCol(textSize/2);
        var RGB_B_Label = RGB_B.NewCol(textSize/2);
        
        var RGB_R_Input = RGB_R.NewCol(textSize, HorizontalJustification.Right, 5);
        var RGB_G_Input = RGB_G.NewCol(textSize, HorizontalJustification.Right, 5);
        var RGB_B_Input = RGB_B.NewCol(textSize, HorizontalJustification.Right, 5);
        

        Vector2 uvPos = new Vector2(s, 1-v);
        
        colorPickerMat.SetFloat("_Hue", h);
        redMat.SetColor("_Color", color);
        greenMat.SetColor("_Color", color);
        blueMat.SetColor("_Color", color);
        
        satMat.SetColor("_Color", colorSat);
        valMat.SetColor("_Color", colorSat);
        
        //Color Picker
        Graphics.DrawTexture(colorPicker, BaseContent.BadTex, 0, 0, 0, 0, colorPickerMat);
        
        var posRect = new Rect(colorPicker.position + (uvPos * colorPicker.size), new Vector2(7, 7));
        Widgets.DrawBoxSolidWithOutline(posRect, color, Color.black);
        
        //RGB Group
        //Widgets.Label(RGB, "RGB");
        Widgets.Label(RGB, "RGB");
        
        Widgets.Label(RGB_R_Label, "R:");
        Widgets.Label(RGB_G_Label, "G:");
        Widgets.Label(RGB_B_Label, "B:");
        
        Graphics.DrawTexture(RGB_R, BaseContent.BadTex, 0, 0, 0, 0, redMat);
        Graphics.DrawTexture(RGB_G, BaseContent.BadTex, 0, 0, 0, 0, greenMat);
        Graphics.DrawTexture(RGB_B, BaseContent.BadTex, 0, 0, 0, 0, blueMat);
        
        var red = colorInt.r;
        var green = colorInt.g;
        var blue = colorInt.b;
        TWidgets.Text.TextFieldNumeric(RGB_R_Input, ref red, 0, 255);
        TWidgets.Text.TextFieldNumeric(RGB_G_Input, ref green, 0, 255);
        TWidgets.Text.TextFieldNumeric(RGB_B_Input, ref blue, 0, 255);
        
        //TODO: Adjust HSV data based on RGB changes
        
        //HSV Group
        Widgets.Label(HSV, "HSV");
        var size = Text.CalcSize("HSV");
        //Widgets.DrawLineHorizontal(HSV.Rect.x + size.x, HSV.Rect.y + HSV.Rect.height/2, HSV.Rect.width - size.x);
        
        Widgets.Label(HSV_H_Label, "H:");
        Widgets.Label(HSV_S_Label, "S:");
        Widgets.Label(HSV_V_Label, "V:");
        
        Graphics.DrawTexture(HSV_H, BaseContent.BadTex, 0, 0, 0, 0, hueMat);
        Graphics.DrawTexture(HSV_S, BaseContent.BadTex, 0, 0, 0, 0, satMat);
        Graphics.DrawTexture(HSV_V, BaseContent.BadTex, 0, 0, 0, 0, valMat);

        TWidgets.Text.TextFieldNumeric(HSV_H_Input, ref h, 0, 360);
        TWidgets.Text.TextFieldNumeric(HSV_S_Input, ref s, 0, 100);
        TWidgets.Text.TextFieldNumeric(HSV_V_Input, ref v, 0, 100);
        
        //TODO: Adjust color data based on HSV changes
    }
}