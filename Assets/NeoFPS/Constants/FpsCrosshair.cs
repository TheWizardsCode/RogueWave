//======================================================================================================
// WARNING: This file is auto-generated.
// Any manual changes will be lost.
// Use the constant generator system instead
//======================================================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.Constants
{
	[Serializable]
	public struct FpsCrosshair
	{
		public const byte Default = 0;
		public const byte None = 1;

		public const int count = 2;

		public static readonly string[] names = new string[]
		{
			"Default",
			"None"
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

		private FpsCrosshair (byte v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (byte v)
		{
			int cast = (int)v;
			return (cast < count);
		}
		
		// Checks
		public static bool operator ==(FpsCrosshair x, FpsCrosshair y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsCrosshair x, byte y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsCrosshair x, FpsCrosshair y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsCrosshair x, byte y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsCrosshair)
				return value == ((FpsCrosshair)obj).value;
			if (obj is byte)
				return value == (byte)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsCrosshair (byte v)
		{
			byte max = (byte)(names.Length - 1);
			if (v > max)
				v = 0; // Reset to default
			return new FpsCrosshair (v);
		}

		public static implicit operator byte (FpsCrosshair dam)
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