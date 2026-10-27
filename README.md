Memory Management Prototype

This project was created in Unity 2022.3.62f1 to demonstrate how different memory management techniques affect resource usage and performance. The prototype focuses on loading and unloading resources, texture management, and object pooling.

Running the Project

Open the project in Unity 2022.3.62f1 and load the SampleScene located in Assets/Scenes. Enter Play mode to begin testing.
The scene has 3 testing systems: ResourceTest, TextureTest, and PoolingTest. To declutter during testing, disable tests you don't want to perform in the Hierarchy.

Resource Loading

The resource-loading test shows how Unity loads and unloads a model, texture, and sound during runtime.
Use the buttons to load each resource and unload it when it is no longer needed. You can view memory usage through Unity's Memory Profiler.

Texture Management

The texture test creates a grid of cubes to compare using one shared texture vs separate texture copies.
The test also compares compressed and uncompressed textures to show how compression affects memory usage.

Object Pooling

The object-pooling test compares creating and destroying objects each time they're needed with reusing objects that have already been created.
Both tests use the same number of objects and repeat the operations through multiple cycles to compare execution times.

Test Results

The results showed that sharing textures and using compression reduced texture memory usage. Object pooling also reduced the time spent performing repeated object operations, although pooled objects remained in memory when inactive.

Memory Profiler screenshots documenting these comparisons are available in the TestResults folder.
