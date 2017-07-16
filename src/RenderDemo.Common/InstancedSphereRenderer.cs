﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class InstancedSphereRenderer : SwappableRenderItem
    {
        public const int InstanceRows = 20;
        public const int InstanceCount = InstanceRows * InstanceRows * InstanceRows;

        private VertexBuffer _sphereGeometryVB;
        private VertexBuffer _instanceVB;
        private IndexBuffer _ib;
        private Material _material;
        private ConstantBuffer _worldBuffer;
        private int _indexCount;

        private static readonly string[] s_stages = { "Standard" };

        public InstancedSphereRenderer(AssetDatabase ad, RenderContext rc)
        {
            InitializeContextObjects(ad, rc);
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            ClearDeviceResources();
            InitializeContextObjects(ad, rc);
        }

        private void ClearDeviceResources()
        {
            _sphereGeometryVB.Dispose();
            _instanceVB.Dispose();
            _ib.Dispose();
            _material.Dispose();
        }

        private void InitializeContextObjects(AssetDatabase ad, RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            MeshData sphere = ad.LoadAsset<ObjFile>(new AssetID("Models/Sphere.obj")).GetFirstMesh();
            Vector3[] spherePositions = sphere.GetVertexPositions();
            _sphereGeometryVB = factory.CreateVertexBuffer(spherePositions.Length * 12, false);
            _sphereGeometryVB.SetVertexData(spherePositions, new VertexDescriptor(12, 1));
            _ib = sphere.CreateIndexBuffer(factory, out _indexCount);

            Random r = new Random();
            int width = InstanceRows;
            InstanceData[] instanceData = new InstanceData[width * width * width];
            for (int z = 0; z < width; z++)
            {
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        instanceData[z * width * width + y * width + x] = new InstanceData(
                            new Vector3(x * 10, y * 10, z * 10),
                            new RgbaFloat((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()));
                    }
                }
            }

            _instanceVB = factory.CreateVertexBuffer(instanceData.Length * InstanceData.SizeInBytes, false);
            _instanceVB.SetVertexData(instanceData, new VertexDescriptor(InstanceData.SizeInBytes, 2, 0, IntPtr.Zero));

            Shader vs = factory.CreateShader(ShaderType.Vertex, ShaderHelper.LoadShaderCode("instanced-simple-vertex", ShaderType.Vertex, rc.ResourceFactory));
            Shader fs = factory.CreateShader(ShaderType.Fragment, ShaderHelper.LoadShaderCode("instanced-simple-frag", ShaderType.Fragment, rc.ResourceFactory));
            VertexInputLayout inputLayout = factory.CreateInputLayout(
                new VertexInputDescription(VertexPosition.SizeInBytes, new VertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3)),
                new VertexInputDescription(
                    InstanceData.SizeInBytes,
                    new VertexInputElement("in_offset", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float3, VertexElementInputClass.PerInstance, 1),
                    new VertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Float4, VertexElementInputClass.PerInstance, 1)));
            ShaderSet shaderSet = factory.CreateShaderSet(inputLayout, vs, fs);
            ShaderResourceBindingSlots constantBindings = factory.CreateShaderConstantBindingSlots(
                shaderSet,
                new ShaderResourceDescription("ProjectionMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderResourceDescription("ViewMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderResourceDescription("WorldMatrixBuffer", ShaderConstantType.Matrix4x4));
            _material = new Material(shaderSet, constantBindings);
            _worldBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);
            Matrix4x4 identity = Matrix4x4.Identity;
            _worldBuffer.SetData(ref identity, 64);
        }

        private struct InstanceData
        {
            public const byte SizeInBytes = 28;
            public readonly Vector3 Position;
            public readonly RgbaFloat Color;
            public InstanceData(Vector3 position, RgbaFloat color) { Position = position; Color = color; }
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return false;
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return RenderOrderKey.Create(_material.GetHashCode());
        }

        public IList<string> GetStagesParticipated()
        {
            return s_stages;
        }

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.SetVertexBuffer(0, _sphereGeometryVB);
            rc.SetVertexBuffer(1, _instanceVB);
            rc.IndexBuffer = _ib;
            _material.Apply(rc);
            rc.SetConstantBuffer(0, SharedDataProviders.ProjectionMatrixBuffer);
            rc.SetConstantBuffer(1, SharedDataProviders.ViewMatrixBuffer);
            rc.SetConstantBuffer(2, _worldBuffer);
            rc.DrawInstancedPrimitives(_indexCount, InstanceCount, 0);
        }
    }
}
