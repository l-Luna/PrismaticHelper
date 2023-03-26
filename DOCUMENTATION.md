# Prismatic Helper

## Entities

### Multi-lock door
Multi-lock doors are doors that require multiple keys (at least one) to unlock. Keys are only consumed when all of the required keys are collected by the player, and the player is within 60 pixels of the door and has a line of sight to its centre.

### Custom Power Source Number
These act like the indicators in Farewell's *Power Source* checkpoint, that light up when you collect certain keys. You can specify a texture to use as its background and one that should be it up, as well as activate it even when lightning is present.

### Attached Watchtower
Watchtowers that attach to solid blocks below them. Intended for use with Floaty Space Blocks or other non-gameplay entities.

### Cassette Kevin
These Kevins can be hit on their allowed sides like normal, but only begin to charge in their direction when the cassette blocks corresponding to their colour turn on. In the meantime, they can be hit any number of times to change their next direction.

Note that you must include at least one cassette block in rooms with cassette Kevins for them to ever activate. Since Bright Sun (yellow) and Malachite (green) blocks are only given turns if they exist in the current room, you'll need to include those blocks if you want to use correspondingly coloured Kevins.

### Boombox
Boomboxes activate on cassette block beats corresponding to their colour, creating an explosion similar to pufferfish/bumpers/respawning seekers. They can be attached to solids below them.

Big Boomboxes give exactly the same boost as other entities. Small ones:
- don't refill your dash or stamina
- provide less speed
- have a smaller radius
- don't provide a puffer/bumper boost
- don't create a dash cooldown.

Like Cassette Kevins, you must include cassette blocks in the room for these to activate, of at least the same index.

### Stylegrounds Panels
These display the stylegrounds of another room, restricted to an area; by default rectangular, but you can specify a mask image to use another shape. You may also specify a tint and opacity (which work on top of a mask, if present), attach them to entities, and modify their parallax.

### World Panels
These display another room, restricted to an area. The options here are identical to Stylegrounds Panels.

The room will always be viewed from its default spawn point (which can be forced using an option on Spawn Points). The player in the room will be unable to react to input, and should generally be placed offscreen or in solid tiles. Allowing the confined player to die or move between rooms is Undefined Behaviour - AKA, don't do that.

For now, World Panels always appear in the foreground, ignoring the Foreground option. This will be fixed in a future release.

## Cutscenes
Prismatic Helper allows you to use custom cutscene triggers in dialog, which allow you to include some fancy effects in your regular dialog. The available triggers are limited to what is defined by Prismatic Helper (or other mods that add triggers), and more complex cutscenes may require Lua Cutscenes or C# code.

The syntax for custom triggers is `{ph_trigger id param_1 param_2 ... param_n}`, where `id` is the ID of the trigger you're running, and the parameters are space seperated strings. You may specify only as many paramaters as you want to use; all triggers have default values for all parameters. All parameters are positional; to specify a parameter, you must specify all parameters before it.

Example: `{ph_trigger camera_zoom}`, equivalent to `{ph_trigger camera_zoom 2 3 cube}`.

You can also use `&ph_trigger` to make a "silent" trigger, which keeps the textbox open while the trigger activates. The text does *not* continue until the trigger ends. Using `~ph_trigger` makes a "concurrent" trigger, which runs seperately to the cutscene, with text and other triggers still continuing.

To run a trigger after a cutscene is skipped, use `{ph_on_skip ...}`. On-skip nodes are only run when the cutscene is skipped, so gameplay effects should be present as both cutscene triggers and on-skip nodes.

(If a cutscene does not exist in the current language, but exists in English, then the English version is already used, and Prismatic Helper triggers will still work.)

### Triggers:

#### `walk, amount = 8`
Make the player walk the specified number of pixels to the right. 1 tile = 8 pixels, negative values go left.

#### `run, amount = 8`
Make the player run the specified number of pixels to the right. 1 tile = 8 pixels, negative values go left.

#### `walk_to, x = 8`
Make the player walk until they reach the given x position, relative to the map origin. Useful for lining the player up with a specific pixel

#### `run_to, x = 8`
Make the player run until they reach the given x position, relative to the map origin. Useful for lining the player up with a specific pixel.

