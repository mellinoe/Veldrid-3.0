using ImageSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    public class Skybox : SwappableRenderItem
    {
        private DependantDataProvider<Matrix4x4> _viewProvider;
        private readonly ImageSharpTexture _front;
        private readonly ImageSharpTexture _back;
        private readonly ImageSharpTexture _left;
        private readonly ImageSharpTexture _right;
        private readonly ImageSharpTexture _top;
        private readonly ImageSharpTexture _bottom;

        // Context objects
        private VertexBuffer _vb;
        private IndexBuffer _ib;
        private Material _material;
        private ConstantBuffer _viewMatrixBuffer;
        private ShaderTextureBinding _cubemapBinding;
        private RasterizerState _rasterizerState;

        public Skybox(RenderContext rc, AssetDatabase ad) : this(rc, ad,
            ad.LoadAsset<ImageSharpTexture>("Textures/cloudtop/cloudtop_ft.png"),
            ad.LoadAsset<ImageSharpTexture>("Textures/cloudtop/cloudtop_bk.png"),
            ad.LoadAsset<ImageSharpTexture>("Textures/cloudtop/cloudtop_lf.png"),
            ad.LoadAsset<ImageSharpTexture>("Textures/cloudtop/cloudtop_rt.png"),
            ad.LoadAsset<ImageSharpTexture>("Textures/cloudtop/cloudtop_up.png"),
            ad.LoadAsset<ImageSharpTexture>("Textures/cloudtop/cloudtop_dn.png"))
        { }

        public Skybox(RenderContext rc, AssetDatabase ad,
            ImageSharpTexture front, ImageSharpTexture back, ImageSharpTexture left,
            ImageSharpTexture right, ImageSharpTexture top, ImageSharpTexture bottom)
        {
            _front = front;
            _back = back;
            _left = left;
            _right = right;
            _top = top;
            _bottom = bottom;
            ChangeRenderContext(ad, rc);
        }

        public unsafe void ChangeRenderContext(AssetDatabase ad, RenderContext rc)
        {
            var factory = rc.ResourceFactory;

            _vb = factory.CreateVertexBuffer(s_vertices.Length * VertexPosition.SizeInBytes, false);
            _vb.SetVertexData(s_vertices, new VertexDescriptor(VertexPosition.SizeInBytes, 1, 0, IntPtr.Zero));

            _ib = factory.CreateIndexBuffer(s_indices.Length * sizeof(int), false);
            _ib.SetIndices(s_indices);

            Shader vs = factory.CreateShader(ShaderType.Vertex, ShaderHelper.LoadShaderCode("skybox-vertex", ShaderType.Vertex, rc.ResourceFactory));
            Shader fs = factory.CreateShader(ShaderType.Fragment, ShaderHelper.LoadShaderCode("skybox-frag", ShaderType.Fragment, rc.ResourceFactory));
            VertexInputLayout inputLayout = factory.CreateInputLayout(
                new VertexInputDescription(
                    12,
                    new VertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float3)));
            ShaderSet shaderSet = factory.CreateShaderSet(inputLayout, vs, fs);
            ShaderResourceBindingSlots constantSlots = factory.CreateShaderConstantBindingSlots(
                shaderSet,
                new ShaderResourceDescription("ProjectionMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderResourceDescription("ViewMatrixBuffer", ShaderConstantType.Matrix4x4),
                new ShaderResourceDescription("Skybox", ShaderResourceType.Texture));

            _material = new Material(shaderSet, constantSlots);
            _viewMatrixBuffer = factory.CreateConstantBuffer(ShaderConstantType.Matrix4x4);

            fixed (Rgba32* frontPin = &_front.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* backPin = &_back.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* leftPin = &_left.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* rightPin = &_right.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* topPin = &_top.Pixels.DangerousGetPinnableReference())
            fixed (Rgba32* bottomPin = &_bottom.Pixels.DangerousGetPinnableReference())
            {
                var cubemapTexture = factory.CreateCubemapTexture(
                    (IntPtr)frontPin,
                    (IntPtr)backPin,
                    (IntPtr)leftPin,
                    (IntPtr)rightPin,
                    (IntPtr)topPin,
                    (IntPtr)bottomPin,
                    _front.Width,
                    _front.Height,
                    _front.PixelSizeInBytes,
                    _front.Format);
                _cubemapBinding = factory.CreateShaderTextureBinding(cubemapTexture);
            }

            _rasterizerState = factory.CreateRasterizerState(FaceCullingMode.None, TriangleFillMode.Solid, false, false);

            _viewProvider = new DependantDataProvider<Matrix4x4>(SharedDataProviders.GetProvider<Matrix4x4>("ViewMatrix"), Utilities.ConvertToMatrix3x3);
        }

        public RenderOrderKey GetRenderOrderKey(Vector3 viewPosition)
        {
            // Render the skybox last.
            return new RenderOrderKey(ulong.MaxValue);
        }

        public IList<string> GetStagesParticipated() => CommonStages.Standard;

        public void Render(RenderContext rc, string pipelineStage)
        {
            rc.VertexBuffer = _vb;
            rc.IndexBuffer = _ib;
            _material.Apply(rc);
            rc.SetConstantBuffer(0, SharedDataProviders.ProjectionMatrixBuffer);
            _viewProvider.SetData(_viewMatrixBuffer);
            rc.SetConstantBuffer(1, _viewMatrixBuffer);
            RasterizerState previousRasterState = rc.RasterizerState;
            rc.SetRasterizerState(_rasterizerState);
            rc.SetTexture(0, _cubemapBinding);
            rc.DrawIndexedPrimitives(s_indices.Length, 0);
            rc.SetRasterizerState(previousRasterState);
        }

        public bool Cull(ref BoundingFrustum visibleFrustum)
        {
            return false;
        }

        private static readonly VertexPosition[] s_vertices = new VertexPosition[]
        {
            // Top
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            // Bottom
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            // Left
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            // Right
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            // Back
            new VertexPosition(new Vector3(20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,20.0f,-20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,-20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,-20.0f)),
            // Front
            new VertexPosition(new Vector3(-20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,20.0f,20.0f)),
            new VertexPosition(new Vector3(20.0f,-20.0f,20.0f)),
            new VertexPosition(new Vector3(-20.0f,-20.0f,20.0f)),
        };

        private static readonly ushort[] s_indices = new ushort[]
        {
            0,1,2, 0,2,3,
            4,5,6, 4,6,7,
            8,9,10, 8,10,11,
            12,13,14, 12,14,15,
            16,17,18, 16,18,19,
            20,21,22, 20,22,23,
        };
    }
}