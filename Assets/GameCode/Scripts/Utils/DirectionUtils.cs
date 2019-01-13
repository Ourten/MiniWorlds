using System;
using UnityEngine;

namespace GameCode.Scripts.Utils
{
    public static class DirectionUtils
    {
        public static Vector3Int Add(this Vector3Int vec, Direction dir)
        {
            var offset = new Vector3Int(vec.x, vec.y, vec.z);
            
            switch (dir)
            {
                case Direction.NORTH:
                    offset.z--;
                    break;
                case Direction.EAST:
                    offset.x++;
                    break;
                case Direction.SOUTH:
                    offset.z++;
                    break;
                case Direction.WEST:
                    offset.x--;
                    break;
                case Direction.UP:
                    offset.y++;
                    break;
                case Direction.DOWN:
                    offset.y--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }

            return offset;
        }
    }
}