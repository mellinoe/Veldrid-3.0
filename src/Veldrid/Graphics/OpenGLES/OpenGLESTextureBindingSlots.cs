﻿using OpenTK.Graphics.ES30;
using System;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESTextureBindingSlots
    {
        private readonly OpenGLESProgramTextureBinding[] _textureBindings;

        public OpenGLESTextureBindingSlots(ShaderSet shaderSet, ShaderResourceDescription[] textureInputs)
        {
            _textureBindings = new OpenGLESProgramTextureBinding[textureInputs.Length];
            for (int i = 0; i < textureInputs.Length; i++)
            {
                var element = textureInputs[i];
                int location = GL.GetUniformLocation(((OpenGLESShaderSet)shaderSet).ProgramID, element.Name);
                Utilities.CheckLastGLES3Error();
                if (location == -1)
                {
                    throw new VeldridException($"No sampler was found with the name {element.Name}");
                }

                _textureBindings[i] = new OpenGLESProgramTextureBinding(location);
            }
        }

        public int GetUniformLocation(int slot)
        {
            if (slot < 0 || slot >= _textureBindings.Length)
            {
                throw new ArgumentOutOfRangeException($"Invalid slot:{slot}. Valid range:{0}-{_textureBindings.Length - 1}.");
            }

            return _textureBindings[slot].UniformLocation;
        }

        private struct OpenGLESProgramTextureBinding
        {
            public readonly int UniformLocation;

            public OpenGLESProgramTextureBinding(int location)
            {
                UniformLocation = location;
            }
        }
    }
}