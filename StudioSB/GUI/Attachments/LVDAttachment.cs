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
using StudioSB.IO.Formats;

namespace StudioSB.GUI.Attachments
{
    /// <summary>
    /// Attachment for editing LVD files
    /// </summary>
    public class LVDAttachment : GroupBox, IAttachment
    {
        /// <summary>
        /// Returns true if this panel the current attachment
        /// </summary>
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

        // UI
        private GroupBox ToolPanel;
        private SBToolStrip Tools;
        private SBToolStrip PointToolStrip;
        private SBTreeView NodeTree;
        private PropertyGrid PropertyGrid;
        private SBButton ExportLVD;

        // LVD
        private LevelData LVD;

        // Options
        private float CollisionDepth = 8.0f;
        private static float PickRange = 2.5f;

        public LVDAttachment()
        {
            Text = "LVD Editor";
            Dock = DockStyle.Fill;

            ApplicationSettings.SkinControl(this);

            ToolPanel = new GroupBox();
            ToolPanel.Text = "Options";
            ToolPanel.ForeColor = ApplicationSettings.ForegroundColor;
            ToolPanel.Dock = DockStyle.Top;
            ToolPanel.Height = 40;

            Tools = new SBToolStrip();
            Tools.Dock = DockStyle.Top;

            ExportLVD = new SBButton("Export LVD");
            ExportLVD.Click += (sender, args) =>
            {
                string fileName;
                if (FileTools.TrySaveFile(out fileName, "Smash Level Data |*.lvd;*.ssf"))
                {
                    if (fileName.EndsWith(".ssf"))
                    {
                        IO_SSF.Export(LVD, fileName);
                    }
                    else
                        LVD.Save(fileName);
                }
            };
            
            PointToolStrip = new SBToolStrip();

            ToolStripButton addVertex = new ToolStripButton();
            addVertex.Text = "Add";
            addVertex.Click += (object sender, EventArgs args) => {
                if (PropertyGrid.SelectedObject is LVDVector2 point)
                    AddVertex(point);
            };
            ToolStripButton deleteVertex = new ToolStripButton();
            deleteVertex.Text = "Delete";
            deleteVertex.Click += (object sender, EventArgs args) => {
                if (PropertyGrid.SelectedObject is LVDVector2 point)
                    DeleteVertex(point);
            };
            PointToolStrip.Items.Add(addVertex);
            PointToolStrip.Items.Add(deleteVertex);

            NodeTree = new SBTreeView();
            NodeTree.Dock = DockStyle.Top;
            NodeTree.AfterSelect += SelectNode;

            PropertyGrid = new PropertyGrid();
            PropertyGrid.Dock = DockStyle.Top;
            PropertyGrid.Size = new Size(200, 500);
            PropertyGrid.PropertySort = PropertySort.Categorized;
            PropertyGrid.SelectedObjectsChanged += SelectObjectChanged;

            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(PropertyGrid);
            Controls.Add(new Splitter() { Dock = DockStyle.Top, Height = 10 });
            Controls.Add(NodeTree);
            Controls.Add(ToolPanel);
        }

        /// <summary>
        /// Reads LVD file data from the given file name.
        /// </summary>
        /// <param name="FileName"></param>
        public void Open(string FileName)
        {
            LevelData lvd = new LevelData(FileName);

            LVD = lvd;
            RefreshNodes();
        }

