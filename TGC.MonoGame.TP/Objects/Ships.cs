using System;
using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP.Objects 
{
    public class Ship 
    {
        public Vector3 Position { get; set; }
        public float velocidad { get; set; }
        private float maxspeed { get; set; }
        private float maxacceleration { get; set; }
        public Model modelo { get; set; }
        public Vector3 orientacion { get; set; }
        public Vector3 orientacionSobreOla { get; set; }
        public float anguloDeGiro { get; set; }
        public float giroBase { get; set; }

        private int _maxLife = 100;

        private int _currentLife = 100;

        private float _shootingCooldownTime = 0.8f;

        private float _timeToCooldown = 0f;

        public bool CanBeControlled = false;

        public int CurrentLife
        {
            get { return _currentLife; }
        }
        
        public int MaxLife
        {
            get { return _maxLife; }
        }

        private TGCGame _game;

        public Ship (Vector3 initialPosition, Model baseModel, Vector3 currentOrientation, float MaxSpeed, TGCGame game) 
        {
            velocidad = 0;
            Position = initialPosition;
            modelo = baseModel;
            orientacion = currentOrientation;
            maxspeed = MaxSpeed;
            maxacceleration = 0.005f;
            anguloDeGiro = 0f;
            giroBase = 0.005f;
            _game = game;
        }


        public void Update(float elsapseGameTime, float timeMultiplier, GameTime gameTime) 
        {
            if (CanBeControlled)
            {
                ProcessKeyboard(elsapseGameTime);
                Move(elsapseGameTime, timeMultiplier);
                if (_timeToCooldown >= float.Epsilon)
                {
                    _timeToCooldown -= Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);   
                }   
            }
        }
        public void Move(float gameTime, float timeMultiplier)
        {
            var newOrientacion = new Vector3((float)Math.Sin(anguloDeGiro), 0, (float)Math.Cos(anguloDeGiro));
            orientacion = newOrientacion;

            var result = orientacionSobreOla - Vector3.Dot(orientacionSobreOla, Vector3.Up) * Vector3.Up; //Projecccion de orientacion sobre el plano x,z

            var extraSpeed = 10;
            if (velocidad == 0) extraSpeed = 0; //Asi no se lo lleva el agua cuando esta parado
            var speed = velocidad + extraSpeed*-Vector3.Dot(orientacionSobreOla, Vector3.Up);

            var newPosition = new Vector3(Position.X - speed*orientacion.X ,Position.Y,Position.Z + speed*orientacion.Z);
            Position = newPosition;
        }

        public Matrix UpdateShipRegardingWaves (float time) {
            
            float waveFrequency = 0.01f;
            float waveAmplitude = 20;
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
            
            //Proyectamos la orientacion sobre el plano formado con la normal del agua para subir o bajar la proa del barco
            orientacionSobreOla = orientacion -  Vector3.Dot(orientacion, waterNormal) * waterNormal;

            return Matrix.CreateLookAt(Vector3.Zero,orientacionSobreOla, waterNormal);
        }
        
        
        private void ProcessKeyboard(float elapsedTime)
        {
            var keyboardState = Keyboard.GetState();


            if (keyboardState.IsKeyDown(Keys.A))
            {
                if(velocidad == 0){}
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
                if(velocidad == 0){}
                else {
                    if(anguloDeGiro+giroBase < 0){
                        anguloDeGiro = anguloDeGiro - giroBase + MathF.PI*2;
                    }
                    else {
                        anguloDeGiro += giroBase;
                    } 
                }
            }

            if (keyboardState.IsKeyDown(Keys.W))
            {
                if(velocidad == maxspeed){}
                else if(velocidad+maxacceleration >= maxspeed){
                    velocidad = maxspeed;
                }
                else {
                    velocidad += maxacceleration;
                }
            }

            if (keyboardState.IsKeyDown(Keys.S))
            {
                if(velocidad == -maxspeed){}
                else if((velocidad-maxacceleration) <= -maxspeed){
                    velocidad = -maxspeed;
                }
                else {
                    velocidad -= maxacceleration;
                }
            }

            if(keyboardState.IsKeyDown(Keys.Space))
            {
                if(velocidad > 0){
                    if(velocidad - maxacceleration*6 <= 0){
                        velocidad = 0;
                    }
                    else {
                        velocidad -= maxacceleration*6;
                    }
                }
                else if (velocidad < 0){
                    if(velocidad + maxacceleration*6 >= 0){
                        velocidad = 0;
                    }
                    else {
                        velocidad += maxacceleration*6;
                    }
                }
                else {
                    velocidad = 0;
                }
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed && _timeToCooldown < float.Epsilon)
            {
                var bulletOrientation = orientacion;
                bulletOrientation.X = -bulletOrientation.X;
                _game.Bullets.Add(new Bullet(_game, Position, bulletOrientation + Vector3.Up * 0.2f));
                _timeToCooldown = _shootingCooldownTime;
            }

        }
    }
}