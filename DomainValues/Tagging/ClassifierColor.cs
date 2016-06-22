using System;
using System.Collections.Generic;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;

namespace DomainValues.Tagging
{
    internal static class ClassifierColor
    {
        internal static Color GetColor(string name)
        {
            var bg = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);

            // If RGB are all less than 50 then assume the theme is Dark
            return bg.R < 50 && bg.G < 50 && bg.B < 50
                ? ClassifierColors[name].Item1
                : ClassifierColors[name].Item2;
        }
        //todo - pick better colours
        internal static Dictionary<string, Tuple<Color, Color>> ClassifierColors = new Dictionary<string, Tuple<Color,Color>>
        {
            {DvContent.DvKeyword,Tuple.Create(Color.FromArgb(255,86,156,214),Colors.Blue) },
            {DvContent.DvComment,Tuple.Create(Color.FromArgb(255,87,166,74),Colors.Green) },
            {DvContent.DvVariable,Tuple.Create(Colors.YellowGreen,Colors.Olive) },
            {DvContent.DvText,Tuple.Create(Colors.MintCream,Colors.Black) },
            {DvContent.DvHeaderRow,Tuple.Create(Colors.MediumPurple,Colors.Crimson) }
        };
    }
}
