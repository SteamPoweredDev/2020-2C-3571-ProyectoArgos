using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.Samples.Cameras;

namespace TGC.MonoGame.TP.Objects
{
    public class Bullet
    {
        private Model _bulletModel;
        private float _bulletSpeed = 1000;
        private float _timeElapsed;
        private Vector3 _originDirection;
        private Vector3 _originPosition;
        private float _lifeTime = 10;

        private TGCGame _game;
        private Camera _camera;
        
        public Bullet(TGCGame game, Vector3 origin, Vector3 direction)
        {
            _game = game;
            _camera = _game.CurrentCamera;
            _originPosition = origin;
            _originDirection = direction;
            _timeElapsed = 0;
        }

        public void Update()
        {
            if (_timeElapsed > _lifeTime)
            {
                _game.BulletsToDelete.Add(this);
            }
        }

        public void Draw(GameTime gameTime)
        {
            _game.BulletModel.Draw(_game.World * Matrix.CreateTranslation(_originPosition + _originDirection * _timeElapsed * _bulletSpeed), _camera.View, _camera.Projection);
            _timeElapsed += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
        }
        
    }
}