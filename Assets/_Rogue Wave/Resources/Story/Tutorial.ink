// Variables
VAR runCount = 0
VAR deathCount = 0
VAR portalCrystalsCollected = 0

// Event driven actions
>>> ResumeOnSceneLoad: RogueWave_ReconstructionScene, Reconstruction

// The Start
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 0 Welcome

All your team are dead.

You are stranded.

We, your nanobots, can keep you alive, but we can't get you home without a portal generator.

To build the portal generator we need resources.

Let's get started by seeing if you can remember who you are, or maybe pick a new identity for yourself.

+ [New Identity] -> New_Identity

=== New_Identity

>>> execute: NavControls_Root.RogueWaveRootNavControls.OnClickNewGame
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 0 We are a part of you

We are a part of you. The interface you see in front of you is controlled by us.

We just clicked the "New Identity" button and suggested a name for this identity.

You can change the name if you want.

When ready click "Create Identity & Start".

+ [Create Identity & Start] -> Create_Identity_And_Start

=== Create_Identity_And_Start

>>> execute: MainGamePanel_CreateProfile.CreateNewProfilePanel.OnClickCreateProfile
>>> HideStoryUI:
>>> WaitForSceneLoad: RogueWave_CombatLevel
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 1 Introduction Part 1

We can "Nanotransfer" you into zones across the planet to gather the resources we need.

Some of us will stay here, where it is safe, others will come with you to help in the field.

Those that stay can keep a link open to you for 5 minutes. After that amount of time we will Nanotransfer you back here.

+ [Scary!] -> Understanding_Nanotransfer

=== Understanding_Nanotransfer

>>> PlayAudio: Nanobots, Story/Tutorial, Beat 1 Introduction Part 2

When you are ready we'll send you out there to take a look around.

At some point the locals are bound to get the better of you. Don't worry we will perform an emergency transfer. There is a cost to this, but at least you will be alive.

Those of us that come with you will form a particle cloud that looks like a person. We'll try to keep you safe. Keep an eye on us, we'll communicate information to you. Don't worry about us though, we can't be harmed by the natives.

You should focus on keeping yourself safe and gathering tne resources we need to get out of here.

+ [Nanotransfer In] -> First_Nanotransfer

=== First_Nanotransfer

>>> AudioMixer: Audio/RogueWave_AudioMixer, EffectsVolume, 0
>>> execute: Rogue Wave Game Mode.RogueWaveGameMode.GenerateLevel
>>> PlayAudio: Nanobots, Story/Tutorial, Beat 2 Zones and Run Upgrades

This is a safe zone but resources here are a little scarce.

Some of the flora emits poisonous spores if disturbed and there are a few roaming animals that may attack. But we've detected no strong enemies here.

As you gather resources we will be able to construct upgrades. Listen out for notifications and watch your HUD for details.

Once you have gathered 500 resources we'll bring you back for some more permanent upgrades.

+ [I'm Ready] -> In_Run

=== In_Run

~runCount = runCount + 1

>>> HideStoryUI:
>>> Execute: Level Menu(Clone).LevelMenu.Spawn
>>> AudioMixer: Audio/RogueWave_AudioMixer, EffectsVolume, 1
>>> ResumeOnSceneLoad: RogueWave_HubScene, In_Hub

    + [Collect Moisannite] -> Collected_Moisannite
    + [Survive] -> In_Hub
    + [Die] -> Reconstruction

=== In_Hub

{portalCrystalsCollected == 0:
    >>> PlayAudio: Nanobots, Story/Tutorial, Beat 3a Special Crystals and Permanent Upgrades

    We now have resources and can make you stronger. Any upgrades you select here in the Hub are permanent. That is. they'll survive an emergency extraction.
    
    However, to build the portal generator we need specific kinds of crystal.
    
    When you feel strong enough we'll send you to a new zone that contains Moisannite. We'll put a marker on it to make it easier for you to collect.
    
    + [Let's Go!] -> In_Run
-else:
    -> Start_Real_Work
}

=== Collected_Moisannite

// This knot it ONLY used in the text version of the tutorial. 
// It is here for debugging only.
// When played in the FPS version the value of portalCrystalsCollected is set by the game.

~ portalCrystalsCollected = 1

Great, you have your first crystal for the portal. Many more to go.

-> In_Hub

=== Start_Real_Work

>>> PlayAudio: Nanobots, Story/Tutorial, Beat 5 Now the work starts

We'll provide links to zones where we have gathered intelligence.

Be sure to check the details of each zone. Choose your path and balance risk and reward. 

Remember you can always revisit earlier zones.

+ [Go!] -> The_End

=== Reconstruction

~ deathCount = deathCount + 1

{deathCount == 1:
    >>> execute: Level Menu(Clone).LevelMenu.Spawn
    >>> AudioMixer: Audio/RogueWave_AudioMixer, EffectsVolume, 1
    >>> ResumeOnSceneLoad: RogueWave_HubScene, Portal_Crystals, true
    >>> PlayAudio: Nanobots, Story/Tutorial, Beat 3b Reconstruction

    We had to pull you out, you were too close to true death.

    Emergency extractions only bring the essentials, you, the Nanobots with you and any resources you have collected. Everything else has to be left behind.
    
    While we reconstruct you take a look at your stats here. They can be useful in figuring out where you are strong or weak.
    
    + [Try Again] -> In_Run
-else:
    >>> HideStoryUI:
    
    {runCount == 1:
        + [Survive] -> In_Hub
    -else:
        + [Survive] -> Start_Real_Work
    }
}


=== The_End

>>> HideStoryUI:

-> END


