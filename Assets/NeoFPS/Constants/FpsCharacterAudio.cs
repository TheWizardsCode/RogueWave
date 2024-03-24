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
	public struct FpsCharacterAudio
	{
		public const int Undefined = 0;
		public const int Oof = 1;
		public const int Pain = 2;
		public const int Collapse = 3;
		public const int BoneBreak = 4;
		public const int Negative = 5;

		public const int count = 6;

		public static readonly string[] names = new string[]
		{
			"Undefined",
			"Oof",
			"Pain",
			"Collapse",
			"BoneBreak",
			"Negative"
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

		private FpsCharacterAudio (int v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (int v)
		{
			int cast = (int)v;
			return (cast >= 0) && (cast < count);
		}

		// Checks
		public static bool operator ==(FpsCharacterAudio x, FpsCharacterAudio y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsCharacterAudio x, int y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsCharacterAudio x, FpsCharacterAudio y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsCharacterAudio x, int y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsCharacterAudio)
				return value == ((FpsCharacterAudio)obj).value;
			if (obj is int)
				return value == (int)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsCharacterAudio (int v)
		{
			int max = count - 1;
			if (v < 0)
				v = 0;
			if (v > max)
				v = 0; // Reset to default
			return new FpsCharacterAudio (v);
		}

		public static implicit operator int (FpsCharacterAudio dam)
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