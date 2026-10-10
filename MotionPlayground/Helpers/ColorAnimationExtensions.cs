using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MotionPlayground.Helpers
{
    public static class ColorAnimationExtensions
    {
        public static void ColorTo(this VisualElement self, Color fromColor, Color toColor, Action<Color> callback, uint length = 250, Easing easing = null)
        {
            var transform = new Animation(v =>
            {
                var r = fromColor.R + (toColor.R - fromColor.R) * v;
                var g = fromColor.G + (toColor.G - fromColor.G) * v;
                var b = fromColor.B + (toColor.B - fromColor.B) * v;
                var a = fromColor.A + (toColor.A - fromColor.A) * v;
                callback(new Color(r, g, b, a));
            });
            self.Animate("ColorTo", transform, 16, length, easing ?? Easing.Linear);
        }
    }
}
