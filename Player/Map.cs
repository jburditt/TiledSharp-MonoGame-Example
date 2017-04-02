﻿using Common;
using DataStore;
using Player.Events;
using Player.Manager;
using System.Collections.Generic;
using System.Linq;
using TiledSharp;

namespace Player
{
    public class Map
    {
        private readonly IIconManager _iconManager;
        private readonly ITilesetManager _tilesetManager;
        private readonly IDataStore _dataStore;

        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        public int Layers { get; set; }

        public TmxMap TiledMap { get; set; }
        public bool[][][] Passable { get; set; }
        public Tile[][][] Tiles { get; set; }
        public List<NPC> NPC { get; set; } = new List<NPC>();

        public Vector Camera;

        int windowTilesWide, windowTilesHigh;

        public Map(IDataStore dataStore, IIconManager iconManager, ITilesetManager tilesetManager, string map, bool isMonoGame = false)
        {
            _dataStore = dataStore;
            _iconManager = iconManager;
            _tilesetManager = tilesetManager;

            TiledMap = new TmxMap(map);

            TileWidth = TiledMap.Tilesets[0].TileWidth;              // width of tile in pixels
            TileHeight = TiledMap.Tilesets[0].TileHeight;            // height of tile

            windowTilesWide = Screen.Width / TileWidth;         // number of columns in window
            windowTilesHigh = Screen.Height / TileHeight;       // number of rows in window

            Width = TiledMap.Width * TileWidth;                      // width of map in pixels
            Height = TiledMap.Height * TileHeight;                   // height of map

            Columns = TiledMap.Width;
            Rows = TiledMap.Height;
            Layers = TiledMap.Layers.Count;

            var tilesets = TiledMap.Tilesets;

            string[] tilesetNames;
            if (isMonoGame)
                tilesetNames = tilesets.Select(n => n.Image.Source).ToList().ToFileList().Select(n => n.Name).ToArray();
            else
                tilesetNames = tilesets.Select(n => n.Image.Source).ToArray();
            tilesetManager.Load(tilesetNames);

            Tiles = Load();
        }

        public Tile[][][] Load()
        {
            var tiles = new Tile[TiledMap.Width][][];

            Passable = _dataStore.Load<bool[][][]>("world2.passable");

            for (var x = 0; x < TiledMap.Width; x++)
            {
                tiles[x] = new Tile[TiledMap.Height][];

                for (var y = 0; y < TiledMap.Height; y++)
                {
                    tiles[x][y] = new Tile[TiledMap.Layers.Count];

                    for (var layer = 0; layer < TiledMap.Layers.Count; layer++)
                    {
                        int tileIndex = x + y * TiledMap.Width;

                        int gid = TiledMap.Layers[layer].Tiles[tileIndex].Gid;
                        if (gid == 0)
                            continue;

                        int tilesetIndex = 0;
                        int tileCount = 0;
                        int row = 0, column = 0;

                        // get tilesetIndex and position in tileset from gid
                        for (var i = 0; i < TiledMap.Tilesets.Count; i++)
                        {
                            if (gid < TiledMap.Tilesets[i].TileCount + tileCount)
                            {
                                tilesetIndex = i;
                                int tileFrame = gid - tileCount - 1;
                                int columns = TiledMap.Tilesets[i].Columns ?? 1;
                                column = tileFrame % columns;
                                row = (tileFrame) / columns;
                                break;
                            }

                            // make sure we tally the tile count, so we know what to subtract from gid
                            tileCount += TiledMap.Tilesets[i].TileCount ?? 0;
                        }

                        var tileWidth = TiledMap.Tilesets[0].TileWidth;
                        var tileHeight = TiledMap.Tilesets[0].TileHeight;

                        tiles[x][y][layer] = new Tile
                        {
                            SpriteRect = new Rect(tileWidth * column, tileHeight * row, tileWidth, tileHeight),
                            Tileset = tilesetIndex,
                            IsPassable = Passable[tilesetIndex][column][row]
                        };
                    }
                }
            }

            return tiles;
        }

        public bool IsCollision(Vector pos, Direction direction)
        {
            var x = pos.X / TileWidth;
            var y = pos.Y / TileHeight;

            var isCollision = false;

            switch (direction)
            {
                case Direction.Up:
                    isCollision = IsTileCollision(pos.X, pos.Y, x, y) || (pos.X % TileWidth != 0 && IsTileCollision(pos.X, pos.Y, x + 1, y));
                    return isCollision;
                case Direction.Right:
                    isCollision = IsTileCollision(pos.X, pos.Y, x + 1, y) || (pos.X % TileHeight != 0 && IsTileCollision(pos.X, pos.Y, x + 1, y + 1));
                    return isCollision;
                case Direction.Down:
                    isCollision = IsTileCollision(pos.X, pos.Y, x, y + 1) || (pos.X % TileWidth != 0 && IsTileCollision(pos.X, pos.Y, x + 1, y + 1));
                    return isCollision;
                case Direction.Left:
                    isCollision = IsTileCollision(pos.X, pos.Y, x, y) || (pos.Y % TileHeight != 0 && IsTileCollision(pos.X, pos.Y, x, y + 1));
                    return isCollision;
            }

            return false;
        }

