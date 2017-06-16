﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpGL
{

    public partial class Scene
    {
        class LegacyPickEventArgs : RenderEventArgs
        {
            public readonly mat4 pickMatrix;
            public readonly int x;
            public readonly int y;
            public readonly Dictionary<uint, RendererBase> hitMap;

            public LegacyPickEventArgs(mat4 pickMatrix, Scene scene, int x, int y)
                : base(scene)
            {
                this.pickMatrix = pickMatrix;
                this.x = x;
                this.y = y;
                this.hitMap = new Dictionary<uint, RendererBase>();
            }

            public override mat4 GetProjectionMatrix()
            {
                return this.pickMatrix * this.Scene.Camera.GetProjectionMatrix();
            }
        }

        /// <summary>
        /// Pick geometry at specified positon.
        /// </summary>
        /// <param name="x">Left Down is (0, 0)</param>
        /// <param name="y">Left Down is (0, 0)</param>
        /// <param name="geometryType"></param>
        /// <returns></returns>
        public List<RendererBase> Pick(int x, int y)
        {
            var viewport = new int[4];
            //	Get the viewport, then convert the mouse point to an opengl point.
            GL.Instance.GetIntegerv((uint)GetTarget.Viewport, viewport);

            //	Create a select buffer.
            var selectBuffer = new uint[512];
            GL.Instance.SelectBuffer(selectBuffer.Length, selectBuffer);
            //	Enter select mode.
            GL.Instance.RenderMode(GL.GL_SELECT);
            //	Initialise the names, and add the first name.
            GL.Instance.InitNames();
            GL.Instance.PushName(0);

            ////	Push matrix, set up projection, then load matrix.
            //GL.Instance.MatrixMode(GL.GL_PROJECTION);
            //GL.Instance.PushMatrix();
            //GL.Instance.LoadIdentity();
            mat4 pickMatrix = glm.pickMatrix(new vec2(x, y), new vec2(4, 4), new ivec4(viewport[0], viewport[1], viewport[2], viewport[3]));
            //mat4 projectionMatrix = this.Camera.GetProjectionMatrix();
            //mat4 viewMatrix = this.Camera.GetViewMatrix();
            //GL.Instance.MatrixMode(GL.GL_PROJECTION);
            //GL.Instance.LoadIdentity();
            //GL.Instance.MultMatrixf((pickMatrix * projectionMatrix * viewMatrix).ToArray());

            var arg = new LegacyPickEventArgs(pickMatrix, this, x, y);

            {
                //const float one = 1.0f;
                //GL.Instance.ClearColor(one, one, one, one);
                //GL.Instance.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT | GL.GL_STENCIL_BUFFER_BIT);

                uint currentName = 1;
                this.RenderForPicking(this.RootElement, arg, ref currentName);
            }

            //GL.Instance.MatrixMode(GL.GL_PROJECTION);
            //GL.Instance.PopMatrix();

            //	Flush commands.
            GL.Instance.Flush();

            //  Create  result set.
            var pickedRenderer = new List<RendererBase>();

            //	End selection.
            int hits = GL.Instance.RenderMode(GL.GL_RENDER);
            uint posinarray = 0;
            //  Go through each name.
            for (int hit = 0; hit < hits; hit++)
            {
                uint nameCount = selectBuffer[posinarray];
                posinarray += 3;

                if (nameCount == 0) { continue; }

                //	Add each hit element to the result set to the array.
                for (int i = 0; i < nameCount; i++)
                {
                    uint hitName = selectBuffer[posinarray++];
                    pickedRenderer.Add(arg.hitMap[hitName]);
                }
            }

            //  Return the result set.
            return pickedRenderer;
        }

        private void RenderForPicking(RendererBase sceneElement, LegacyPickEventArgs arg, ref uint currentName)
        {
            if (sceneElement != null)
            {
                var pickable = sceneElement as IRenderable;
                if (pickable != null)
                {
                    //  Load and map the name.
                    GL.Instance.LoadName(currentName);
                    arg.hitMap[currentName] = sceneElement;

                    pickable.Render(arg);

                    //  Increment the name.
                    currentName++;
                }

                var node = sceneElement as ITreeNode<RendererBase>;
                if (node != null)
                {
                    foreach (var item in node.Children)
                    {
                        this.RenderForPicking(item, arg, ref currentName);
                    }
                }
            }
        }
    }
}