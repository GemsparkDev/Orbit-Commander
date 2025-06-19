# Lagrange Commander  
  
Lagrange Commander is a game made using C# in monogame. It is a 2D top down wave based shooter where you must repair and upgrade your ship by salvaging from enemies you defeat. It features realistic N-Body gravitation, which affects all entities and projectiles.
  
# Changelog  
  
10/3/2022: Current content at time of commit  
Alpha release 1  
	-Basic engine.  
	-Player.  
 	-Basic round.  
  	-Fighter.  
  	-Scrap.  
  	-Various sounds (death, firing and hitting).  
  	-Alpha sprites for various entities.  
  	-Wave timer.  
  	-Debug mode.  
  
10/16/2022   
Update 0.0.1  
	-Docking with motherdrone.  
  	-Mother drone storage.  
 	-Player can carry 1 material at a time and input in motherdrone.  
 	-Can move motherdrone when docked.  
	-Velocity clamp for various entities.  
   
10/17/2022  
Update 0.0.2  
  	-Velocity arrow for player.  
  	-Leashed scrap stays near the player.  
10/18/2022  
 	-Added Git Repository.  
	-Updated to Visual Studio 2022.  
	-Changed name to “Space Wars”.  
   
 10/21/2022  
 Update 0.0.3  
  	-Added Myra support and references.  
  	-Added basic framework for creating UI boxes.  
 10/23/2022   
 	 -Pause menu with quit button.  
  	-Mothership resource menu display.  
 11/25/2022    
  	-resource amounts are no longer able to be changed directly from the menu.  
  
1/16/2023   
Update 0.0.4  
	-Added cruiser drone.  
	-Fixed issue #3.  
        
1/22/2023  
Update 0.0.5  
	-Started implementation of the wave system.  
	-Made player turn towards the mouse.  
1/29/2023:  
        -fixed bug #4.  
	
2/1/2023  
Update 0.0.6  
        -Added background.  
	-Changed framework to .net 6.0.  
	-Begun implimentation of the module system. 
         
 6/24/2023   
 Update 0.0.7  
         -New GUI library and font.  
         -2 new menus, pause menu and main menu.  
	 -Better debug menu, w/ debug log.  
	 -Various utility functions such as player respawn, sound falloff, and better distance function.  
  	 -Fixed issues #1 and #2.  
  
6/28/2023   
Update 0.0.8  
   	 -Various new UI and Sound assets.  
     	 -New enemy type (sniper).  
         -New player weapon type (shotgun).  
	 -New UI type (Draggable icon).  
  	 -A set of player health statistics that affect player abilities.  
         -Repair system.   
	 -various other code improvements.
    
7/29/2023  
Update 0.0.9  
   	 -Added karma based drops - after a period of bad luck, drop chance is increased.  
   	 -Added enemy health bars.  
   	 -Added a mothership inventory and a furnace to refine scrap. If the inventory is full, any scrap brought to the mothership is released.  
   	 -Scrap no longer directly increases how much material you have available, instead it must be refined first.  
   	 -Made motion frame independent.  
   	 -Added the spiral shot weapon for the player.  
   	 -Changed the graphics to be a black background with neon colored entities.  
   	 -Changed the IconSlot UI element to the ItemSlot UI element, and made the module inherit from the Item class.  
   	 -Temporarily removed the module health square.  
   	 -Added the TabbedWindow and Slider UI elements.  
   	 -Changed the implimentation of the GetWidgetOver function, so that if there are two applicable widgets, it picks the one closest to the cursor.  
   	 -Added multiple new UI assets.  
   	 -Added the EventHandler class, which manages the interactions between the UI and the game.  
   	 -Changed sound volume dropoff to be linear.  
8/3/2023  
	-Added the Item class and the Module class, which inherits from the item class.  
	-Static item return class for constant item creation.  
	-Fixed bug #6.  
    
8/14/2023  
Update 0.1  
	-Added various assets, such as an improved mothership sprite.  
	-Removed the sentry due to technical debt.  
	-Increased the range of the fighter enemy by 50 units.  
	-Added a sound manager for playing, pausing, and looping sounds.  
	-Updated the ClampVelocity() function to be the same in all directions.  
	-Added two celestial bodies to the game, a large earth like planet and a dense moon orbiting it.  
	-Added the GravitationalSource class, which creates a planet at a specific location with a radius, gravitational strength, and the ability to host moons.  
	-Added a currently WIP crafting system.  
	-Changed how modules work, with the player having a grid of delegate functions that the module of a specific type and weapon id can select with it's ability.  
	-Completely removed the MothershipArrow child class from the game.  
	-Added a basic particle system to the game.  
	-Added support for container transparency.  
	-Items now quickly glide to a stop.  
	-Added the GameState class and it's context class, CurrentGameState, which determines what managers are allowed to update and when.  
	-Split the EntityManager.Update() function into EntityManager.Update() and EntityManager.PlayerUpdate().  
	-Added the garage, which provides an environment that allows module repair and reslotting. 
	-Fixed bug #8.  
	-Added the WIP interface IAnimatable.  
    
