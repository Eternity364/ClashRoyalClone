using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pathfinding
{
    public class GridDirection
    {
        public readonly Vector2Int Vector;

        private GridDirection(int x, int y)
        {
            Vector = new Vector2Int(x, y);
        }

        public static implicit operator Vector2Int(GridDirection dir) => dir.Vector;

        public static GridDirection GetDirectionFromV2I(Vector2Int vector)
        {
            return CardinalAndIntercardinalDirections.DefaultIfEmpty(None).FirstOrDefault(direction => direction == vector);
        }

        public static readonly GridDirection None = new GridDirection(0, 0);
        public static readonly GridDirection North = new GridDirection(0, 1);
        public static readonly GridDirection East = new GridDirection(1, 0);
        public static readonly GridDirection South = new GridDirection(0, -1);
        public static readonly GridDirection West = new GridDirection(-1, 0);
        public static readonly GridDirection NorthEast = new GridDirection(1, 1);
        public static readonly GridDirection NorthWest = new GridDirection(-1, 1);
        public static readonly GridDirection SouthEast = new GridDirection(1, -1);
        public static readonly GridDirection SouthWest = new GridDirection(-1, -1);

        public static readonly List<GridDirection> CardinalDirections = new List<GridDirection>
        {
            North,
            East,
            South,
            West
        };

        public static readonly List<GridDirection> CardinalAndIntercardinalDirections = new List<GridDirection>
        {
            East,
            NorthEast,
            North,
            NorthWest,
            West,
            SouthWest,
            South,
            SouthEast
        };

        public static readonly List<GridDirection> AllDirections = new List<GridDirection>
        {
            None,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        };

        // Return degrees where 0 = East (positive X). Handles several common representations.
        public float GetAngle()
        {
            float multiplier = 360f / CardinalAndIntercardinalDirections.Count;
            return multiplier * CardinalAndIntercardinalDirections.IndexOf(this);
        }
    }
}
