# 2D-Deformable-body-in-Unity
A 2D Deformable body simulation in Unity using FEM

This a port of the 2D FEM project by [Miles Macklin](http://blog.mmacklin.com/) found [here](https://github.com/mmacklin/sandbox) from C++/OpenGL to C# Unity. The code is not that stable and the physics can break if pushed too far but its a nice introduction to deformable bodies using the Finite Element Method.

The original did also implement fracturing of the mesh but I have not fully ported that and probably wont. The project provides a few options to load the scene with various meshes and has some neat code that creates a mesh from a image.

See the [home page](https://www.digital-dust.com/single-post/2017/12/31/2D-deformable-body-in-Unity) for the Unity package download. 

There's a armadillo, bunny and a donut.

![Armadillo](https://static.wixstatic.com/media/1e04d5_92f4e7e3e02240568ecfe6254222960b~mv2.png/v1/fill/w_486,h_486,al_c,usm_0.66_1.00_0.01/1e04d5_92f4e7e3e02240568ecfe6254222960b~mv2.png)

![Bunny](https://static.wixstatic.com/media/1e04d5_460ba6280c8d46bebd8e7de2126a16cf~mv2.png/v1/fill/w_486,h_486,al_c,usm_0.66_1.00_0.01/1e04d5_460ba6280c8d46bebd8e7de2126a16cf~mv2.png)

There's also a few basic shapes created from code like a beam, torus and a random convex as shown below.

![Randon Convex](https://static.wixstatic.com/media/1e04d5_b7f438f3e62a40168537f7c35542815e~mv2.png/v1/fill/w_486,h_486,al_c,usm_0.66_1.00_0.01/1e04d5_b7f438f3e62a40168537f7c35542815e~mv2.png)

And here's a GIF of the armadillo being thrown around.

![Armadillo GIF](https://static.wixstatic.com/media/1e04d5_2d75c1c2b83b4fd59f5d95cb3d260973~mv2.gif)
