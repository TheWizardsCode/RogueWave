namespace WizardsCode.RogueWave
{
    public interface IWeaponFiringBehaviour
    {
        /// <summary>
        /// Indicate if this weapon does damage over time or instantly
        /// </summary>
        public bool DamageOverTime { get; }
        /// <summary>
        /// The amount of damage per second, or the total damage if not damage over time.
        /// </summary>
        public float DamageAmount { get; set; }
    }
}