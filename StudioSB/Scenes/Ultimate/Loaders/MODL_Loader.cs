using System.Collections.Generic;
using SSBHLib.Formats;

namespace StudioSB.Scenes.Ultimate
{
    public class MODL_Loader
    {

        public static Modl CreateMODLFile(SBUltimateModel model)
        {
            Modl modl = new Modl();
            modl.ModelEntries = new ModlEntry[model.Meshes.Count];
            Dictionary<string, int> subindex = new Dictionary<string, int>();
            int i = 0;
            foreach(var mesh in model.Meshes)
            {
                var entry = new ModlEntry();
                modl.ModelEntries[i++] = entry;

                if (!subindex.ContainsKey(mesh.Name))
                    subindex.Add(mesh.Name, 0);

                entry.MeshName = mesh.Name;
                entry.SubIndex = subindex[mesh.Name];
                if (mesh.Material != null)
                    entry.MaterialLabel = mesh.Material.Label;
                else
                    SBConsole.WriteLine("Warning: Missing material");
                subindex[mesh.Name]++;
                SBConsole.WriteLine($"Creating modl entry: {entry.MeshName} {entry.SubIndex} {entry.MaterialLabel}");
            }

            return modl;
        }
    }
}
