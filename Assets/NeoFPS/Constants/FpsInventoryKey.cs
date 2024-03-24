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
	public struct FpsInventoryKey
	{
		public const int Undefined = 0;
		public const int MeleeKnife = 1;
		public const int MeleeBaton = 2;
		public const int MeleeReserved01 = 3;
		public const int MeleeReserved02 = 4;
		public const int MeleeReserved03 = 5;
		public const int FirearmPistol = 6;
		public const int FirearmRevolver = 7;
		public const int FirearmShotgun = 8;
		public const int FirearmAssaultRifle = 9;
		public const int FirearmSniperRifle = 10;
		public const int FirearmGrenadeLauncher = 11;
		public const int ThrownFragGrenade = 12;
		public const int ThrownReserved01 = 13;
		public const int WeaponReserved01 = 14;
		public const int WeaponReserved02 = 15;
		public const int Ammo9mm = 16;
		public const int Ammo357magnum = 17;
		public const int Ammo12gauge = 18;
		public const int Ammo556mm = 19;
		public const int Ammo762mm = 20;
		public const int Ammo40mmHE = 21;
		public const int AmmoReserved01 = 22;
		public const int AmmoReserved02 = 23;
		public const int AmmoReserved03 = 24;
		public const int AmmoReserved04 = 25;
		public const int ArmourBody = 26;
		public const int ArmourHelmet = 27;
		public const int KeyRing = 28;
		public const int Lockpick = 29;

		public const int count = 30;

		public static readonly string[] names = new string[]
		{
			"Undefined",
			"MeleeKnife",
			"MeleeBaton",
			"MeleeReserved01",
			"MeleeReserved02",
			"MeleeReserved03",
			"FirearmPistol",
			"FirearmRevolver",
			"FirearmShotgun",
			"FirearmAssaultRifle",
			"FirearmSniperRifle",
			"FirearmGrenadeLauncher",
			"ThrownFragGrenade",
			"ThrownReserved01",
			"WeaponReserved01",
			"WeaponReserved02",
			"Ammo9mm",
			"Ammo357magnum",
			"Ammo12gauge",
			"Ammo556mm",
			"Ammo762mm",
			"Ammo40mmHE",
			"AmmoReserved01",
			"AmmoReserved02",
			"AmmoReserved03",
			"AmmoReserved04",
			"ArmourBody",
			"ArmourHelmet",
			"KeyRing",
			"Lockpick"
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

		private FpsInventoryKey (int v)
		{
			m_Value = v;
		}

		public static bool IsWithinBounds (int v)
		{
			int cast = (int)v;
			return (cast >= 0) && (cast < count);
		}

		// Checks
		public static bool operator ==(FpsInventoryKey x, FpsInventoryKey y)
		{
			return (x.value == y.value);
		}
		public static bool operator ==(FpsInventoryKey x, int y)
		{
			return (x.value == y);
		}

		public static bool operator !=(FpsInventoryKey x, FpsInventoryKey y)
		{
			return (x.value != y.value);
		}
		public static bool operator !=(FpsInventoryKey x, int y)
		{
			return (x.value != y);
		}

		public override bool Equals (object obj)
		{
			if (obj is FpsInventoryKey)
				return value == ((FpsInventoryKey)obj).value;
			if (obj is int)
				return value == (int)value;
			return false;
		}

		// Implicit conversions
		public static implicit operator FpsInventoryKey (int v)
		{
			int max = count - 1;
			if (v < 0)
				v = 0;
			if (v > max)
				v = 0; // Reset to default
			return new FpsInventoryKey (v);
		}

		public static implicit operator int (FpsInventoryKey dam)
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