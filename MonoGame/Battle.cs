﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Player;
using Common;
using System.IO;

namespace MonoGame
{
    public class Battle
    {
        private Dictionary<string, Texture2D> BattleBg = new Dictionary<string, Texture2D>();

        public Battle(ContentManager Content)
        {
            var filepaths = FileManager.GetFilepaths("../../../Content/battlebg");

            foreach (var filepath in filepaths)
            {
                var filename = Path.GetFileNameWithoutExtension(filepath);
                BattleBg.Add(filename, Content.Load<Texture2D>("battlebg\\" + filename));
            }
        }

        public void Draw(SpriteBatch spriteBatch, Dialog dialog, SpriteFont spriteFont, ActorManager actorManager, EnemyManager enemyManager)
        {
            spriteBatch.Draw(BattleBg["1"], new Vector2(0, 0), Color.White);
            dialog.Draw(spriteBatch, new Rectangle(0, 330, 640, 150), 0);

            var i = 0;

            foreach (var actor in actorManager.Party)
            {
                DrawActor(i, actorManager, spriteBatch);
                DrawActorInfo(i, actor, spriteBatch, spriteFont);
                i++;
            }

            // draw enemies
            spriteBatch.Draw(enemyManager.Enemies["DarkTroll"], new Rectangle(60, 200, enemyManager.Enemies["DarkTroll"].Width, enemyManager.Enemies["DarkTroll"].Height), Color.White);
        }

        public void DrawActor(int i, ActorManager actorManager, SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(actorManager.BattleChars[actorManager.Party[i].BattleChar], new Rectangle(560, 140 + i * 48, 48, 48),
                actorManager.Party[i].Rect.ToRectangle(), Color.White);
        }

        public void DrawActorInfo(int i, Actor actor, SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            spriteBatch.DrawString(spriteFont, actor.Name, new Vector2(280, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, "HP: " + actor.Hp.ToString(), new Vector2(360, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, "/", new Vector2(410, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, actor.MaxHp.ToString(), new Vector2(415, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, "MP: " + actor.Mp.ToString(), new Vector2(460, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, "/", new Vector2(510, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, actor.MaxMp.ToString(), new Vector2(520, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, "Limit " + actor.Limit, new Vector2(550, 350 + i * 30), Color.White);
            spriteBatch.DrawString(spriteFont, "%", new Vector2(610, 350 + i * 30), Color.White);
        }
    }
}