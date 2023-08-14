# Space Wars  
  
Space Wars is a game made using C# in monogame. It is a 2D top down wave based shooter where you must repair and upgrade your ship by salvaging from enemies you defeat.  
  
# Changelog  
  
10/3/2022: Started changelog, current content  
Alpha release 1  
	-Basic engine  
	-Player  
 	-Basic round  
  	-Fighter  
  	-Scrap  
  	-Various sounds (death, firing and hitting)  
  	-Alpha sprites for various entities  
  	-Wave timer  
  	-Debug mode  
  
10/16/2022: added basic motherdrone systems  
Update 0.0.1  
	-Docking with motherdrone  
  	-Mother drone storage  
 	-Player can carry 1 material at a time and input in motherdrone  
 	-Can move motherdrone when docked  
	-Velocity clamp for various entities  
   
10/17/2022: Velocity arrow for player  
Update 0.0.2  
  	-Velocity arrow for player  
  	-Leashed scrap stays near the player  
10/18/2022  
 	-Added Git Repository  
	-Updated to Visual Studio 2022  
	-Changed name to “Space Wars”  
   
 10/21/2022: Added basic UI support  
 Update 0.0.3  
  	-Added Myra support and references  
  	-Added basic framework for creating UI boxes  
 10/23/2022: Added two ui menus  
 	 -Pause menu with quit button  
  	-Mothership resource menu display  
 11/25/2022: Corrected mothership resource display  
  	-resource amounts are no longer able to be changed directly from the menu  
  
1/16/2023: Reimplimented enemy typing system  
Update 0.0.4  
	-Added cruiser drone  
	-Fixed issue #3  
        
1/22/2023: Wave system initial implementation  
Update 0.0.5  
	-Started implementation of the wave system  
	-Made player turn towards the mouse  
1/29/2023:  
        -fixed bug #4  
	
2/1/2023: Upgraded framework + packages, added programmer graphics  
Update 0.0.6  
        -Added background  
	-Changed framework to .net 6.0  
	-Begun implimentation of the module system 
         
 6/24/2023: Changed Myra for custom GUI library, added various utility functions  
 Update 0.0.7  
         -New GUI library and font  
         -2 new menus, pause menu and main menu  
	 -Better debug menu, w/ debug log  
	 -Various utility functions such as player respawn, sound falloff, and better distance function  
  	 -Fixed issues #1 and #2  
  
6/28/2023: More assets, better player health mechanics  
Update 0.0.8  
   	 -Various new UI and Sound assets  
     	 -New enemy type (sniper)  
         -New player weapon type (shotgun)  
	 -New UI type (Draggable icon)  
  	 -A set of player health statistics that affect player abilities  
         -Repair system   
	 -various other code improvements
    
7/29/2023: More UI and mechanics  
Update 0.0.9  
   	 -Added karma based drops - after a period of bad luck, drop chance is increased  
   	 -Added enemy health bars  
   	 -Added a mothership inventory and a furnace to refine scrap. If the inventory is full, any scrap brought to the mothership is released  
   	 -Scrap no longer directly increases how much material you have available, instead it must be refined first  
   	 -Made motion frame independent  
   	 -Added the spiral shot weapon for the player  
   	 -Changed the graphics to be a black background with neon colored entities  
   	 -Changed the IconSlot UI element to the ItemSlot UI element, and made the module inherit from the Item class  
   	 -Temporarily removed the module health square  
   	 -Added the TabbedWindow and Slider UI elements  
   	 -Changed the implimentation of the GetWidgetOver function, so that if there are two applicable widgets, it picks the one closest to the cursor  
   	 -Added multiple new UI assets  
   	 -Added the EventHandler class, which manages the interactions between the UI and the game  
   	 -Changed sound volume dropoff to be linear  
8/3/2023  
	-Added the Item class and the Module class, which inherits from the item class  
	-Static item return class for constant item creation  
	-Fixed bug #6  
    
8/14/2023  
Update 0.1  
    -Added various assets, such as an improved mothership sprite  
    -Removed the sentry due to technical debt  
    -Increased the range of the fighter enemy by 50 units  
    -Added a sound manager for playing, pausing, and looping sounds  
    -Updated the ClampVelocity() function to be the same in all directions  
    -Added two celestial bodies to the game, a large earth like planet and a dense moon orbiting it  
    -Added the GravitationalSource class, which creates a planet at a specific location with a radius, gravitational strength, and the ability to host moons  
    -Added a currently WIP crafting system  
    -Changed how modules work, with the player having a grid of delegate functions that the module of a specific type and weapon id can select with it's ability  
    -Completely removed the MothershipArrow child class from the game  
    -Added a basic particle system to the game  
    -Added support for container transparency  
    -Items now quickly glide to a stop  
    -Added the GameState class and it's context class, CurrentGameState, which determines what managers are allowed to update and when  
    -Split the EntityManager.Update() function into EntityManager.Update() and EntityManager.PlayerUpdate()  
    -Added the garage, which provides an environment that allows module repair and reslotting. Fixed bug #8  
    -Added the WIP interface IAnimatable  
  
