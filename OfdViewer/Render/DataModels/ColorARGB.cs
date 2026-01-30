using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace OFDViewer.Render.DataModels
{
    public struct ColorARGB
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public static implicit operator SKColor(ColorARGB color)
            => new SKColor(color.R, color.G, color.B, color.A);
    }
}
