using StudioSB.Tools;
using System.ComponentModel;

namespace StudioSB.Scenes.LVD
{
    public enum LVDCollisionMaterialType : uint
    {
        Basic = 0x00,               //
        Rock = 0x01,                //
        Grass = 0x02,               // Produces a grass shard effect.
        Soil = 0x03,                // Produces a soil particle effect when landing.
        Wood = 0x04,                //
        LightMetal = 0x05,          // Internally "Iron"
        HeavyMetal = 0x06,          // Internally "NibuIron"
        Carpet = 0x07,              //
        Slimy = 0x08,               // Internally "Numenume"
        Creature = 0x09,            //
        Shoal = 0x0A,               // Internally "Asase." Produces a water splash effect.
        Soft = 0x0B,                //
        Slippery = 0x0C,            // Internally "Turuturu." Friction value of 0.1.
        Snow = 0x0D,                //
        Ice = 0x0E,                 // Friction value of 0.2. Produces an ice particle effect when landing.
        GameWatch = 0x0F,           // Used on the Flat Zone X stage.
        Oil = 0x10,                 // Used on the Flat Zone X stage on the Helmet game. Friction value of 0.1.
        Cardboard = 0x11,           // Internally "Danbouru" 
        Damage1 = 0x12,             // "damage_id" value of 1.
        Damage2 = 0x13,             // "damage_id" value of 2.
        Damage3 = 0x14,             // "damage_id" value of 3.
        Hanenbow = 0x15,            // Used on the Hanenbow stage.
        Cloud = 0x16,               // Force-enables supersoft properties.
        Subspace = 0x17,            // Internally "Akuukan"
        Brick = 0x18,               //
        NoAttr = 0x19,              // Same as the "Basic" material but does not produce any special graphical or sound effects.
        Famicom = 0x1A,             // Used on the Mario Bros. stage.
        WireNetting = 0x1B,         //
        Sand = 0x1C,                // Produces a sand particle effect when landing.
        Homerun = 0x1D,             //
        AsaseEarth = 0x1E,          //
        Death = 0x1F,               // "damage_id" value of 4.
        BoxingRing = 0x20,          // Used on the Boxing Ring stage.
        Glass = 0x21,               //
        SlipDx = 0x22,              // Used for slippery surfaces on newly-returning Melee stages. Friction value of 0.4.
        SpPoison = 0x23,            // Used for environmental floor hazards in spirit battles.
        SpFlame = 0x24,             // Used for environmental floor hazards in spirit battles.
        SpElectricShock = 0x25,     // Used for environmental floor hazards in spirit battles.
        SpSleep = 0x26,             // Used for environmental floor hazards in spirit battles.
        SpFreezing = 0x27,          // Used for environmental floor hazards in spirit battles. Same as the "Ice" material.
        SpAdhesion = 0x28,          // Used for environmental floor hazards in spirit battles.
        IceNoSlip = 0x29,           // Same as the "Ice" material but does not have any reduced traction.
        CloudNoThrough = 0x2A,      // Same as the "Cloud" material but does not force-enable supersoft properties.
        Metaverse = 0x2B,           // Internally "Jack_Mementoes." Produces the Metaverse splash effect.
    }

    public class LVDCollisionAttribute
    {
        [ReadOnly(true), Category("Version")]
        public byte Version { get; internal set; } = 1;

        private uint[] MaterialData = new uint[3];

        [Category("Material")]
        public LVDCollisionMaterialType Type
        {
            get { return (LVDCollisionMaterialType)MaterialData[0]; }
            set { MaterialData[0] = (uint)value; }
        }

        [Category("Attributes")]
        public bool Unpaintable
        {
            get { return GetFlag(5); }
            set { SetFlag(5, value); }
        }

        [Category("Attributes")]
        public bool RightWallOverride
        {
            get { return GetFlag(8); }
            set { SetFlag(8, value); }
        }
        
        [Category("Attributes")]
        public bool LeftWallOverride
        {
            get { return GetFlag(9); }
            set { SetFlag(9, value); }
        }

        [Category("Attributes")]
        public bool CeilingOverride
        {
            get { return GetFlag(10); }
            set { SetFlag(10, value); }
        }

        [Category("Attributes")]
        public bool FloorOverride
        {
            get { return GetFlag(11); }
            set { SetFlag(11, value); }
        }

        [Category("Attributes")]
        public bool NoWallJump
        {
            get { return GetFlag(12); }
            set { SetFlag(12, value); }
        }

        [Category("Attributes")]
        public bool DropThrough
        {
            get { return GetFlag(13); }
            set { SetFlag(13, value); }
        }

        [Category("Attributes")]
        public bool LeftLedge
        {
            get { return GetFlag(14); }
            set { SetFlag(14, value); }
        }

        [Category("Attributes")]
        public bool RightLedge
        {
            get { return GetFlag(15); }
            set { SetFlag(15, value); }
        }

        [Category("Attributes")]
        public bool IgnoreLinkFromLeft
        {
            get { return GetFlag(16); }
            set { SetFlag(16, value); }
        }

        [Category("Attributes")]
        public bool Supersoft
        {
            get { return GetFlag(17); }
            set { SetFlag(17, value); }
        }

        [Category("Attributes")]
        public bool IgnoreLinkFromRight
        {
            get { return GetFlag(18); }
            set { SetFlag(18, value); }
        }

        public bool GetFlag(int n)
        {
            return (MaterialData[2] & 1 << n) != 0;
        }

        public void SetFlag(int flag, bool value)
        {
            uint mask = (uint)(1 << flag);

            if (value)
            {
                MaterialData[2] |= mask;
            }
            else
            {
                MaterialData[2] &= ~mask;
            }
        }

        public void Read(BinaryReaderExt reader)
        {
            Version = reader.ReadByte();

            for (int i = 0; i < MaterialData.Length; i++)
            {
                MaterialData[i] = reader.ReadUInt32();
            }
        }

        public void Write(BinaryWriterExt writer)
        {
            writer.Write(Version);

            for (int i = 0; i < MaterialData.Length; i++)
            {
                writer.Write(MaterialData[i]);
            }
        }
    }
}
