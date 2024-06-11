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
	public struct FpsSwappableCategory
	{
		public const int Firearm = 0;
		public const int Thrown = 1;

		public const int count = 2;

		public static readonly string[] names = new string[]
		{
			"Firearm",
			"Thrown"
		};

		[SerializeField] 
		private int m_Value;
		public int value
		{
			get { return m_Value; }
			set
			{
				int max = (int)(count - 1);
				if (value < 0)
					value = 0;
				if (value > max)
					value = 0; // Reset to default
				m_Value = value;
			}
		}

		private FpsSwappableCategory (int v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (int v)
		{
			int cast = (int)v;
			return (cast >= 0) && (cast < count);
		}

		// Checks
		public static bool operator ==(FpsSwappableCategory x, FpsSwappableCategory y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsSwappableCategory x, int y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsSwappableCategory x, FpsSwappableCategory y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsSwappableCategory x, int y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsSwappableCategory)
				return value == ((FpsSwappableCategory)obj).value;
			if (obj is int)
				return value == (int)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsSwappableCategory (int v)
		{
			int max = count - 1;
			if (v < 0)
				v = 0;
			if (v > max)
				v = 0; // Reset to default
			return new FpsSwappableCategory (v);
		}

		public static implicit operator int (FpsSwappableCategory dam)
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