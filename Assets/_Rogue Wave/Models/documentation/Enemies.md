# Developer Notes

To create a new enemy:

1. Copy the `Gem` as a base enemy and rename it
2. Place the new enemy in the scene and unpack the prefab
3. Copy the Enemy Definition Scriptable Object
4. Apply the new Enemy Definition to the new enemy
5. Delete the existing model
6. Add the new model
7. Ensure the model is tagged as AI
8. If necessary adjust the position of the mesh
9. Adjust the position of the sensor so that it is outside the collider
10. Add a `collider` to the model (if there is more than one mesh then we need one on each)
11. Add a `Basic Damage Handler` to the model (if there is more than one mesh then we need one on each)
12. Add a `Simple Surface` and select the surface (if there is more than one mesh then we need one on each)
13. Adjust the `Health Manager` on the root
14. Adjust the `Enemy Controller` on the root
15. Adjust or replace the weapon
16. Place it into at least one level
