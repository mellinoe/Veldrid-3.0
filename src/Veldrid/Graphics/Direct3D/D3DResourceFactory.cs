﻿using System;
using SharpDX.Direct3D11;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SharpDX.D3DCompiler;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DResourceFactory : ResourceFactory
    {
        private const ShaderFlags DefaultShaderFlags
#if DEBUG
            = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
            = ShaderFlags.OptimizationLevel3;
#endif

        protected override string GetShaderFileExtension() => "hlsl";

        protected override GraphicsBackend PlatformGetGraphicsBackend() => GraphicsBackend.Direct3D11;

        private readonly Device _device;

        public D3DResourceFactory(Device device)
        {
            _device = device;
        }

        public override ConstantBuffer CreateConstantBuffer(int sizeInBytes)
        {
            return new D3DConstantBuffer(_device, sizeInBytes);
        }

        public override Framebuffer CreateFramebuffer()
        {
            return new D3DFramebuffer(_device);
        }

        public override IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic, IndexFormat format)
        {
            return new D3DIndexBuffer(_device, sizeInBytes, isDynamic, D3DFormats.VeldridToD3DIndexFormat(format));
        }

        public override CompiledShaderCode ProcessShaderCode(ShaderType type, string shaderCode)
        {
            string entryPoint;
            switch (type)
            {
                case ShaderType.Vertex:
                    entryPoint = "VS";
                    break;
                case ShaderType.Geometry:
                    entryPoint = "GS";
                    break;
                case ShaderType.Fragment:
                    entryPoint = "PS";
                    break;
                default:
                    throw Illegal.Value<ShaderType>();
            }

            string profile;
            switch (type)
            {
                case ShaderType.Vertex:
                    profile = "vs_5_0";
                    break;
                case ShaderType.Geometry:
                    profile = "gs_5_0";
                    break;
                case ShaderType.Fragment:
                    profile = "ps_5_0";
                    break;
                default: throw Illegal.Value<ShaderType>();
            }

            return new D3DShaderBytecode(shaderCode, entryPoint, profile, DefaultShaderFlags);
        }

        public override CompiledShaderCode LoadProcessedShader(byte[] data)
        {
            return new D3DShaderBytecode(data);
        }

        public override Shader CreateShader(ShaderType type, CompiledShaderCode compiledShaderCode)
        {
            D3DShaderBytecode d3dBytecode = (D3DShaderBytecode)compiledShaderCode;

            switch (type)
            {
                case ShaderType.Vertex:
                    return new D3DVertexShader(_device, d3dBytecode.Bytecode);
                case ShaderType.Geometry:
                    return new D3DGeometryShader(_device, d3dBytecode.Bytecode);
                case ShaderType.Fragment:
                    return new D3DFragmentShader(_device, d3dBytecode.Bytecode);
                default: throw Illegal.Value<ShaderType>();
            }
        }

        public override VertexInputLayout CreateInputLayout(VertexInputDescription[] vertexInputs)
        {
            return new D3DVertexInputLayout(vertexInputs);
        }

        public override ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader fragmentShader)
        {
            return new D3DShaderSet(inputLayout, vertexShader, null, fragmentShader);
        }

        public override ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader geometryShader, Shader fragmentShader)
        {
            return new D3DShaderSet(inputLayout, vertexShader, geometryShader, fragmentShader);
        }

        public override ShaderResourceBindingSlots CreateShaderConstantBindingSlots(
            ShaderSet shaderSet,
            ShaderResourceDescription[] constants)
        {
            return new D3DShaderResourceBindingSlots(_device, (D3DShaderSet)shaderSet, constants);
        }

        public override VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic)
        {
            return new D3DVertexBuffer(_device, sizeInBytes, isDynamic);
        }

        public override DeviceTexture2D CreateTexture(
            int mipLevels,
            int width,
            int height,
            PixelFormat format,
            DeviceTextureCreateOptions createOptions)
        {
            int pixelSizeInBytes = FormatHelpers.GetPixelSizeInBytes(format);
            SharpDX.DXGI.Format dxgiFormat = D3DFormats.VeldridToD3DPixelFormat(format);
            BindFlags bindFlags = BindFlags.ShaderResource;
            if (createOptions == DeviceTextureCreateOptions.DepthStencil)
            {
                if (format != PixelFormat.R16_UInt)
                {
                    throw new NotImplementedException("R16_UInt is the only supported depth texture format.");
                }

                dxgiFormat = SharpDX.DXGI.Format.R16_Typeless;
                bindFlags |= BindFlags.DepthStencil;
            }
            else if (createOptions == DeviceTextureCreateOptions.RenderTarget)
            {
                bindFlags |= BindFlags.RenderTarget;
            }

            D3DTexture2D texture = new D3DTexture2D(
                _device,
                bindFlags,
                ResourceUsage.Default,
                CpuAccessFlags.None,
                dxgiFormat,
                mipLevels,
                width,
                height,
                width * pixelSizeInBytes);
            return texture;
        }

        protected override SamplerState CreateSamplerStateCore(
            SamplerAddressMode addressU,
            SamplerAddressMode addressV,
            SamplerAddressMode addressW,
            SamplerFilter filter,
            int maxAnisotropy,
            RgbaFloat borderColor,
            DepthComparison comparison,
            int minimumLod,
            int maximumLod,
            int lodBias)
        {
            return new D3DSamplerState(_device, addressU, addressV, addressW, filter, maxAnisotropy, borderColor, comparison, minimumLod, maximumLod, lodBias);
        }

        public override CubemapTexture CreateCubemapTexture(
            IntPtr pixelsFront,
            IntPtr pixelsBack,
            IntPtr pixelsLeft,
            IntPtr pixelsRight,
            IntPtr pixelsTop,
            IntPtr pixelsBottom,
            int width,
            int height,
            int pixelSizeinBytes,
            PixelFormat format)
        {
            return new D3DCubemapTexture(_device, pixelsFront, pixelsBack, pixelsLeft, pixelsRight, pixelsTop, pixelsBottom, width, height, pixelSizeinBytes, format);
        }

        public override ShaderTextureBinding CreateShaderTextureBinding(DeviceTexture texture)
        {
            D3DTexture d3dTexture = (D3DTexture)texture;
            ShaderResourceViewDescription srvd = d3dTexture.GetShaderResourceViewDescription();
            ShaderResourceView srv = new ShaderResourceView(_device, d3dTexture.DeviceTexture, srvd);
            return new D3DTextureBinding(srv, d3dTexture);
        }

        protected override BlendState CreateCustomBlendStateCore(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc,
            RgbaFloat blendFactor)
        {
            return new D3DBlendState(_device, isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc, blendFactor);
        }

        protected override DepthStencilState CreateDepthStencilStateCore(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
        {
            return new D3DDepthStencilState(_device, isDepthEnabled, comparison, isDepthWriteEnabled);
        }

        protected override RasterizerState CreateRasterizerStateCore(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled)
        {
            return new D3DRasterizerState(_device, cullMode, fillMode, isDepthClipEnabled, isScissorTestEnabled);
        }
    }
}
