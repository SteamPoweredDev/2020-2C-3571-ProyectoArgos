﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.TP.Objects;
using TGC.MonoGame.TP.SkyboxSimple;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffect = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";
        
        private float time;
        private Vector3 boatPosition;
        private Matrix WaterMatrix;

        private float timeMultiplier = 0.5f;

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            // Descomentar para que el juego sea pantalla completa.
            // Graphics.IsFullScreen = true;
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        private Model Model { get; set; }
        private Model Model2 { get; set; }
        private Model Model3 { get; set; }
        private Model Model4 { get; set; }

        private Ship PlayerShip { get; set; }
        private Camera Camera;

        private TargetCamera TargetCamera;

        private Effect WaterEffect { get; set; }
        private float Rotation { get; set; }
        private Matrix World { get; set; }
        private Matrix view { get; set; }
        private SkyBox2 SkyBox { get; set; }
        private float angle { get; set; }
        private float distance { get; set; }
        public Matrix MatrixSkybox { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: todo procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
            
            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 720;
            Graphics.ApplyChanges();
            
            // Configuramos nuestras matrices de la escena.
            World = Matrix.CreateRotationY(MathHelper.Pi);
           // View = Matrix.CreateLookAt(Vector3.UnitZ * 500 + Vector3.Up * 150, Vector3.Zero, Vector3.Up);
           // Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 1000);

            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-350, 50, 400), screenSize);
            boatPosition = new Vector3(-150, 40, -600);
            TargetCamera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(boatPosition.X, boatPosition.Y + 150, boatPosition.Z - 250), boatPosition);
            WaterMatrix = Matrix.Identity;

            /*
            Matrix world = Matrix.Identity;
            Matrix view = Matrix.CreateLookAt(new Vector3(20, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 600f, 0.1f, 100f);
            Vector3 cameraPosition;
            */

            angle = 0;
            distance = 20;

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el
        ///     procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            // Cargo el modelo del logo.
            Model = Content.Load<Model>(ContentFolder3D + "t-22/T-22");
           //Model = Content.Load<Model>(ContentFolder3D + "axis");
            Model2 = Content.Load<Model>(ContentFolder3D + "nagato/Nagato");
            Model3 = Content.Load<Model>(ContentFolder3D + "Isla_V2");
            Model4 = Content.Load<Model>(ContentFolder3D + "water");
            // Obtengo su efecto para cambiarle el color y activar la luz predeterminada que tiene MonoGame.
            var modelEffect = (BasicEffect) Model.Meshes[0].Effects[0];
            modelEffect.DiffuseColor = Color.DarkBlue.ToVector3();
            modelEffect.EnableDefaultLighting();

            PlayerShip = new Ship(boatPosition,Model,new Vector3(0,0,-1), 20);

            WaterEffect = Content.Load<Effect>(ContentFolderEffect + "WaterShader");
            
            WaterEffect.Parameters["KAmbient"]?.SetValue(0.15f);
            WaterEffect.Parameters["KDiffuse"]?.SetValue(0.75f);
            WaterEffect.Parameters["KSpecular"]?.SetValue(1f);
            WaterEffect.Parameters["Shininess"]?.SetValue(10f);
            
            WaterEffect.Parameters["AmbientColor"]?.SetValue(new Vector3(1f, 0.98f, 0.98f));
            WaterEffect.Parameters["DiffuseColor"]?.SetValue(new Vector3(0.0f, 0.5f, 0.7f));
            WaterEffect.Parameters["SpecularColor"]?.SetValue(new Vector3(1f, 1f, 1f));

            var skyBox = Content.Load<Model>(ContentFolder3D + "skybox/cube");
            //var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/sunset/sunset");
            //var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/sun-in-space/sun-in-space");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "/skyboxes/skybox/skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffect + "SkyBox");
            SkyBox = new SkyBox2(skyBox, skyBoxTexture, skyBoxEffect);

            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) * timeMultiplier;
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            PlayerShip.Update(time, timeMultiplier);
            TargetCamera.Update(gameTime);
            WaterMatrix = PlayerShip.UpdateShipRegardingWaves(time);
            
            // Capturar Input teclado
            Camera.Update(gameTime);

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                //Salgo del juego.
            
            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(Color.Black);
            //Finalmente invocamos al draw del modelo.
            //Model.Draw(World * Matrix.CreateRotationY(Rotation), View, Projection);
            Model.Draw(World * WaterMatrix * Matrix.CreateTranslation(PlayerShip.Position), Camera.View, Camera.Projection);
            //Model.Draw(World * WaterMatrix, Camera.View, Camera.Projection);
           // Model2.Draw(World * Matrix.CreateTranslation(-120, 20, 0), Camera.View, Camera.Projection);
            //Model3.Draw(World * Matrix.CreateTranslation(0, 0, 0), Camera.View, Camera.Projection);
            //Model4.Draw(World * Matrix.CreateTranslation(0, 45, 0), Camera.View, Camera.Projection);
            var waterMesh = Model4.Meshes[0];
            
            if (waterMesh != null)
           {
                   var part = waterMesh.MeshParts[0];
                   part.Effect = WaterEffect;
                   WaterEffect.Parameters["World"].SetValue(World);
                   WaterEffect.Parameters["View"].SetValue(Camera.View);
                   WaterEffect.Parameters["Projection"].SetValue(Camera.Projection);
                   WaterEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(World)));
                   WaterEffect.Parameters["Time"]?.SetValue(time);
                   WaterEffect.Parameters["CameraPosition"]?.SetValue(Camera.Position);
                   //Effect.Parameters["WorldViewProjection"].SetValue(Camera.WorldMatrix * Camera.View * Camera.Projection);
                   //Effect.Parameters["ModelTexture"].SetValue(Texture);
                 //  Effect.Parameters["Time"]?.SetValue(time);
                   waterMesh.Draw();
           }

            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;

            //MatrixSkybox = Matrix.CreateOrthographic(100, 100, 0.1f, 100f);

            //SkyBox.Draw(Camera.View, MatrixSkybox, Camera.Position);
            
            SkyBox.Draw(Camera.View, Camera.Projection, Camera.Position);

            GraphicsDevice.RasterizerState = originalRasterizerState;


            base.Draw(gameTime);
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}