8/18/2023  
Update 0.1.1  
        -Changed the enemy range to be much more reliable and frame independent.  
        -Further buffed the fighter enemy.  
        -Added the missile enemy.  
        -Increased the range of the carrier enemy, and replaced the fighter spawn with a new missile enemy.  
        -Added a one frame particle and particle emitter, which currently draws a circle when spawned.  
        -Fixed the trajectory of the player to be almost completely accurate for the current system.  
        -Changed how the trajectory was rendered, including a variety of quality of life changes.  
        -Added the ability of the trajectory to automatically render based on the location of any moon if patched conics is enabled.  
        -Removed the ability to calculate the trajectory of a celestial body.  
        -Moons now attract each other, but is not reflected in the player trajectory currently.  
        -Added a proper victory condition based on spending 25 scrap on fixing your mothership.  
        -Added a 1 second invincibility cooldown when damaged, and a 0.5 second cooldown for dashing.  
        -Added an upward velocity when undocked.  
        -Added proper UI scaling.  
        -Added an enemy credit system when spawning enemies.  
        -Changed a variety of the buttons to use a wider button sprite.  
        -Changed the main menu to be a tabbed window.  
	  
10/7/2023   
Update 0.2   
	-Polished the UI and various entity sprite assets, including icons for tabs.  
	-Added a small selection of sounds.  
	-Increased the chance of dropping scrap significantly for all enemies.  
	-Increased the player inventory by one slot.  
	-Decreased the spread and damage of the shotgun.  
	-Added a maximum range to the player, and an arrow that points to the mothership.  
	-Swapped the smelting tab and the garage tab.  
	-Expanded the size of the garage menu significantly.  
	-Added a wave counter to the player menu.  
	-Changed the name of the game from Space Wars to Lagrange Commander.  
	-Added a proper title screen, and expanded the victory screen to include your time.  
	-Added the training simulator mode.  
 	-Fixed bug #10.  
	-Fixed bug #11.  
	
2/5/2024
Update 0.3
	-Updated and added various sprites, including the cursor, projectiles, and several entities.  
	-Added two new bosses, Symmetry and Overload. Symmetry fires a burst shot that gets faster as it's health decreases, and occasionally shoots 4 missiles in a cross pattern. Overload chases and tries to touch the player with several shields, occasionally shooting a barrage of bullets. When all 4 shields are destroyed, Overload is vulnerable for 7.5 seconds before regenerating all 4 shields. Either boss can spawn on wave 20.  
	-Changed the texture and sound system to use an enum that refers to a dictionary as opposed to a reference to the texture / sound.  
	-Increased the max amount of projectiles to 150.  
	-Increased the mass of the primary planet to 15000.  
	-Added the energy system, which is consumed by various abilities and functions of the player. It regenerates slowly when usage stops.  
 Hotfix 0.3.0.1  
	-Fixed bug #13  
 	
4/30/2025 
Update 0.4
	-Added simple component system to entities.  
	-Added screen shaking.  
	-Updated the fighter enemy code to allow for allying with the player.  
	-Added the mission system, allowing for more advanced levels.  
	-Symmetry now shoots more missiles when at low health.  
	-Overload can now perform a charge attack.  
	-The mothership is now an enemy.  
	-Added the turret cannon.  
	-Added the hovercraft.  
	-Added the advanced fighter.  
	-Added the excursion and wyvern bosses.  
	-Added the stealth system, which determines what you and enemies are capable of seeing.  
	-Added the ringed planet.  
	-Modified the collision system for planets to be easier to interact with.  
	-Added the pickup system, allowing for conversion of ingame entities and player modules.  
	-Added the module restarting system, which causes modules to randomly fail and stop functioning when the player is hit.  
	-Added the sniper, shotgun, silenced, and LMG weapon types.  
	-Added the shield ability.  
	-Added the assassin projectile type.  
	-Moved the UI classes into their own library.  
	-Added music.  
 	