        public bool IsTileCollision(int playerX, int playerY, int x, int y)
        {
            // don't check tiles out of bounds
            if (x < 0 || x >= Columns || y < 0 || y >= Height)
                return true;

            for (int layerIndex = 0; layerIndex < Layers; layerIndex++)
            {
                var tile = Tiles[x][y][layerIndex];

                if (tile != null && !tile.IsPassable)
                {
                    var playerRect = new Rect(playerX, playerY + 8, 24, 24);
                    var tileRect = new Rect(x * TileWidth, y * TileHeight, TileWidth, TileHeight);

                    if (playerRect.Intersects(tileRect))
                        return true;
                }
            }

            return false;
        }

        public void Draw(Vector player)
        {
            for (var y = 0; y <= windowTilesHigh; y++)
                for (var x = 0; x <= windowTilesWide; x++)
                {
                    // don't draw extra tile if on the boundary
                    if (player.Y >= Height - Screen.HalfHeight && y == windowTilesHigh)
                        break;
                    if (player.X >= Width - Screen.HalfWidth && x == windowTilesWide)
                        continue;

                    int playerTileX, playerTileY;
                    int offsetX = 0, offsetY = 0;

                    // handle player X coordinate
                    if (player.X < Screen.HalfWidth)
                    {
                        // offset player when near the left boundary of the map
                        playerTileX = 0;
                    }
                    else if (player.X > Width - Screen.HalfWidth)
                    {
                        // offset player when near the right boundary of the map
                        playerTileX = (Width - Screen.Width) / TileWidth;
                    }
                    else
                    {
                        // position the player in the middle of the screen
                        playerTileX = (player.X - Screen.HalfWidth) / TileWidth;
                        offsetX = player.X % TileWidth;
                    }

                    // handle player Y coordinate
                    if (player.Y < Screen.HalfHeight)
                    {
                        // offset player when near the top boundary of the map
                        playerTileY = 0;
                    }
                    else if (player.Y > Height - Screen.HalfHeight)
                    {
                        // offset player when near the bottom boundary of the map
                        playerTileY = (Height - Screen.Height) / TileHeight;
                    }
                    else
                    {
                        // position the player in the middle of the screen
                        playerTileY = (player.Y - Screen.HalfHeight) / TileHeight;
                        offsetY = (player.Y + 16) % TileHeight;
                    }

                    // draw all tile layers
                    for (var layer = 0; layer < Layers; layer++)
                    {
                        var tile = Tiles[x + playerTileX][y + playerTileY][layer];

                        if (tile != null)
                        {
                            var drawRect = new Rect(x * TileWidth - offsetX, y * TileHeight - offsetY, TileWidth, TileHeight);

                            _tilesetManager.Draw(tile.Tileset, drawRect, tile.SpriteRect);

                            if (layer == 0)
                                DrawEventCollection(tile.EventCollection, new Vector(x + playerTileX, y + playerTileY));
                        }
                    }
                }
        }

        public void DrawTile(int x, int y)
        {
            for (var layer = 0; layer < Layers; layer++)
            {
                var tile = Tiles[x][y][layer];

                if (tile != null)
                {
                    var drawRect = new Rect(x * TileWidth, y * TileHeight, TileWidth, TileHeight);

                    _tilesetManager.Draw(tile.Tileset, drawRect, tile.SpriteRect);
                }
            }
        }

        public void DrawEventCollection(EventCollection n, Vector vector)
        {
            if (n == null)
                return;

            foreach (var e in n)
            {
                foreach (var eventPage in e.EventPages)
                {
                    if (eventPage.TriggerCollection.IsTriggered())
                        DrawEvent(eventPage, vector);
                }
            }
        }

        public void DrawEvent(EventPage eventPage, Vector vector)
        {
            switch (eventPage.ImageType)
            {
                case ImageType.Icon:
                    _iconManager.Draw(eventPage.ImageKey, vector);
                    break;
                //case ImageType.Tileset:
                //    _tilesetManager.Draw(eventPage.ImageKey, )
            }
        }

        public void DrawWorld()
        {
            for (var x = 0; x < Columns; x++)
                for (var y = 0; y < Rows; y++)
                    DrawTile(x, y);
        }

        public VectorF Transform(VectorF p)
        {
            float x = Screen.HalfWidth;
            if (p.X < Screen.HalfWidth) x = p.X;
            if (p.X > Width - Screen.HalfWidth)
                x = p.X - Screen.Width - Screen.HalfWidth;

            float y = Screen.HalfHeight;
            if (p.Y < Screen.HalfHeight) y = p.Y;
            if (p.Y > Height - Screen.HalfHeight)
                y = p.Y - Height + Screen.Height;

            return new VectorF(x, y);
        }

        public void UpdateCamera(Vector pos)
        {
            if (pos.X < Screen.HalfWidth)
            {
                Camera.X = 0;
            }
            else if (pos.X > Width - Screen.HalfWidth)
            {
                Camera.X = Width - Screen.Width;
            }
            else
            {
                Camera.X = pos.X - Screen.HalfWidth;
            }

            if (pos.Y < Screen.HalfHeight)
            {
                Camera.Y = 0;
            }
            else if (pos.Y > Height - Screen.HalfHeight)
            {
                Camera.Y = Height - Screen.Height;
            }
            else
            {
                Camera.Y = pos.Y - Screen.HalfHeight;
            }
        }
    }
}