using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Linq;

namespace Diamond_Miner
{
    public class Player
    {
        enum Direction
        {
            LEFT = 0, RIGHT = 1, UP = 2, DOWN = 3
        };

        Direction direction = Direction.DOWN;
        Vector2 dir = Vector2.Zero; // Direction as a vector
        public int nrMoves = 0, nrTNT = 0, nrDiamonds = 0, nrLifes;
        public Vector2 playerPos; // position of the player
        Texture2D player;
        public bool moving = false;   // are we animating?
        int frame = 0;  // current frame on tileSize animation
        Game1 game;

        public Player(Game1 game, Vector2 initialPos, int playerLifes)
        {
            playerPos = initialPos;
            this.game = game;
            player = game.Content.Load<Texture2D>("ss");
            nrLifes = playerLifes;
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
                // Validate detonation
                if (KeyManager.GetKeyDown(Keys.Space) && nrTNT > 0 && !game.detonating)
                {
                    game.startedTNT.Play(); // sound
                    nrTNT--;
                    game.detonating = true;
                    game.detonatingPos = playerPos.ToPoint();
                }

                dir = Vector2.Zero;

                if (KeyManager.GetKey(Keys.S))
                {
                    dir = Vector2.UnitY;
                    direction = Direction.DOWN;
                }
                if (KeyManager.GetKey(Keys.W))
                {
                    dir = -Vector2.UnitY;
                    direction = Direction.UP;
                }
                if (KeyManager.GetKey(Keys.D))
                {
                    dir = Vector2.UnitX;
                    direction = Direction.RIGHT;
                }
                if (KeyManager.GetKey(Keys.A))
                {
                    dir = -Vector2.UnitX;
                    direction = Direction.LEFT;
                }

                // target position is empty?
                if (dir.LengthSquared() != 0)
                {
                    moving = true; // we are mooooooving!!!

                    Point playerTargetPos = (playerPos + dir).ToPoint();
                    Point stoneTargetPos = (playerPos + dir * 2).ToPoint();

                    if (game.getStone(playerTargetPos.ToVector2()) != null)
                    {
                        if (game.getFallingEntity(stoneTargetPos.ToVector2()) == null && game.map[stoneTargetPos.Y, stoneTargetPos.X] == ' ' && 
                            dir != Vector2.UnitY && dir != -Vector2.UnitY)
                        {
                            playerPos += dir;
                            nrMoves++;
                            game.walking.Play(); // sound

                            FallingEntity s = game.getStone(playerTargetPos.ToVector2());
                            s.moving = true;
                            s.dir = dir;
                            s.fallingEntityPos = stoneTargetPos.ToVector2();
                        }
                        else { moving = false; }
                    }
                    else if (game.getDiamond(playerTargetPos.ToVector2()) != null)
                    {
                        playerPos += dir;
                        nrMoves++;

                        game.acquireDiamond.Play(); // sound
                        nrDiamonds++;
                        FallingEntity d = game.getDiamond(playerTargetPos.ToVector2());
                        game.fallingEntities.Remove(d);
                    }
                    else if (game.map[playerTargetPos.Y, playerTargetPos.X] == 'T' && !(game.detonating && playerTargetPos == game.detonatingPos))
                    {
                        playerPos += dir;
                        nrMoves++;

                        game.acquireTNT.Play(); // sound
                        nrTNT++;
                        game.map[playerTargetPos.Y, playerTargetPos.X] = ' ';
                    }
                    else if (game.map[playerTargetPos.Y, playerTargetPos.X] == ' ' || game.map[playerTargetPos.Y, playerTargetPos.X] == 'G')
                    {
                        playerPos += dir;
                        nrMoves++;
                        game.walking.Play(); // sound

                        game.map[playerTargetPos.Y, playerTargetPos.X] = ' ';
                    }
                    else { moving = false; }
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (moving)
            {
                int col = (frame % 16) / 8 + 1;
                spriteBatch.Draw(player,
                       playerPos * Game1.tileSize + dir * (frame - Game1.tileSize),
                       new Rectangle(
                           new Point(Game1.tileSize * col,
                           Game1.tileSize * (int)direction),
                           (Vector2.One * Game1.tileSize).ToPoint()),
                       Color.White);
            }
            else
            {
                spriteBatch.Draw(player, 
                        playerPos * Game1.tileSize,
                        new Rectangle(
                            new Point(0, 
                            Game1.tileSize * (int)direction),
                            (Vector2.One * Game1.tileSize).ToPoint()),
                        Color.White);
            }
        }
    }
}