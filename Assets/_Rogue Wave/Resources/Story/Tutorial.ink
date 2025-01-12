>>> PlayAudio: Nanobots, Story/Tutorial, Beat 0 Welcome

All your team are dead. 

You are stranded.

We, your nanobots, can keep you alive, but we can't get you home without a portal generator. 

To build the portal generator we need resources.

Let's get started by seeing if you can remember who you are, or maybe pick a new identity for yourself. 

+ [New Identity] -> Create_New_Character

=== Create_New_Character

>>> execute: NavControls_Root.RogueWaveRootNavControls.OnClickNewGame
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 0 We are a part of you

We are a part of you. The interface you see in front of you is controlled by us.
We just clicked the "New Identity" button and suggested a name for this identity.

You can change the name if you want. 

When ready click "Create New Identity".

+ [Create Identity & Start] -> Introduction_Part_1

=== Introduction_Part_1 ===

>>> execute: MainGamePanel_CreateProfile.CreateNewProfilePanel.OnClickCreateProfile
>>> execute: Story Manager.StoryManager.HideUI
>>> WaitFor: SceneLoaded, RogueWave_CombatLevel
>>> execute: Story Manager.StoryManager.ShowUI
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 1 Introduction Part 1

We can "Nanotransfer" you into zones across the planet to gather the resources we need. 

Some of us will stay here, where it is safe, others will come with you to help in the field. 

Those that stay can keep a link open to you for 5 minutes. After that amount of time we will Nanotransfer you back here.

+ [Scary!] -> Introduction_Part_2

=== Introduction_Part_2 ===

>>> PlayAudio: Nanobots, Story/Tutorial, Beat 1 Introduction Part 2

When you are ready we'll send you out there to take a look around. 

At some point the locals are bound to get the better of you. Don't worry we will perform an emergency transfer. There is a cost to this, but at least you will be alive.

Those of us that come with you will form a particle cloud that looks like a person. We'll try to keep you safe. Keep an eye on us, we'll communicate information to you. Don't worry about us though, we can't be harmed by the natives. 

You should focus on keeping yourself safe and gathering tne resources we need to get out of here. 

+ [Nanotransfer In] -> Zones_And_Run_Upgrades

=== Zones_And_Run_Upgrades

>>> AudioMixer: Audio/RogueWave_AudioMixer, EffectsVolume, 0
>>> execute: Level Menu(Clone).LevelMenu.GenerateLevel
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 2 Zones and Run Upgrades

This is a safe zone but resources here are a little scarce. 

Some of the flora emits poisonous spores if disturbed and there are a few roaming animals that may attack. But we've detected no strong enemies here.

As you gather resources we will be able to construct upgrades. Listen out for notifications and watch your HUD for details.

Once you have gathered 500 resources we'll bring you back for some more permanent upgrades.

+ [I'm ready] -> Nanotransfer
+ [Die] -> Reconstruction

=== Nanotransfer

>>> execute: Story Manager.StoryManager.HideUI
>>> execute: Level Menu(Clone).LevelMenu.Spawn
>>> AudioMixer: Audio/RogueWave_AudioMixer, EffectsVolume, 1

+ [Survive] -> Portal_Crystals
+ [Die] -> Reconstruction

=== Portal_Crystals

>>> execute: Story Manager.StoryManager.ShowUI
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 3a Special Crystals and Permanent Upgrades

We now have resources and can make you stronger. Any upgrades you select here in the Hub are permanent. That is. they'll survive an emergency extraction. 

However, to build the portal generator we need specific kinds of crystal. 

When you have finished upgrading let us know by selecting "Prepare for Nanotransfer" and we'll send you to a new zone that contains Moisannite.

+ [Survive] -> One_Is_Never_Enough
+ [Die] -> Reconstruction

=== Reconstruction

>>> execute: Story Manager.StoryManager.ShowUI
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 3b Reconstruction

We had to pull you out, you were too close to true death. 

Emergency extractions only bring the essentials, you, the Nanobots with you and any resources you have collected. Everything else has to be left behind.

While we reconstruct you take a look at your stats here. They can be useful in figuring out where you are strong or weak.

+ [Let me try again.] -> Portal_Crystals

=== One_Is_Never_Enough

>>> PlayAudio: Nanobots, Story/Tutorial, Beat 4 One is never enough

Wonderful, you have gathered the first portal crystal, Moisannite. But we need others and the natives have figured out what we are up to. They want to stop us. They've created spawners that will send in waves of enemies to protect their resources. 

Think carefully about your upgrades. Maybe you'll focus on offense, or defense or a combination of both. Maybe you'll seek to improve yourself or you'll power us up.

It's your choice.

+ [I'm tooled up.] -> Now_The_Work_Starts

=== Now_The_Work_Starts

>>> PlayAudio: Nanobots, Story/Tutorial, Beat 5 Now the work starts

OK, it's time to face the real enemies. We'll provide links to zones where we have gathered intelligence.

Be sure to check the details of each zone. Balance risk and reward. And remember you can always revisit earlier zones.

Let's get to work!

-> END