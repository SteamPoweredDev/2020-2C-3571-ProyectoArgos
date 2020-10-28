using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.GUI
{
    public class GameUI
    {
        
        public SpriteBatch _spriteBatch;
        private TGCGame _game;
        private SpriteFont _font;
        private Texture2D _targetTexture;
        private Texture2D _panelTexture;
        
        public GameUI(TGCGame game)
        {
            _game = game;
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            _font = _game.Content.Load<SpriteFont>("Fonts/AATypewriter");
            _targetTexture = _game.Content.Load<Texture2D>("Textures/GUI/target");
        }

        public void Draw()
        {
            var textScale = 10;
            var textSpeed = "Current Speed: " + _game.PlayerShip.speed;
            var textLife = $"Health: {_game.PlayerShip.CurrentLife} / {_game.PlayerShip.MaxLife}";
            
            var viewportWidth = _game.GraphicsDevice.Viewport.Width;
            var viewportHeight = _game.GraphicsDevice.Viewport.Height;
            
            var size = textScale;

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, textSpeed, new Vector2(10, viewportHeight - 50), Color.White);
            _spriteBatch.DrawString(_font, textLife, new Vector2(10, viewportHeight - 80), Color.White);

            _spriteBatch.Draw(_targetTexture, new Vector2(viewportWidth / 2 - _targetTexture.Width / 2, viewportHeight / 2 - _targetTexture.Height / 2), Color.White);
            
            _spriteBatch.End();
        }
    }
}