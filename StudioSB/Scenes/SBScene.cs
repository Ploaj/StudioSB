using SFGraphics.Cameras;
using StudioSB.GUI;
using StudioSB.GUI.Attachments;
using StudioSB.IO.Models;
using System;
using System.Collections.Generic;

namespace StudioSB.Scenes
{
    /// <summary>
    /// A scene handles rendering and processing of 3d world geometry
    /// </summary>
    public class SBScene
    {
        // Determines whether of not the main form displays the panels for bones and meshes
        public bool HasBones { get; set; } = false;

        public bool HasMesh { get; set; } = false;

        // Extra Nodes can be used for whatever your scene needs
        public bool HasExtraNodes { get; set; } = false;

        // common for all scenes
        public ISBSkeleton Skeleton { get; set; }

        public List<SBSurface> Surfaces { get; } = new List<SBSurface>();

        public List<ISBMaterial> Materials { get; } = new List<ISBMaterial>();

        public List<Type> AttachmentTypes { get; } = new List<Type>() { typeof(SBMeshList), typeof(SBBoneTree), typeof(SBTextureList) };

        /// <summary>
        /// Renders the scene to the current gl context
        /// </summary>
        /// <param name="ModelViewProjetion"></param>
        public void Render(Camera camera)
        {
            if (ApplicationSettings.UseLegacyRendering)
            {
                RenderLegacy();
            }
            else
            {
                RenderShader(camera);
            }
        }

        /// <summary>
        /// Loads the scene from a file
        /// </summary>
        /// <param name="FileName"></param>
        public virtual void LoadFromFile(string FileName)
        {

        }

        /// <summary>
        /// Exports the scene to a file
        /// </summary>
        /// <param name="FileName"></param>
        public virtual void ExportSceneToFile(string FileName)
        {

        }

        /// <summary>
        /// Gets an IO model containing the scene information
        /// </summary>
        /// <returns></returns>
        public virtual IOModel GetIOModel()
        {
            return new IOModel();
        }

        /// <summary>
        /// Imports an IO model into the scene
        /// </summary>
        /// <param name="iomodel"></param>
        public virtual void FromIOModel(IOModel iomodel)
        {

        }

        /// <summary>
        /// Renders the scene using Legacy OpenGL
        /// </summary>
        public virtual void RenderLegacy()
        {
            if (ApplicationSettings.BoneOverlay)
                OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            if (ApplicationSettings.RenderBones && Skeleton != null)
                Skeleton.RenderLegacy();
            if (ApplicationSettings.BoneOverlay)
                OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
        }

        /// <summary>
        /// Renders the scene using Shaders
        /// </summary>
        public virtual void RenderShader(Camera camera)
        {
            if (ApplicationSettings.BoneOverlay)
                OpenTK.Graphics.OpenGL.GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
            if (ApplicationSettings.RenderBones && Skeleton != null)
                Skeleton.RenderShader(camera);
            if (ApplicationSettings.BoneOverlay)
                OpenTK.Graphics.OpenGL.GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.DepthTest);
        }

        /// <summary>
        /// Gets the mesh objects inside of the scene
        /// </summary>
        /// <returns></returns>
        public virtual ISBMesh[] GetMeshObjects()
        {
            return null;
        }

        /// <summary>
        /// Gets the materials inside of the scene
        /// </summary>
        /// <returns></returns>
        public virtual ISBMaterial[] GetMaterials()
        {
            return null;
        }
    }
}
