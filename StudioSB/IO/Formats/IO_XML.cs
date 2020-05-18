using StudioSB.Scenes;
using System.Xml;

namespace StudioSB.IO.Formats
{
    public class IO_XML : IExportableSkeleton
    {
        public string Name => "Extensible Markup Language";

        public string Extension => ".xml";

        public void ExportSBSkeleton(string FileName, SBSkeleton skeleton)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            using (XmlWriter file = XmlWriter.Create(FileName, settings))
                Write(file, skeleton);
        }

        private void Write(XmlWriter file, SBSkeleton skeleton)
        {
            foreach (var bone in skeleton.Roots)
            {
                WriteBone(file, bone);
            }
        }

        private void WriteBone(XmlWriter file, SBBone bone)
        {
            file.WriteStartElement("SBBone");
            file.WriteAttributeString("Name", bone.Name);
            file.WriteAttributeString("TranslateX", "" + bone.Translation.X);
            file.WriteAttributeString("TranslateY", "" + bone.Translation.Y);
            file.WriteAttributeString("TranslateZ", "" + bone.Translation.Z);
            file.WriteAttributeString("RotationEulerX", "" + bone.RotationEuler.X);
            file.WriteAttributeString("RotationEulerY", "" + bone.RotationEuler.Y);
            file.WriteAttributeString("RotationEulerZ", "" + bone.RotationEuler.Z);

            foreach (var child in bone.Children)
            {
                WriteBone(file, child);
            }

            file.WriteEndElement();
        }
    }
}