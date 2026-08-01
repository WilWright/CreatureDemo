# Intro

This demo showcases my implementation of some basic NPC navigation and behavior. The core of these systems are a [Terrain Scanner](Scripts/Systems/Navigation/TerrainScanner.cs), which builds a [Navigation Scan](Scripts/Systems/Navigation/NavigationScan.cs), from which a [Navigation Graph](Scripts/Systems/Navigation/NavigationGraph.cs) is made for a particular [Navigation Unit Config](Scriptable%20Objects/Navigation/Scripts/NavigationUnitConfig.cs), and a controller / state machine (in this demo, the [Bumble Bloom Controller](Scripts/Creatures/BumbleBloom/BumbleBloomController.cs)), with a [Navigation Unit](Scripts/Systems/Navigation/NavigationUnit.cs) that requests a [Navigation Path](Scripts/Systems/Navigation/NavigationPath.cs) utilizing A* pathfinding.

The only 3rd Party assets used are some free terrain textures and trees. Otherwise, all assets and code have been created by me for this demo or pulled from my own tools.

\
This video shows a span of 2 days in-game of NPC behavior and navigation (middle click to open in new tab)
[![Video](https://img.youtube.com/vi/I9MeQVMLbJI/0.jpg)](https://www.youtube.com/watch?v=I9MeQVMLbJI)

The video itself is not sped up. Time is set in-engine at 20x, with a 15 minute long day at 1x. While the scene is basic, it still makes use of asynchronous pathfinding and optimizations to maintain a smooth playback despite the increased speed at which it has to run.

At 7:00 AM the Bumble Blooms leave the forest to soak up some sun in the field while they rest, and at 7:00 PM they return to the forest to play in the bushes through the night, hidden from predators. While navigating you can see each unit display a purple line for their current path.

# Systems

### Navigation
Navigation begins with the [Terrain Scanner](Scripts/Systems/Navigation/TerrainScanner.cs), which designates an area to scan and the size of node to represent the terrain. The scanner raycasts downward along each node and detects any [Navigation Terrain](Scripts/Systems/Navigation/NavigationTerrain.cs) on colliders set up in the scene ([GetNavigationScan()](Scripts/Systems/Navigation/TerrainScanner.cs#L421-L525)). The scanner does some additional basic checks, such as a minimum clearance for any unit, as well as ledge detection to determine safe walkable areas and transition nodes. This data is baked and saved in the project as a [Navigation Scan](Scripts/Systems/Navigation/NavigationScan.cs) to be loaded later in-game. The data is currently serialized as a JSON object, but can be further optimized for faster loading and parsing by using a BinaryWriter/Reader.

The navigation scan is then loaded and processed further into a [Navigation Graph](Scripts/Systems/Navigation/NavigationGraph.cs) for a specific unit using its [Navigation Unit Config](Scriptable%20Objects/Navigation/Scripts/NavigationUnitConfig.cs), which defines things like the size of the unit, and the maximum slope it can traverse. While there is some logic for stepping implemented (to detect when a unit is able to step over a collision instead of walking around it), it is still WIP, so all nodes in the graph are currently connected to any adjacent nodes to represent walkable paths. This is good enough for the terrain in the demo, but can be much further improved to allow units to cross gaps, jump up, and climb terrain.

Here you can see some test terrain that contains different cases of navigation a unit may come across, especially if you don't want to comb through every inch of terrain and optimize it or smooth it out.
![](README%20Files/debug_terrain.png)

Here it is after a scan, with blue nodes representing the walking points, purple nodes representing areas that a unit cannot safely rest but can traverse through to another safe area, and green node to represent ledges.
![](README%20Files/debug_terrain_nodes.png)

And now with the nodes connected by edges for the pathfinder to use.
![](README%20Files/debug_terrain_graph.png)

A graph like this is relatively quick to search (average 2-50ms depending on obstacles), but I would like to optimize this even further into a polygonal mesh to drastically reduce the search space and allow for some smoother or more direct paths across the terrain.

Here is the graph for the demo scene.
![](README%20Files/demo_terrain_graph.png)

A [Navigation Unit](Scripts/Systems/Navigation/NavigationUnit.cs) can request a path through its unit type's respective [Navigation Map](Scripts/Systems/Navigation/NavigationMap.cs), which initializes the [Navigation Path Searches](Scripts/Systems/Navigation/NavigationPathSearch.cs) that will be available during runtime. Pathfinding is processed asynchronously and returns a path to the unit when ready, where in the mean time it can show a transition animation. The search can take a world position and map it to the closest node to find the start and end points.

The pathfinding uses A* and takes advantage of a couple optimizations to initialize with minimal allocations and run as fast as possible.
- The search graphs are reused. Instead of clearing data after each search they just increment an index to overwrite old data
- The open set is represented by a [Min Heap](Scripts/Data/MinHeap.cs) to make sorting for the lowest cost path fast
- The only allocations made are when the search graph is initialized and copied from the baked graph, when the open set needs to expand capacity, and when the final path is initialized and precalculates some navigation data for the unit to run faster movement logic

### Chunking
The current demo scene is all contained within one chunk, but the systems are expandable to allow for dynamic chunk loading and management. I decided a chunk system with spatial cells would be best for managing units, loading terrain, and setting AI LOD, based on range from the player chunk.

The [Chunk Manager](Scripts/Systems/ChunkManager.cs) first loads the chunk the player is in, and in a real game would load the 8 adjacent chunks around it as well. As each [Chunk](Scripts/Systems/Chunk.cs) loads, it also loads any navigation maps asynchronously for spawned units and keeps track of their positions in spatial cells. A chunk contains a 9x9 of cells, so that a unit can reasonably query any 8 adjacent cells to gather data about its environment and act on it, rather than having to query and filter through all loaded units/chunks.

### Unit Behavior
This demo showcases the Bumble Bloom, a nocturnal animal-plant hybrid that gains energy through photosynthesis. It's [Controller](Scripts/Creatures/BumbleBloom/BumbleBloomController.cs) and state machine currently has two simple states, [Idle](Scripts/Creatures/BumbleBloom/StateMachine/States/BumbleBloomState_Idle.cs) and [Walk](Scripts/Creatures/BumbleBloom/StateMachine/States/BumbleBloomState_Walk.cs). During the day, it will seek out a random position in a field from the navigation data to sleep and photosynthesize, and at night do similar for a bush in the forest, and when reached will wander around using a random nearby point. I initially started with a state machine as that is what I was familiar with, but after learning more about different AI systems I think a behavior tree with utility AI selectors would be the most appropriate to represent creatures like the Bumble Bloom. It would allow each unit to have personalities, act on higher level goals such as hunger and danger, and react seamlessly to stimuli from the environment, all within a manageable and expandable code base.
