using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Extensions
{
    [ContentProperty("BackgroundColor")]
    public class RandomBGColorExtension : IMarkupExtension
    {
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var colors = new List<Color>
            {
            Color.SkyBlue,
            Color.Salmon,
            Color.Orange,
            Color.Gray,
            Color.SeaGreen,
            Color.Turquoise,
            Color.DarkGray,
            Color.Green,
            Color.Blue,
            Color.Indigo,
            Color.Violet,
            Color.BurlyWood
            };

            return colors[new Random().Next(0, 11)];
        }
    }
}
