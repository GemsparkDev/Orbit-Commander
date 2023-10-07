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
     
	
