using StudioSB.Scenes;
using System.Xml;
using System.Collections.Generic;
using OpenTK;

namespace StudioSB.IO.Formats
{
    public class IO_XML : IExportableSkeleton, IImportableSkeleton
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

        public void ImportSBSkeleton(string FileName, SBSkeleton skeleton)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (XmlReader reader = XmlReader.Create(FileName, settings))
                Read(reader, skeleton);
        }

        private void Read(XmlReader reader, SBSkeleton skeleton)
        {
            List<SBBone> roots = new List<SBBone>();
            foreach (var bone in skeleton.Roots)
            {
                SBBone newBone = ReadBone(reader, bone);
                if (newBone == null)
                {
                    SBConsole.WriteLine("Failed to read XML skeleton.");
                    return;
                }
                roots.Add(newBone);
            }

            UpdateSkeleton(skeleton, roots);
        }

        private void UpdateSkeleton(SBSkeleton skeleton, List<SBBone> roots)
        {
            foreach (var root in roots)
            {
                UpdateBone(skeleton, root);
            }
        }

        private void UpdateBone(SBSkeleton skeleton, SBBone bone)
        {
            skeleton[bone.Name].X = bone.X;
            skeleton[bone.Name].Y = bone.Y;
            skeleton[bone.Name].Z = bone.Z;
            skeleton[bone.Name].RX = bone.RX;
            skeleton[bone.Name].RY = bone.RY;
            skeleton[bone.Name].RZ = bone.RZ;

            foreach (var child in bone.Children)
            {
                UpdateBone(skeleton, child);
            }
        }

        private bool WriteValue(SBBone bone, string attribute, float value)
        {
            if (attribute.Equals("TranslateX"))
            {
                bone.X = value;
            }
            else if (attribute.Equals("TranslateY"))
            {
                bone.Y = value;
            }
            else if (attribute.Equals("TranslateZ"))
            {
                bone.Z = value;
            }
            else if (attribute.Equals("RotationEulerX"))
            {
                bone.RX = value;
            }
            else if (attribute.Equals("RotationEulerY"))
            {
                bone.RY = value;
            }
            else if (attribute.Equals("RotationEulerZ"))
            {
                bone.RZ = value;
            }
            else
            {
                SBConsole.WriteLine("Internal failure");
                return false;
            }

            return true;
        }

        private bool HandleAttribute(XmlReader reader, SBBone bone, string attribute)
        {
            string value = reader.GetAttribute(attribute);
            float val;

            if (value == null)
            {
                SBConsole.WriteLine("Expected attribute \"" + attribute + "\"");
                return false;
            }
            else if (!float.TryParse(value, out val))
            {
                SBConsole.WriteLine("Failed to parse attribute \"" + attribute + "\"");
                return false;
            }

            if (!WriteValue(bone, attribute, val))
            {
                return false;
            }

            return true;
        }

        private SBBone ReadBone(XmlReader reader, SBBone bone)
        {
            SBBone newBone = new SBBone();
            newBone.Transform = Matrix4.Identity;
            if (!reader.ReadToFollowing("SBBone") || reader.NodeType != XmlNodeType.Element || reader.Name != "SBBone")
            {
                SBConsole.WriteLine("Expected bone element");
                return null;
            }

            string name = reader.GetAttribute("Name");
            if (name != bone.Name)
            {
                SBConsole.WriteLine("Expected bone definition for " + bone.Name +
                                    ", received \"" + name + "\"");
                return null;
            }

            newBone.Name = bone.Name;
            if (!HandleAttribute(reader, newBone, "TranslateX") ||
                !HandleAttribute(reader, newBone, "TranslateY") ||
                !HandleAttribute(reader, newBone, "TranslateZ") ||
                !HandleAttribute(reader, newBone, "RotationEulerX") ||
                !HandleAttribute(reader, newBone, "RotationEulerY") ||
                !HandleAttribute(reader, newBone, "RotationEulerZ"))
            {
                return null;
            }

            foreach (var child in bone.Children)
            {
                SBBone newChild = ReadBone(reader, child);
                if (newChild == null)
                {
                    return null;
                }
                newBone.AddChild(newChild);
            }

            return newBone;
        }
    }
}