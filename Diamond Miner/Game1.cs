using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.IO;
using System;

/* O código usado neste projeto é totalmente baseado no código criado pelo professor para o jogo Sokoban 
   Todos os efeitos de som presentes neste jogo, foram obtidos na biblioteca de áudio gratuita do Youtube */

namespace Diamond_Miner
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public char[,] map;
        int width, height; // width and height of the map (lines/columns)
        public const int tileSize = 64; // size of each tile texture
        Texture2D wall, gravel, tnt, end;
        SpriteFont arialBlack30;
        string[] levels = { "level0.txt", "gameover.txt" };
        int currentLevel = 0;
        float levelTime;
        public Player player;
        public List<FallingEntity> fallingEntities;
        public Vector2 endPos;
        bool isReloading = false;
        public bool detonating = false;
        public Point detonatingPos;
        float tntDelay;
        public SoundEffect walking, stoneNoise, acquireDiamond, acquireTNT, startedTNT, explosion, winLevel, loosingLife;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            new KeyManager();
            LoadLevel(levels[currentLevel], 3);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            wall = Content.Load<Texture2D>("Wall_Black");
            gravel = Content.Load<Texture2D>("GroundGravel_Dirt");
            tnt = Content.Load<Texture2D>("Crate_Red");
            end = Content.Load<Texture2D>("Ground_Grass");

            arialBlack30 = Content.Load<SpriteFont>("ArialBlack30");

            walking = Content.Load<SoundEffect>("soundeffects/walking");
            stoneNoise = Content.Load<SoundEffect>("soundeffects/stoneNoise");
            acquireDiamond = Content.Load<SoundEffect>("soundeffects/acquireDiamond");
            acquireTNT = Content.Load<SoundEffect>("soundeffects/acquireTNT");
            startedTNT = Content.Load<SoundEffect>("soundeffects/startedTNT");
            explosion = Content.Load<SoundEffect>("soundeffects/explosion");
            winLevel = Content.Load<SoundEffect>("soundeffects/winLevel");
            loosingLife = Content.Load<SoundEffect>("soundeffects/loosingLife");
        }
        
        protected override void Update(GameTime gameTime)
        {
            // level time
            levelTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!player.moving)
            {
                // Validate victory
                if (player.playerPos == endPos && diamondsLeft() <= 0) // if player is in door and has all diamonds
                {
                    winLevel.Play(); // sound

                    currentLevel++;
                    if (currentLevel == levels.Length)
                    {
                        Exit();
                    }
                    else
                    {
                        LoadLevel(levels[currentLevel], player.nrLifes);
                    }
                }

                FallingEntity diamondToRemove = null;
                foreach (FallingEntity fe in fallingEntities)
                {
                    if (fe.fallingEntityPos == player.playerPos)
                    {
                        // test loss of lives
                        if (fe.type.Equals(FallingEntity.Type.STONE))
                        {
                            looseLife();
                        }
                        // test acquisition diamonds
                        else
                        {
                            acquireDiamond.Play(); // sound
                            player.nrDiamonds++;
                            diamondToRemove = fe;
                            break;
                        }
                    }
                }
                if (diamondToRemove != null)
                    fallingEntities.Remove(diamondToRemove);
            }

            // Test detonating
            if (detonating)
            {
                map[detonatingPos.Y, detonatingPos.X] = 'T';
                if (tntDelay < 3)
                {
                    tntDelay += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else
                {
                    explosion.Play(); // sound
                    for (int i = detonatingPos.Y - 1; i <= detonatingPos.Y + 1; i++)
                    {
                        for (int j = detonatingPos.X - 1; j <= detonatingPos.X + 1; j++)
                        {
                            if (map[i, j] != ' ' && i > 0 && i < height - 1 && j > 0 && j < width - 1)
                                map[i, j] = ' ';
                            if (getFallingEntity(new Vector2(j, i)) != null)
                                fallingEntities.Remove(getFallingEntity(new Vector2(j, i)));
                            if (player.playerPos == new Vector2(j, i))
                                looseLife();
                        }
                    }

                    tntDelay = 0;
                    detonating = false;
                }
            }

            if (!isReloading)
            {
                if (KeyManager.GetKeyDown(Keys.R))
                {
                    LoadLevel(levels[currentLevel], player.nrLifes);
                    isReloading = true;  // make sure we do not reload the level while user presses key
                }
            }
            else
            {
                isReloading = Keyboard.GetState().GetPressedKeys().Length != 0;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || KeyManager.GetKeyDown(Keys.Escape))
                Exit();

            foreach (FallingEntity fe in fallingEntities) { fe.Update(gameTime); }

            player.Update(gameTime);
            KeyManager.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    switch (map[j, i])
                    {
                        case 'W':
                            spriteBatch.Draw(wall, new Vector2(i, j) * tileSize, Color.White);
                            break;
                        case 'G':
                            spriteBatch.Draw(gravel, new Vector2(i, j) * tileSize, Color.White);
                            break;
                        case 'T':
                            if (detonating)
                            {
                                int col;
                                if (tntDelay < 1) col = 1;
                                else if (tntDelay < 2) col = 2;
                                else col = 3;
                                spriteBatch.Draw(tnt,
                                       new Vector2(i, j) * Game1.tileSize,
                                       new Rectangle(
                                           (new Vector2(col, 0) * Game1.tileSize).ToPoint(),
                                           (Vector2.One * Game1.tileSize).ToPoint()),
                                       Color.White);
                            }
                            else
                            {
                                spriteBatch.Draw(tnt,
                                        new Vector2(i, j) * Game1.tileSize,
                                        new Rectangle(
                                            Point.Zero,
                                            (Vector2.One * Game1.tileSize).ToPoint()),
                                        Color.White);
                            }
                            break;
                    }
                }
            }

            // Draw fallingEntities
            foreach (FallingEntity fe in fallingEntities) { fe.Draw(gameTime, spriteBatch); }

            // End
            spriteBatch.Draw(end, endPos * tileSize, Color.White);

            // Draw player
            player.Draw(gameTime, spriteBatch);

            #region UI
            // --- UI --- 
            int uiVerticalPos = height * tileSize + 3;
            spriteBatch.DrawString(arialBlack30,
                String.Format("Level {0}/{1}", currentLevel + 1, levels.Length),
                  new Vector2(2, uiVerticalPos), Color.DarkOrange);

            string text = String.Format("{0} Lifes", player.nrLifes);
            Vector2 dim = arialBlack30.MeasureString(text);
            spriteBatch.DrawString(arialBlack30, text,
                new Vector2((width * tileSize - dim.X) / 5, uiVerticalPos),
                Color.DarkOrange);

            text = String.Format("{0} Diamonds", player.nrDiamonds);
            dim = arialBlack30.MeasureString(text);
            spriteBatch.DrawString(arialBlack30, text,
                new Vector2((width * tileSize - dim.X) / 5 * 2, uiVerticalPos),
                Color.DarkOrange);

            text = String.Format("{0} TNT's", player.nrTNT);
            dim = arialBlack30.MeasureString(text);
            spriteBatch.DrawString(arialBlack30, text,
                new Vector2((width * tileSize - dim.X) / 5 * 3, uiVerticalPos),
                Color.DarkOrange);

            text = String.Format("{0} Moves", player.nrMoves);
            dim = arialBlack30.MeasureString(text);
            spriteBatch.DrawString(arialBlack30, text,
                new Vector2((width * tileSize - dim.X) / 5 * 4, uiVerticalPos),
                Color.DarkOrange);

            text = String.Format("Level Time {0:F0} s", levelTime);
            dim = arialBlack30.MeasureString(text);
            spriteBatch.DrawString(arialBlack30,
                text, new Vector2(width * tileSize - (dim.X + 2),
                                   uiVerticalPos),
                Color.DarkOrange);
            #endregion


            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void LoadLevel(string name, int playerLives)
        {
            fallingEntities = new List<FallingEntity>();
            string[] lines = File.ReadAllLines("Content/" + name);
            height = lines.Length;
            width = lines[0].Length;
            map = new char[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    map[i, j] = lines[i][j];
                    if (map[i, j] == 'S')
                    {
                        fallingEntities.Add(new FallingEntity(this, new Vector2(j, i), FallingEntity.Type.STONE));
                        map[i, j] = ' ';
                    }
                    if (map[i, j] == 'D')
                    {
                        fallingEntities.Add(new FallingEntity(this, new Vector2(j, i), FallingEntity.Type.DIAMOND));
                        map[i, j] = ' ';
                    }
                    if (map[i, j] == 'P')
                    {
                        player = new Player(this, new Vector2(j, i), playerLives);
                        map[i, j] = ' ';
                    }
                    if (map[i, j] == 'E')
                    {
                        endPos = new Vector2(j, i);
                        map[i, j] = ' ';
                    }
                }
            }
            graphics.PreferredBackBufferHeight = height * tileSize + 32;
            graphics.PreferredBackBufferWidth = width * tileSize;
            graphics.ApplyChanges();
            levelTime = 0f;
        }

        public FallingEntity getDiamond(Vector2 coord)
        {
            foreach (FallingEntity fe in fallingEntities)
                if (fe.fallingEntityPos == coord && fe.type.Equals(FallingEntity.Type.DIAMOND))
                    return fe;

            return null;
        }

        public FallingEntity getStone(Vector2 coord)
        {
            foreach (FallingEntity fe in fallingEntities)
                if (fe.fallingEntityPos == coord && fe.type.Equals(FallingEntity.Type.STONE))
                    return fe;

            return null;
        }

        public FallingEntity getFallingEntity(Vector2 coord)
        {
            foreach (FallingEntity fe in fallingEntities)
                if (fe.fallingEntityPos == coord)
                    return fe;

            return null;
        }

        int diamondsLeft()
        {
            int numDiamonds = 0;
            foreach (FallingEntity fe in fallingEntities)
                if (fe.type.Equals(FallingEntity.Type.DIAMOND))
                    numDiamonds++;

            return numDiamonds;
        }

        void looseLife()
        {
            loosingLife.Play(); // sound
            player.nrLifes--;

            if (player.nrLifes <= 0)
                currentLevel = levels.Length - 1;

            LoadLevel(levels[currentLevel], player.nrLifes);
        }
    }    
}
