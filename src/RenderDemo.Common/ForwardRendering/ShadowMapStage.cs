﻿using System;
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
            _depthTexture = rc.ResourceFactory.CreateDepthTexture(DepthMapWidth, DepthMapHeight, sizeof(ushort), PixelFormat.Alpha_UInt16);
            _shadowMapFramebuffer = rc.ResourceFactory.CreateFramebuffer();
            _shadowMapFramebuffer.DepthTexture = _depthTexture;
            rc.GetTextureContextBinding(_contextBindingName).Value = _depthTexture;
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
            throw new NotImplementedException();
        }

        private void Dispose()
        {
            _shadowMapFramebuffer.Dispose();
        }
    }
}
