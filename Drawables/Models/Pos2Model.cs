﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTkProject.Drawables.Models
{
    class Pos2Model : Model
    {
        protected Matrix4 orientation2;
        protected Matrix4 modelMatrix2;

        public Pos2Model(GameObject parent)
            : base(parent)
        {
        }

        public Matrix4 Orientation2
        {
            get { return orientation2; }
            set { orientation2 = value; }
        }

        public Matrix4 ModelMatrix2
        {
            get { return modelMatrix2; }
            set { modelMatrix2 = value; }
        }

        public Vector3 Position2
        {
            get { return position; }
            set
            {
                position = value;
                updateModelMatrix2();
            }
        }


        private void updateModelMatrix2()
        {
            //modelMatrix = Matrix4.Identity;
            Matrix4 scaleMatrix = Matrix4.Scale(Size);
            Matrix4 translationMatrix = Matrix4.CreateTranslation(position);

            //Matrix4.Mult(ref translationMatrix, ref modelMatrix, out modelMatrix);
            Matrix4.Mult(ref scaleMatrix, ref translationMatrix, out modelMatrix2);
            //Matrix4.Mult(ref orientation, ref modelMatrix, out modelMatrix);
        }

        protected override void setupMatrices(ViewInfo curView, Shader curShader)
        {
            base.setupMatrices( curView,  curShader);

            GL.UniformMatrix4(curShader.rotationMatrixLocation2, false, ref orientation2);
            GL.UniformMatrix4(curShader.modelMatrixLocation2, false, ref modelMatrix2);
        }
    }
}