#### `goto, x = 0, y = 0`
Instantly teleports the player to the specified position in pixels, relative to the map origin. (This is intended for on-skip nodes, to normalize the player's position after skipping a cutscene.)

#### `look, direction = left`
Make the player look in the specified direction, either `left` or `right`. Other values are assumed to be right.

#### `camera_zoom, zoom = 2, duration = 3, easer = cube`
Zooms the camera to the specified zoom level, with higher values zooming in, over the specified number of seconds. Zooming out further than `1` won't render outside of the regular screen size. The `easer` controls how the zoom progresses during that time.

Note that the camera will not automatically zoom to the normal scale after the cutscene; you will have to reset it yourself.

#### `camera_zoom_back, duration = 1`
Returns the camera to its regular scale over the given duration.

#### `camera_pan, x = 0, y = 0, duration = 3, easer = cube`
Pans the camera by the specified number of pixels on the X and Y axis relative to its current position, over the specified number of seconds, using the specified easer to control progress.

#### `camera_pan_to, x = 0, y = 0, duration = 3, easer = cube`
Pans the camera to the specified X and Y position, over the specified number of seconds, using the specified easer to control progress.

#### `attach_camera_to_player`
Reattaches the camera to the player after panning the camera. This is done automatically when a cutscene ends, and this is only needed when moving the player after panning the camera.

#### `wait_for_ground`
Waits for the player to touch the ground before continuing the cutscene.

#### `disable_skip`
Disables the "Skip Cutscene" button for the duration of the cutscene. *Use sparingly*; it it strongly preferable to use `goto` to fix the player's position after a cutscene, and other on-skip nodes to ensure the player is in a valid state after skipping,
rather than disabling it outright.

#### `hide_entities, entityType = `
Hides all entities of a given type in the room. These entities are invisible, but still have collision and are interactible. The `entityType` parameter should be the short name of the corresponding C# class, like `Booster`, `Player`, `LockBlock`, or `Bird`.

#### `show_next_booster`
Reveals the first invisible booster in the room, or does nothing if no boosters have been hidden.

#### `show_next_door, soundIndex = 1`
Reveals the first invisible door in the room, or does nothing if no doors have been hidden. Only Farewell's doors have a reveal animation by default, but other doors can be given one with a custom `Sprites.xml`. `soundIndex` changes the sound used when the door appears; should be between 1-5 inclusive.

#### `player_animation, anim = idle, mode = start`
Makes the player play the specified animation. See `Sprites.xml` for the available animation. If `mode` is `play`, the cutscene waits for the animation to end; otherwise, the animation continues while other triggers and dialog occurs.

#### `player_inventory, inventory = Default`
Sets the player's inventory to the specified inventory. (Note that you will need to also add `{ph_on_skip player_inventory ...}` if you want the inventory to be set on skipping the cutscene.)

#### `baddy_appear, xOffset = 0, yOffset = y`
Makes Badeline appear at the specified position relative to the player. Only one Badeline can be made by these triggers; attempting to summon another will make the previous one vanish.

#### `baddy_split, xOffset = 0, yOffset = y, facePlayer = true`
Makes Badeline split from the player, and move to the specified position relative to them. Only one Badeline can be made by these triggers; attempting to summon another will make the previous one vanish. Does not affect the player's inventory or hair colour.

#### `baddy_float_to, x = 0, y = 0, look = true`
Makes Badeline float to the specified position in map coordinates, optionally facing in the direction they move.

#### `baddy_float_by, x = 0, y = 0, look = true`
Makes Badeline float to the specified position relative to their current position, optionally facing in the direction they move.

#### `baddy_float_by_player, x = 0, y = 0, look = true`
Makes Badeline float to the specified position relative to the player's current position, optionally facing in the direction they move.

#### `baddy_look, direction = left`
Makes Badeline look in the specified direction, either `left` or `right`. Other values are assumed to be right.

#### `baddy_combine`
Makes Badeline float towards the player and combine, disappearing. Does not affect the player's inventory or hair colour.

#### `baddy_vanish`
Makes Badeline vanish in-place.

#### `baddy_animation, anim = idle, mode = start`
Makes Badeline play the specified animation. See `Sprites.xml` for the available animation. If `mode` is `play`, the cutscene waits for the animation to end; otherwise, the animation continues while other triggers and dialog occurs.

#### `set_flag, flag = , value = true`
Sets the flag with the given name to the given value (`true` or `false`).

#### `run_playback, recording =`
Makes the player act out the given playback recording, starting from their current position. Collision with solids is disabled during playback, as the player follows the recording exactly.

### Parameter values

#### Easers
The available easers are: `linear`, `quad`, `cube`, `quint`, `exp`, `back`, `big_back`, `elastic`, and `bounce`. All of these except `linear` have an `_in` and `_out` version.

#### Inventories
The available inventories are: `Default`, `CH6End`, `Core`, `OldSite`, `Prologue`, `TheSummit` and `Farewell`.