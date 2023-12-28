Recipes describe upgrades that the player may get during play.

# Create the Interactive Pickup

Recipes use the NeoFPS pickup system to describe what will be made. This allows us to create either a pickup for the player to collect or to create the item directly. There is nothing special about these pickups so follow the NeoFPS docs on their creation.

# Create a Generic Recipe

The following steps are the same for all kinds of Recipe. See the following sections for any type specific steps.

1. Right click in `Resources/Recipes` (or a subfolder if you wish to structure your files) select `Create -> Playground -> [pickup type]`
2. Name it appropriately
3. In the Inspector set the Display Name, Description and Hero Image
4. If you want this pickup to be available as a permanent powerup which can be purchased between levels then chek the `Is Power Up` box
5. Drop the pickup that describes the item to be created in the `Pickup` field of the `Item` section
6. Set the cost (in resources) and the time to build (in game seconds)
7. All the items in the `Feedback` section are optional. If you leave these values empty then defaults will be used.

# Creating a Weapon Recipe

The following steps are unique to the creation of a weapon recipe:

1. If the weapon requires ammo then place the recipe for its creation in the `Ammo Recipe` field. Whenever a player is given a weapon recipe they will also recieve this recipe
