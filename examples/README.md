# Examples

This document describes available example projects for the graphy-db. 

## Example 1: Spatial inference

The purpose of the project is to showcase abilities of the graphy-db for relative spatial inference problem. The project is divided into 3 responsibility parts.
  
### Client-Camera
It sends messages with information about seen objects on the scene. Every object is described in terms of **color, shape, position X, and position Y**.

An illustration of the camera view.
![primitive-objects](https://user-images.githubusercontent.com/4092658/39752669-d2e692c6-52c4-11e8-8b33-d0779f335094.PNG)

### Client-Requester
This is a client-side console application that gives an opportunity to send spatial requests to the server. For instance, it allows you to retrieve all objects on the left side from the described one.

One of the possible user-cases.
![requester](https://user-images.githubusercontent.com/4092658/39752820-45d9c442-52c5-11e8-9b35-db88c3d2a340.PNG)

### Server
This part is in control of handling messages from the camera and requester clients. It uses graphy-db to track all objects and their spatial connectivity to each other. 

Example of the server logging.

![server](https://user-images.githubusercontent.com/4092658/39752965-cd00b5c0-52c5-11e8-965c-7b86b894eb36.PNG)
