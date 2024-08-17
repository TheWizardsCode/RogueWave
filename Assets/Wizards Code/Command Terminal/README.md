# Command Terminal

Easily add a command terminal to your game and execute command. Can be used to aid testing, debugging, or just to add a fun easter egg to your game.

Caveat, as currently written these instructions assume that you are using assembly definitions. If you are not using assembly definitions, you should be able to simply delete the assembly definition in this project and it will work. This is untested though, we recommend you add assembly definitions to your project.

## Getting Started

This section provides a quick overview of how to get started with Command Terminal. For a more detailed explanation, see the [Complete Example](#complete-example) section.

Create an empty game object and add the Terminal component to it. You can also add the CommandTerminal component to an existing game object.

Command Terminal comes with a few useful commands, so we will start with those, later we will create our own commands. 

Play your scene. Now you can open the terminal by pressing the backtick key, alternatively press SHIFT+backtick to open a larger terminal. The key used to open the terminal can be changed in the inspector of the Terminal component.

You can now type `help` to see a list of all available commands. You can also type `help <command>` to get more information about a specific command.

Hit `TAB` to auto complete the command. If there are multiple commands that start with the same letters, you can hit `TAB` multiple times to cycle through them.

The Up and Down arrow keys can be used to cycle through the command history.

## Creating your own commands

There are two steps to including your own commands in the terminal. First you need to create a class that implements the commands, then you need to register the commands with the terminal.

### Creating a command class

There are 3 options to register commands to be used in the Command Terminal.

#### Option 1: Using the RegisterCommand attribute:

The command method must be static (public or non-public). It may not be desirable to have static methods like this, however, where you can use them this is the simplest approach.  See Option 2 below for an approach that does not require static methods. Either way we recommend creating a standalone class that wraps you game code rather than including the commands in the game code itself.

Note that these methods must provide a `CommandArg[] args` parameter, even if no arguments are expected.

As an example, the SetQuality command included by default is implemented like this:

```csharp
[RegisterCommand(Help = "Set the quality level of the game. Provide no parameters to list available levels. Provide a number to set the level. Higher numbers are higher quality.", MinArgCount = 0, MaxArgCount = 1)]
public static void SetQuality(CommandArg[] args)
{
    if (args.Length == 0 || args[0].Int < 0 || args[0].Int >= QualitySettings.names.Length)
    {
        // Iterate over the qualitysettings.names and display a numbered list of the available settings
        for (int i = 0; i < QualitySettings.names.Length; i++)
        {
            Terminal.Log($"{i} {QualitySettings.names[i]}");
        }
        Terminal.Log("Run the command SetQuality <number> to set the quality level");
        return;
    }

    QualitySettings.SetQualityLevel(args[0].Int);
    Debug.Log($"Quality level set to {QualitySettings.names[args[0].Int]}.");
}
```

`MinArgCount` and `MaxArgCount` allows the Command Interpreter to issue an error if arguments have been passed incorrectly, this way you can index the `CommandArg` array, knowing the array will have the correct size.

You can use a `Name` parameter to specify a different name for the command than the method name. This is useful if you want to use a method name that is not a valid command name, or if you want to use a different name for the command. For example:

```csharp
[RegisterCommand(Name = "GraphicsQuality", Help = "Set the quality level of the game. Provide no paramerters to list available levels. Provide a number to set the level. Higher numbers are higher quality.", MinArgCount = 0, MaxArgCount = 1)]
```

By default commands will be grouped by the class name that implements them, however, this can fail in some circumstances. If you want to group commands in a different way, you can use the `Group` parameter. For example:

```csharp
[RegisterCommand(Group = "Performance", Name = "GraphicsQuality", Help = "Set the quality level of the game. Provide no paramerters to list available levels. Provide a number to set the level. Higher numbers are higher quality.", MinArgCount = 0, MaxArgCount = 1)]
```

#### Option 2: Explicit command registration

The `RegisterCommand` attribute only works for static methods. In addition this create a dependency from your code on the CommandTerminal code, this will often be undesirable, especially if you only want the terminal to be present in the Unity Editor. To use a non-static methods, you may add the command manually as follows:

```csharp
Terminal.Shell.AddCommand("sub", Subtract, 2, 2, "Subtracts 2 numbers");
```

#### Option 3: Using a FrontCommand method:

Here you still use the `RegisterCommand` attribute, but the arguments are handled in a separate method, prefixed with `FrontCommand`. This enables you to add the `RegisterCommand` attribute to code in your main classes but provide a separate method in another class that "fronts" this class. Since the fronting method provides the parameters the `MaxArgCount` and `MinArgCount` parameters are automatically inferred and thus not needed.

For example, here we define a `ListScenes` command that lists all scenes in the build, note that:

```csharp
[RegisterCommand(Group="Scene Management", Name="ListScenes", Help = "Lists all the scenes that are available in this build.")]
public static string List()
{
    StringBuilder sb = new StringBuilder();
    for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
    {
        string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        sb.AppendLine(sceneName);
    }

    return sb.ToString();
}
```

The `FrontCommand` method is defined in a separate file called `SceneManagementCommands`:

```csharp
static void FrontCommandListScenes(CommandArg[] args)
{
    if (Terminal.IssuedError) return;

    Terminal.print(Scenes.List());
}
```

### Registering commands with the terminal

If you use the `AddCommand` method  (option 2 above) you have already registered your command with the terminal, and you can skip this section. Otherwise you need to ensure that the terminal knows about these commands. 

This means you need to add your codes assembly name(s) to the `Terminal` component inspector.

# Complete Example

In this section we will write a set of commands for a sister asset of ours, Achievements. We will create commands to unlock achievements, and to list all achievements. We will also create a command to list all achievements that have been unlocked. This will demonstrate how the CommandTerminal can be used to create a useful tool for testing and debugging.

## Create the Achievements project

As is always the case you will either create a new project, or a subfolder in an existing project to contain the Achievements code.

In my case I tend to develop these assets as part of my game development process. So I create a `Wizards Code/Achievements` folder. I will worry about turning it into an asset for packaging later. Since I use assembly definitions for all my work I also add an assembly definition to this folder and reference the `WizardsCode.ConsoleTerminal` in the project assembly.

## Create the Achievements code

It's beyond the scope of this tutorial to describe the entire Achievements code, instead we will look at the overall structure of the code, and then focus on the parts that are relevant to the CommandTerminal.

There are three basic concepts you need to understand to manage your achievements:

  * Achievement: An achievement is a goal that a player can achieve in your game. It has a name, a description, and a set of conditions that must be met in order to unlock it. It is implemented as a ScriptableObject called `WizardsCoce.Achievement.Achievement`
  * AchievementList: An achievement list is a collection of related achievements. Players can choose to pick off the achievements one at a time, ignoring the lists, or they can set their sights on completing an entire list. It is implemented as a ScriptableObject called `WizardsCode.Achievement.AchievementList`
  * AchievementManager: The achievement manager is a singleton that manages the achievements and achievement lists. It is implemented as a MonoBehaviour called `WizardsCode.Achievement.AchievementManager`

## Create the Achievements and Lists

When creating the Achievements code I started out by creating the following achievements and achievement lists:

  * Earn Achievements: Unlock a Basic Achievement, Unlock a Progress Achievement
  * Creating Achievement: Create a Basic Achievement, Create a Progress Achievement

## Create the First Command

The first command we will create is the `UnlockAchievementCommand` method. This command will unlock an achievement. It will take a single argument, the id of the achievement to unlock. In order to implement it we will need to add a `UnlockAchievement` method to the `AchievementManager` class that will unlock an achievement by id. We will also need to add an `Unlock` method to the `Achievement` class that will unlock the achievement.

The part that is unique to the CommandTerminal is the `UnlockAchievementCommand` method. This class will be responsible for parsing the arguments, and calling the appropriate methods on the `AchievementManager` and `Achievement` classes. This first version of this command will be very simple, it will just unlock the achievement, and not do any error checking. We will improve it later.

```csharp
[RegisterCommand(Help = "Unlock an achievement identified by its unique ID.", MinArgCount = 1, MaxArgCount = 1)]
public static void UnlockAchievementCommand(CommandArg[] args)
{
    if (Terminal.IssuedError) return;

    string id = args[0].String;
    AchievementManager.Instance.UnlockAchievement(id);
    Terminal.print($"Achievement unlocked: {id}");
}
```

## Testing the first command

Now that we have created the first command, we can test it. To do this we will need to add the `Terminal` components to our scene. Configure the `Terminal` to load commands from our `WizardsCode.Achievement` assembly. Then hit play.

Bring up the terminal by pressing the backtick key. Type `help` to see a list of all available commands. You should see the `UnlockAchievement` command in the list. Type `unlockachievement unlock_a_basic_achievement` (where `unlock_a_basic_achievement` is an ID of an achievement) and you should be told the achievement has been unlocked. 

## Improving the Unlock command

This works well, but how do we find the IDs for the available achievements? We could add a command to list all the achievements, but that would be a lot of output. Instead we will add a command to list all the achievements in a list. We will also add a command to list all the unlocked achievements. This will allow us to find the ID of the achievement we want to unlock.

```csharp
[RegisterCommand(Name = "ShowAchievements", Help ="List all the achievements in a list identified by an ID.", MinArgCount = 1, MaxArgCount = 1)]
public static void ShowAchievementsInListCommand(CommandArg[] args)
{
    if (Terminal.IssuedError) return;

    string listId = args[0].String;
    string achievements = AchievementManager.Instance.achievementLists.Find(x => x.id == listId).ToString();
    Terminal.print(achievements);
}
```

Now we have the problem of knowing the ID for the list. We will therefore add a similar command to list all the lists.

```csharp
[RegisterCommand(Name = "ShowAchievementLists", Help = "List all the achievement lists in the game", MinArgCount = 0, MaxArgCount = 0)]
public static void ShowAchievementListsCommand(CommandArg[] args)
{
    if (Terminal.IssuedError) return;

    Terminal.print(AchievementManager.Instance.ListsToString());
}
```

At this point we have added three commands for managing achievements to our terminal. It has only taken a handful of lines of code, over and above the implementation of the Achievement system itself. You should now know enough to be able to add your own commands to the terminal. Happy Game Development!

## Reporting Errors

The `Terminal` has a `Terminal.IssuedError` property that can be used to determine if an error has been issued. Checking this before executing your command, as shown above, will prevent you from running erroneous commands. However, this will only catch errors in the syntax of the commands issued. 

If you want to catch errors in your own code, you will need to catch them yourself, and report them with `Terminal.LogError`. For example:

```csharp
[RegisterCommand(Help = "Unlock an achievement identified by its unique ID.", MinArgCount = 1, MaxArgCount = 1)]
public static void UnlockAchievementCommand(CommandArg[] args)
{
    if (Terminal.IssuedError) return;

    string id = args[0].String;
    try
    {
        bool unlocked = AchievementManager.Instance.UnlockAchievement(id);
        if (unlocked)
        {
            Terminal.print($"Achievement unlocked: {id}");
        }
        else
        {
            Terminal.print($"Progress registered: {id}");
        }
    }
    catch (System.ArgumentException)
    {
        Terminal.LogError($"No achievement with id {id} found.");
    }
}
```
