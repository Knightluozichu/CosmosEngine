﻿using System;
using FixMath.NET;
namespace Cosmos
{
    public struct RectangleFix64 : IEquatable<RectangleFix64>
    {
        public Fix64 CenterX { get; set; }
        public Fix64 CenterY { get; set; }
        public Fix64 Width { get; set; }
        public Fix64 Height { get; set; }
        public Fix64 Top { get { return CenterY + HalfHeight; } }
        public Fix64 Bottom { get { return CenterY - HalfHeight; } }
        public Fix64 Left { get { return CenterX - HalfWidth; } }
        public Fix64 Right { get { return CenterX + HalfWidth; } }
        public Fix64 HalfWidth { get { return Width * (Fix64)0.5f; } }
        public Fix64 HalfHeight { get { return Height * (Fix64)0.5f; } }
        public RectangleFix64(Fix64 x, Fix64 y, Fix64 width, Fix64 height)
        {
            CenterX = x;
            CenterY = y;
            Width = width;
            Height = height;
        }
        public bool Contains(Fix64 x, Fix64 y)
        {
            if (x < Left || x > Right) return false;
            if (y > Top || y < Bottom) return false;
            return true;
        }
        public static bool operator ==(RectangleFix64 a, RectangleFix64 b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(RectangleFix64 a, RectangleFix64 b)
        {
            return !a.Equals(b);
        }
        public override bool Equals(object obj)
        {
            return obj is RectangleFix64 && Equals((RectangleFix64)obj);
        }
        public bool Equals(RectangleFix64 other)
        {
            return this.CenterX == other.CenterX && this.CenterY == other.CenterY &&
                  this.Width == other.Width && this.Height == other.Height;
        }
        public override int GetHashCode()
        {
            var hashStr = $"{CenterX}{CenterY}{Width}{Height}";
            return hashStr.GetHashCode();
        }
        public override string ToString()
        {
            return $"[ X:{CenterX} ,Y:{CenterY} ],[ Width:{Width},Height:{Height} ]";
        }
        public static readonly RectangleFix64 Zero = new RectangleFix64(Fix64.Zero, Fix64.Zero, Fix64.Zero, Fix64.Zero);
        public static readonly RectangleFix64 One = new RectangleFix64(Fix64.One, Fix64.One, Fix64.One, Fix64.One);
    }
}

