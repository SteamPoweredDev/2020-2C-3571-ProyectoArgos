using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.TP.GUI;
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
        
        public float ElapsedTime;
        private Vector3 boatPosition;
        private Matrix WaterMatrixForPlayer;
        private Matrix WaterMatrixForEnemy;

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
        private Model IslandModel { get; set; }
        private Model WaterModel { get; set; }

        private Model _bulletModel;

        public Model BulletModel => _bulletModel;

        public Ship PlayerShip { get; set; }
        public Ship EnemyShip { get; set; }
        private Camera FreeCamera;

        private TargetCamera TargetCamera;
        private Effect WaterEffect { get; set; }

        private Matrix _world;
        public Matrix World => _world;

        private SkyBox2 SkyBox { get; set; }

        private GameUI _gameUi;

        private List<Bullet> _bullets;

        private List<Bullet> _bulletsToDelete;
        public List<Bullet> BulletsToDelete => _bulletsToDelete; 
        public List<Bullet> Bullets => _bullets;
        public Camera CurrentCamera => TargetCamera;

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
            _world = Matrix.CreateRotationY(MathHelper.Pi);

            var screenCenter = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            
            FreeCamera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(-350, 50, 400), screenCenter);
            boatPosition = new Vector3(-150, 40, -600);

            TargetCamera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(boatPosition.X, boatPosition.Y + 150, boatPosition.Z - 250), boatPosition, screenCenter, (float)(GraphicsDevice.Viewport.Height), (float)(GraphicsDevice.Viewport.Width));

            _gameUi = new GameUI(this);
            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el
        ///     procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            IslandModel = Content.Load<Model>(ContentFolder3D + "Isla_V2");
            WaterModel = Content.Load<Model>(ContentFolder3D + "water");
            _bulletModel = Content.Load<Model>(ContentFolder3D + "bullet");

            _bullets = new List<Bullet>();
            _bulletsToDelete = new List<Bullet>();
            
            PlayerShip = new Ship(boatPosition,new Vector3(0,0,-1), 5, this);
            PlayerShip.CanBeControlled = true;
            PlayerShip.ModelName = "t-22/T-22";
            PlayerShip.LoadContent();
            
            EnemyShip = new Ship(new Vector3(-600, 20, 100), Vector3.Forward, 5, this);
            EnemyShip.ModelName = "nagato/Nagato";
            EnemyShip.LoadContent();

            WaterEffect = Content.Load<Effect>(ContentFolderEffect + "WaterShader");
            
            WaterEffect.Parameters["KAmbient"]?.SetValue(0.15f);
            WaterEffect.Parameters["KDiffuse"]?.SetValue(0.75f);
            WaterEffect.Parameters["KSpecular"]?.SetValue(1f);
            WaterEffect.Parameters["Shininess"]?.SetValue(10f);
            
            WaterEffect.Parameters["AmbientColor"]?.SetValue(new Vector3(1f, 0.98f, 0.98f));
            WaterEffect.Parameters["DiffuseColor"]?.SetValue(new Vector3(0.0f, 0.5f, 0.7f));
            WaterEffect.Parameters["SpecularColor"]?.SetValue(new Vector3(1f, 1f, 1f));

            var skyBox = Content.Load<Model>(ContentFolder3D + "skybox/cube");
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
            ElapsedTime += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) * timeMultiplier;

            PlayerShip.Update(gameTime);
            EnemyShip.Update(gameTime);
            
            TargetCamera.Update(gameTime);
            TargetCamera.UpdatePosition(gameTime, PlayerShip.Position);

            foreach (var bullet in _bullets)
            {
                bullet.Update();
            }

            _bullets = _bullets.Except(_bulletsToDelete).ToList();
            _bulletsToDelete.Clear();
            
            // Capturar Input teclado
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
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.Opaque;

            for (int i = 0; i < _bullets.Count; i++)
            {
                _bullets[i].Draw(gameTime);
            }
            
            PlayerShip.Draw();
            EnemyShip.Draw();
            IslandModel.Draw(World * Matrix.CreateTranslation(0, 70, 0), TargetCamera.View, TargetCamera.Projection);
            var waterMesh = WaterModel.Meshes[0];
            
            if (waterMesh != null)
            {
                   var part = waterMesh.MeshParts[0];
                   part.Effect = WaterEffect;
                   WaterEffect.Parameters["World"].SetValue(_world);
                   WaterEffect.Parameters["View"].SetValue(TargetCamera.View);
                   WaterEffect.Parameters["Projection"].SetValue(TargetCamera.Projection);
                   WaterEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(_world)));
                   WaterEffect.Parameters["Time"]?.SetValue(ElapsedTime);
                   WaterEffect.Parameters["CameraPosition"]?.SetValue(TargetCamera.Position);
                   waterMesh.Draw();
            }
 
            var originalRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Graphics.GraphicsDevice.RasterizerState = rasterizerState;
            
            SkyBox.Draw(TargetCamera.View, TargetCamera.Projection, TargetCamera.Position);

            GraphicsDevice.RasterizerState = originalRasterizerState;
    
            _gameUi.Draw();

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