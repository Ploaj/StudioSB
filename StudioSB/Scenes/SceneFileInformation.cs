using System;

namespace StudioSB.Scenes
{
    public class SceneFileInformation : Attribute
    {
        public string Extension { get; set; }
        public string Description { get; set; }
        public string SceneTypeName { get; set; }
        public string SceneCode { get; set; }

        public SceneFileInformation(string sceneTypeName, string extension, string description, string sceneCode = "")
        {
            Extension = extension;
            Description = description;
            SceneTypeName = sceneTypeName;
            SceneCode = sceneCode;
        }
    }
}