5/3/2025   
Update 0.4.0.1  
	-Planets no longer have moons, and every planet can attract every other planet.  
	-Fixed bug #9  
        
5/4/2025    
Update 0.4.1    
    -Added two new entity components, currently not in use.    
    -Added simple cutscene system with demo intro cutscene for first mission.    
    -Fixed bug #7.    
    -Fixed bug #14.    
      
5/5/2025    
Update 0.4.2    
    -Added a debug collider renderer.    
    -Modified particle system such that emitters are managed by entities.    
    -Debug velocity now renders as a single line in the direction of motion.    
    -Developed intro cutscene and dialed in cutscene backend.    
    
5/7/2025    
Update 0.4.3    
    -Added the stealth fighter, demonstrating the stealth system.    
    -Modules now store their ability rather than an integer referring to said ability. 
    
5/9/2025    
0.4.3.1    
    -EntityManager is no longer a static class.    
    -Cutscenes now use a simplified actor class instead of entities.    
    -The core module now completely disables player visuals and control when failed.    
    -Modules no longer have the cost member, as it continues to remain unusued.    
    -Completely removed the training simulator.    
    -The input class now records keyboard state as well.    
    -Fixed bug #15    
    -Fixed bug #16    
        
0.4.3.2     
5/24/2025     
    -Added random point exploration to stealth fighters.    
    -Added proper core failure logic.  
    
0.4.3.3     
5/27/2025    
    -Added an escape drone that allows the player to bring items back to the mothership on some missions.    
    -The stealth fighter now drops scrap and explodes.    
        
0.4.3.4    
5/28/2025    
    -The fuse menu has been added, which can be accessed by pressing F.    
    -The player can place and remove fuses, spending their spare fuses.
    -The player starts with 1 spare fuse.        
    -When a module has less than 3 fuses, it begins to work less effectively.    
    -A module failing causes one of it's corresponding fuses to break.    
    
0.4.4   
5/29/2025    
    -The player is now able to add an additional fuse to a module, increasing it's power.    
    -All fuses on a column require a corresponding core fuse to work.    
    -The sensor module now has a modification to it's sensing value based on the quantity of fuses on it.    
    -The player's sensing value is further decreased when the sensor is failed.    
    -The player no longer starts with a spare fuse.    
    -Damaging a fuse now has the added text "Fuse damaged!" instead of "Check fuses!"   
        
0.4.5    
5/30/2025     
    -Updated Monogame to 3.6.3.    
    -The fuse menu now has decals w/ modules for each row.    
    -The game now renders with a bloom shader.    
    
0.4.5.1    
5/31/2025    
    -Changed the color of the enemy health bar to green and a dark grey.    
    -The stealth dropoff range is reduced based on how many fuses are on the sensors.    
    -Added a toggle for the global shader.    
    -Significantly reduced the strength of the shader.    
   
0.4.5.2        
6/1/2025        
    -Fixed a potential issue with docking to an expired entity.    
    -The waves are now slightly longer.    
    -When a wave ends, scrap still on the field will periodically take decay damage.    
    -Restarting a module restores a couple health points to the corresponding module.    
    -Inlined the ControlShip function.    
    -Missions now use the EntityConstructor class.    
    -The fuse menu and the garage menu now update the game state.    
    -Fixed bug #18
    
0.4.5.3    
6/2/2025    
    -Rearranged mission select menu.    
    -Modified what actions fall under player update and ingame update.    
    -Fixed bug #19.    
    
0.4.5.4    
6/3/2025    
    -Mission select game state now handles all mission selection UI.    
    -Added every current mission to the mission select menu.    
    -The system change button cycles through all 3 systems.    
    -Mission planets now properly orbit their sun.    
    -Selecting a mission now only requires hitting the path rather than the planet itself.    
    -Missions have colors that change depending on whether they are active, inactive, and selected.    
    
0.4.6    
6/4/2025        
 - Added a small ship icon to the mission select menu.    
 - Rearranged various global modules in code.    
 - Added the SaveGame class, allowing for modular saves and data storage between missions.    
 - Added a UI Scale slider.    
 - Added inventory slots and a module config checker to the mission select menu.    
 - The mission select menu modules now properly syncs to the player.    
 - The crash landing intro cutscene now fails the player's core module.    
 - Fixed bug #17.
    
0.4.6.1    
6/5/2025    
 - Added the framework for the crafting queue system.    
 - Moved the information on completed missions to the SaveGame class.    
 - Inlined the MarkComplete Function.    
 - Fixed bug #20.    
    
