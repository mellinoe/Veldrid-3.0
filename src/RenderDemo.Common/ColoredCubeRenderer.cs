﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class ColoredCubeRenderer : SwappableRenderItem, IDisposable
    {
        private static VertexBuffer s_vb0;
        private static VertexBuffer s_vb1;
        private static IndexBuffer s_ib;
        private static Material s_material;
        private static ConstantBuffer s_modelViewBuffer;
        private static RenderContext s_currentContext;

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.One * .75f;

        public ColoredCubeRenderer(AssetDatabase ad, RenderContext context)
        {
            if (context != s_currentContext)
            {
                if (s_currentContext == null)
                {
                    InitializeContextObjects(context);
                }
                else
                {
                    ChangeRenderContext(ad, context);
                }
            }
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext context)
        {
            if (s_currentContext != context)
            {
                Dispose();
                InitializeContextObjects(context);
            }
        }

        private void InitializeContextObjects(RenderContext context)
        {
            s_currentContext = context;
            ResourceFactory factory = context.ResourceFactory;

            s_vb0 = factory.CreateVertexBuffer(12 * s_cubeVertices.Length, false);
            VertexDescriptor desc = new VertexDescriptor(12, 1, 0, IntPtr.Zero);
            s_vb0.SetVertexData(s_cubeVertices.Select(vpc => vpc.Position).ToArray(), desc);

            s_vb1 = factory.CreateVertexBuffer(16 * s_cubeVertices.Length, false);
            VertexDescriptor desc2 = new VertexDescriptor(16, 1, 0, IntPtr.Zero);
            s_vb1.SetVertexData(s_cubeVertices.Select(vpc => vpc.Color).ToArray(), desc2);

            s_ib = factory.CreateIndexBuffer(s_cubeIndices, false);

            VertexInputDescription materialInputs0 = new VertexInputDescription(
                12,
                new VertexInputElement[]
                {
                    new VertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                });

            VertexInputDescription materialInputs1 = new VertexInputDescription(
                16,
                new VertexInputElement[]
                {
                    new VertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Float4)
                });

            ShaderResourceDescription[] constants = new[]
            {
                new ShaderResourceDescription("ProjectionMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderResourceDescription("ModelViewMatrixBuffer", ShaderConstantType.Matrix4x4)
            };

            s_material = factory.CreateMaterial(
                context,
                VertexShaderSource,
                FragmentShaderSource,
                materialInputs0,
                materialInputs1,
                constants);

            s_modelViewBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);
        }

        public IList<string> GetStagesParticipated() => CommonStages.Standard;

        public void Render(RenderContext rc, string pipelineStage)
        {
            float rotationAmount = (float)DateTime.Now.TimeOfDay.TotalMilliseconds / 1000;
            var mvData =
                Matrix4x4.CreateScale(Scale)
                * Matrix4x4.CreateRotationX((rotationAmount * .5f) * Position.X)
                * Matrix4x4.CreateRotationY(rotationAmount)
                * Matrix4x4.CreateRotationZ((rotationAmount * .33f) * Position.Z)
                * Matrix4x4.CreateTranslation(Position)
                * Matrix4x4.CreateTranslation((float)Math.Sin(rotationAmount) * Vector3.UnitY)
                * SharedDataProviders.GetProvider<Matrix4x4>("ViewMatrix").Data;
            s_modelViewBuffer.SetData(ref mvData, 64);
            rc.SetVertexBuffer(0, s_vb0);
            rc.SetVertexBuffer(1, s_vb1);
            rc.IndexBuffer = s_ib;
            s_material.Apply(rc);
            rc.SetConstantBuffer(0, SharedDataProviders.ProjectionMatrixBuffer);
            rc.SetConstantBuffer(1, s_modelViewBuffer);
            rc.DrawIndexedPrimitives(s_cubeIndices.Length, 0);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return new RenderOrderKey();
        }

        public void Dispose()
        {
            s_vb0.Dispose();
            s_vb1.Dispose();
            s_ib.Dispose();
            s_material.Dispose();
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            float radius = 3f * (Scale.X * Scale.X) / 2f;
            var boundingSphere = new BoundingSphere(Position, radius);
            return visibleFrustum.Contains(boundingSphere) == ContainmentType.Disjoint;
        }

        private static readonly VertexPositionColor[] s_cubeVertices = new VertexPositionColor[]
        {
            // Top
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Red),
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.Red),
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.Red),
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Red),
            // Bottom
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.Grey),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Grey),
            // Left
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Blue),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Blue),
            // Right
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.White),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.White),
            // Back
            new VertexPositionColor(new Vector3(.5f,.5f,-.5f),     RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(-.5f,.5f,-.5f),    RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(-.5f,-.5f,-.5f),   RgbaFloat.Yellow),
            new VertexPositionColor(new Vector3(.5f,-.5f,-.5f),    RgbaFloat.Yellow),
            // Front
            new VertexPositionColor(new Vector3(-.5f,.5f,.5f),     RgbaFloat.Green),
            new VertexPositionColor(new Vector3(.5f,.5f,.5f),      RgbaFloat.Green),
            new VertexPositionColor(new Vector3(.5f,-.5f,.5f),     RgbaFloat.Green),
            new VertexPositionColor(new Vector3(-.5f,-.5f,.5f),    RgbaFloat.Green)
        };

        private static readonly ushort[] s_cubeIndices = new ushort[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };

        private static readonly string VertexShaderSource = "simple-vertex";
        private static readonly string FragmentShaderSource = "simple-frag";
    }
}
