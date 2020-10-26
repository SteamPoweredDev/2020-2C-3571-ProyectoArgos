using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Objects 
{
    public class Ship 
    {
        public Vector3 Position { get; set; }
        public float speed { get; set; }
        private float maxspeed { get; set; }
        private float maxacceleration { get; set; }
        public Model modelo { get; set; }
        public Vector3 orientacion { get; set; }
        public float anguloDeGiro { get; set; }
        public float giroBase { get; set; }
        private Boolean pressedAccelerator { get; set; }
        private int currentGear { get; set; }
        private Boolean HandBrake { get; set; }
        private Boolean pressedReverse { get; set; }
        public Ship (Vector3 initialPosition, Model baseModel, Vector3 currentOrientation, float MaxSpeed) 
        {
            speed = 0;
            Position = initialPosition;
            modelo = baseModel;
            orientacion = currentOrientation;
            maxspeed = MaxSpeed;
            maxacceleration = 0.005f;
            anguloDeGiro = 0f;
            giroBase = 0.003f;
            pressedAccelerator = false;
            currentGear = 0;
            HandBrake = false;
            pressedReverse = false;
        }


        public void Update(float gameTime, float timeMultiplier) {

            ProcessKeyboard(gameTime);
            UpdateMovementSpeed(gameTime);
            Move(gameTime, timeMultiplier);

        }
        public void Move(float gameTime, float timeMultiplier)
        {
            var newOrientacion = new Vector3((float)Math.Sin(anguloDeGiro), 0, (float)Math.Cos(anguloDeGiro));
            orientacion = newOrientacion;
            var newPosition = new Vector3(Position.X - speed*orientacion.X,Position.Y,Position.Z + speed*orientacion.Z );
            Position = newPosition;
        }

        public Matrix UpdateShipRegardingWaves (float time) {
            
            float waveFrequency = 0.01f;
            float waveAmplitude = 20;
           // float waveAmplitude2 = 50;
           time *= 2;
           
            var worldVector = Position;
            
            var newY = (MathF.Sin(worldVector.X*waveFrequency + time) + MathF.Sin(worldVector.Z*waveFrequency + time))*waveAmplitude;

            var tangent1 = Vector3.Normalize(new Vector3(1, 
                (MathF.Cos(worldVector.X*waveFrequency+time)*waveFrequency*waveAmplitude) * 0.5f
                ,0));
            var tangent2 = Vector3.Normalize(new Vector3(0, 
                (MathF.Cos(worldVector.Z*waveFrequency+time)*waveFrequency*waveAmplitude) * 0.5f
                ,1));
            
            worldVector = new Vector3(worldVector.X, newY, worldVector.Z);
            
            Position = worldVector;
            
            var waterNormal = Vector3.Normalize(Vector3.Cross(tangent2, tangent1));

            return Matrix.CreateLookAt(Vector3.Zero,orientacion, waterNormal);
        }
        
        private void UpdateMovementSpeed(float gameTime) 
        {
            float acceleration;
            if(HandBrake) acceleration = maxacceleration;
            else acceleration = maxacceleration * 8;
            float GearMaxSpeed = (maxspeed*currentGear/3);
            if(speed > GearMaxSpeed) {
                if(speed - acceleration < GearMaxSpeed){
                    speed = GearMaxSpeed;
                }
                else {
                    speed -= acceleration;
                }
            }
            else if(speed < GearMaxSpeed) {
                if(speed + acceleration > GearMaxSpeed){
                    speed = GearMaxSpeed;
                }
                else {
                    speed += acceleration;
                }
            }
        }
        private void ProcessKeyboard(float elapsedTime)
        {
            var keyboardState = Keyboard.GetState();
            

            if (keyboardState.IsKeyDown(Keys.A))
            {
                if(speed == 0){}
                else {
                    if(anguloDeGiro+giroBase >= MathF.PI*2){
                        anguloDeGiro = anguloDeGiro + giroBase - MathF.PI*2;
                    }
                    else {
                        anguloDeGiro -= giroBase;
                    } 
                }
            }

            if (keyboardState.IsKeyDown(Keys.D))
            {
                if(speed == 0){}
                else {
                    if(anguloDeGiro+giroBase < 0){
                        anguloDeGiro = anguloDeGiro - giroBase + MathF.PI*2;
                    }
                    else {
                        anguloDeGiro += giroBase;
                    } 
                }
            }

            if (this.pressedAccelerator == false && keyboardState.IsKeyDown(Keys.W) && currentGear < 3)
            {
                currentGear++;
                pressedAccelerator = true;
                if(HandBrake) HandBrake = false;
            }
            if(this.pressedAccelerator == true && keyboardState.IsKeyUp(Keys.W))
            {
                pressedAccelerator = false;
            }

            if (this.pressedReverse == false && keyboardState.IsKeyDown(Keys.S) && currentGear > -2)
            {
                currentGear--;
                pressedReverse = true;
                if(HandBrake) HandBrake = false;
            }
            if(this.pressedReverse == true && keyboardState.IsKeyUp(Keys.S))
            {
                pressedReverse = false;
            }

            if(HandBrake == false && keyboardState.IsKeyDown(Keys.Space))
            {
                HandBrake = true;
                currentGear = 0;
            }
        }
    }
}