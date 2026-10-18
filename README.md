# Enemy Highlighter

A Mount & Blade II: Bannerlord mod (v1.4.8) that puts a coloured outline on every enemy near you when you hold a key, or toggles it on and off with one press. It works in field battles and sieges, hideouts, arena fights and tournaments, and fights in towns, castles and villages (alley fights, prison breaks, lord's hall fights). Only enemies within the distance you set get an outline, so troops far across the field don't clutter the screen.

## How it works

- **Hold mode** (default): hold the highlight key (Left Alt by default) and every enemy within range gets an outline. Let go and the outlines disappear.
- **Toggle mode**: press the key once to turn the outlines on, press it again to turn them off. A short message tells you which. The toggle happens when you let go of the key, so Alt+Tab and Alt+F4 never flip it. You can have it stay on from one scene to the next.
- **Any key**: pick the highlight key in Mod Options. Every keyboard key and the extra mouse buttons are in the list.
- **Maximum distance**: only enemies closer than this (60 m by default, 5 to 300 m) are outlined. It is measured from you, or from the camera once you're knocked out.
- The outline uses the game engine's own outline effect and is updated ten times a second. Enemies lose their outline the moment they die, are knocked out, leave your range or stop being hostile.
- An enemy is anyone on a team hostile to yours, so arena practice fights (everyone against everyone), tournament teams and town fights all work the same way.
- Leaving the game window (Alt+Tab) never leaves the outlines stuck on: after you come back, the key counts again once you've let go of it.
- Stealth missions (the disguise mission, prison breaks, the sneaking part of a hideout attack) get no outlines unless you allow them, and outlines hide during cutscenes and photo mode.
- In big battles only the nearest enemies get an outline (100 by default), to keep the frame rate up.
- The mod only removes outlines it put on itself, and puts back the game's own outline on the person you're looking at.
- If your highlight key is also bound to one of the game's controls, a message says so once per session.
- It only draws outlines. Nothing about AI, combat or your save is changed, and you can add or remove the mod at any time.

Left Alt is also the game's Show Indicators key, so your own troop banners show at the same time. Careful while holding Left Alt: Alt+F4 closes the game, and F4 is a battle order key. Another key avoids that.

## Options (Mod Options, MCM)

| Option | Default | What it does |
|---|---|---|
| Enable | On | Turns the mod on or off. |
| Highlight key | Left Alt | The key that shows the outlines. |
| Toggle mode | Off | On: press to turn on, press again to turn off. Off: outlines show while you hold the key. |
| Keep toggle between scenes | Off | In toggle mode, outlines that were on stay on in the next scene. |
| Message when toggled | On | In toggle mode, a line in the message log each time the outlines turn on or off. |
| Warn about key conflicts | On | Once per session, a message if the highlight key is also bound to a game control. |
| Maximum distance | 60 m | Only enemies closer than this get an outline (5 to 300 m). |
| Most outlines at once | 100 | Only the nearest this many enemies get an outline. 0 = no limit. |
| Outline colour | Red | Red, orange, yellow, magenta, cyan, white, or one of two colour-blind friendly colours. |
| Show through walls | On | Off: only the visible parts of an enemy get an outline. |
| Outline fleeing enemies | On | Off: enemies running away get no outline. |
| Outline enemy horses | Off | Also outline the horse an enemy is riding. |
| Battles and sieges | On | Field battles, sieges, village raids and custom battles. |
| Hideouts | On | Hideout raids, including the bandit leader fight. |
| Arenas and tournaments | On | Arena practice, duels and tournament rounds. |
| Towns, castles and villages | On | Alley fights, lord's hall fights and anyone hostile to you in a settlement. |
| Stealth missions | Off | Disguise mission, prison breaks and the sneaking part of hideout attacks. |
| Hide during cutscenes | On | No outlines during cutscenes and in photo mode. |
| Debug messages | Off | Shows chat messages about what the mod is doing, for testing. |

All options take effect right away, also in the middle of a fight.

## Requirements

- Mount & Blade II: Bannerlord v1.4.8, single player
- [Harmony](https://www.nexusmods.com/mountandblade2bannerlord/mods/2006), [ButterLib](https://www.nexusmods.com/mountandblade2bannerlord/mods/2018), [UIExtenderEx](https://www.nexusmods.com/mountandblade2bannerlord/mods/2102) and [Mod Configuration Menu v5](https://www.nexusmods.com/mountandblade2bannerlord/mods/612)

## Installing

1. Install the four requirements above.
2. Download the zip from the [Releases](../../releases) page and extract it into your Bannerlord `Modules` folder, so you end up with `Modules\EnemyHighlighter\SubModule.xml`.
3. In the launcher's **Mods** tab, tick **Enemy Highlighter** and make sure it loads after Harmony, ButterLib, UIExtenderEx and Mod Configuration Menu.
4. If the launcher says the DLL couldn't be loaded, Windows blocked the downloaded file: right-click `Modules\EnemyHighlighter\bin\Win64_Shipping_Client\EnemyHighlighter.dll`, choose **Properties**, tick **Unblock** and click **OK**.

The options are under **Options > Mod Options > Enemy Highlighter** in game.

## Compatibility

- Safe to add to or remove from a running campaign: nothing is stored in your save.
- Multiplayer is not affected; the mod only runs in single player.
- Other mods that outline characters (for example ones that outline lords and companions) can run alongside it. When both outline the same soldier, whichever set the colour last wins, and letting go of the highlight key can remove the other mod's outline from an enemy until that mod sets it again.

## Building

You need the [.NET SDK](https://dotnet.microsoft.com/download) (any recent version; the project targets .NET Framework 4.7.2 through the `Microsoft.NETFramework.ReferenceAssemblies` package, so no Visual Studio is required) and Bannerlord with the four requirements installed, because the project references their DLLs.

From the `src` folder run `dotnet build -c Release`. The build copies the dll, `SubModule.xml` and `ModuleData` into `Modules\EnemyHighlighter` in the game folder. If the game is installed somewhere other than the default Steam folder, pass `/p:GameFolder="..."`.

## Translations

All English text is in the code as `{=id}English text`. `Module/ModuleData/Languages/std_EnemyHighlighter.xml` lists every string and is the template for translations.

To translate the mod, for example into German:

1. Create the folder `Module/ModuleData/Languages/DE/`.
2. Copy `std_EnemyHighlighter.xml` into it as `std_EnemyHighlighter-de.xml` and change `<tag language="English"/>` to `<tag language="Deutsch"/>`.
3. Translate each `text` attribute. Keep every `id` and every `{VARIABLE}` exactly as it is.
4. Add `language_data.xml` next to it:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <LanguageData id="Deutsch">
     <LanguageFile xml_path="DE/std_EnemyHighlighter-de.xml"/>
   </LanguageData>
   ```

The language ids are the game's own; see `Modules/SandBox/ModuleData/Languages/<folder>/language_data.xml` for the one to use. Pull requests with translations are welcome.

## Credits

This mod was heavily inspired by [Highlight Enemies in Battle (Red Enemy Outline)](https://www.nexusmods.com/mountandblade2bannerlord/mods/13705) by offtheclock. I wanted a little more control: holding or toggling the key, choosing the key, and using the outlines in towns, arenas and hideouts too, with the settings in Mod Configuration Menu. That meant depending on the usual extra libraries (Harmony, ButterLib, UIExtenderEx, MCM), so this is a separate mod written from scratch rather than a change to theirs. If you'd rather have no dependencies at all, use theirs.

## License

Public domain, under [The Unlicense](LICENSE). Copy it, change it, reupload it, use it in your own mod or modpack, with or without credit: anything goes.

## Support

If you enjoy the mod, you can support me on [Ko-fi](https://ko-fi.com/darkjeffrey).
