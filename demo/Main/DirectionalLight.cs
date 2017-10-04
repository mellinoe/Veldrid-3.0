﻿using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid.Graphics;

namespace Veldrid.NeoDemo
{
    public class DirectionalLight
    {
        private RgbaFloat _color = RgbaFloat.White;
        public Transform Transform { get; } = new Transform();

        public Vector3 Direction => Transform.Forward;

        public event Action<RgbaFloat> ColorChanged;

        public RgbaFloat Color { get => _color; set { _color = value; ColorChanged?.Invoke(value); } }

        public DirectionalLight()
        {
            Vector3 lightDir = Vector3.Normalize(new Vector3(-0.3f, -1f, -0.3f));
            Transform.Rotation = Utilities.FromToRotation(-Vector3.UnitZ, lightDir);
        }

        internal DirectionalLightInfo GetInfo()
        {
            return new DirectionalLightInfo { Direction = Transform.Forward, Color = Color.ToVector4() };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DirectionalLightInfo
    {
        public Vector3 Direction;
        private float _padding;
        public Vector4 Color;
    }
}