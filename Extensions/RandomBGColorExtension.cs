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
            Color.SandyBrown,
            Color.SeaGreen,
            Color.Turquoise,
            Color.DarkGray
            };

            return colors[new Random().Next(0, 5)];
        }
    }
}
