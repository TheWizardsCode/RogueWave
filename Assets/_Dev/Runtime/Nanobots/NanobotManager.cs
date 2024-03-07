using NaughtyAttributes;
using NeoFPS;
using NeoFPS.ModularFirearms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RogueWave
{
    public class NanobotManager : MonoBehaviour
    {
        enum voiceDescriptionLevel { Silent, Low, Medium, High }

        [Header("Building")]
        [SerializeField, Tooltip("Cooldown between recipe builds.")]
        private float buildingCooldown = 4;
        [SerializeField, Tooltip("How many resources are needed for a level up recipe offer. This will be multiplied by the current level * 1.5. Meaning the higher the level the more resources are required for a reward crate.")]
        private int baseResourcesPerLevel = 100;
        [SerializeField, Tooltip("This is the multiplier for the resources required for the next level. This is multiplied by the current level and the baseResourcesPerLevel to get the resources required for the next level.")]
        private float resourcesPerLevelMultiplier = 1.5f;
        [SerializeField, Tooltip("The time between recipe offers from the home planet. Once a player has levelled up they will recieve an updated offer until they accept one. This is the freqency at which the offer will be changed.")]
        private float timeBetweenRecipeOffers = 10;
        [SerializeField, Tooltip("How far away from the player will built pickup be spawned.")]
        private float pickupSpawnDistance = 3;

        [Header("Feedbacks")]
        [SerializeField, Tooltip("Define how 'chatty' the nanobots are. This will affect how often and in what level of detail they announce things to the player.")]
        private voiceDescriptionLevel voiceDescription = voiceDescriptionLevel.High;
        [SerializeField, Tooltip("The sound to play to indicate a new recipe is available from home planet. This will be played before the name of the recipe to tell the player that they can call it in if they want.")]
        private AudioClip[] recipeRequestPrefix;
        [SerializeField, Tooltip("The sound to play to indicate a recipe has been requested.")]
        private AudioClip[] recipeRequested;
        [SerializeField, Tooltip("The sound to play to indicate a recipe request has been queued. This will be played if the player requests the recipe, but the nanobots are busy with something else at that time.")]
        private AudioClip[] recipeRequestQueued;
        [SerializeField, Tooltip("The sound to play to indicate a new recipe has been recieved. This will be played before the name of the recipe to tell the player that the recipe has been recieved.")]
        private AudioClip[] recipeRecievedPrefix;
        [SerializeField, Tooltip("The sound to play when the build is started. Note that this can be overridden in the recipe.")]
        private AudioClip[] buildStartedClips;
        [SerializeField, Tooltip("The sound to play when the build is complete. Note that this can be overridden in the recipe.")]
        private AudioClip[] buildCompleteClips;
        [SerializeField, Tooltip("The default particle system to play when a pickup is spawned. Note that this can be overridden in the recipe."), FormerlySerializedAs("pickupSpawnParticlePrefab")]
        ParticleSystem defaultPickupParticlePrefab;
        [SerializeField, Tooltip("The default audio clip to play when a recipe name is needed, but the recipe does not have a name clip. This should never be used in practice.")]
        AudioClip defaultRecipeName;

        [Header("Debug")]
        [SerializeField, Tooltip("Turn on debug features for the Nanobot Manager"), Foldout("Debug")]
        bool isDebug = false;

        private List<ArmourPickupRecipe> armourRecipes = new List<ArmourPickupRecipe>();
        private List<HealthPickupRecipe> healthRecipes = new List<HealthPickupRecipe>();
        private List<ShieldPickupRecipe> shieldRecipes = new List<ShieldPickupRecipe>();
        private List<WeaponPickupRecipe> weaponRecipes = new List<WeaponPickupRecipe>();
        private List<AmmoPickupRecipe> ammoRecipes = new List<AmmoPickupRecipe>();
        private List<AmmunitionEffectUpgradeRecipe> ammoUpgradeRecipes = new List<AmmunitionEffectUpgradeRecipe>();
        private List<ToolPickupRecipe> toolRecipes = new List<ToolPickupRecipe>();
        private List<GenericItemPickupRecipe> itemRecipes = new List<GenericItemPickupRecipe>();
        private List<PassiveItemPickupRecipe> passiveRecipes = new List<PassiveItemPickupRecipe>();

        private int numInGameRewards = 3;
        internal int resourcesForNextNanobotLevel = 0;

        public delegate void OnResourcesChanged(float from, float to, float resourcesUntilNextLevel);
        public event OnResourcesChanged onResourcesChanged;

        public delegate void OnNanobotLevelUp();
        public event OnNanobotLevelUp onNanobotLevelUp;

        public delegate void OnStatusChanged(Status status);
        public event OnStatusChanged onStatusChanged;

        public delegate void OnOfferChanged(IRecipe[] offers);
        public event OnOfferChanged onOfferChanged;

        float earliestTimeOfNextItemSpawn = 0;
        private Coroutine rewardCoroutine;

        public enum Status
        {
            Collecting,
            OfferingRecipe,
            RequestQueued,
            Requesting,
            RequestRecieved,
            Building
        }

        Status _status;
        public Status status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;

                _status = value;

                if (onStatusChanged != null)
                    onStatusChanged(_status);
            }
        }

        IRecipe[] _currentOffers;
        public IRecipe[] currentOfferRecipes
        {
            get { return _currentOffers; }
            set
            {
                if (_currentOffers == value)
                    return;

                _currentOffers = value;

                if (onOfferChanged != null)
                    onOfferChanged(_currentOffers);
            }
        }
        
        private float timeOfLastRewardOffer = 0;

        private float timeOfNextBuiild = 0;

        private void Start()
        {
            currentOfferRecipes = new IRecipe[numInGameRewards];

            resourcesForNextNanobotLevel = GetRequiredResourcesForNextNanobotLevel();

            foreach (var healthRecipe in healthRecipes)
            {
                healthRecipe.Reset();
            }
            foreach (var weaponRecipe in weaponRecipes)
            {
                weaponRecipe.Reset();
            }
            foreach (var ammoRecipe in ammoRecipes)
            {
                ammoRecipe.Reset();
            }
            foreach (var ammoRecipe in ammoUpgradeRecipes)
            {
                ammoRecipe.Reset();
            }
            foreach (var toolRecipe in toolRecipes)
            {
                toolRecipe.Reset();
            }
            foreach (var itemRecipe in itemRecipes)
            {
                itemRecipe.Reset();
            }
        }

        private void Update()
        {
            if (status != Status.Collecting)
            {
                return;
            }

            // Are we leveling up?
            if (resourcesForNextNanobotLevel <= 0)
            {
                LevelUp();
                if (rewardCoroutine != null)
                {
                    StopCoroutine(rewardCoroutine);
                }
                rewardCoroutine = StartCoroutine(OfferInGameRewardRecipe());
            }

            if (timeOfNextBuiild > Time.timeSinceLevelLoad)
            {
                return;
            }

            if (status == Status.OfferingRecipe || status == Status.Requesting) {
                return;
            }

            // Prioritize building ammo if the player is low on ammo
            if (TryAmmoRecipes(0.1f))
            {
                return;
            }

            // Health is the next priority, got to stay alive, but only build at this stage if fairly badly hurt
            if (TryHealthRecipes(0.6f))
            {
                return;
            }

            // Prioritize building shield if the player does not have a shield or is low on shield
            if (TryShieldRecipes(0.4f))
            {
                return;
            }

            // Prioritize building armour if the player does not have armour or is low on armour
            if (TryArmourRecipes(0.4f))
            {
                return;
            }

            // If we are in good shape then see if there's a new weapon we can build
            if (TryWeaponRecipes())
            {
                return;
            }

            // Health is the next priority, got to stay alive, but only build at this stage we should only need a small topup
            if (TryHealthRecipes(0.85f))
            {
                return;
            }

            // If we can't afford a powerup, build ammo up to near mazimum (not maximum because there will often be half used clips lying around)
            if (TryAmmoRecipes(0.9f))
            {
                return;
            }

            // If we are in good shape then see if there is a generic item we can build
            if (TryItemRecipes())
            {
                return;
            }

            // Prioritize building shield if the player does not have a shield or there is some damage to the shiled
            if (TryShieldRecipes(0.8f))
            {
                return;
            }

            // Prioritize building armour if the player does not have full armour
            if (TryArmourRecipes(0.9f))
            {
                return;
            }

            // May as well get to maximum health if there is nothing else to build
            if (TryHealthRecipes(1f))
            {
                return;
            }
        }

        internal List<AmmunitionEffectUpgradeRecipe> GetAmmunitionEffectUpgradesFor(SharedAmmoType ammoType)
        {   
            List<AmmunitionEffectUpgradeRecipe> matchingRecipes = new List<AmmunitionEffectUpgradeRecipe>();

            foreach (var recipe in ammoUpgradeRecipes)
            {
                if (recipe.ammoType == ammoType)
                {
                    matchingRecipes.Add(recipe);
                }
            }

            return matchingRecipes;
        }

        IEnumerator OfferInGameRewardRecipe()
        {
            timeOfLastRewardOffer = Time.timeSinceLevelLoad;
            List<IRecipe> offers = RecipeManager.GetOffers(numInGameRewards, 0);

            if (offers.Count == 0)
            {
                yield break;
            }

            currentOfferRecipes = offers.ToArray();
            status = Status.OfferingRecipe;
            yield return null;

            // Announce a recipe is available
            AudioClip clip = recipeRequestPrefix[Random.Range(0, recipeRequestPrefix.Length)];
            AudioClip[] recipeNames = new AudioClip[currentOfferRecipes.Length];
            for (int i = 0; i < currentOfferRecipes.Length; i++) {
                if (currentOfferRecipes[i].NameClip == null)
                {
                    recipeNames[i] = defaultRecipeName;
                    Debug.LogWarning($"Recipe {currentOfferRecipes[i].DisplayName} (offer) does not have an audio clip for its name. Used default name.");
                }
                else
                {
                    recipeNames[i] = currentOfferRecipes[i].NameClip;
                }   
            }
            StartCoroutine(Announce(clip, recipeNames));

            status = Status.OfferingRecipe;

            while (true)
            {
                int i = int.MaxValue;
                // TODO: add these keys to the NeoFPS input manager so that it is configurable
                if (Input.GetKey(KeyCode.B))
                {
                    i = 0;
                } else if (Input.GetKey(KeyCode.N))
                {
                    i = 1;
                } else if (Input.GetKey(KeyCode.M))
                {
                    i = 2;
                }

                if (i < int.MaxValue)
                {
                    if (status == Status.Building) {
                        status = Status.RequestQueued;
                        clip = recipeRequestQueued[Random.Range(0, recipeRequestQueued.Length)];
                        yield return Announce(clip);
                        
                        while (status == Status.Building)
                        {
                            yield return new WaitForSeconds(1);
                        }
                    }

                    yield return new WaitForSeconds(2);

                    status = Status.Requesting;
                    timeOfNextBuiild = Time.timeSinceLevelLoad + currentOfferRecipes[i].TimeToBuild + 5f;

                    // Announce request made
                    clip = recipeRequested[Random.Range(0, recipeRequested.Length)];
                    if (Time.timeSinceLevelLoad - timeOfLastRewardOffer > 5)
                    {
                        yield return Announce(clip);
                    }

                    yield return new WaitForSeconds(currentOfferRecipes[i].TimeToBuild);

                    // Announce request recieved
                    status = Status.RequestRecieved;
                    clip = recipeRecievedPrefix[Random.Range(0, recipeRecievedPrefix.Length)];
                    Announce(clip);
                    if (currentOfferRecipes[0].TimeToBuild > 5)
                    {
                        yield return Announce(clip);
                    }

                    RogueLiteManager.runData.Add(currentOfferRecipes[i]);
                    Add(currentOfferRecipes[i]);

                    status = Status.Collecting;

                    break;
                }

                // if the player does not want the recipe then we will offer another one in a few seconds
                if (status != Status.Requesting && timeOfLastRewardOffer + timeBetweenRecipeOffers < Time.timeSinceLevelLoad)
                {
                    if (rewardCoroutine != null)
                    {
                        StopCoroutine(rewardCoroutine);
                    }
                    rewardCoroutine = StartCoroutine(OfferInGameRewardRecipe());
                }

                yield return null;
            }
        }

        private void LevelUp()
        {
            resourcesForNextNanobotLevel = GetRequiredResourcesForNextNanobotLevel();
            RogueLiteManager.persistentData.currentNanobotLevel++;
            onNanobotLevelUp?.Invoke();
        }

        /// <summary>
        /// Make an announcement to the player. This will play the clip at the position of the nanobot manager.
        /// </summary>
        /// <param name="mainClip">The main clip to play</param>
        private IEnumerator Announce(AudioClip mainClip)
        {
            if (voiceDescription == voiceDescriptionLevel.Silent)
            {
                yield break;
            }

            yield return Announce(mainClip, new AudioClip[] { });
        }

        /// <summary>
        /// Make an announcement to the player. This will play the clip at the position of the nanobot manager.
        /// </summary>
        /// <param name="mainClip">The main clip to play</param>
        /// <param name="recipeName">OPTIONAL: if not null then this recipe name clip will be announced after the main clip</param>
        private IEnumerator Announce(AudioClip mainClip, AudioClip recipeName)
        {
            yield return Announce(mainClip, recipeName == null ? null : new AudioClip[] { recipeName });
        }

        /// <summary>
        /// Make an announcement to the player. This will play the clip at the position of the nanobot manager.
        /// </summary>
        /// <param name="mainClip">The main clip to play</param>
        /// <param name="recipeName">OPTIONAL: if not null then these recipe name clips will be announced after the main clip</param>
        private IEnumerator Announce(AudioClip mainClip, AudioClip[] recipeNames)
        {
            if (voiceDescription == voiceDescriptionLevel.Silent)
            {
                yield break;
            }

            //Debug.Log($"Announcing {mainClip.name} with recipe name {recipeName?.name}");

            NeoFpsAudioManager.PlayEffectAudioAtPosition(mainClip, transform.position, 1);
            yield return new WaitForSeconds(mainClip.length);

            if (recipeNames == null || recipeNames.Length == 0 || voiceDescription < voiceDescriptionLevel.Medium)
            {
                yield break;
            }

            foreach (AudioClip recipeName in recipeNames)
            {
                NeoFpsAudioManager.PlayEffectAudioAtPosition(recipeName, transform.position, 1);
                yield return new WaitForSeconds(recipeName.length + 0.3f);
            }
        }

        private int GetRequiredResourcesForNextNanobotLevel()
        {
            return Mathf.RoundToInt((RogueLiteManager.persistentData.currentNanobotLevel + 1) * resourcesPerLevelMultiplier * baseResourcesPerLevel);
        }

        // TODO: we can probably generalize these Try* methods now that we have refactored the recipes to use interfaces/Abstract classes

        /// <summary>
        /// Build the best health recipe available. The best is the one that will heal to MaxHealth with the minimum waste.
        /// </summary>
        /// <param name="minimumHealthAmount">The % (0-1) of health that is the minimum required before health will be built</param>
        /// <returns></returns>
        private bool TryHealthRecipes(float minimumHealthAmount = 1)
        {
            HealthPickupRecipe chosenRecipe = null;
            float chosenAmount = 0;
            for (int i = 0; i < healthRecipes.Count; i++)
            {
                if (healthRecipes[i].HasAmount(minimumHealthAmount))
                {
                    return false;
                }

                if (RogueLiteManager.persistentData.currentResources >= healthRecipes[i].BuildCost && healthRecipes[i].ShouldBuild)
                {
                    float healAmount = Mathf.Min(1, healthRecipes[i].healAmountPerCent);
                    if (healAmount > chosenAmount)
                    {
                        chosenRecipe = healthRecipes[i];
                        chosenAmount = healAmount;
                    } else if (healAmount == chosenAmount && (chosenRecipe == null || (chosenRecipe != null && chosenRecipe.BuyCost > healthRecipes[i].BuyCost)))
                    {
                        chosenRecipe = healthRecipes[i];
                        chosenAmount = healAmount;
                    }
                }
            }

            if (chosenRecipe != null)
            {
                return TryRecipe(chosenRecipe);
            }

            return false;
        }

        private bool TryShieldRecipes(float minimumShieldAmount)
        {
            for (int i = 0; i < shieldRecipes.Count; i++)
            {
                if (!shieldRecipes[i].HasAmount(minimumShieldAmount))
                {
                    if (TryRecipe(shieldRecipes[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryArmourRecipes(float minimumArmourAmount)
        {
            for (int i = 0; i < armourRecipes.Count; i++)
            {
                if (!armourRecipes[i].HasAmount(minimumArmourAmount))
                {
                    if (TryRecipe(armourRecipes[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryWeaponRecipes()
        {
            // Build weapons in the build order first
            foreach (string id in RogueLiteManager.persistentData.WeaponBuildOrder)
            {
                IRecipe weapon;
                if (RecipeManager.TryGetRecipeFor(id, out weapon))
                {
                    if (((WeaponPickupRecipe)weapon).InInventory == false)
                    {
                        return TryRecipe(weapon as WeaponPickupRecipe);
                    }
                }
            }

            // If we have built everything in the build order then try to build anything we bought during this run
            foreach (IRecipe recipe in RogueLiteManager.runData.Recipes)
            {
                WeaponPickupRecipe weapon = recipe as WeaponPickupRecipe;
                if (weapon != null && weapon.InInventory == false)
                {
                    return TryRecipe(weapon);
                }
            }

            return false;
        }

        private bool TryItemRecipes()
        {
            if (earliestTimeOfNextItemSpawn > Time.timeSinceLevelLoad)
            {
                return false;
            }

            float approximateFrequency = 10;
            for (int i = 0; i < itemRecipes.Count; i++)
            {
                // TODO: make a decision on whether to make a generic item in a more intelligent way
                // TODO: can we make tests that are dependent on the pickup, e.g. when the pickup is triggered it will only be picked up if needed 
                if (RogueLiteManager.persistentData.currentResources < itemRecipes[i].BuildCost)
                {
                    continue;
                }

                if (TryRecipe(itemRecipes[i]))
                {
                    earliestTimeOfNextItemSpawn = Time.timeSinceLevelLoad + (approximateFrequency * Random.Range(0.7f, 1.3f));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If the ammo available for the currently equipped weapon is below the minimum level, try to build ammo.
        /// </summary>
        /// <param name="minimumAmmoAmount">The % (0-1) of ammo that is the minimum required</param>
        /// <returns></returns>
        private bool TryAmmoRecipes(float minimumAmmoAmount)
        {
            AmmoPickupRecipe chosenRecipe = null;
            float chosenAmount = 0;
            for (int i = 0; i < ammoRecipes.Count; i++)
            {
                if (ammoRecipes[i].HasAmount(minimumAmmoAmount))
                {
                    continue;
                }

                if (RogueLiteManager.persistentData.currentResources >= ammoRecipes[i].BuildCost && ammoRecipes[i].ShouldBuild)
                {
                    float ammoAmount = Mathf.Min(1, ammoRecipes[i].ammoAmountPerCent);
                    if (ammoAmount > chosenAmount)
                    {
                        chosenRecipe = ammoRecipes[i];
                        chosenAmount = ammoAmount;
                    } else if (ammoAmount == chosenAmount && (chosenRecipe == null || (chosenRecipe != null && chosenRecipe.BuildCost > ammoRecipes[i].BuyCost)))
                    {
                        chosenRecipe = ammoRecipes[i];
                        chosenAmount = ammoAmount;
                    }
                }
            }

            if (chosenRecipe != null)
            {
                return TryRecipe(chosenRecipe);
            }

            return false;
        }

        private bool TryRecipe(IRecipe recipe)
        {
            if (recipe == null)
            {
                Debug.LogError("Attempting to build a null recipe.");
                return false;
            }

            if (RogueLiteManager.persistentData.currentResources < recipe.BuildCost) 
            {
                return false;
            }

            if (recipe.ShouldBuild)
            {
                StartCoroutine(BuildRecipe(recipe));
                return true;
            }

            return false;
        }

        internal IEnumerator BuildRecipe(IRecipe recipe)
        {
#if UNITY_EDITOR
            if (isDebug)
            {
                Debug.Log($"Building {recipe.DisplayName}");
            }
#endif
            status = Status.Building;
            resources -= recipe.BuildCost;

            if (recipe.BuildStartedClip != null)
            {
                yield return Announce(recipe.BuildStartedClip);
            } else
            {
                AudioClip recipeName = recipe.NameClip;
                if (recipeName == null)
                {
                    recipeName = defaultRecipeName;
                    Debug.LogError($"Recipe {recipe.DisplayName} ({recipe}) does not have an audio clip for its name. Used default of `Unkown`.");
                }

                yield return Announce(buildStartedClips[Random.Range(0, buildStartedClips.Length)], recipeName);
            }

            yield return new WaitForSeconds(recipe.TimeToBuild);


            IItemRecipe itemRecipe = recipe as IItemRecipe;
            if (itemRecipe != null)
            {
                // TODO Use the pool manager to create the item
                GameObject go = Instantiate(itemRecipe.Item.gameObject);

                Vector3 position = transform.position + (transform.forward * pickupSpawnDistance) + (transform.up * 1f);
                int positionCheck = 0;
                while (Physics.CheckSphere(position, 0.5f) || positionCheck > 10)
                {
                    positionCheck++;
                    position -= transform.forward;
                }

                go.transform.position = position;

                // TODO: Use the pool manager to create the particle system
                if (recipe.PickupParticles != null)
                {
                    ParticleSystem ps = Instantiate(recipe.PickupParticles, go.transform);
                    ps.Play();
                }
                else if (defaultPickupParticlePrefab != null)
                {
                    ParticleSystem ps = Instantiate(defaultPickupParticlePrefab, go.transform);
                    ps.Play();
                }

                if (recipe.BuildCompleteClip != null)
                {
                    yield return Announce(recipe.BuildCompleteClip);
                }
                else
                {
                    yield return Announce(buildCompleteClips[Random.Range(0, buildCompleteClips.Length)]);
                }
            }
            else
            {
                Debug.LogError("TODO: handle building recipes of type: " + recipe.GetType().Name);
            }

            recipe.BuildFinished();

            status = Status.Collecting;
            timeOfNextBuiild = Time.timeSinceLevelLoad + buildingCooldown;
        }

        /// <summary>
        /// Adds the recipe to the list of recipes available to these nanobots on this run.
        /// </summary>
        /// <param name="recipe">The recipe to add.</param>
        internal void Add(IRecipe recipe)
        {
            if (recipe == null)
            {
                Debug.LogError("Attempting to add a null recipe to the NanobotManager.");
                return;
            }

            // TODO: This is messy, far too many if...else statements. Do we really need to keep separate lists now that they have a common AbstractRecipe base class?
            if (recipe is AmmoPickupRecipe ammo && !ammoRecipes.Contains(ammo))
            {
                ammoRecipes.Add(ammo);
            }
            else if(recipe is ArmourPickupRecipe armour && !armourRecipes.Contains(armour))
            {
                armourRecipes.Add(armour);
            }
            else if (recipe is HealthPickupRecipe health && !healthRecipes.Contains(health))
            {
                healthRecipes.Add(health);
            } 
            else if (recipe is WeaponPickupRecipe weapon && !weaponRecipes.Contains(weapon))
            {
                weaponRecipes.Add(weapon);
                if (weapon.ammoRecipe != null)
                {
                    Add(weapon.ammoRecipe);
                }
            }
            else if (recipe is ToolPickupRecipe tool && !toolRecipes.Contains(tool))
            {
                toolRecipes.Add(tool);
            }
            else if (recipe is ShieldPickupRecipe shield && !shieldRecipes.Contains(shield))
            {
                shieldRecipes.Add(shield);
            }
            else if (recipe is GenericItemPickupRecipe item && !itemRecipes.Contains(item))
            {
                itemRecipes.Add(item);
            } 
            else if (recipe is BaseStatRecipe statRecipe)
            {
                Apply(statRecipe);
            }
            else if (recipe is AmmunitionEffectUpgradeRecipe ammoUpgradeRecipe)
            {
                ammoUpgradeRecipe.Apply();
                ammoUpgradeRecipes.Add(ammoUpgradeRecipe);
            }
            else if (recipe is PassiveItemPickupRecipe passiveRecipe)
            {
                if (passiveRecipe.Apply(this))
                {
                    passiveRecipes.Add(passiveRecipe);
                }
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogWarning($"Either {recipe.DisplayName} ({recipe.GetType().Name}) is an unkown recipe type or we tried to add it a second time.");
            }
#endif
        }

        internal void Apply(BaseStatRecipe statRecipe)
        {
            if (statRecipe != null)
            {
                statRecipe.Apply();
                return;
            }
        }

        /// <summary>
        /// The amount of resources the player currently has.
        /// </summary>
        public int resources
        {
            get { return RogueLiteManager.persistentData.currentResources; }
            private set
            {
                if (RogueLiteManager.persistentData.currentResources == value)
                    return;

                if (onResourcesChanged != null)
                    onResourcesChanged(RogueLiteManager.persistentData.currentResources, value, resourcesForNextNanobotLevel);

                RogueLiteManager.persistentData.currentResources = value;
            }
        }

        public void CollectResources(int amount)
        {
            resources += amount;
            resourcesForNextNanobotLevel -= amount;
        }

        private void ShuffleWeaponRecipes()
        {
            int n = weaponRecipes.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                WeaponPickupRecipe value = weaponRecipes[k];
                weaponRecipes[k] = weaponRecipes[n];
                weaponRecipes[n] = value;
            }
        }

        private void ShuffleToolRecipes()
        {
            int n = toolRecipes.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                ToolPickupRecipe value = toolRecipes[k];
                toolRecipes[k] = toolRecipes[n];
                toolRecipes[n] = value;
            }
        }

        private void ShuffleItemRecipes()
        {
            int n = itemRecipes.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                GenericItemPickupRecipe value = itemRecipes[k];
                itemRecipes[k] = itemRecipes[n];
                itemRecipes[n] = value;
            }
        }

        internal int GetAppliedCount(PassiveItemPickupRecipe passiveItemPickupRecipe)
        {
            return passiveRecipes.FindAll(x => x == passiveItemPickupRecipe).Count;
        }

#if UNITY_EDITOR
        [Button]
        private void Add10000Resources()
        {
            RogueLiteManager.persistentData.currentResources += 10000;
        }
#endif
    }
}