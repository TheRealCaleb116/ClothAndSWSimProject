---
layout: default
---
<div id="HeaderPics">

 <img src="./assets/img/ClothSimPic.png" alt=""> 
 <img src="./assets/img/ShallowWaterSimPic.png" alt=""> 
 
</div>

# Cloth and Shallow Water Simulation - Caleb Wiebolt

Below is the write-up for my Cloth simulation project which also includes a separate Shallow Water Simulation as well. It was created in Unity for my Animation and Planning in Games class. All the graded features that I attempted can be found below. To look at the source code or a pre-built executable click the button below. 

<a href="{{ site.github.repository_url }}" class="btn btn-dark">Go to the Code</a>



## Features Attempted
### Showcase Video


{% include video.html %}


### Feature List

| Feature                           | Description       | TimeCode |
|:-------------                     |:------------------|:------|
| Cloth Simulation          | A natural-looking cloth sim that collides with an object. | 0:03-4:34  |
| 3D Simulation        | The Cloth sim is in 3D and has a natural camera. | 0:03-4:34   |
| High-Quality Render  | I used texturing, lighting, particle systems, and a camera controller to make a compelling render | Throughout the video. (The wind particles turned out nice) |
| Air Drag on Cloth | The cloth is affected by air drag and an included wind system. | 1:38-3:35  |
| Continuum Fluid Simulation   | I implemented a Shallow Water Simulation and integrated it into a scene. | 4:35-7:34   |



## Tools and Libraries Used
*   Unity 2022.3.9f1 and Visual Studio


## Assets Used
*   2D Pixel Art Platformer \| Biome - American Forest art assets by <a href="https://assetstore.unity.com/packages/2d/environments/2d-pixel-art-platformer-biome-american-forest-255694"> OOO Superposition Principle Inc.</a> on the Unity Asset Store
* Beachball Textures by <a href="https://www.robinwood.com/Catalog/FreeStuff/Textures/TexturePages/BallMaps.html">Robin Wood</a>
* Skybox Texture by <a href="https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-series-free-103633"> Unamed666</a> on the Unity Asset Store 
* Wood Decking Texture provided by <a href="https://architextures.org/textures/487"> architextures.org</a>
* Cloth Texture provided by <a href="https://freepbr.com/materials/diagonal-stripe-weave-pbr/"> freepbr.com</a>


## Difficulties Encountered
Building these simulations was a first for me. I have made games before, but implementing my own simulation code is a challenge I have never tackled before. One of the main challenges I had while implementing my simulations, specifically the cloth simulation, was just the finickyness of tuning all the physics simulation parameters. What time step should I use? What k values? More nodes or less? If I changed one value everything else needed to be changed. Beyond this, I was having a strange issue where my cloth would never come to a complete rest. There was enough inaccuracy in the calculations that the cloth would always have a little energy. After hours of double-checking the code, trying different higher-order integration schemes, I couldn't get anything to work. Finally, after some inspiration from a cloth simulation reference project I saw online, I added a janky dampening to the velocity of each node that is relative to the node's total velocity. Is it 'technically' accurate, probably not. It might be hacky, but it's fast, looks good, and ends up being one line of code. Just another reminder that at the end of the day, it's all about how the simulation looks. People watch your simulation, not your code.

## Art Contest
 <img id="gif" src="./assets/img/ArtEntryCalebWiebolt.gif" alt=""> 


