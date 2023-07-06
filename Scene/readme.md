# Scene dependencies

Shareable scene starts first and lives until the game stops. It initializes all game's core objects and provides them to all other scenes. From a shareable scene, all other scenes are being loaded additively.

Scene loading order looks like this:

```
[Shareable or list of shareables] -> [Game] -> [Level chunks, game modes, etc.]

```

It's not convenient to always load the scenes in this order while the game is being developed. If the scene you are working on right now doesn't need core objects like a log manager, a sound manager, some analytics, and so on, you can run them in standard way (by hitting Play in the Editor). Otherwise, if you need to initialize core objects before your scene starts, use the GameLib.Scene helpers. To access "Run with dependencies" button press "Overlay menu" in the scene right corner. Then choose Gamelib toolbar. The black play button is what you need. This button can be moved to panel as shown in the picture:

![PlayButtonExample](DocImages~/PlayButtonExample.png)


If you hit "Run with dependencies" button SceneLoader which is located in the Start Scene will load current active scene with all its dependencies. If there are other scene loaded they will not be loaded for a run time. In other worlds you will run active scene with its dependencies in isolation. 

If you have multiple scenes loaded in hierarchy right click on the scene and "Set Active Scene" will make it active.