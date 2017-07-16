﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class GeometryShaderBox : SwappableRenderItem
    {
        private readonly int _indexCount = 1;
        private readonly Camera _camera;

        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;
        private ConstantBuffer _worldMatrixBuffer;
        private readonly string _geometryShaderName;

        public Vector3 Position { get; set; }

        public GeometryShaderBox(AssetDatabase ad, RenderContext rc, Camera camera, string geometryShaderName = "cube-geometry")
        {
            _geometryShaderName = geometryShaderName;
            InitializeContextObjects(ad, rc);
            Matrix4x4 m = Matrix4x4.CreateTranslation(Position);
            _worldMatrixBuffer.SetData(ref m, 64);
            _camera = camera;
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            ClearDeviceResources();
            InitializeContextObjects(ad, rc);
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return false;
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return RenderOrderKey.Create(Vector3.Distance(Position, viewPosition), 0);
        }

        public IList<string> GetStagesParticipated() => CommonStages.Standard;

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.VertexBuffer = _vb;
            rc.IndexBuffer = _ib;
            _material.Apply(rc);
            Matrix4x4 worldMatrix = Matrix4x4.CreateTranslation(Position);
            _worldMatrixBuffer.SetData(ref worldMatrix, 64);
            rc.SetConstantBuffer(0, SharedDataProviders.ProjectionMatrixBuffer);
            rc.SetConstantBuffer(1, SharedDataProviders.ViewMatrixBuffer);
            rc.SetConstantBuffer(2, SharedDataProviders.CameraInfoBuffer);
            rc.SetConstantBuffer(3, _worldMatrixBuffer);
            rc.DrawIndexedPrimitives(_indexCount, 0, PrimitiveTopology.PointList);
        }

        private void InitializeContextObjects(AssetDatabase ad, RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vb = factory.CreateVertexBuffer(new[] { new VertexPosition(Vector3.Zero) }, new VertexDescriptor(12, 1), false);
            _ib = factory.CreateIndexBuffer(new ushort[] { 0 }, false);
            Shader vertexShader = factory.CreateShader(ShaderType.Vertex, ShaderHelper.LoadShaderCode("geometry-vertex", ShaderType.Vertex, rc.ResourceFactory));
            Shader geometryShader = factory.CreateShader(ShaderType.Geometry, ShaderHelper.LoadShaderCode(_geometryShaderName, ShaderType.Geometry, rc.ResourceFactory));
            Shader fragmentShader = factory.CreateShader(ShaderType.Fragment, ShaderHelper.LoadShaderCode("geometry-frag", ShaderType.Fragment, rc.ResourceFactory));
            VertexInputLayout inputLayout = factory.CreateInputLayout(
                new VertexInputDescription(12, new VertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3)));
            ShaderSet shaderSet = factory.CreateShaderSet(inputLayout, vertexShader, geometryShader, fragmentShader);
            ShaderResourceBindingSlots constantBindings = factory.CreateShaderConstantBindingSlots(
                shaderSet,
                    new ShaderResourceDescription("ProjectionMatrixBuffer", ShaderConstantType.Matrix4x4), // Global
                    new ShaderResourceDescription("ViewMatrixBuffer", ShaderConstantType.Matrix4x4), // Global
                    new ShaderResourceDescription("CameraInfoBuffer", Unsafe.SizeOf<Camera.Info>()), // Global
                    new ShaderResourceDescription("WorldMatrixBuffer", ShaderConstantType.Matrix4x4)); // Local
            _material = new Material(shaderSet, constantBindings);
            _worldMatrixBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);
        }

        private void ClearDeviceResources()
        {
            _vb.Dispose();
            _ib.Dispose();
            _material.Dispose();
        }
    }
}