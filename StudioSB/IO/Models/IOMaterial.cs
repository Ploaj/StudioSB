using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace StudioSB.IO.Models
{
    public class IOMaterial
    {
        public string Name { get; set; }

        public string DiffuseTexture { get; set; }
    }

    public class IOMaterialPhong : IOMaterial
    {
        public Color AmbientColor { get; set; } = Color.Black;

        public Color DiffuseColor { get; set; } = Color.White;

        public Color SpecularColor { get; set; } = Color.Black;
    }
}
