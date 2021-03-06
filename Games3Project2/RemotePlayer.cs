﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

using Games3Project2.Globals;
using Camera3D;
using Geometry;

namespace Games3Project2
{
    public class RemotePlayer : Collidable
    {
        public int score;
        Sphere sphere;
        Cube cube;
        Matrix cubeTransformation;
        const int gunLength = 3;
        public bool isJuggernaut; //Uh, be sure to call setAsJuggernaut or NotJuggernaut instead of editing this variable directly.
        public float yaw;
        public float pitch;
        public NetworkGamer gamer;
        const int PACKET_INTERVAL = 10;
        float currentSmoothing;
        
        struct RemotePlayerState
        {
            public Vector3 position;
            public Vector3 velocity;
            public float pitch;
            public float yaw;
        }

        RemotePlayerState simulationState;
        RemotePlayerState previousState;

        public RemotePlayer(Vector3 pos, NetworkGamer associatedGamer) :
            base(Global.game, pos, Vector3.Zero, Global.Constants.PLAYER_RADIUS)
        {
            score = 0;
            Texture2D blankTex = Global.game.Content.Load<Texture2D>(@"Textures\blankTexture");
            sphere = new Sphere(Global.game, Global.Constants.DEFAULT_PLAYER_COLOR, pos);
            cube = new Cube(blankTex, Color.Gray); 
            sphere.localScale = Matrix.CreateScale(5);
            sphere.SetWireframe(1);
            cube.wireFrame = false;
            cube.textured = false;
            cubeTransformation = Matrix.CreateScale(1, 1, gunLength) * Matrix.CreateTranslation(new Vector3(radius, 0, gunLength));
            isJuggernaut = false;
            yaw = 0;
            pitch = 0;

            simulationState = new RemotePlayerState();
            simulationState.position = position;
            simulationState.velocity = velocity;
            simulationState.pitch = pitch;
            simulationState.yaw = yaw;
            previousState = new RemotePlayerState();
            previousState.position = position;
            previousState.velocity = velocity;
            previousState.pitch = pitch;
            previousState.yaw = yaw;
            currentSmoothing = 1;
        }

        public void update()
        {
            simulationState.position += simulationState.velocity * Global.Constants.MOVEMENT_VELOCITY * (float)Global.gameTime.ElapsedGameTime.TotalSeconds * Global.gameTime.ElapsedGameTime.Milliseconds;
            previousState.position += previousState.velocity * Global.Constants.MOVEMENT_VELOCITY * (float)Global.gameTime.ElapsedGameTime.TotalSeconds * Global.gameTime.ElapsedGameTime.Milliseconds;
            currentSmoothing -= 1.0f / (float)PACKET_INTERVAL;
            if (currentSmoothing < 0)
                currentSmoothing = 0;
            position = Vector3.Lerp(simulationState.position, previousState.position, currentSmoothing);
            velocity = Vector3.Lerp(simulationState.velocity, previousState.velocity, currentSmoothing);
            sphere.Position = position;
            sphere.Update(Global.gameTime);
        }

        public void draw()
        {
            sphere.Draw(Global.CurrentCamera);
            cube.Draw(Global.CurrentCamera, cubeTransformation *
                Matrix.CreateRotationX(MathHelper.ToRadians(pitch)) * Matrix.CreateRotationY(MathHelper.ToRadians(yaw))
                * Matrix.CreateTranslation(position));
        }

        public void receiveNewPacketUpdate(Vector3 newPos, Vector3 newVel, float newPitch, float newYaw)
        {
            previousState.position = position;
            previousState.velocity = velocity;
            previousState.pitch = pitch;
            previousState.yaw = yaw;

            simulationState.position = newPos;
            simulationState.velocity = newVel;
            simulationState.pitch = newPitch;
            simulationState.yaw = newYaw;

            yaw = newYaw;
            pitch = newPitch;

            currentSmoothing = 1;
        }

        public void setAsJuggernaut()
        {
            isJuggernaut = true;
            sphere.ChangeAllVertexColors(Global.Constants.JUGGERNAUT_COLOR);
            //TODO: Play "New Juggernaut" sound if not triggered in the network manager.
        }

        public void setAsNotJuggernaut()
        {
            isJuggernaut = false;
            sphere.ChangeAllVertexColors(Global.Constants.DEFAULT_PLAYER_COLOR);
        }
    }
}
