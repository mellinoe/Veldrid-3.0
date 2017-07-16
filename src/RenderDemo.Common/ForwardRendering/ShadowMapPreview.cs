﻿using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo.ForwardRendering
{
    public class ShadowMapPreview : SwappableRenderItem
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Material _material;
        private ConstantBuffer _worldMatrixBuffer;
        private ConstantBuffer _projectionMatrixBuffer;
        private float _imageWidth = 200f;
        private DepthStencilState _depthDisabledState;

        public Vector2 ScreenPosition { get; set; }
        public Vector2 Scale { get; set; }

        public ShadowMapPreview(AssetDatabase ad, RenderContext rc)
        {
            ChangeRenderContext(ad, rc);
        }

        public void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            var factory = rc.ResourceFactory;
            _vertexBuffer = factory.CreateVertexBuffer(VertexPositionTexture.SizeInBytes, false);
            _vertexBuffer.SetVertexData(new VertexPositionTexture[]
            {
                new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 0, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(0, 1, 0), new Vector2(0, 1)),
            }, new VertexDescriptor(VertexPositionTexture.SizeInBytes, VertexPositionTexture.ElementCount, 0, IntPtr.Zero),
            0);

            _indexBuffer = factory.CreateIndexBuffer(sizeof(byte) * 6, false);
            _indexBuffer.SetIndices(new ushort[] { 0, 1, 2, 0, 2, 3 });

            _material = factory.CreateMaterial(
                rc,
                "simple-2d-vertex",
                "simple-2d-frag",
                new VertexInputDescription(
                    VertexPositionTexture.SizeInBytes,
                    new VertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3),
                    new VertexInputElement("in_texCoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2)),
                new[]
                {
                    new ShaderResourceDescription("WorldMatrixBuffer", ShaderConstantType.Matrix4x4),
                    new ShaderResourceDescription("ProjectionMatrixBuffer", ShaderConstantType.Matrix4x4),
                    new ShaderResourceDescription("SurfaceTexture", ShaderResourceType.Texture)
                });

            _worldMatrixBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);
            _projectionMatrixBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);

            _depthDisabledState = factory.CreateDepthStencilState(false, DepthComparison.Always);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            return new RenderOrderKey();
        }

        public IList<string> GetStagesParticipated() => CommonStages.Overlay;

        public void Render(RenderContext rc, string pipelineStage)
        {
            Matrix4x4 orthoProjection = Matrix4x4.CreateOrthographicOffCenter(
                0f,
                rc.Viewport.Width,
                rc.Viewport.Height,
                0f,
                -1.0f,
                1.0f);
            Matrix4x4 proj = orthoProjection;
            _projectionMatrixBuffer.SetData(ref proj, 64);

            float width = _imageWidth;
            Matrix4x4 world = Matrix4x4.CreateScale(width)
                * Matrix4x4.CreateTranslation(rc.Viewport.Width - width - 20, 20, 0);
            _worldMatrixBuffer.SetData(ref world, 64);

            rc.VertexBuffer = _vertexBuffer;
            rc.IndexBuffer = _indexBuffer;
            _material.Apply(rc);
            rc.SetConstantBuffer(0, _worldMatrixBuffer);
            rc.SetConstantBuffer(1, _projectionMatrixBuffer);
            rc.SetTexture(0, SharedTextures.GetTextureBinding("ShadowMap"));
            rc.SetDepthStencilState(_depthDisabledState);
            rc.DrawIndexedPrimitives(6, 0);
            rc.SetDepthStencilState(rc.DefaultDepthStencilState);
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return false;
        }
    }
}
