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
	public struct FpsCharacterAudioSource
	{
		public const int Head = 0;
		public const int Body = 1;
		public const int Feet = 2;

		public const int count = 3;

		public static readonly string[] names = new string[]
		{
			"Head",
			"Body",
			"Feet"
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

		private FpsCharacterAudioSource (int v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (int v)
		{
			int cast = (int)v;
			return (cast >= 0) && (cast < count);
		}

		// Checks
		public static bool operator ==(FpsCharacterAudioSource x, FpsCharacterAudioSource y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsCharacterAudioSource x, int y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsCharacterAudioSource x, FpsCharacterAudioSource y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsCharacterAudioSource x, int y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsCharacterAudioSource)
				return value == ((FpsCharacterAudioSource)obj).value;
			if (obj is int)
				return value == (int)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsCharacterAudioSource (int v)
		{
			int max = count - 1;
			if (v < 0)
				v = 0;
			if (v > max)
				v = 0; // Reset to default
			return new FpsCharacterAudioSource (v);
		}

		public static implicit operator int (FpsCharacterAudioSource dam)
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