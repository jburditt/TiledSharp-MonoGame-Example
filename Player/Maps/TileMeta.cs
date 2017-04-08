﻿using Player.Events;

namespace Player.Maps
{
    public class TileMeta
    {
        public Rect SpriteRect { get; set; }
        public string Tileset { get; set; }
        public bool IsBlocked { get; set; }

        public EventCollection EventCollection { get; set; } = new EventCollection();
        public NPC NPC { get; set; }
    }

    public static class TileExtensions
    {
        public static int Rows(this TileMeta[,] tiles)
        {
            return tiles.Rank > 0 ? tiles.GetLength(0) : 0;
        }

        public static int Columns(this TileMeta[,] tiles)
        {
            return tiles.Rank > 1 ? tiles.GetLength(1) : 0;
        }
    }
}
