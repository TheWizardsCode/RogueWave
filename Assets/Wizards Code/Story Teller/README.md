Story Teller is a Unity asset to support story elements in many types of game. 
It is not a visual novel engine, nor is it a dialogue system. 
It is an engine for the telling of stories, which can include dialog and which may be visual novels, 
but they may also be embeded as tutorials or story beats in any other kind of game.

This is asset is intended to be used by coders to deliver the writers work within their game.
It uses the fabulous [Ink](https://github.com/inkle/ink), and open source scripting language for writing interactive narratives, to
script the story and provides tools for telling those stories within Unity.

## Asset Contents

This asset contains the following primary components:

* [ink-unity-integration](https://github.com/inkle/ink-unity-integration) - an API for playing Ink scripts in Unity
* [Inky](https://github.com/inkle/inky) - a Windows editor for Ink files
* Story Teller - a set of extensions and tools for integrating Ink into projects

## Ink Extensions

This asset includes some extensions to Ink that are designed to make it easier to integrate Ink scripted
stories into Unity games. These come in the form of special "Directions" entered into the Ink script. We
called them Directions because, for the most part, they are akin to a directors instructions for the
telling of the story.

To write a direction you use the format:

```
>>> DIRECTION_NAME: PARAMETER1, PARAMETER2, PARAMETER3 ...
```

The available directions are listed below with more details in the following sections.

  * Audio - play an audio file
  * Execute - execute a method on a component of a named object.
  * Wait For - wait for an amount of time or for an actor to reach a given mark

### Audio

Play an audio file.

Direction:

`audio`

Parameters:

* SOURCE: is the name if the game object from which the sound will be played (must have an AudioSource)
* TYPE: is an arbitrary name that allows you to organize your audio files on disk (see below).
* NAME: is the name of the actual clip file
* LOOP: [OPTIONAL] if 'true' the sound will loop. Any other value will be interpreted as false. Defaults to false.

Notes:

The audio file must be stored in a folder called `Resources/Audio` and will have a path of `TYPE/NAME`. The type can 
have `/` within it to create a hierarchy of folders.

Example:

```
>>> audio: Narrator, Spoken/Backstory, Welcome 
```

This will use an audio source on an object called "Narrator" to play a file stored in `Resources/Audio/Spoken/Backstory/Welcome.wav` 
(note the file type can be any supported type and the extension should not be included in the `NAME` parameter).


```
>>> audio: Environment, Ambiance, Factory, true 
```

This will play a looping audio file stored in `Resources/Audio/Ambiance/Factory.wav` via an audio source on a game object called `Environment`.

### Execute

Execute a method on a given component on a named object.

Direction:

`execute`

Parameters:

* OBJECT_NAME.COMPONENT_TYPE.METHOD_NAME - the signature of the method to be executed.

Example:

```
>>> execute: NavControls_Root.RogueWaveRootNavControls.OnClickNewGame
```

Execute the method `OnClickNewGame` on the `RogueWaveRootNavControls` of the object named `NavControls_Root`.

### WaitFor

Wait for a an actor to have reached their move target or wait for a duration (in seconds)

Direction:

`WaitFor 5`

Parameters:

* Actor: the name of the actor we are to wait for
* Duration: the duration of the wait, in seconds.

Examples:

```
>>> WaitFor: Billy
```

This will wait for a game object called `Billy` to reach their current destination.

```
>>> WaitFor: 5
```

This will wait for 5 seconds.
