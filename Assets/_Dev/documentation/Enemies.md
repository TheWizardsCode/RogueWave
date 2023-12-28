# Developer Notes

To create a new enemy:

1. Copy the `Gem` as a base enemy and rename it
2. Delete the existing model
2. If necessary adjust the position of the mesh
3. Add a `collider` to the model (if there is more than one mesh then we need one on each)
4. Add a `Basic Damage Handler` (if there is more than one mesh then we need one on each)
5. Add a `Simple Surface` and select the surface (if there is more than one mesh then we need one on each)
6. Adjust the `Health Manager` on the root
7. Adjust the `Enemy Controller` on the root
8. Ensure the damage collider on the root is correctly set
9. Add an animation controller as needed
10. In the Enemy Dev scene change the spawned enemy to your new prefab and test