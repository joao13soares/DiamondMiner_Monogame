using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using System; // because we are using Random type

namespace Diamond_Miner
{
    public class FallingEntity
    {
        public enum Type
        {
            STONE, DIAMOND
        };

        public Type type;
        public Vector2 dir = Vector2.Zero; // Direction as a vector
        public Vector2 fallingEntityPos; // position of the fallingEntity
        Texture2D fallingEntity;
        public bool moving = false;   // are we animating?
        int frame = 0;  // current frame on tileSize animation
        Game1 game;

        public FallingEntity(Game1 game, Vector2 initialPos, Enum type)
        {
            fallingEntityPos = initialPos;
            this.game = game;

            // if STONE
            if (type.Equals(Type.STONE))
            {
                this.type = Type.STONE;
                fallingEntity = game.Content.Load<Texture2D>("CrateDark_Gray");
            }
            // if DIAMOND
            else
            {
                this.type = Type.DIAMOND;
                fallingEntity = game.Content.Load<Texture2D>("Crate_Blue");
            }
        }

        public void Update(GameTime gameTime)
        {
            if (moving)
            {
                if (frame < Game1.tileSize)
                {
                    frame += 2;
                }
                else
                {
                    frame = 0;
                    moving = false;
                }
            }
            else
            {

                // will the fallingEntity fall?
                if (game.map[fallingEntityPos.ToPoint().Y + 1, fallingEntityPos.ToPoint().X] == ' ' && game.getFallingEntity(fallingEntityPos + Vector2.UnitY) == null &&
                    (fallingEntityPos + Vector2.UnitY) != game.endPos && fallingEntityPos != game.player.playerPos)
                {
                    game.stoneNoise.Play(); // sound
                    moving = true; // fallingEntity is falliiiiing!!!
                    dir = Vector2.UnitY;
                    fallingEntityPos += dir;
                }
                // will the fallingEntity move sideways?
                else if (game.getFallingEntity(fallingEntityPos + Vector2.UnitY) != null)
                {
                    if (game.map[fallingEntityPos.ToPoint().Y, fallingEntityPos.ToPoint().X - 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(-1, 0)) == null &&
                        game.map[fallingEntityPos.ToPoint().Y + 1, fallingEntityPos.ToPoint().X - 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(-1, 1)) == null &&
                        fallingEntityPos + new Vector2(-1, 0) != game.player.playerPos && fallingEntityPos + new Vector2(-1, 1) != game.player.playerPos &&
                        game.map[fallingEntityPos.ToPoint().Y, fallingEntityPos.ToPoint().X + 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(1, 0)) == null &&
                        game.map[fallingEntityPos.ToPoint().Y + 1, fallingEntityPos.ToPoint().X + 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(1, 1)) == null &&
                        fallingEntityPos + new Vector2(1, 0) != game.player.playerPos && fallingEntityPos + new Vector2(1, 1) != game.player.playerPos)
                    {
                        game.stoneNoise.Play(); // sound
                        moving = true; // fallingEntity is mooooooving sideways randomly!!!

                        Random r = new Random();
                        if (r.Next(2) == 0) dir = -Vector2.UnitX;
                        else dir = Vector2.UnitX;

                        fallingEntityPos += dir;
                    }
                    else if (game.map[fallingEntityPos.ToPoint().Y, fallingEntityPos.ToPoint().X - 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(-1, 0)) == null &&
                        game.map[fallingEntityPos.ToPoint().Y + 1, fallingEntityPos.ToPoint().X - 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(-1, 1)) == null &&
                        fallingEntityPos + new Vector2(-1, 0) != game.player.playerPos && fallingEntityPos + new Vector2(-1, 1) != game.player.playerPos)
                    {
                        game.stoneNoise.Play(); // sound
                        moving = true; // fallingEntity is mooooooving left!!!
                        dir = -Vector2.UnitX;
                        fallingEntityPos += dir;
                    }
                    else if (game.map[fallingEntityPos.ToPoint().Y, fallingEntityPos.ToPoint().X + 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(1, 0)) == null &&
                        game.map[fallingEntityPos.ToPoint().Y + 1, fallingEntityPos.ToPoint().X + 1] == ' ' && game.getFallingEntity(fallingEntityPos + new Vector2(1, 1)) == null &&
                        fallingEntityPos + new Vector2(1, 0) != game.player.playerPos && fallingEntityPos + new Vector2(1, 1) != game.player.playerPos)
                    {
                        game.stoneNoise.Play(); // sound
                        moving = true; // fallingEntity is mooooooving right !!!
                        dir = Vector2.UnitX;
                        fallingEntityPos += dir;
                    }
                    else { moving = false; }
                }
                else { moving = false; }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (moving)
            {
                spriteBatch.Draw(fallingEntity, fallingEntityPos * Game1.tileSize + dir * (frame - Game1.tileSize),  Color.White);
            }
            else
            {
                spriteBatch.Draw(fallingEntity, fallingEntityPos * Game1.tileSize,  Color.White);
            }
        }
    }
}