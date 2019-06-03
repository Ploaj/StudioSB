using StudioSB.Rendering.Bounding;
using StudioSB.Scenes.LVD;
using System;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using StudioSB.GUI.Menus;
using StudioSB.Tools;
using System.Drawing;
using OpenTK.Input;

namespace StudioSB.GUI.Attachments
{
    public class LVDAttachment : GroupBox, IAttachment
    {
        private bool IsActive
        {
            get
            {
                if (Parent != null && Parent.Parent is SBTabPanel tabPanel)
                {
                    if (tabPanel.SelectedTab == Parent)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private SBToolStrip Tools;
        private SBTreeView NodeTree;
        private PropertyGrid PropertyGrid;

        private float PlatformWidth = 8f;

        private LevelData LVD;

        private bool CameraLocked = false;

        public LVDAttachment()
        {
            Text = "LVD Editor";
            Dock = DockStyle.Fill;

            ApplicationSettings.SkinControl(this);

            Tools = new SBToolStrip();
            Tools.Dock = DockStyle.Top;

            ToolStripButton lockCamera = new ToolStripButton();
            lockCamera.Click += (object sender, EventArgs args) => {
                CameraLocked = !CameraLocked;
            };
            Tools.Items.Add(lockCamera);

            NodeTree = new SBTreeView();
            NodeTree.Dock = DockStyle.Top;
            NodeTree.AfterSelect += SelectNode;

            PropertyGrid = new PropertyGrid();
            PropertyGrid.Dock = DockStyle.Top;
            PropertyGrid.Size = new Size(200, 500);

            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(PropertyGrid);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(NodeTree);
            Controls.Add(Tools);
        }

        public void Open(string FileName)
        {
            LevelData lvd = new LevelData();
            lvd.Open(FileName);

            var lvdNode = new TreeNode() { Text = "LVD", Tag = lvd };
            NodeTree.Nodes.Clear();
            NodeTree.Nodes.Add(lvdNode);

            foreach(var col in lvd.Collisions)
            {
                var colNode = new TreeNode();
                colNode.Text = col.EntryName;
                colNode.Tag = col;
                lvdNode.Nodes.Add(colNode);
            }

            LVD = lvd;
        }

        private void SelectNode(object sender, EventArgs args)
        {
            if(NodeTree.SelectedNode != null)
            {
                if(NodeTree.SelectedNode.Tag != null)
                {
                    PropertyGrid.SelectedObject = NodeTree.SelectedNode.Tag;
                }
            }
        }

        public string Extension()
        {
            return ".lvd";
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public void AttachToPanel(SBViewportPanel viewportPanel)
        {
            viewportPanel.TabPanel.AddTab("Level Data", this);
        }

        private Vector3 Picked = Vector3.Zero;

        public void Pick(Ray ray)
        {
            if (!IsActive)
                return;

            PropertyGrid.SelectedObject = null;

            var depthPicked = ray.GetPlaneIntersection(-Vector3.UnitZ, new Vector3(0, 0, -PlatformWidth / 2));
            Picked = ray.GetPlaneIntersection(-Vector3.UnitZ, Vector3.Zero);

            Vector2 nearestLine;
            float closest = float.MaxValue;
            LVDCollisionMaterial collisionMat = null;
            Vector3 sphereHit;

            foreach(var spawn in LVD.Spawns)
            {
                if(CrossMath.FastDistance(Picked, new Vector3(spawn.X, spawn.Y, 0), 5))
                {
                    PropertyGrid.SelectedObject = spawn;
                    return;
                }
            }
            foreach (var spawn in LVD.Respawns)
            {
                if (CrossMath.FastDistance(Picked, new Vector3(spawn.X, spawn.Y, 0), 5))
                {
                    PropertyGrid.SelectedObject = spawn;
                    return;
                }
            }
            foreach (var col in LVD.Collisions)
            {
                for(int i =0; i < col.Vertices.Count; i++)
                {
                    var vert = col.Vertices[i];
                    if (i < col.Materials.Count)
                    {
                        var vert2 = col.Vertices[i+1];
                        var dis = ray.GetDistanceToSegment(depthPicked.Xy, new Vector2(vert.X, vert.Y), new Vector2(vert2.X, vert2.Y), out nearestLine);
                        if (dis < PlatformWidth / 4 & dis < closest)
                        {
                            closest = dis;
                            collisionMat = col.Materials[i];
                        }
                    }
                    if (CrossMath.FastDistance(Picked, new Vector3(vert.X, vert.Y, 0), 1))
                    {
                        PropertyGrid.SelectedObject = vert;
                        return;
                    }
                }
            }

            if (PropertyGrid.SelectedObject == null && collisionMat != null)
                PropertyGrid.SelectedObject = collisionMat;
        }

        public void RemoveFromPanel(SBViewportPanel viewportPanel)
        {
        }


        bool DeleteDown = false;
        bool ADown = false;

        public void Step(SBViewport viewport)
        {
            var mouseP = viewport.GetMousePosition();
            var deltaMouse = PrevMousePosition - mouseP;
            PrevMousePosition = mouseP;
            if (!IsActive)
                return;
            if (PropertyGrid.SelectedObject is LVDVector2 v)
            {
                if (Keyboard.GetState().IsKeyDown(Key.AltLeft))
                {
                    if (Keyboard.GetState().IsKeyDown(Key.A))
                    {
                        if (!ADown)
                        {
                            ADown = true;
                            AddNewPoint(v);
                        }
                    }
                    else
                        ADown = false;
                    if (Mouse.GetState().IsButtonDown(MouseButton.Left))
                    {
                        v.X -= deltaMouse.X / 8;
                        v.Y += deltaMouse.Y / 8;

                        // recalculate normals
                        // is there a better way to do this?
                        foreach(var col in LVD.Collisions)
                        {
                            int index = col.Vertices.IndexOf(v);
                            if (index == -1) continue;
                            
                            if (index < col.Normals.Count)
                                col.Normals[index] = LVDVector2.GenerateNormal(v, col.Vertices[index + 1]);
                            if (index > 0)
                                col.Normals[index - 1] = LVDVector2.GenerateNormal(col.Vertices[index - 1], v);
                            break;
                        }
                        PropertyGrid.SelectedObject = PropertyGrid.SelectedObject;
                    }
                }
                if (Keyboard.GetState().IsKeyDown(Key.Delete))
                {
                    if (!DeleteDown)
                    {
                        DeleteDown = true;
                        DeleteVertex(v);
                    }
                }
                else
                    DeleteDown = false;
            }
        }

        private Vector2 PrevMousePosition;

        public void Update(SBViewport viewport)
        {
        }

        /// <summary>
        /// adds new point after vector
        /// </summary>
        /// <param name="v"></param>
        private void AddNewPoint(LVDVector2 v)
        {
            foreach (var col in LVD.Collisions)
            {
                int index = col.Vertices.IndexOf(v);
                if (index == -1) continue;

                if (index == col.Vertices.Count - 1)
                {
                    var newPoint = new LVDVector2(v.X + 3, v.Y);
                    var newMaterial = new LVDCollisionMaterial();
                    newMaterial.Physics = col.Materials[index - 1].Physics;
                    col.Vertices.Add(newPoint);
                    col.Normals.Add(LVDVector2.GenerateNormal(v, newPoint));
                    col.Materials.Add(newMaterial);
                    PropertyGrid.SelectedObject = newPoint;
                }
                else
                {
                    var newPoint = new LVDVector2((v.X + col.Vertices[index + 1].X) / 2, (v.Y + col.Vertices[index + 1].Y) / 2);
                    var newMaterial = new LVDCollisionMaterial();
                    newMaterial.Physics = col.Materials[index].Physics;
                    var newNormal = LVDVector2.GenerateNormal(newPoint, col.Vertices[index + 1]);
                    col.Normals.Insert(index + 1, newNormal);
                    col.Materials.Insert(index + 1, newMaterial);
                    col.Vertices.Insert(index + 1, newPoint);
                    PropertyGrid.SelectedObject = newPoint;
                }

                break;
            }
        }

        /// <summary>
        /// deletes lvdvector2
        /// </summary>
        /// <param name="v"></param>
        private void DeleteVertex(LVDVector2 v)
        {
            foreach (var col in LVD.Collisions)
            {
                int index = col.Vertices.IndexOf(v);
                if (index == -1) continue;

                if (index >= 1)
                    PropertyGrid.SelectedObject = col.Vertices[index - 1];

                if(col.Normals.Count > 0)
                {
                    if (index == col.Normals.Count)
                    {
                        col.Normals.RemoveAt(index - 1);
                        col.Materials.RemoveAt(index - 1);
                    }
                    else
                    {
                        col.Normals.RemoveAt(index);
                        col.Materials.RemoveAt(index);
                    }
                }
                
                col.Vertices.RemoveAt(index);

                if (index == col.Vertices.Count)
                    index--;
                
                if (col.Normals.Count > 0 && index > 0)
                {
                    col.Normals[index - 1] = LVDVector2.GenerateNormal(col.Vertices[index - 1], col.Vertices[index]);
                }
            }
        }

        public void Save(string FilePath)
        {

        }

        #region Rendering

        private int FlashTimer = 0;
        private int FlashInterval = 15;
        private static Vector3 FlashColor1 = new Vector3(1f, 1f, 1f);
        private static Vector3 FlashColor2 = new Vector3(1f, 1f, 0f);
        private Vector3 FlashColor = FlashColor1;

        public void Render(SBViewport viewport, float frame = 0)
        {
            //if (!IsActive)
            //    return;
            FlashTimer++;
            if (FlashTimer > FlashInterval)
            {
                FlashTimer = 0;
                if (FlashColor == FlashColor1)
                {
                    FlashColor = FlashColor2;
                }
                else
                    FlashColor = FlashColor1;
            }

            // TODO: draw with shader
            if (LVD != null)
            {
                GL.PushAttrib(AttribMask.AllAttribBits);

                GL.Disable(EnableCap.CullFace);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                GL.UseProgram(0);

                GL.Color3(1f, 1f, 0);
                GL.LineWidth(2f);

                GL.PointSize(5f);
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(Picked.X, Picked.Y, Picked.Z);
                GL.End();

                RenderCollisions();

                foreach(var blast in LVD.BlastZoneBounds)
                    RenderBounds(blast, Color.LightPink);

                foreach (var camera in LVD.CameraBounds)
                    RenderBounds(camera, Color.SkyBlue);

                int playerIndex = 1;
                foreach (var spawn in LVD.Spawns)
                {
                    Rendering.TextRenderer.Draw(viewport.Camera, "P" + playerIndex++, Matrix4.CreateTranslation(new Vector3(spawn.X, spawn.Y, 0)));
                    //Rendering.Shapes.Spawn.RenderSpawn(spawn.X, spawn.Y, 5);
                }

                foreach (var spawn in LVD.Respawns)
                {
                    Rendering.Shapes.Spawn.RenderSpawn(spawn.X, spawn.Y, 5);
                }

                GL.PopAttrib();
            }
        }

        private void RenderCollisions()
        {
            foreach (var col in LVD.Collisions)
            {
                for (int i = 0; i < col.Materials.Count; i++)
                {
                    if(PropertyGrid.SelectedObject == col.Vertices[i])
                    {
                        GL.Color3(1f, 1f, 0);
                        GL.PointSize(12f);
                        GL.Begin(PrimitiveType.Points);
                        GL.Vertex3(col.Vertices[i].X, col.Vertices[i].Y, 0);
                        GL.End();
                    }
                    RenderWall(col, col.Vertices[i], col.Vertices[i+1], col.Materials[i], new Vector2(col.Normals[i].X, col.Normals[i].Y));
                }
            }
        }

        private void RenderWall(LVDCollision col, LVDVector2 p1, LVDVector2 p2, LVDCollisionMaterial mat, Vector2 normal)
        {
            Vector2 v1 = new Vector2(p1.X, p1.Y);
            Vector2 v2 = new Vector2(p2.X, p2.Y);
            Vector2 mid = (v1 + v2) / 2;
            Vector2 nrm = mid + normal * 3;

            // material
            var materialColor = GetMatlColor(mat);
            GL.Color4(materialColor.R / 255f, materialColor.G / 255f, materialColor.B / 255f, 0.75f);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(p1.X, p1.Y, 0);
            GL.Vertex3(p1.X, p1.Y, -PlatformWidth);
            GL.Vertex3(p2.X, p2.Y, -PlatformWidth);
            GL.Vertex3(p2.X, p2.Y, 0);
            GL.End();

            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            
            // point line 1
            GL.Color3(GetElementColor(p1));
            GL.Vertex3(v1.X, v1.Y, 0);
            GL.Color3(GetElementColor(p1));
            GL.Vertex3(v1.X, v1.Y, -PlatformWidth);

            // point line 2
            GL.Color3(GetElementColor(p2));
            GL.Vertex3(v2.X, v2.Y, 0);
            GL.Color3(GetElementColor(p2));
            GL.Vertex3(v2.X, v2.Y, -PlatformWidth);

            // front line
            GL.Color3(GetElementColor(p1));
            GL.Vertex3(v1.X, v1.Y, 0);
            GL.Color3(GetElementColor(p2));
            GL.Vertex3(v2.X, v2.Y, 0);

            // back line
            GL.Color3(GetElementColor(p1));
            GL.Vertex3(v1.X, v1.Y, -PlatformWidth);
            GL.Color3(GetElementColor(p2));
            GL.Vertex3(v2.X, v2.Y, -PlatformWidth);
            
            // normal
            GL.Color3(GetNormalColor(col, normal, mat));
            GL.Vertex3(mid.X, mid.Y, -PlatformWidth/2);
            GL.Color3(GetNormalColor(col, normal, mat));
            GL.Vertex3(nrm.X, nrm.Y, -PlatformWidth/2);

            GL.End();
        }


        private static void RenderBounds(LVDBounds b, Color color)
        {
            Vector3 sPos = b.UseStartPosition ? new Vector3(b.StartPosition.X, b.StartPosition.Y, b.StartPosition.Z) : new Vector3(0, 0, 0);

            GL.Color4(Color.FromArgb(128, color));

            GL.LineWidth(2);

            GL.Begin(PrimitiveType.LineLoop);

            GL.Vertex3(b.Left + sPos.X, b.Top + sPos.Y, sPos.Z);
            GL.Vertex3(b.Right + sPos.X, b.Top + sPos.Y, sPos.Z);
            GL.Vertex3(b.Right + sPos.X, b.Bottom + sPos.Y, sPos.Z);
            GL.Vertex3(b.Left + sPos.X, b.Bottom + sPos.Y, sPos.Z);

            GL.End();
        }

        private void RenderSpawn()
        {

        }

        private Color GetNormalColor(LVDCollision c, Vector2 normals, LVDCollisionMaterial material)
        {
            float angle = (float)(Math.Atan2(normals.Y, normals.X) * 180 / Math.PI);

            if (c.Flag4)
                return Color.FromArgb(128, Color.Yellow);
            else if (material.GetFlag(4) && ((angle <= 0 && angle >= -70) || (angle <= -110 && angle >= -180) || angle == 180))
                return Color.FromArgb(128, Color.Purple);
            else if ((angle <= 0 && angle >= -70) || (angle <= -110 && angle >= -180) || angle == 180)
                return Color.FromArgb(128, Color.Lime);
            else if (normals.Y < 0)
                return Color.FromArgb(128, Color.Red);
            else
                return Color.FromArgb(128, Color.Cyan);
        }

        private Color GetMatlColor(LVDCollisionMaterial mat)
        {
            if (PropertyGrid.SelectedObject == mat)
                return Color.FromArgb(255, (int)(FlashColor.X * 255), (int)(FlashColor.Y * 255), (int)(FlashColor.Z * 255));
            switch (mat.Physics)
            {
                case CollisionMatType.Basic:
                    return Color.AliceBlue;
                case CollisionMatType.Brick:
                    return Color.SaddleBrown;
                case CollisionMatType.Cloud:
                    return Color.FromArgb(0xFF, 0xF6, 0x9A, 0xB0);
                case CollisionMatType.Alien:
                    return Color.DarkGreen;
                case CollisionMatType.Cardboard:
                    return Color.SandyBrown;
                    case CollisionMatType.Carpet:
                    return Color.PaleVioletRed;
                case CollisionMatType.Electroplankton:
                    return Color.DarkSeaGreen;
                case CollisionMatType.GameWatch:
                    return Color.LightGray;
                case CollisionMatType.Grass:
                    return Color.ForestGreen;
                case CollisionMatType.Grate:
                    return Color.Gray;
                case CollisionMatType.Hazard2SSEOnly:
                    return Color.Gray;
                case CollisionMatType.Hazard3SSEOnly:
                    return Color.Gray;
                case CollisionMatType.HeavyMetal:
                    return Color.LightGray;
                case CollisionMatType.Homerun:
                    return Color.LawnGreen;
                case CollisionMatType.Hurt:
                    return Color.IndianRed;
                case CollisionMatType.Ice:
                    return Color.CornflowerBlue;
                case CollisionMatType.LightMetal:
                    return Color.LightGray;
                case CollisionMatType.MasterFortress:
                    return Color.DarkSlateGray;
                case CollisionMatType.NES8Bit:
                    return Color.Gray;
                case CollisionMatType.Oil:
                    return Color.DarkSlateGray;
                case CollisionMatType.Rock:
                    return Color.RosyBrown;
                case CollisionMatType.Sand:
                    return Color.SandyBrown;
                case CollisionMatType.Snow:
                    return Color.LightBlue;
                case CollisionMatType.Soft:
                    return Color.LightPink;
                case CollisionMatType.Soil:
                    return Color.Brown;
                case CollisionMatType.SpikesTargetTest:
                    return Color.IndianRed;
                case CollisionMatType.Wood:
                    return Color.Brown;
                default:
                    return Color.Black;
            }
        }

        private Vector3 GetElementColor(object o)
        {
            if (PropertyGrid.SelectedObject == o)
                return (FlashColor);
            else
                return new Vector3(0f, 0f, 0f);
        }

#endregion
    }
}