0.4.6.2    
6/6/2025    
 - Added buttons to queue items for crafting.    
 - Added a mission select inventory to store items between missions.    
 - Added a small image showing every queued item.    
    
0.4.6.3    
6/7/2025    
 - Fixed an issue with rendering scaling with different resolutions.    
 - Added a remove queue item button.    
 - Smelting and repairing now requires the player to click the button with the pickup.    
 - Setting sliders now update the value stored.    
 - Fixed bug #21.    

0.4.6.4    
6/8/2025    
 - Added the grappling hook module with a cooldown of 5 seconds.    
 - The grappling hook can attach to entities or planets, and will drag the player toward it if they get too far.    
 - Moved the ItemFactory class into it's own file.    
 - The ScreenSize parameter now defaults to 1920 by 1080. Use the new BackBuffer parameter to get monitor size.    
 - Windows now have alignment fields which affect how they scale with UIScale.    
 - Mission now has a general IsColliding function for every planet.    
    
0.4.6.5    
6/9/2025    
 - Removed the particle transparency and particle fades out fields.    
 - Particles now use the alpha channel to control transparency.    
 - Docking with the ship now disables ability summons (like the grappling hook).    
 - The grappling hook line is now dashed.    
 - The coloration of the grappling hook line becomes more extreme when the player is near the max range.    
 - Fixed bug #22.    
    
0.4.6.6    
6/10/2025    
 - Added the hunter enemy.    
 - The hunter enemy will try to grapple the player, revealing it to other enemies in the process.    
 - Added the reveal field for entities allowing enemies to have their stealth removed temporarily.    
 - The grapple hook now turns red if it's being used by enemies.    
       
0.4.6.7    
6/11/2025    
 - Added the construct type, allowing for future deployable traps.    
 - Enemies now spawn with trails.    
 - The timer now no longer counts down while there are enemies alive.    
 - Enemies now project their future positions.    
 - Inlined the SpawnWaveBatch function.    
    
0.4.6.8    
6/12/2025    
 - Added the barricade and trap construct.    
 - The barricade can take 10 hits before breaking.    
 - The trap will periodically shoot 9 bullets toward enemies nearby it.    
 - Added two buttons to construct both new constructs.    
 - Future waves now appear slightly brighter and in yellow.    
    
0.4.6.9    
6/13/2025    
 - Added proper sprites for the trap and the barricade.    
 - The trap now rotates, and the barricade always points alway from the origin.    
 - The miner and the sentry can no longer heal using constructs or modules.    
 - Increased the health of the barricade to 20, and the health of the trap to 8.    
 - The future enemy spawn predictor is now orange and flickers.
    
0.4.7    
6/14/2025    
 - Enemies now spawn in small formations.    
 - Formations will almost always be 3 or more enemies.    
     
0.4.7.1    
6/15/2025    
 - Player abilities are now tied to the core rather than the engine module.    
 - Added the EnemyDeath behavior, allowing for code reuse.    
 - Reorganized the enemy AI in code to aid organization.    
 - The player ability cooldown now properly displays for all modules.    
 - Seperated some elements of the ModuleType enum into the new Modules enum.    
 - Some bosses now drop weapons instead of scrap: Overload - Shotgun, Symmetry - Missile, Excursion - Sniper

0.4.7.2    
6/16/2025    
 - Added a health and ability bar to the screen menu.    
 - Moved the mission index and system index to the SaveGame class.    
 - The planets in the mission select menu now start at random locations.    
    
0.4.7.2    
6/17/2025    
 - Added the bomb construct, which explodes when integrity reaches zero.    
 - Additionally, the bomb construct costs 5 scrap to create.    
 - Added the Explode function, which damages all entities in it's radius.    
 - The missile, along with all exploding entities, now use said explode function.    
 - Added the ExtraUpdates property to projectiles, which causes projectiles to get updated several times a frame.    
    
0.4.7.3    
6/18/2025    
 - Added sprites for the bomb and it's explosion.    
 - The bomb now show's it's explosion radius.    
 - The trap has an enemy detection radius shown during debug mode.    
 - The miner and the turret can rotate properly.    
 - Friendly entities create a small projectile indicating it's direction when offscreen.    
 - Added a cap on the screen shake factor.    
 - Added a mission where you are required to assalut a small mining operation.    
 - The explode function now properly targets the player.    
 - Downgraded the MonoGame Content Builder Task to 3.8.2.1105 due to stalling issues during compilation.    