        /// <summary>
        /// Refreshes the LVD object node list.
        /// </summary>
        private void RefreshNodes()
        {
            var levelDataNode = new TreeNode() { Text = "LVD", Tag = LVD };

            NodeTree.Nodes.Clear();
            NodeTree.Nodes.Add(levelDataNode);

            var collisionNodes = new TreeNode() { Text = "Collisions" };
            var startPositionNodes = new TreeNode() { Text = "Start Positions" };
            var restartPositionNodes = new TreeNode() { Text = "Restart Positions" };
            var cameraRegionNodes = new TreeNode() { Text = "Camera Regions" };
            var deathRegionNodes = new TreeNode() { Text = "Death Regions" };
            var enemyGeneratorNodes = new TreeNode() { Text = "Enemy Generators" };
            var damageShapeNodes = new TreeNode() { Text = "Damage Shapes" };
            var itemPopupRegionNodes = new TreeNode() { Text = "Item Popup Regions" };
            var ptrainerRangeNodes = new TreeNode() { Text = "Pokémon Trainer Ranges" };
            var ptrainerFloatingFloorNodes = new TreeNode() { Text = "Pokémon Trainer Platforms" };
            var generalShape2DNodes = new TreeNode() { Text = "General Shapes (2D)" };
            var generalShape3DNodes = new TreeNode() { Text = "General Shapes (3D)" };
            var shrinkedCameraRegionNodes = new TreeNode() { Text = "Shrinked Camera Regions" };
            var shrinkedDeathRegionNodes = new TreeNode() { Text = "Shrinked Death Regions" };

            levelDataNode.Nodes.Add(collisionNodes);
            levelDataNode.Nodes.Add(startPositionNodes);
            levelDataNode.Nodes.Add(restartPositionNodes);
            levelDataNode.Nodes.Add(cameraRegionNodes);
            levelDataNode.Nodes.Add(deathRegionNodes);
            levelDataNode.Nodes.Add(enemyGeneratorNodes);
            levelDataNode.Nodes.Add(damageShapeNodes);
            levelDataNode.Nodes.Add(itemPopupRegionNodes);
            levelDataNode.Nodes.Add(ptrainerRangeNodes);
            levelDataNode.Nodes.Add(ptrainerFloatingFloorNodes);
            levelDataNode.Nodes.Add(generalShape2DNodes);
            levelDataNode.Nodes.Add(generalShape3DNodes);
            levelDataNode.Nodes.Add(shrinkedCameraRegionNodes);
            levelDataNode.Nodes.Add(shrinkedDeathRegionNodes);

            foreach (var collision in LVD.Collisions)
            {
                var collisionNode = new TreeNode();

                collisionNode.Text = collision.MetaInfo.Name;
                collisionNode.Tag = collision;
                collisionNodes.Nodes.Add(collisionNode);
            }

            foreach (var startPosition in LVD.StartPositions)
            {
                var startPositionNode = new TreeNode();

                startPositionNode.Text = startPosition.MetaInfo.Name;
                startPositionNode.Tag = startPosition;
                startPositionNodes.Nodes.Add(startPositionNode);
            }

            foreach (var restartPosition in LVD.RestartPositions)
            {
                var restartPositionNode = new TreeNode();

                restartPositionNode.Text = restartPosition.MetaInfo.Name;
                restartPositionNode.Tag = restartPosition;
                restartPositionNodes.Nodes.Add(restartPositionNode);
            }

            foreach (var cameraRegion in LVD.CameraRegions)
            {
                var cameraRegionNode = new TreeNode();

                cameraRegionNode.Text = cameraRegion.MetaInfo.Name;
                cameraRegionNode.Tag = cameraRegion;
                cameraRegionNodes.Nodes.Add(cameraRegionNode);
            }

            foreach (var deathRegion in LVD.DeathRegions)
            {
                var deathRegionNode = new TreeNode();

                deathRegionNode.Text = deathRegion.MetaInfo.Name;
                deathRegionNode.Tag = deathRegion;
                deathRegionNodes.Nodes.Add(deathRegionNode);
            }

            foreach (var enemyGenerator in LVD.EnemyGenerators)
            {
                var enemyGeneratorNode = new TreeNode();

                enemyGeneratorNode.Text = enemyGenerator.MetaInfo.Name;
                enemyGeneratorNode.Tag = enemyGenerator;
                enemyGeneratorNodes.Nodes.Add(enemyGeneratorNode);
            }

            foreach (var damageShape in LVD.DamageShapes)
            {
                var damageShapeNode = new TreeNode();

                damageShapeNode.Text = damageShape.MetaInfo.Name;
                damageShapeNode.Tag = damageShape;
                damageShapeNodes.Nodes.Add(damageShapeNode);
            }

            foreach (var itemPopupRegion in LVD.ItemPopupRegions)
            {
                var itemPopupRegionNode = new TreeNode();

                itemPopupRegionNode.Text = itemPopupRegion.MetaInfo.Name;
                itemPopupRegionNode.Tag = itemPopupRegion;
                itemPopupRegionNodes.Nodes.Add(itemPopupRegionNode);
            }

            foreach (var ptrainerRange in LVD.PTrainerRanges)
            {
                var ptrainerRangeNode = new TreeNode();

                ptrainerRangeNode.Text = ptrainerRange.MetaInfo.Name;
                ptrainerRangeNode.Tag = ptrainerRange;
                ptrainerRangeNodes.Nodes.Add(ptrainerRangeNode);
            }

            foreach (var ptrainerFloatingFloor in LVD.PTrainerFloatingFloors)
            {
                var ptrainerFloatingFloorNode = new TreeNode();

                ptrainerFloatingFloorNode.Text = ptrainerFloatingFloor.MetaInfo.Name;
                ptrainerFloatingFloorNode.Tag = ptrainerFloatingFloor;
                ptrainerFloatingFloorNodes.Nodes.Add(ptrainerFloatingFloorNode);
            }

            foreach (var generalShape2D in LVD.GeneralShapes2D)
            {
                var generalShape2DNode = new TreeNode();

                generalShape2DNode.Text = generalShape2D.MetaInfo.Name;
                generalShape2DNode.Tag = generalShape2D;
                generalShape2DNodes.Nodes.Add(generalShape2DNode);
            }

            foreach (var generalShape3D in LVD.GeneralShapes3D)
            {
                var generalShape3DNode = new TreeNode();

                generalShape3DNode.Text = generalShape3D.MetaInfo.Name;
                generalShape3DNode.Tag = generalShape3D;
                generalShape3DNodes.Nodes.Add(generalShape3DNode);
            }

            foreach (var shrinkedCameraRegion in LVD.ShrinkedCameraRegions)
            {
                var shrinkedCameraRegionNode = new TreeNode();

                shrinkedCameraRegionNode.Text = shrinkedCameraRegion.MetaInfo.Name;
                shrinkedCameraRegionNode.Tag = shrinkedCameraRegion;
                shrinkedCameraRegionNodes.Nodes.Add(shrinkedCameraRegionNode);
            }

            foreach (var shrinkedDeathRegion in LVD.ShrinkedDeathRegions)
            {
                var shrinkedDeathRegionNode = new TreeNode();

                shrinkedDeathRegionNode.Text = shrinkedDeathRegion.MetaInfo.Name;
                shrinkedDeathRegionNode.Tag = shrinkedDeathRegion;
                shrinkedDeathRegionNodes.Nodes.Add(shrinkedDeathRegionNode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SelectObjectChanged(object sender, EventArgs args)
        {
            ToolPanel.Controls.Clear();
            ToolPanel.Text = "LVD Options";

            if (PropertyGrid.SelectedObject is LVDVector2)
            {
                ToolPanel.Text = "Point Options";
                ToolPanel.Controls.Add(PointToolStrip);
            }
            else
            {
                ToolPanel.Controls.Add(ExportLVD);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SelectNode(object sender, EventArgs args)
        {
            if (NodeTree.SelectedNode != null)
            {
                if (NodeTree.SelectedNode.Tag != null)
                {
                    PropertyGrid.SelectedObject = NodeTree.SelectedNode.Tag;
                }
            }
        }

        public string[] Extension()
        {
            return new string[] { ".lvd" };
        }

        public bool AllowMultiple()
        {
            return false;
        }

        public bool OverlayScene()
        {
            return true;
        }
        
        public void OnAttach(SBViewportPanel viewportPanel)
        {
            viewportPanel.TabPanel.AddTab("Level Data", this);
        }

        private Vector3 Picked = Vector3.Zero;

        public void Pick(Ray ray)
        {
            if (!IsActive)
            {
                return;
            }

            PropertyGrid.SelectedObject = null;

            var depthPicked = ray.GetPlaneIntersection(-Vector3.UnitZ, new Vector3(0.0f, 0.0f, -CollisionDepth / 2.0f));
            Picked = ray.GetPlaneIntersection(-Vector3.UnitZ, Vector3.Zero);

            Vector2 nearestLine;
            float closest = float.MaxValue;
            LVDCollisionAttribute collisionAttribute = null;

            foreach (var collision in LVD.Collisions)
            {
                Vector2 startPos = (collision.IsDynamic && collision.Dynamic) ? new Vector2(collision.DynamicOffset.X, collision.DynamicOffset.Y) : new Vector2(0.0f, 0.0f);

                for (int i = 0; i < collision.Vertices.Count; i++)
                {
                    var v1 = collision.Vertices[i];

                    if (i < collision.Attributes.Count)
                    {
                        var v2 = collision.Vertices[i + 1];
                        var distance = Ray.GetDistanceToSegment(depthPicked.Xy, new Vector2(v1.X + startPos.X, v1.Y + startPos.Y), new Vector2(v2.X + startPos.X, v2.Y + startPos.Y), out nearestLine);

                        if (distance < CollisionDepth / 4.0f & distance < closest)
                        {
                            closest = distance;
                            collisionAttribute = collision.Attributes[i];
                        }
                    }

                    if (CrossMath.FastDistance(Picked, new Vector3(v1.X + startPos.X, v1.Y + startPos.Y, 0.0f), PickRange))
                    {
                        PropertyGrid.SelectedObject = v1;
                        return;
                    }
                }
            }

            foreach (var startPosition in LVD.StartPositions)
            {
                if (CrossMath.FastDistance(Picked, new Vector3(startPosition.Position.X, startPosition.Position.Y, 0.0f), PickRange * 2.5f))
                {
                    PropertyGrid.SelectedObject = startPosition;
                    return;
                }
            }

            foreach (var restartPosition in LVD.RestartPositions)
            {
                if (CrossMath.FastDistance(Picked, new Vector3(restartPosition.Position.X, restartPosition.Position.Y, 0.0f), PickRange * 2.5f))
                {
                    PropertyGrid.SelectedObject = restartPosition;
                    return;
                }
            }

            foreach (var cameraRegion in LVD.CameraRegions)
            {
                if (Ray.CheckBoundHit(Picked.Xy, new Vector2(cameraRegion.Rectangle.Left, cameraRegion.Rectangle.Top), new Vector2(cameraRegion.Rectangle.Right, cameraRegion.Rectangle.Bottom), PickRange))
                {
                    PropertyGrid.SelectedObject = cameraRegion;
                    return;
                }
            }

            foreach (var deathRegion in LVD.DeathRegions)
            {
                if (Ray.CheckBoundHit(Picked.Xy, new Vector2(deathRegion.Rectangle.Left, deathRegion.Rectangle.Top), new Vector2(deathRegion.Rectangle.Right, deathRegion.Rectangle.Bottom), PickRange))
                {
                    PropertyGrid.SelectedObject = deathRegion;
                    return;
                }
            }

            foreach (var generalShape3D in LVD.GeneralShapes3D)
            {
                if (generalShape3D.Shape.Type == LVDShape3Type.Point && CrossMath.FastDistance(Picked, new Vector3(generalShape3D.Shape.X, generalShape3D.Shape.Y, generalShape3D.Shape.Z), PickRange * 2.5f))
                {
                    PropertyGrid.SelectedObject = generalShape3D;
                    return;
                }
            }

            foreach (var shrinkedCameraRegion in LVD.ShrinkedCameraRegions)
            {
                if (Ray.CheckBoundHit(Picked.Xy, new Vector2(shrinkedCameraRegion.Rectangle.Left, shrinkedCameraRegion.Rectangle.Top), new Vector2(shrinkedCameraRegion.Rectangle.Right, shrinkedCameraRegion.Rectangle.Bottom), PickRange))
                {
                    PropertyGrid.SelectedObject = shrinkedCameraRegion;
                    return;
                }
            }

            foreach (var shrinkedDeathRegion in LVD.ShrinkedDeathRegions)
            {
                if (Ray.CheckBoundHit(Picked.Xy, new Vector2(shrinkedDeathRegion.Rectangle.Left, shrinkedDeathRegion.Rectangle.Top), new Vector2(shrinkedDeathRegion.Rectangle.Right, shrinkedDeathRegion.Rectangle.Bottom), PickRange))
                {
                    PropertyGrid.SelectedObject = shrinkedDeathRegion;
                    return;
                }
            }

            if (PropertyGrid.SelectedObject == null && collisionAttribute != null)
                PropertyGrid.SelectedObject = collisionAttribute;
        }

        public void OnRemove(SBViewportPanel viewportPanel)
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
            {
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Key.AltLeft))
            {
                if (Keyboard.GetState().IsKeyDown(Key.A))
                {
                    if (!ADown)
                    {
                        ADown = true;

                        if (PropertyGrid.SelectedObject is LVDVector2 v)
                        {
                            AddVertex(v);
                        }
                    }
                }
                else
                {
                    ADown = false;
                }

                if (Mouse.GetState().IsButtonDown(MouseButton.Left))
                {
                    if (PropertyGrid.SelectedObject is LVDPoint point)
                    {
                        point.DynamicOffset.X -= deltaMouse.X / 4.0f;
                        point.DynamicOffset.Y += deltaMouse.Y / 4.0f;
                        point.Position.X -= deltaMouse.X / 4.0f;
                        point.Position.Y += deltaMouse.Y / 4.0f;
                        PropertyGrid.SelectedObject = PropertyGrid.SelectedObject;
                    }

                    if (PropertyGrid.SelectedObject is LVDGeneralShape3 generalShape3D)
                    {
                        generalShape3D.DynamicOffset.X -= deltaMouse.X / 4.0f;
                        generalShape3D.DynamicOffset.Y += deltaMouse.Y / 4.0f;
                        generalShape3D.Shape.X -= deltaMouse.X / 4.0f;
                        generalShape3D.Shape.Y += deltaMouse.Y / 4.0f;
                        PropertyGrid.SelectedObject = PropertyGrid.SelectedObject;
                    }

                    if (PropertyGrid.SelectedObject is LVDVector2 v)
                    {
                        v.X -= deltaMouse.X / 4.0f;
                        v.Y += deltaMouse.Y / 4.0f;

                        // Recalculate all collision unit normal vectors.
                        // TODO: Is there a better way to do this?
                        foreach (var collision in LVD.Collisions)
                        {
                            int index = collision.Vertices.IndexOf(v);

                            if (index == -1)
                            {
                                continue;
                            }

                            if (index < collision.Normals.Count)
                            {
                                collision.Normals[index] = LVDVector2.GenerateNormal(v, collision.Vertices[index + 1]);
                            }

                            if (index > 0)
                            {
                                collision.Normals[index - 1] = LVDVector2.GenerateNormal(collision.Vertices[index - 1], v);
                            }

                            break;
                        }

                        PropertyGrid.SelectedObject = PropertyGrid.SelectedObject;
                    }
                }
            }

            if (Keyboard.GetState().IsKeyDown(Key.Delete))
            {
                if (!DeleteDown)
                {
                    DeleteDown = true;

                    if (PropertyGrid.SelectedObject is LVDVector2 v)
                    {
                        DeleteVertex(v);
                    }
                }
            }
            else
            {
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
        private void AddVertex(LVDVector2 v)
        {
            foreach (var collision in LVD.Collisions)
            {
                int index = collision.Vertices.IndexOf(v);

                if (index == -1)
                {
                    continue;
                }

                if (index == collision.Vertices.Count - 1)
                {
                    var newPoint = new LVDVector2(v.X + 3.0f, v.Y);
                    var newAttribute = new LVDCollisionAttribute();
                    newAttribute.Type = collision.Attributes[index - 1].Type;
                    collision.Vertices.Add(newPoint);
                    collision.Normals.Add(LVDVector2.GenerateNormal(v, newPoint));
                    collision.Attributes.Add(newAttribute);
                    PropertyGrid.SelectedObject = newPoint;
                }
                else
                {
                    var newPoint = new LVDVector2((v.X + collision.Vertices[index + 1].X) / 2.0f, (v.Y + collision.Vertices[index + 1].Y) / 2.0f);
                    var newAttribute = new LVDCollisionAttribute();
                    newAttribute.Type = collision.Attributes[index].Type;
                    var newNormal = LVDVector2.GenerateNormal(newPoint, collision.Vertices[index + 1]);
                    collision.Normals.Insert(index + 1, newNormal);
                    collision.Attributes.Insert(index + 1, newAttribute);
                    collision.Vertices.Insert(index + 1, newPoint);
                    PropertyGrid.SelectedObject = newPoint;
                }

                break;
            }
        }

        /// <summary>
        /// Deletes a vertex.
        /// </summary>
        /// <param name="v"></param>
        private void DeleteVertex(LVDVector2 v)
        {
            LVDCollision remove = null;

            foreach (var collision in LVD.Collisions)
            {
                int index = collision.Vertices.IndexOf(v);

                if (index == -1)
                {
                    continue;
                }

                if (index >= 1)
                {
                    PropertyGrid.SelectedObject = collision.Vertices[index - 1];
                }

                if (collision.Normals.Count > 0)
                {
                    if (index == collision.Normals.Count)
                    {
                        collision.Normals.RemoveAt(index - 1);
                        collision.Attributes.RemoveAt(index - 1);
                    }
                    else
                    {
                        collision.Normals.RemoveAt(index);
                        collision.Attributes.RemoveAt(index);
                    }
                }
                
                collision.Vertices.RemoveAt(index);

                if (index == collision.Vertices.Count)
                {
                    index--;
                }
                
                if (collision.Normals.Count > 0 && index > 0)
                {
                    collision.Normals[index - 1] = LVDVector2.GenerateNormal(collision.Vertices[index - 1], collision.Vertices[index]);
                }

                if (collision.Vertices.Count < 2)
                {
                    remove = collision;
                }

                break;
            }

            // Remove collision that is marked for removal
            if (remove != null)
            {
                LVD.Collisions.Remove(remove);
                RefreshNodes();
            }
        }

        public void Save(string FilePath)
        {
        }

        #region Rendering

        private int FlashTimer = 0;
        private int FlashInterval = 15;
        private static Vector3 FlashColor1 = new Vector3(1.0f, 1.0f, 1.0f);
        private static Vector3 FlashColor2 = new Vector3(1.0f, 1.0f, 0.0f);
        private Vector3 FlashColor = FlashColor1;

        public void Render(SBViewport viewport, float frame = 0.0f)
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
                {
                    FlashColor = FlashColor1;
                }
            }

            // TODO: Draw with shader
            if (LVD != null)
            {
                GL.PushAttrib(AttribMask.AllAttribBits);

                GL.Disable(EnableCap.CullFace);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                GL.UseProgram(0);

                GL.Color3(1.0f, 1.0f, 0.0f);
                GL.LineWidth(2.0f);

                GL.PointSize(5.0f);
                GL.Begin(PrimitiveType.Points);
                GL.Vertex3(Picked.X, Picked.Y, Picked.Z);
                GL.End();

                RenderCollisions();

                int playerIndex = 1;

                foreach (var startPosition in LVD.StartPositions)
                {
                    if (PropertyGrid.SelectedObject == startPosition)
                    {
                        Rendering.TextRenderer.Draw(viewport.Camera, "P" + playerIndex++, Matrix4.CreateTranslation(new Vector3(startPosition.Position.X, startPosition.Position.Y, 0.0f)), FlashColor);
                    }
                    else
                    {
                        Rendering.TextRenderer.Draw(viewport.Camera, "P" + playerIndex++, Matrix4.CreateTranslation(new Vector3(startPosition.Position.X, startPosition.Position.Y, 0.0f)));
                    }
                }

                foreach (var restartPosition in LVD.RestartPositions)
                {
                    if (PropertyGrid.SelectedObject == restartPosition)
                    {
                        Rendering.Shapes.Spawn.RenderSpawn(restartPosition.Position.X, restartPosition.Position.Y, 5.0f, FlashColor);
                    }
                    else
                    {
                        Rendering.Shapes.Spawn.RenderSpawn(restartPosition.Position.X, restartPosition.Position.Y, 5.0f, new Vector3(0.95f, 0.95f, 0.95f));
                    }
                }

                foreach (var cameraRegion in LVD.CameraRegions)
                {
                    RenderRegion(cameraRegion, Color.SkyBlue);
                }

                foreach (var deathRegion in LVD.DeathRegions)
                {
                    RenderRegion(deathRegion, Color.LightPink);
                }

                foreach (var itemPopupRegion in LVD.ItemPopupRegions)
                {
                    foreach (var region in itemPopupRegion.Regions)
                    {
                        RenderShape(region);
                    }
                }

                foreach (var generalShape3D in LVD.GeneralShapes3D)
                {
                    Rendering.Shapes.VectorGraphicType graphic;
                    Vector3 color;

                    if (generalShape3D.DynamicName.Contains("KirifudaPit"))
                    {
                        graphic = Rendering.Shapes.VectorGraphicType.FireEmblem;
                        color = new Vector3(1.0f, 0.85f, 0.75f);
                    }
                    else if (generalShape3D.DynamicName.Contains("KirifudaPikmin"))
                    {
                        graphic = Rendering.Shapes.VectorGraphicType.Pikmin;
                        color = new Vector3(0.65f, 1.0f, 0.65f);
                    }
                    else if (generalShape3D.DynamicName.Contains("KirifudaIke"))
                    {
                        graphic = Rendering.Shapes.VectorGraphicType.FireEmblem;
                        color = new Vector3(1.0f, 0.85f, 0.75f);
                    }
                    else
                    {
                        graphic = Rendering.Shapes.VectorGraphicType.StarStorm;
                        color = new Vector3(0.75f, 0.85f, 1.0f);
                    }

                    if (generalShape3D == PropertyGrid.SelectedObject)
                    {
                        color = FlashColor;
                    }

                    Rendering.Shapes.VectorGraphic.RenderGraphic(graphic, Matrix4.CreateTranslation(generalShape3D.Shape.X, generalShape3D.Shape.Y, generalShape3D.Shape.Z), color, 8.0f);
                }

                foreach (var shrinkedCameraRegion in LVD.ShrinkedCameraRegions)
                {
                    RenderRegion(shrinkedCameraRegion, Color.SkyBlue);
                }

                foreach (var shrinkedDeathRegion in LVD.ShrinkedDeathRegions)
                {
                    RenderRegion(shrinkedDeathRegion, Color.LightPink);
                }

                GL.PopAttrib();
            }

            if (PropertyGrid.SelectedObject is LVDVector2)
            {
                Rendering.TextRenderer.DrawOrtho(viewport.Camera, "Alt+Mouse: Move Point", new Vector2(4.0f, (float)(viewport.Camera.RenderHeight - 30)));
                Rendering.TextRenderer.DrawOrtho(viewport.Camera, "Alt + A  : Add Point", new Vector2(4.0f, (float)(viewport.Camera.RenderHeight - 16)));
                Rendering.TextRenderer.DrawOrtho(viewport.Camera, "Delete   : Delete", new Vector2(4.0f, (float)(viewport.Camera.RenderHeight - 2)));
            }
            else
            {
                Rendering.TextRenderer.DrawOrtho(viewport.Camera, "Double Click to Select", new Vector2(4.0f, (float)(viewport.Camera.RenderHeight - 16)));
            }
        }

        /// <summary>
        /// Renders all collisions to the viewport using legacy OpenGL.
        /// </summary>
        private void RenderCollisions()
        {
            foreach (var collision in LVD.Collisions)
            {
                for (int i = 0; i < collision.Normals.Count; i++)
                {
                    if (PropertyGrid.SelectedObject == collision.Vertices[i] || PropertyGrid.SelectedObject == collision.Vertices[i + 1])
                    {
                        Vector3 startPos = (collision.IsDynamic && collision.Dynamic) ? new Vector3(collision.DynamicOffset.X, collision.DynamicOffset.Y, collision.DynamicOffset.Z) : new Vector3(0.0f, 0.0f, 0.0f);

                        GL.Color3(1.0f, 1.0f, 0.0f);
                        GL.PointSize(12.0f);
                        GL.Begin(PrimitiveType.Points);
                        GL.Vertex3(
                            (PropertyGrid.SelectedObject == collision.Vertices[i] ? collision.Vertices[i].X : collision.Vertices[i + 1].X) + startPos.X,
                            (PropertyGrid.SelectedObject == collision.Vertices[i] ? collision.Vertices[i].Y : collision.Vertices[i + 1].Y) + startPos.Y,
                            startPos.Z
                        );
                        GL.End();
                    }

                    RenderEdge(
                        collision,
                        collision.Vertices[i],
                        collision.Vertices[i + 1],
                        collision.Attributes.Count != 0 ? collision.Attributes[i] : new LVDCollisionAttribute(),
                        new Vector2(collision.Normals[i].X, collision.Normals[i].Y)
                    );
                }
            }
        }

        /// <summary>
        /// Renders an edge to the viewport using legacy OpenGL.
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="attribute"></param>
        /// <param name="normal"></param>
        private void RenderEdge(LVDCollision collision, LVDVector2 p1, LVDVector2 p2, LVDCollisionAttribute attribute, Vector2 normal)
        {
            Vector2 startPos = (collision.IsDynamic && collision.Dynamic) ? new Vector2(collision.DynamicOffset.X, collision.DynamicOffset.Y) : new Vector2(0.0f, 0.0f);
            Vector2 v1 = new Vector2(p1.X, p1.Y);
            Vector2 v2 = new Vector2(p2.X, p2.Y);
            Vector2 mid = (v1 + v2) / 2.0f;
            Vector2 nrm = mid + normal * 3.0f;

            Vector3 p1Color = GetElementColor(p1);
            Vector3 p2Color = GetElementColor(p2);

            if (PropertyGrid.SelectedObject == collision)
            {
                p1Color = FlashColor;
                p2Color = FlashColor;
            }

            // material
            var materialColor = GetMaterialColor(attribute);
            GL.Color4(materialColor.R / 255.0f, materialColor.G / 255.0f, materialColor.B / 255.0f, 0.75f);
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex3(p1.X + startPos.X, p1.Y + startPos.Y, 0.0f);
            GL.Vertex3(p1.X + startPos.X, p1.Y + startPos.Y, -CollisionDepth);
            GL.Vertex3(p2.X + startPos.X, p2.Y + startPos.Y, -CollisionDepth);
            GL.Vertex3(p2.X + startPos.X, p2.Y + startPos.Y, 0.0f);
            GL.End();

            GL.LineWidth(2.0f);
            GL.Begin(PrimitiveType.Lines);
            
            // point line 1
            GL.Color3(p1Color);
            GL.Vertex3(v1.X + startPos.X, v1.Y + startPos.Y, 0.0f);
            GL.Color3(p1Color);
            GL.Vertex3(v1.X + startPos.X, v1.Y + startPos.Y, -CollisionDepth);

            // point line 2
            GL.Color3(p2Color);
            GL.Vertex3(v2.X + startPos.X, v2.Y + startPos.Y, 0.0f);
            GL.Color3(p2Color);
            GL.Vertex3(v2.X + startPos.X, v2.Y + startPos.Y, -CollisionDepth);

            // front line
            GL.Color3(p1Color);
            GL.Vertex3(v1.X + startPos.X, v1.Y + startPos.Y , 0.0f);
            GL.Color3(p2Color);
            GL.Vertex3(v2.X + startPos.X, v2.Y + startPos.Y, 0.0f);

            // back line
            GL.Color3(p1Color);
            GL.Vertex3(v1.X + startPos.X, v1.Y + startPos.Y, -CollisionDepth);
            GL.Color3(p2Color);
            GL.Vertex3(v2.X + startPos.X, v2.Y + startPos.Y, -CollisionDepth);
            
            // normal
            GL.Color3(GetNormalColor(collision, normal, attribute));
            GL.Vertex3(mid.X + startPos.X, mid.Y + startPos.Y, -CollisionDepth / 2.0f);
            GL.Color3(GetNormalColor(collision, normal, attribute));
            GL.Vertex3(nrm.X + startPos.X, nrm.Y + startPos.Y, -CollisionDepth / 2.0f);

            GL.End();
        }

        /// <summary>
        /// Renders a rectangular region to the viewport using legacy OpenGL.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="color"></param>
        private void RenderRegion(LVDRegion region, Color color)
        {
            GL.Color4(Color.FromArgb(128, color));

            if (PropertyGrid.SelectedObject == region)
            {
                GL.Color3(FlashColor);
            }

            GL.LineWidth(2.0f);

            GL.Begin(PrimitiveType.LineLoop);

            GL.Vertex3(region.Rectangle.Left, region.Rectangle.Top, 0.0f);
            GL.Vertex3(region.Rectangle.Right, region.Rectangle.Top, 0.0f);
            GL.Vertex3(region.Rectangle.Right, region.Rectangle.Bottom, 0.0f);
            GL.Vertex3(region.Rectangle.Left, region.Rectangle.Bottom, 0.0f);

            GL.End();
        }

        /// <summary>
        /// Returns color of the unit normal vector.
        /// </summary>
        /// <param name="collision"></param>
        /// <param name="normal"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private Color GetNormalColor(LVDCollision collision, Vector2 normal, LVDCollisionAttribute attribute)
        {
            float angle = (float)Math.Atan2(normal.Y, normal.X) * 180.0f / (float)Math.PI;

            // Check edge attributes first, then determine unit normal vector color from the angle of the edge.
            if (collision.DropThrough || attribute.DropThrough)
            {
                return Color.FromArgb(128, Color.Yellow);
            }
            else if (attribute.RightWallOverride || attribute.LeftWallOverride)
            {
                return Color.FromArgb(128, Color.Lime);
            }
            else if (attribute.CeilingOverride)
            {
                return Color.FromArgb(128, Color.Red);
            }
            else if (attribute.FloorOverride || (angle > 40.0f && angle < 140.0f))
            {
                return Color.FromArgb(128, Color.Cyan);
            }
            else if (((angle >= 140.0f && angle <= 180.0f) || angle < -110.0f) || (angle <= 40.0f && angle > -70.0f))
            {
                return attribute.NoWallJump ? Color.FromArgb(128, Color.Purple) : Color.FromArgb(128, Color.Lime);
            }
            else if (angle >= -110.0f && angle <= -70.0f)
            {
                return Color.FromArgb(128, Color.Red);
            }
            else
            {
                return Color.FromArgb(128, Color.Black);
            }
        }

        /// <summary>
        /// Renders an LVD shape to the viewport using legacy OpenGL.
        /// </summary>
        /// <param name="shape"></param>
        private void RenderShape(LVDShape2 shape)
        {
            switch (shape.Type)
            {
                case LVDShape2Type.Point:
                    break;
                case LVDShape2Type.Circle:
                    break;
                case LVDShape2Type.Rectangle:
                    break;
                case LVDShape2Type.Path:
                    GL.Color3(Color.Bisque);
                    GL.Begin(PrimitiveType.LineStrip);

                    foreach (var p in shape.Points)
                    {
                        GL.Vertex3(p.X, p.Y, 0.0f);
                    }

                    GL.End();
                    break;
            }
        }

        /// <summary>
        /// Returns the color of the collision's edge's assigned material.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private Color GetMaterialColor(LVDCollisionAttribute attribute)
        {
            if (PropertyGrid.SelectedObject == attribute)
            {
                return Color.FromArgb(255, (int)(FlashColor.X * 255.0f), (int)(FlashColor.Y * 255.0f), (int)(FlashColor.Z * 255.0f));
            }

            switch (attribute.Type)
            {
                case LVDCollisionMaterialType.Basic:
                    return Color.WhiteSmoke;
                case LVDCollisionMaterialType.Rock:
                    return Color.SlateGray;
                case LVDCollisionMaterialType.Grass:
                    return Color.ForestGreen;
                case LVDCollisionMaterialType.Soil:
                    return Color.Sienna;
                case LVDCollisionMaterialType.Wood:
                    return Color.BurlyWood;
                case LVDCollisionMaterialType.LightMetal:
                    return Color.LightGray;
                case LVDCollisionMaterialType.HeavyMetal:
                    return Color.DarkGray;
                case LVDCollisionMaterialType.Carpet:
                    return Color.Tomato;
                case LVDCollisionMaterialType.Slimy:
                    return Color.Goldenrod;
                case LVDCollisionMaterialType.Creature:
                    return Color.DarkOliveGreen;
                case LVDCollisionMaterialType.Shoal:
                    return Color.LightSkyBlue;
                case LVDCollisionMaterialType.Soft:
                    return Color.LightPink;
                case LVDCollisionMaterialType.Slippery:
                    return Color.LightGreen;
                case LVDCollisionMaterialType.Snow:
                    return Color.Snow;
                case LVDCollisionMaterialType.Ice:
                    return Color.CornflowerBlue;
                case LVDCollisionMaterialType.GameWatch:
                    return Color.DarkKhaki;
                case LVDCollisionMaterialType.Oil:
                    return Color.DarkSlateGray;
                case LVDCollisionMaterialType.Cardboard:
                    return Color.Peru;
                case LVDCollisionMaterialType.Damage1:
                case LVDCollisionMaterialType.Damage2:
                case LVDCollisionMaterialType.Damage3:
                    return Color.Gray;
                case LVDCollisionMaterialType.Hanenbow:
                    return Color.DarkSeaGreen;
                case LVDCollisionMaterialType.Cloud:
                    return Color.FromArgb(0xFF, 0xF6, 0x9A, 0xB0);
                case LVDCollisionMaterialType.Subspace:
                    return Color.PaleVioletRed;
                case LVDCollisionMaterialType.Brick:
                    return Color.DarkSalmon;
                case LVDCollisionMaterialType.NoAttr:
                    return Color.AliceBlue;
                case LVDCollisionMaterialType.Famicom:
                    return Color.OrangeRed;
                case LVDCollisionMaterialType.WireNetting:
                    return Color.DimGray;
                case LVDCollisionMaterialType.Sand:
                    return Color.SandyBrown;
                case LVDCollisionMaterialType.Homerun:
                    return Color.Gray;
                case LVDCollisionMaterialType.AsaseEarth:
                    return Color.LightSkyBlue;
                case LVDCollisionMaterialType.Death:
                    return Color.IndianRed;
                case LVDCollisionMaterialType.BoxingRing:
                    return Color.DeepSkyBlue;
                case LVDCollisionMaterialType.Glass:
                    return Color.GhostWhite;
                case LVDCollisionMaterialType.SlipDx:
                    return Color.LightSlateGray;
                case LVDCollisionMaterialType.SpPoison:
                    return Color.MediumOrchid;
                case LVDCollisionMaterialType.SpFlame:
                    return Color.DarkOrange;
                case LVDCollisionMaterialType.SpElectricShock:
                    return Color.Yellow;
                case LVDCollisionMaterialType.SpSleep:
                    return Color.Violet;
                case LVDCollisionMaterialType.SpFreezing:
                    return Color.RoyalBlue;
                case LVDCollisionMaterialType.SpAdhesion:
                    return Color.FromArgb(0xFF, 0x70, 0x64, 0x4A);
                case LVDCollisionMaterialType.IceNoSlip:
                    return Color.CornflowerBlue;
                case LVDCollisionMaterialType.CloudNoThrough:
                    return Color.FromArgb(0xFF, 0xF6, 0x9A, 0xB0);
                case LVDCollisionMaterialType.Metaverse:
                    return Color.Crimson;
                default:
                    return Color.Black;
            }
        }

        /// <summary>
        /// Gets the color of the specified object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private Vector3 GetElementColor(object o)
        {
            if (PropertyGrid.SelectedObject == o)
            {
                return (FlashColor);
            }
            else
            {
                return new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
        #endregion
    }
}
