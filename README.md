# Av&Al
Top-down roguelite shooter prototype with procedure generation of floors based on Unity
# Description
## Controls
* WASD - move
* Mouse - aim
* LMB - shoot
* RMB - dodgeroll
* E - take item/weapon
* F (hold) - drop weapon
* ESC - pause
* TAB - open full map
## Player Target
To explore the stage, find items that boost stats (optionally), then find boss room and fight the boss to go to the next level  
## HUD
* Top left - health points. Player loses 1 hp when touches enemy or gets hit by enemy bullet. Player can get healing pickups as reward for killing all enemies in the room
* Top right - minimap
  + Red square - player current position
  + White squares - visited rooms
  + Grey squares - rooms that are adjacent to visited rooms
  + Yellow squares - item rooms
  + Blue square - boss icon
* Bottom right - collected items
  + Red - damage ups
  + Blue - firerate ups
  + Yellow - speed ups

![image](https://github.com/Karkusha23/WSM-PEWM/assets/16138259/78ee049b-a3cb-4c01-889f-8dd29a010fc9)
## Stages
Stages are composed of small and big (2x2 of small) rectangular rooms. When entering the room, doors will be closed and enemies will spawn. To open the doors player has to kill all enemies in the room  

There are several item rooms on each stage. Item room contains one item (red, blue or yellow) that boosts certain player's stat

Stages are generated procedurally using parameters such as floor grid size, room count bounds and probability of big room generation  

Room layout (obstacles and enemies locations) is chosen randomly from room layout pool  

![image](https://github.com/Karkusha23/WSM-PEWM/assets/16138259/e4dab1f9-a2af-453c-ae8b-b073e67f8f92)
## Enemies
Enemies can build paths using A* algorithm  

Enemy types
* Chaser - approaches player and deals contact damage
* Shooter - keeps player in his line of sight as well as trying to keep distance. Will retreat if player gets to close
* DVD - wanders diagonally and bouncing off the walls like DVD logo. Periodically stops and shoots several bullets in 4 directions
* Wanderer - just walks randomly
### Boss
Has healthbar at the bottom of the screen. Player has to defeat it to go to the next level  

Has 2 attacks
* Spiral

![image](https://github.com/Karkusha23/WSM-PEWM/assets/16138259/2b62a982-e88b-4354-b3a8-4ffde960c83c)
* Machinegun

![image](https://github.com/Karkusha23/WSM-PEWM/assets/16138259/77694c81-58b0-41a2-8df0-941202ec7600)

# Released features
Player
* Moving
* Aiming and shooting
* Dodgeroll - small dash in direction of moving with invincibility frames. Player's cube will change color to purple while invincible
* Stat boosting by picking items
* Animation upon taking damage (blinking)

UI
* Health points
* Minimap
* Full map
* Item count
* Pause menu

Enemy
* 4 types of regular enemies
* Boss
* Navigation system based on room grid and A* algorithm

Stages
* Procedural stage generation
* Generation of item rooms and boss room
* Different room layouts assigned to every room
* Healing pickup upon room completion
* Transition on new stage upon defeating boss
