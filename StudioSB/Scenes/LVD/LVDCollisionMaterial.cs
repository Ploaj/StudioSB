using StudioSB.Tools;

namespace StudioSB.Scenes.LVD
{
    public enum CollisionMatType : byte
    {
        Basic = 0x00,            //
        Rock = 0x01,             //
        Grass = 0x02,            //Increased traction (1.5)
        Soil = 0x03,             //
        Wood = 0x04,             //
        LightMetal = 0x05,       //"Iron" internally.
        HeavyMetal = 0x06,       //"NibuIron" (Iron2) internally.
        Carpet = 0x07,           //Used for Delfino Plaza roof things
        Alien = 0x08,            //"NumeNume" (squelch sound) internally. Used on Brinstar
        MasterFortress = 0x09,   //"Creature" internally.
        WaterShallow = 0x0a,     //"Asase" (shallows) internally. Used for Delfino Plaza shallow water
        Soft = 0x0b,             //Used on Woolly World
        TuruTuru = 0x0c,         //Reduced traction (0.1). Unknown meaning and use
        Snow = 0x0d,             //
        Ice = 0x0e,              //Reduced traction (0.2). Used on P. Stadium 2 ice form
        GameWatch = 0x0f,        //Used on Flat Zone
        Oil = 0x10,              //Reduced traction (0.1). Used for Flat Zone oil spill (presumably; not found in any collisions)
        Cardboard = 0x11,        //"Danbouru" (corrugated cardboard) internally. Used on Paper Mario
        SpikesTargetTest = 0x12, //Unknown. From Brawl, and appears to still be hazard-related but is probably not functional
        Hazard2SSEOnly = 0x13,   //See above
        Hazard3SSEOnly = 0x14,   //See above
        Electroplankton = 0x15,  //"ElectroP" internally. Not known to be used anywhere in this game
        Cloud = 0x16,            //Used on Skyworld, Magicant
        Subspace = 0x17,         //"Akuukan" (subspace) internally. Not known to be used anywhere in this game
        Brick = 0x18,            //Used on Skyworld, Gerudo V., Smash Run
        Unknown1 = 0x19,         //Unknown. From Brawl
        NES8Bit = 0x1a,          //"Famicom" internally. Not known to be used anywhere in this game
        Grate = 0x1b,            //Used on Delfino and P. Stadium 2
        Sand = 0x1c,             //
        Homerun = 0x1d,          //From Brawl, may not be functional
        WaterNoSplash = 0x1e,    //From Brawl, may not be functional
        Hurt = 0x1f,             //Takes hitbox data from stdat. Used for Cave and M. Fortress Danger Zones
        Unknown2 = 0x20          //Unknown. Uses bomb SFX?
    }

    public class LVDCollisionMaterial
    {
        private byte[] MaterialData = new byte[0xC];

        public CollisionMatType Physics
        {
            get { return (CollisionMatType)MaterialData[3]; }
            set { MaterialData[3] = (byte)value; }
        }

        public bool LeftLedge
        {
            get { return GetFlag(6); }
            set { SetFlag(6, value); }
        }

        public bool RightLedge
        {
            get { return GetFlag(7); }
            set { SetFlag(7, value); }
        }

        public bool NoWallJump
        {
            get { return GetFlag(4); }
            set { SetFlag(4, value); }
        }

        public bool GetFlag(int n)
        {
            return ((MaterialData[10] & (1 << n)) != 0);
        }

        public void SetFlag(int flag, bool value)
        {
            byte mask = (byte)(1 << flag);
            bool isSet = (MaterialData[10] & mask) != 0;
            if (value)
                MaterialData[10] |= mask;
            else
                MaterialData[10] &= (byte)~mask;
        }

        public void Read(BinaryReaderExt r)
        {
            r.ReadByte();

            MaterialData = r.ReadBytes(0xC);
        }
    }
}
