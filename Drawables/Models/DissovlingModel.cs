﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace OpenTkProject.Drawables.Models
{
    class DissovlingModel : Model
    {
        float state = 1;
        public DissovlingModel(GameObject parent):base(parent){
            color = new Vector4(0.8f, 0.3f, 0.8f, 1.0f) * 0.2f;
        }

        public override void update()
        {
            state *= 0.97f;

            Vector3 oldSize = Size;

            Size = new Vector3(oldSize.X, state, oldSize.Z);
            //updateModelMatrix();
            

            if (state < 0.05f)
            {
                kill();
            }

            updateChilds();
        }

        protected override void setSpecialUniforms(Shader curShader)
        {
            GL.Uniform1(curShader.modLocation, 1, ref state);
        }
    }
}
