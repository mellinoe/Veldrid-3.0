﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Veldrid.Graphics
{
    /// <summary>
    /// Contains functionality for creating GPU device resources, including buffers, textures, and other pipeline
    /// state objects.
    /// </summary>
    public abstract class ResourceFactory
    {
        // TODO: These are problematic because of ownership confusion.
        /*
        private readonly Dictionary<SamplerStateCacheKey, SamplerState> _cachedSamplers
            = new Dictionary<SamplerStateCacheKey, SamplerState>();
        private readonly Dictionary<BlendStateCacheKey, BlendState> _cachedBlendStates
            = new Dictionary<BlendStateCacheKey, BlendState>();
        private readonly Dictionary<DepthStencilStateCacheKey, DepthStencilState> _cachedDepthStencilStates
            = new Dictionary<DepthStencilStateCacheKey, DepthStencilState>();
        private readonly Dictionary<RasterizerStateCacheKey, RasterizerState> _cachedRasterizerStates
            = new Dictionary<RasterizerStateCacheKey, RasterizerState>();
        */

        public GraphicsBackend BackendType => PlatformGetGraphicsBackend();

        /// <summary>
        /// Creates a <see cref="VertexBuffer"/> with the given storage size.
        /// </summary>
        /// <param name="sizeInBytes">The total capacity in bytes of the buffer.</param>
        /// <param name="isDynamic">A value indicating whether or not the buffer should be optimized for dynamic access.</param>
        /// <returns>A new <see cref="VertexBuffer"/>.</returns>
        public abstract VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic);

        /// <summary>
        /// Creates a <see cref="VertexBuffer"/> containing the given vertex data.
        /// </summary>
        /// <typeparam name="T">The type of vertex data; must be a value type.</typeparam>
        /// <param name="data">An array of vertices to store in the <see cref="VertexBuffer"/></param>
        /// <param name="descriptor">A description of the vertex content.</param>
        /// <param name="isDynamic"></param>
        /// <returns>A new <see cref="VertexBuffer"/> containing the given data.</returns>
        public VertexBuffer CreateVertexBuffer<T>(T[] data, VertexDescriptor descriptor, bool isDynamic) where T : struct
        {
            VertexBuffer vb = CreateVertexBuffer(descriptor.VertexSizeInBytes * data.Length, isDynamic);
            vb.SetVertexData(data, descriptor);
            return vb;
        }

        /// <summary>
        /// Constructs a <see cref="IndexBuffer"/> with the given storage size.
        /// The IndexBuffer will be configured to store 16-bit unsigned index data.
        /// </summary>
        /// <param name="sizeInBytes">The total capacity in bytes of the buffer.</param>
        /// <param name="isDynamic">A value indicating whether or not the buffer should be optimized for dynamic access.</param>
        /// <returns>A new <see cref="IndexBuffer"/></returns>
        public IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic) => CreateIndexBuffer(sizeInBytes, isDynamic, IndexFormat.UInt16);

        /// <summary>
        /// Creates an <see cref="IndexBuffer"/> containing the given <see cref="System.Int32"/> index data.
        /// </summary>
        /// <param name="indices">The index data.</param>
        /// <param name="isDynamic">A value indicating whether or not the buffer should be optimized for dynamic access.</param>
        /// <returns>A new <see cref="IndexBuffer"/></returns>
        public IndexBuffer CreateIndexBuffer(uint[] indices, bool isDynamic)
        {
            IndexBuffer ib = CreateIndexBuffer(sizeof(uint) * indices.Length, isDynamic, IndexFormat.UInt32);
            ib.SetIndices(indices);
            return ib;
        }

        /// <summary>
        /// Creates an <see cref="IndexBuffer"/> containing the given <see cref="System.Int32"/> index data.
        /// </summary>
        /// <param name="indices">The index data.</param>
        /// <param name="isDynamic">A value indicating whether or not the buffer should be optimized for dynamic access.</param>
        /// <returns>A new <see cref="IndexBuffer"/></returns>
        public IndexBuffer CreateIndexBuffer(ushort[] indices, bool isDynamic)
        {
            IndexBuffer ib = CreateIndexBuffer(sizeof(ushort) * indices.Length, isDynamic, IndexFormat.UInt16);
            ib.SetIndices(indices);
            return ib;
        }

        /// <summary>
        /// Constructs a <see cref="IndexBuffer"/> with the given storage size.
        /// </summary>
        /// <param name="sizeInBytes">The total capacity in bytes of the buffer.</param>
        /// <param name="isDynamic">A value indicating whether or not the buffer should be optimized for dynamic access.</param>
        /// <param name="format">The format of index data that will be contained in the buffer.</param>
        /// <returns>A new <see cref="IndexBuffer"/></returns>
        public abstract IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic, IndexFormat format);

        public CompiledShaderCode ProcessShaderCode(ShaderStages type, string shaderCode)
            => ProcessShaderCode(type, shaderCode, GetDefaultEntryPoint(type));

        public abstract CompiledShaderCode ProcessShaderCode(ShaderStages type, string shaderCode, string entryPoint);

        public abstract CompiledShaderCode LoadProcessedShader(byte[] data);

        /// <summary>
        /// Creates a new <see cref="Shader"/> from shader source code.
        /// </summary>
        /// <param name="type">The type of the <see cref="Shader"/></param>
        /// <param name="shaderCode">The raw text source of the <see cref="Shader"/></param>
        /// <param name="name">The name of the <see cref="Shader"/>, generally used for debugging.</param>
        /// <returns>A new Shader object.</returns>
        public Shader CreateShader(ShaderStages type, string shaderCode)
        {
            return CreateShader(type, ProcessShaderCode(type, shaderCode));
        }

        public abstract Shader CreateShader(ShaderStages type, CompiledShaderCode compiledShaderCode);

        /// <summary>
        /// Creates a <see cref="ShaderSet"/> from the given vertex inputs and shaders.
        /// </summary>
        /// <param name="inputLayout">The device-specific vertex input layout of the vertex shader.</param>
        /// <param name="vertexShader">The vertex shader.</param>
        /// <param name="fragmentShader">The fragment shader.</param>
        /// <returns></returns>
        public abstract ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader fragmentShader);

        /// <summary>
        /// Creates a <see cref="ShaderSet"/> from the given vertex inputs and shaders.
        /// </summary>
        /// <param name="inputLayout">The device-specific vertex input layout of the vertex shader.</param>
        /// <param name="vertexShader">The vertex shader.</param>
        /// <param name="geometryShader">The geometry shader.</param>
        /// <param name="fragmentShader">The fragment shader.</param>
        /// <returns></returns>
        public abstract ShaderSet CreateShaderSet(VertexInputLayout inputLayout, Shader vertexShader, Shader geometryShader, Shader fragmentShader);

        /// <summary>
        /// Creates a <see cref="ShaderSet"/> from the given vertex inputs and shaders.
        /// </summary>
        /// <param name="inputLayout">The device-specific vertex input layout of the vertex shader.</param>
        /// <param name="vertexShader">The vertex shader.</param>
        /// <param name="tessellationControlShader">The tessellation control shader.</param>
        /// <param name="tessellationEvaluationShader">The tessellation evaluation shader.</param>
        /// <param name="geometryShader">The geometry shader.</param>
        /// <param name="fragmentShader">The fragment shader.</param>
        /// <returns></returns>
        public abstract ShaderSet CreateShaderSet(
            VertexInputLayout inputLayout,
            Shader vertexShader, 
            Shader tessellationControlShader,
            Shader tessellationEvaluationShader,
            Shader geometryShader, 
            Shader fragmentShader);

        /// <summary>
        /// Creates a device-specific representation of the resource slots available to a set of shaders.
        /// </summary>
        /// <param name="shaderSet">The shader set for which the <see cref="ShaderResourceBindingSlots"/> will be applicable.</param>
        /// <param name="resources">A description of the resources.</param>
        /// <returns></returns>
        public abstract ShaderResourceBindingSlots CreateShaderResourceBindingSlots(
            ShaderSet shaderSet,
            params ShaderResourceDescription[] resources);

        public VertexInputLayout CreateInputLayout(params VertexInputElement[] inputElements)
        {
            int totalSize = 0;
            for (int i = 0; i < inputElements.Length; i++)
            {
                totalSize += inputElements[i].SizeInBytes;
            }

            return CreateInputLayout(new VertexInputDescription(totalSize, inputElements));
        }

        /// <summary>
        /// Creates a device-specific <see cref="VertexInputLayout"/> from a generic description.
        /// </summary>
        /// <param name="vertexShader">The vertex <see cref="Shader"/>.</param>
        /// <param name="vertexInputs">An array of vertex input descriptions, one for each <see cref="VertexBuffer"/> input.</param>
        /// <returns>A new <see cref="VertexInputLayout"/>.</returns>
        public abstract VertexInputLayout CreateInputLayout(params VertexInputDescription[] vertexInputs);

        public ConstantBuffer CreateConstantBuffer(ShaderConstantType type)
        {
            if (!FormatHelpers.GetShaderConstantTypeByteSize(type, out int sizeInBytes))
            {
                throw new VeldridException(
                    $"ShaderConstantType passed to CreateConstantBuffer must have a defined size. {type} does not.");
            }

            return CreateConstantBuffer(sizeInBytes);
        }

        /// <summary>
        /// Creates a new <see cref="ConstantBuffer"/>, used for storing global <see cref="Shader"/> parameters.
        /// </summary>
        /// <param name="sizeInBytes">The initial capacity in bytes of the buffer.</param>
        /// <returns>A new <see cref="ConstantBuffer"/>.</returns>
        public abstract ConstantBuffer CreateConstantBuffer(int sizeInBytes);

        /// <summary>
        /// Creates a new <see cref="Framebuffer"/>, with no color and depth textures initially attached.
        /// </summary>
        /// <returns>A new, empty <see cref="Framebuffer"/>.</returns>
        public abstract Framebuffer CreateFramebuffer();

        /// <summary>
        /// Creates a new <see cref="Framebuffer"/> with the given dimensions.
        /// </summary>
        /// <param name="width">The width of the color and depth textures.</param>
        /// <param name="height">The height of the color and depth textures.</param>
        /// <returns></returns>
        public Framebuffer CreateFramebuffer(int width, int height)
        {
            Framebuffer fb = CreateFramebuffer();
            DeviceTexture2D colorTexture = CreateTexture(
                1,
                width,
                height,
                PixelFormat.R8_G8_B8_A8_UInt,
                DeviceTextureCreateOptions.RenderTarget);
            DeviceTexture2D depthTexture = CreateTexture(
                1,
                width,
                height,
                PixelFormat.R16_UInt,
                DeviceTextureCreateOptions.DepthStencil);
            fb.ColorTexture = colorTexture;
            fb.DepthTexture = depthTexture;

            return fb;
        }

        /// <summary>
        /// Creates a new <see cref="DeviceTexture2D"/>.
        /// </summary>
        /// <typeparam name="T">The type of pixel data; must be a value type.</typeparam>
        /// <param name="pixelData">An array of pixel information.</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height of the texture</param>
        /// <param name="pixelSizeInBytes">The total size in bytes of the pixel data.</param>
        /// <param name="format">The format of pixel information.</param>
        /// <returns>A new <see cref="DeviceTexture2D"/> containing the given pixel data.</returns>
        public DeviceTexture2D CreateTexture<T>(T[] pixelData, int width, int height, PixelFormat format) where T : struct
        {
            int pixelSizeInBytes = FormatHelpers.GetPixelSizeInBytes(format);
            DeviceTexture2D tex = CreateTexture(1, width, height, format);
            GCHandle handle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            tex.SetTextureData(0, 0, 0, width, height, handle.AddrOfPinnedObject(), pixelSizeInBytes * width * height);
            handle.Free();
            return tex;
        }

        /// <summary>
        /// Creates a new <see cref="DeviceTexture2D"/>.
        /// </summary>
        /// <param name="pixelData">An array of pixel information.</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height of the texture</param>
        /// <param name="pixelSizeInBytes">The total size in bytes of the pixel data.</param>
        /// <param name="format">The format of pixel information.</param>
        /// <returns>A new <see cref="DeviceTexture2D"/> containing the given pixel data.</returns>
        public DeviceTexture2D CreateTexture(IntPtr pixelData, int width, int height, PixelFormat format)
        {
            int pixelSizeInBytes = FormatHelpers.GetPixelSizeInBytes(format);
            DeviceTexture2D tex = CreateTexture(1, width, height, format);
            tex.SetTextureData(0, 0, 0, width, height, pixelData, pixelSizeInBytes * width * height);
            return tex;
        }

        /// <summary>
        /// Creates a new 2D device texture with the given properties.
        /// </summary>
        /// <param name="mipLevels">The number of mipmap levels contained in the texture.</param>
        /// <param name="width">The width of the largest mipmap level.</param>
        /// <param name="height">The height of the largest mipmap level.</param>
        /// <param name="pixelSizeInBytes">The size of individual pixels, in bytes.</param>
        /// <param name="format">The pixel format of the texture.</param>
        /// <param name="createOptions">Specifies how the texture will be used.</param>
        /// <returns></returns>
        public DeviceTexture2D CreateTexture(
            int mipLevels,
            int width,
            int height,
            PixelFormat format) => CreateTexture(mipLevels, width, height, format, DeviceTextureCreateOptions.Default);

        /// <summary>
        /// Creates a new 2D device texture with the given properties.
        /// </summary>
        /// <param name="mipLevels">The number of mipmap levels contained in the texture.</param>
        /// <param name="width">The width of the largest mipmap level.</param>
        /// <param name="height">The height of the largest mipmap level.</param>
        /// <param name="pixelSizeInBytes">The size of individual pixels, in bytes.</param>
        /// <param name="format">The pixel format of the texture.</param>
        /// <param name="createOptions">Specifies how the texture will be used.</param>
        /// <returns></returns>
        public abstract DeviceTexture2D CreateTexture(
            int mipLevels,
            int width,
            int height,
            PixelFormat format,
            DeviceTextureCreateOptions createOptions);

        /// <summary>
        /// Creates a new <see cref="SamplerState"/> with the given properties.
        /// </summary>
        /// <param name="addressU">A value controlling U-coordinate (R-coordinate in OpenGL) sampling.</param>
        /// <param name="addressV">A value controlling V-coordinate (S-coordinate in OpenGL) sampling.</param>
        /// <param name="addressW">A value controlling W-coordinate (T-coordinate in OpenGL) sampling.</param>
        /// <param name="filter">The filter kind to use.</param>
        /// <param name="maxAnisotropy">If <paramref name="filter"/> is equal to Anisotropic, then this parameter
        /// controls the level of anisotropic filtering used.</param>
        /// <param name="borderColor">When a border-type filter is used, this controls the sampled border color.</param>
        /// <param name="comparison">Controls the comparison used when a Comparison-type filter is used.</param>
        /// <param name="minimumLod">The highest-quality LOD level (lowest level) the sampler will access.</param>
        /// <param name="maximumLod">The lowerst-quality LOD level (lowest level) the sampler will access.</param>
        /// <param name="lodBias">A bias to use when choosing which LOD level to sample from.</param>
        /// <returns>A new <see cref="SamplerState"/> which supports the given properties.</returns>
        public SamplerState CreateSamplerState(
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
            return CreateSamplerStateCore(
                addressU,
                addressV,
                addressW,
                filter,
                maxAnisotropy,
                borderColor,
                comparison,
                minimumLod,
                maximumLod,
                lodBias);
        }

        protected abstract SamplerState CreateSamplerStateCore(
            SamplerAddressMode addressU,
            SamplerAddressMode addressV,
            SamplerAddressMode addressW,
            SamplerFilter filter,
            int maxAnisotropy,
            RgbaFloat borderColor,
            DepthComparison comparison,
            int minimumLod,
            int maximumLod,
            int lodBias);

        /// <summary>
        /// Creates a new <see cref="ShaderTextureBinding"/> for the given <see cref="DeviceTexture"/>.
        /// </summary>
        /// <param name="texture">The <see cref="DeviceTexture"/> to associate with the binding.</param>
        /// <returns>A new <see cref="ShaderTextureBinding"/>.</returns>
        public abstract ShaderTextureBinding CreateShaderTextureBinding(DeviceTexture texture);

        /// <summary>
        /// Creates a new <see cref="CubemapTexture"/> from six pointers containing each cube face's texture data.
        /// </summary>
        /// <param name="pixelsFront">The front face pixel data.</param>
        /// <param name="pixelsBack">The back face pixel data.</param>
        /// <param name="pixelsLeft">The left face pixel data.</param>
        /// <param name="pixelsRight">The right face pixel data.</param>
        /// <param name="pixelsTop">The top face pixel data.</param>
        /// <param name="pixelsBottom">The bottom face pixel data.</param>
        /// <param name="width">The width of each cube face texture.</param>
        /// <param name="height">The height of each cube face texture.</param>
        /// <param name="pixelSizeinBytes">The total size in bytes of the pixel data for each individual cube face.</param>
        /// <param name="format">The format of the pixel data.</param>
        /// <returns>A new <see cref="CubemapTexture"/>.</returns>
        public abstract CubemapTexture CreateCubemapTexture(
            IntPtr pixelsFront,
            IntPtr pixelsBack,
            IntPtr pixelsLeft,
            IntPtr pixelsRight,
            IntPtr pixelsTop,
            IntPtr pixelsBottom,
            int width,
            int height,
            int pixelSizeinBytes,
            PixelFormat format);

        /// <summary>
        /// Creates a new <see cref="BlendState"/> used to control the device's output merger blending behavior.
        /// </summary>
        /// <param name="isBlendEnabled">A value indicating whether blending is enabled in the <see cref="BlendState"/>.</param>
        /// <param name="srcBlend">The source blend factor.</param>
        /// <param name="destBlend">The destination blend factor.</param>
        /// <param name="blendFunc">The blend function.</param>
        /// <returns>A new <see cref="BlendState"/>.</returns>
        public BlendState CreateCustomBlendState(
            bool isBlendEnabled,
            Blend srcBlend, Blend destBlend, BlendFunction blendFunc)
        {
            return CreateCustomBlendState(isBlendEnabled, srcBlend, destBlend, blendFunc, srcBlend, destBlend, blendFunc, RgbaFloat.Black);
        }

        /// <summary>
        /// Creates a new <see cref="BlendState"/>, used to control blending behavior in the device's output merger.
        /// with separate factors for alpha and color blending.
        /// </summary>
        /// <param name="isBlendEnabled">A value indicating whether blending is enabled in the <see cref="BlendState"/>.</param>
        /// <param name="srcAlpha">The source alpha blend factor.</param>
        /// <param name="destAlpha">The destination alpha blend factor.</param>
        /// <param name="alphaBlendFunc">The alpha blend function.</param>
        /// <param name="srcColor">The source color blend factor.</param>
        /// <param name="destColor">The destenation color blend factor.</param>
        /// <param name="colorBlendFunc">The color blend function.</param>
        /// <param name="blendFactor">The blend factor to use for parameterized blend states.</param>
        /// <returns>A new <see cref="BlendState"/>.</returns>
        public BlendState CreateCustomBlendState(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc,
            RgbaFloat blendFactor)
        {
            return CreateCustomBlendStateCore(isBlendEnabled, srcAlpha, destAlpha, alphaBlendFunc, srcColor, destColor, colorBlendFunc, blendFactor);
        }

        /// <summary>
        /// Creates a new <see cref="BlendState"/>, used to control blending behavior in the device's output merger.
        /// with separate factors for alpha and color blending.
        /// </summary>
        /// <param name="isBlendEnabled">A value indicating whether blending is enabled in the <see cref="BlendState"/>.</param>
        /// <param name="srcAlpha">The source alpha blend factor.</param>
        /// <param name="destAlpha">The destination alpha blend factor.</param>
        /// <param name="alphaBlendFunc">The alpha blend function.</param>
        /// <param name="srcColor">The source color blend factor.</param>
        /// <param name="destColor">The destenation color blend factor.</param>
        /// <param name="colorBlendFunc">The color blend function.</param>
        /// <param name="blendFactor">The blend factor to use for parameterized blend states.</param>
        /// <returns>A new <see cref="BlendState"/>.</returns>
        protected abstract BlendState CreateCustomBlendStateCore(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc,
            RgbaFloat blendFactor);

        /// <summary>
        /// Creates a new <see cref="DepthStencilState"/>, used to control depth and stencil comparisons in the device's output merger.
        /// </summary>
        /// <param name="isDepthEnabled">A value indicating whether depth testing is enabled in the new state.</param>
        /// <param name="comparison">The kind of <see cref="DepthComparison"/> to use in the new state.</param>
        /// <returns>A new <see cref="DepthStencilState"/>.</returns>
        public DepthStencilState CreateDepthStencilState(bool isDepthEnabled, DepthComparison comparison)
        {
            return CreateDepthStencilState(isDepthEnabled, comparison, true);
        }

        /// <summary>
        /// Creates a new <see cref="DepthStencilState"/>, used to control depth and stencil comparisons in the device's output merger.
        /// </summary>
        /// <param name="isDepthEnabled">A value indicating whether depth testing is enabled in the new state.</param>
        /// <param name="comparison">The kind of <see cref="DepthComparison"/> to use in the new state.</param>
        /// <param name="isDepthWriteEnabled">A value indicating whether the depth buffer is written to when drawing.</param>
        /// <returns>A new <see cref="DepthStencilState"/>.</returns>
        public DepthStencilState CreateDepthStencilState(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled)
        {
            return CreateDepthStencilStateCore(isDepthWriteEnabled, comparison, isDepthWriteEnabled);
        }

        /// <summary>
        /// Creates a new <see cref="DepthStencilState"/>, used to control depth and stencil comparisons in the device's output merger.
        /// </summary>
        /// <param name="isDepthEnabled">A value indicating whether depth testing is enabled in the new state.</param>
        /// <param name="comparison">The kind of <see cref="DepthComparison"/> to use in the new state.</param>
        /// <param name="isDepthWriteEnabled">A value indicating whether the depth buffer is written to when drawing.</param>
        /// <returns>A new <see cref="DepthStencilState"/>.</returns>
        protected abstract DepthStencilState CreateDepthStencilStateCore(bool isDepthEnabled, DepthComparison comparison, bool isDepthWriteEnabled);

        /// <summary>
        /// Creates a new <see cref="RasterizerState"/>, used to control various behaviors of the device's rasterizer.
        /// </summary>
        /// <param name="cullMode">Controls which primitive faces are culled.</param>
        /// <param name="fillMode">The kind of triangle filling to use.</param>
        /// <param name="isDepthClipEnabled">Whether or not primitives are clipped by depth.</param>
        /// <param name="isScissorTestEnabled">Whether or not primitives are clipped by the scissor test.</param>
        /// <returns>A new <see cref="RasterizerState"/>.</returns>
        public RasterizerState CreateRasterizerState(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled)
        {
            return CreateRasterizerStateCore(cullMode, fillMode, isDepthClipEnabled, isScissorTestEnabled);
        }

        /// <summary>
        /// Creates a new <see cref="RasterizerState"/>, used to control various behaviors of the device's rasterizer.
        /// </summary>
        /// <param name="cullMode">Controls which primitive faces are culled.</param>
        /// <param name="fillMode">The kind of triangle filling to use.</param>
        /// <param name="isDepthClipEnabled">Whether or not primitives are clipped by depth.</param>
        /// <param name="isScissorTestEnabled">Whether or not primitives are clipped by the scissor test.</param>
        /// <returns>A new <see cref="RasterizerState"/>.</returns>
        protected abstract RasterizerState CreateRasterizerStateCore(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled);

        protected abstract GraphicsBackend PlatformGetGraphicsBackend();

        private string GetDefaultEntryPoint(ShaderStages type)
        {
            switch (type)
            {
                case ShaderStages.Vertex:
                    return "VS";
                case ShaderStages.TessellationControl:
                    return "TC";
                case ShaderStages.TessellationEvaluation:
                    return "TE";
                case ShaderStages.Geometry:
                    return "GS";
                case ShaderStages.Fragment:
                    return "FS";
                default: throw Illegal.Value<ShaderStages>();
            }
        }
    }
}
