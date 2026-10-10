using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MotionPlayground.Models
{
    public class AnimationItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public Color PreviewColor { get; set; }
        public Color StageBackground { get; set; } // фон под анимацию
    }
}
