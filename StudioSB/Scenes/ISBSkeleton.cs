using OpenTK;
using SFGraphics.Cameras;

namespace StudioSB.Scenes
{
    /// <summary>
    /// Interface for skeleton data inside a scene
    /// </summary>
    public interface ISBSkeleton
    {
        /// <summary>
        /// Returns bones in an array
        /// </summary>
        SBBone[] Bones
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        SBBone this[string i] { get; }

        /// <summary>
        /// Returns true if a bone with the given name exists
        /// </summary>
        /// <returns></returns>
        bool ContainsBone(string s);

        /// <summary>
        /// Gets the index of the given bone
        /// </summary>
        /// <param name="bone"></param>
        /// <returns>the index of the given bone and -1 if it doesn't exist</returns>
        int IndexOfBone(SBBone bone);

        /// <summary>
        /// Gets the inverse world transform * animation world transform of all bones
        /// </summary>
        /// <returns></returns>
        Matrix4[] GetBindTransforms();

        /// <summary>
        /// Gets the world transform of all bones
        /// </summary>
        /// <returns></returns>
        Matrix4[] GetWorldTransforms();

        /// <summary>
        /// Renders the skeleton using Legacy OpenGL on the current thread
        /// Warning: slow
        /// </summary>
        void RenderLegacy();

        /// <summary>
        /// Resets the animated positions of the bones to the defaults
        /// </summary>
        void Reset();

        /// <summary>
        /// Renders the skeleton using Shaders on the current thread
        /// </summary>
        void RenderShader(Camera camera);
    }
}
