﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using System;
using System.Numerics;
using Veldrid.Assets;
using Veldrid.Graphics;
using Veldrid.Graphics.Pipeline;

namespace Veldrid.RenderDemo.ForwardRendering
{
    public class ShadowMapStage : PipelineStage
    {
        private bool _takeScreenshot = false;

        private string _contextBindingName = "ShadowMap";

        public int DepthMapWidth { get; private set; } = 2048;
        public int DepthMapHeight { get; private set; } = 2048;

        private readonly RenderQueue _queue = new RenderQueue();

        private Framebuffer _shadowMapFramebuffer;
        private DeviceTexture2D _depthTexture;
        private ShaderTextureBinding _depthTextureBinding;

        public bool Enabled { get; set; } = true;

        public string Name => "ShadowMap";

        public RenderContext RenderContext { get; private set; }

        public Vector3 DirectionalLightPosition { get; set; }

        public ShadowMapStage(RenderContext rc, string contextBindingName = "ShadowMap")
        {
            RenderContext = rc;
            _contextBindingName = contextBindingName;
            InitializeContextObjects(rc);
        }

        public void ChangeRenderContext(RenderContext rc)
        {
            RenderContext = rc;
            InitializeContextObjects(rc);
        }

        private void InitializeContextObjects(RenderContext rc)
        {
            _depthTexture = rc.ResourceFactory.CreateTexture(
                1,
                DepthMapWidth,
                DepthMapHeight,
                PixelFormat.R16_UInt,
                DeviceTextureCreateOptions.DepthStencil);
            _depthTextureBinding = rc.ResourceFactory.CreateShaderTextureBinding(_depthTexture);
            _shadowMapFramebuffer = rc.ResourceFactory.CreateFramebuffer();
            _shadowMapFramebuffer.DepthTexture = _depthTexture;
            SharedTextures.SetTextureBinding(_contextBindingName, _depthTextureBinding);
        }

        public void ExecuteStage(VisibiltyManager visibilityManager, Vector3 viewPosition)
        {
            RenderContext.ClearScissorRectangle();
            RenderContext.SetFramebuffer(_shadowMapFramebuffer);
            RenderContext.ClearBuffer();
            RenderContext.SetViewport(0, 0, DepthMapWidth, DepthMapHeight);
            _queue.Clear();
            visibilityManager.CollectVisibleObjects(_queue, "ShadowMap", DirectionalLightPosition);
            _queue.Sort();
            foreach (RenderItem item in _queue)
            {
                item.Render(RenderContext, "ShadowMap");
            }

            if (_takeScreenshot)
            {
                _takeScreenshot = false;
                SaveDepthTextureToFile();
            }
        }

        public void SaveNextFrame() => _takeScreenshot = true;

        private void SaveDepthTextureToFile()
        {
            int width = DepthMapWidth;
            int height = DepthMapHeight;
            var cpuDepthTexture = new RawTextureDataArray<ushort>(width, height, sizeof(ushort), PixelFormat.R16_UInt);
            _depthTexture.GetTextureData(0, cpuDepthTexture.PixelData);

            Image<Rgba32> image = new Image<Rgba32>(width, height);
            unsafe
            {
                fixed (Rgba32* pixelsPtr = &image.DangerousGetPinnableReferenceToPixelBuffer())
                {
                    PixelFormatConversion.ConvertPixelsUInt16DepthToRgbaFloat(width * height, cpuDepthTexture.PixelData, pixelsPtr);
                }
            }
            ImageSharpTexture rgbaDepthTexture = new ImageSharpTexture(image);
            Console.WriteLine($"Saving file: {width} x {height}, ratio:{(double)width / height}");
            rgbaDepthTexture.SaveToFile(Environment.TickCount + ".png");
        }

        private void Dispose()
        {
            _shadowMapFramebuffer.Dispose();
        }
    }
}
