using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Extensions
{
    [ContentProperty("BackgroundColor")]
    public class RandomBGColorExtension : IMarkupExtension
    {
        static int random;

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var colors = new List<Color>
            {
                Color.Bisque,
                Color.Brown,
                Color.BurlyWood,
                Color.Chocolate,
                Color.DarkKhaki,
                Color.DarkOliveGreen,
                Color.SkyBlue,
                Color.Salmon,
                Color.Gray,
                Color.SeaGreen,
                Color.Turquoise,
                Color.Green
            };

            int randomNum;

            do
            {
                randomNum = new Random().Next(0, 11);
            }
            while (randomNum == random);

            random = randomNum;

            return colors[randomNum];
        }
    }
}
