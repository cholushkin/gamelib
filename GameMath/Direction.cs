using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLib
{
    [Serializable]
    public static class Direction2D
    {
        /*
        02 03 04      ↖ ↑ ↗ 
        01[00]05  or  ←[c]→  
        08 07 06      ↙ ↓ ↘ 
    
        iteration is in CW order ( poisitive rotation)
        */

        [Flags]
        public enum RelativeDirection
        {
            Center = 0, // same as no direction 
            Left = 1, // ←, other names: West
            LeftUp = 2, // ↖, other names: TopLeft NorthWest
            Up = 4, // ↑, other names: North, Top
            UpRight = 8, // ↗, other names: RightTop, NorthEast 
            Right = 16, // →, other names: East
            RightDown = 32, // ↘, other names: BottomRight SouthEast
            Down = 64, // ↓, other names: South, Down, Bottom
            DownLeft = 128, // ↙, other names: LeftBottom, SouthWest 
        }

        public static bool IsDirectionSet(RelativeDirection direction, RelativeDirection flag)
        {
            return (direction & flag) != 0;
        }

        public static RelativeDirection SetDirection(RelativeDirection direction, RelativeDirection flag)
        {
            return direction | flag;
        }

        public static RelativeDirection UnsetDirection(RelativeDirection direction, RelativeDirection flag)
        {
            return (direction & ~flag);
        }

        public static bool HasDirections(RelativeDirection direction)
        {
            return direction > 0;
        }

        public static int GetConnectionsCount(RelativeDirection direction)
        {
            int cnt = 0;
            if ((direction & RelativeDirection.Left) != 0)
                ++cnt;
            if ((direction & RelativeDirection.LeftUp) != 0)
                ++cnt;
            if ((direction & RelativeDirection.Up) != 0)
                ++cnt;
            if ((direction & RelativeDirection.UpRight) != 0)
                ++cnt;
            if ((direction & RelativeDirection.Right) != 0)
                ++cnt;
            if ((direction & RelativeDirection.RightDown) != 0)
                ++cnt;
            if ((direction & RelativeDirection.Down) != 0)
                ++cnt;
            if ((direction & RelativeDirection.DownLeft) != 0)
                ++cnt;
            return cnt;
        }

        // todo: diagonal directions and epsilon
        public static RelativeDirection FromVector(Vector3 v3Normalized)
        {
            if (v3Normalized == Vector3.left)
                return RelativeDirection.Left;
            if (v3Normalized == Vector3.right)
                return RelativeDirection.Right;
            if (v3Normalized == Vector3.up)
                return RelativeDirection.Up;
            if (v3Normalized == Vector3.down)
                return RelativeDirection.Down;

            return RelativeDirection.Center;
        }

        public static IEnumerable<RelativeDirection> GetValues() // starting from center, spiral clockwise direction
        {
            return Enum.GetValues(typeof(RelativeDirection)).Cast<RelativeDirection>();
        }

        public static bool IsVertical(RelativeDirection direction)
        {
            return direction == RelativeDirection.Up || direction == RelativeDirection.Down;
        }

        public static bool IsHorizontal(RelativeDirection direction)
        {
            return direction == RelativeDirection.Left || direction == RelativeDirection.Right;
        }

        public static bool IsVertical(Vector3 direction)
        {
            return (direction == Vector3.up || direction == Vector3.down);
        }

        public static bool IsHorizontal(Vector3 direction)
        {
            return !IsVertical(direction);
        }

        public static RelativeDirection Opposite(RelativeDirection dir)
        {
            if (dir == RelativeDirection.Left)
                return RelativeDirection.Right;
            if (dir == RelativeDirection.LeftUp)
                return RelativeDirection.RightDown;
            if (dir == RelativeDirection.Up)
                return RelativeDirection.Down;
            if (dir == RelativeDirection.UpRight)
                return RelativeDirection.DownLeft;
            if (dir == RelativeDirection.Right)
                return RelativeDirection.Left;
            if (dir == RelativeDirection.RightDown)
                return RelativeDirection.LeftUp;
            if (dir == RelativeDirection.Down)
                return RelativeDirection.Up;
            if (dir == RelativeDirection.DownLeft)
                return RelativeDirection.UpRight;
            return RelativeDirection.Center;
        }

        public static Vector2Int Offset(RelativeDirection direction, Vector2Int fromPos = new Vector2Int())
        {
            switch (direction)
            {
                case RelativeDirection.DownLeft:
                    return fromPos + new Vector2Int(-1, -1);
                case RelativeDirection.Down:
                    return fromPos + new Vector2Int(0, -1);
                case RelativeDirection.RightDown:
                    return fromPos + new Vector2Int(1, -1);
                case RelativeDirection.Left:
                    return fromPos + new Vector2Int(-1, 0);
                case RelativeDirection.Right:
                    return fromPos + new Vector2Int(1, 0);
                case RelativeDirection.LeftUp:
                    return fromPos + new Vector2Int(-1, 1);
                case RelativeDirection.Up:
                    return fromPos + new Vector2Int(0, 1);
                case RelativeDirection.UpRight:
                    return fromPos + new Vector2Int(1, 1);
                default:
                    return fromPos;
            }
        }

        public static TextAnchor ToTextAnchor(RelativeDirection direction)
        {
            switch (direction)
            {
                case RelativeDirection.LeftUp:
                    return TextAnchor.UpperLeft;
                case RelativeDirection.Up:
                    return TextAnchor.UpperCenter;
                case RelativeDirection.UpRight:
                    return TextAnchor.UpperRight;

                case RelativeDirection.Left:
                    return TextAnchor.MiddleLeft;
                case RelativeDirection.Right:
                    return TextAnchor.MiddleRight;
                case RelativeDirection.Center:
                    return TextAnchor.MiddleCenter;

                case RelativeDirection.DownLeft:
                    return TextAnchor.LowerLeft;
                case RelativeDirection.Down:
                    return TextAnchor.LowerCenter;
                case RelativeDirection.RightDown:
                    return TextAnchor.LowerRight;
                
                default:
                    throw new System.NotImplementedException("Didn't account for " + direction.ToString());
            }
        }
    }
}
