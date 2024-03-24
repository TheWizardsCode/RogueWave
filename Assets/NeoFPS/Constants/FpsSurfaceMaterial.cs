//======================================================================================================
// WARNING: This file is auto-generated.
// Any manual changes will be lost.
// Use the constant generator system instead
//======================================================================================================

using System;
using UnityEngine;

namespace NeoFPS.Constants
{
	[Serializable]
	public struct FpsSurfaceMaterial
	{
		public const byte Default = 0;
		public const byte Concrete = 1;
		public const byte Stone = 2;
		public const byte Dirt = 3;
		public const byte Grass = 4;
		public const byte Gravel = 5;
		public const byte Wood = 6;
		public const byte Plastic = 7;
		public const byte Gadget = 8;
		public const byte Flesh = 9;
		public const byte MetalSolid = 10;
		public const byte MetalHollowLight = 11;
		public const byte MetalHollowHeavy = 12;
		public const byte Treadplate = 13;
		public const byte Glass = 14;
		public const byte GlassBottle = 15;
		public const byte TinCan = 16;
		public const byte PaintTin = 17;
		public const byte Shield = 18;

		public const int count = 19;

		public static readonly string[] names = new string[]
		{
			"Default",
			"Concrete",
			"Stone",
			"Dirt",
			"Grass",
			"Gravel",
			"Wood",
			"Plastic",
			"Gadget",
			"Flesh",
			"MetalSolid",
			"MetalHollowLight",
			"MetalHollowHeavy",
			"Treadplate",
			"Glass",
			"GlassBottle",
			"TinCan",
			"PaintTin",
			"Shield"
		};

		[SerializeField] 
		private byte m_Value;
		public byte value
		{
			get { return m_Value; }
			set
			{
				byte max = (byte)(count - 1);
				if (value > max)
					value = 0; // Reset to default
				m_Value = value;
			}
		}

		private FpsSurfaceMaterial (byte v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (byte v)
		{
			int cast = (int)v;
			return (cast < count);
		}
		
		// Checks
		public static bool operator ==(FpsSurfaceMaterial x, FpsSurfaceMaterial y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsSurfaceMaterial x, byte y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsSurfaceMaterial x, FpsSurfaceMaterial y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsSurfaceMaterial x, byte y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsSurfaceMaterial)
				return value == ((FpsSurfaceMaterial)obj).value;
			if (obj is byte)
				return value == (byte)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsSurfaceMaterial (byte v)
		{
			byte max = (byte)(names.Length - 1);
			if (v > max)
				v = 0; // Reset to default
			return new FpsSurfaceMaterial (v);
		}

		public static implicit operator byte (FpsSurfaceMaterial dam)
		{
			return dam.value;
		}

		public override string ToString ()
		{
			return names [value];
		}

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
	}
}