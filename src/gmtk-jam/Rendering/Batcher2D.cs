using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using gmtk_jam.Extended;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace gmtk_jam.Rendering
{
    public class Batcher2D
    {
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl,
                    EntryPoint = "SDL_GL_GetProcAddress", ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(string proc);

        private delegate void SetLineWidthDelegate(float width);
        private readonly SetLineWidthDelegate SetLineWidth;

        private class DrawInfo
        {
            public readonly Effect Effect;
            public readonly PrimitiveType PrimitiveType;
            public readonly Texture2D T1;
            public readonly Texture2D T2;
            public readonly int LineWidth;

            public DrawInfo(Effect effect, PrimitiveType primitiveType, Texture2D t1, Texture2D t2, int lineWidth)
            {
                Effect = effect;
                PrimitiveType = primitiveType;
                T1 = t1;
                T2 = t2;
                LineWidth = lineWidth;
            }

            public static DrawInfo ForLine(Effect effect, Texture2D tex, int width)
            {
                return new DrawInfo(effect, PrimitiveType.LineList, tex, null, width);
            }

            public static DrawInfo ForFill(Effect effect, Texture2D tex)
            {
                return new DrawInfo(effect, PrimitiveType.TriangleList, tex, null, -1);
            }

            public bool Equals(DrawInfo other)
            {
                return Effect == other.Effect &&
                       (T1 == null || other.T1 == null || T1 == other.T1) &&
                       (T2 == null || other.T2 == null || T1 == other.T1) &&
                       PrimitiveType == other.PrimitiveType &&
                       (PrimitiveType != PrimitiveType.LineList || LineWidth == other.LineWidth);
            }
        }

        private class BatchInfo
        {
            public readonly DrawInfo DrawInfo;
            public readonly int Startindex;
            public readonly int IndexCount;

            public BatchInfo(DrawInfo drawInfo, int startindex, int indexCount)
            {
                DrawInfo = drawInfo;
                Startindex = startindex;
                IndexCount = indexCount;
            }

            public int GetPrimitiveCount()
            {
                switch (DrawInfo.PrimitiveType)
                {
                    case PrimitiveType.LineList:
                        return IndexCount / 2;
                    case PrimitiveType.LineStrip:
                        return IndexCount - 1;
                    case PrimitiveType.TriangleList:
                        return IndexCount / 3;
                    case PrimitiveType.TriangleStrip:
                        return IndexCount - 2;
                }

                throw new NotSupportedException();
            }
        }

        private static int _setLineWidth = 1;

        public const int DefaultMaxVertices = 2048;
        public const int DefaultMaxIndices = 4096;

        public readonly BasicEffect BasicEffect;
        private readonly Texture2D _blankTexture;

        private readonly VertexPositionColorTexture[] _vb;
        private readonly int[] _ib;
        private readonly GraphicsDevice _gd;
        public Matrix CameraMatrix { get; set; }
        public readonly MatrixChain Matrices;

        private int _nextToDraw;
        private int _indicesInBatch;
        private DrawInfo _lastDrawInfo;
        private int _verticesSubmitted;

        private readonly List<BatchInfo> _batches;

        public Batcher2D(GraphicsDevice gd)
        {
            BasicEffect = new BasicEffect(gd)
            {
                LightingEnabled = false,
                VertexColorEnabled = true,
                TextureEnabled = false,
            };

            _blankTexture = new Texture2D(gd, 1, 1);
            _blankTexture.SetData(new[] {Color.White.PackedValue});

            _vb = new VertexPositionColorTexture[DefaultMaxVertices];
            _ib = new int[DefaultMaxIndices];
            _gd = gd;
            _batches = new List<BatchInfo>();

            Matrices = new MatrixChain();

            // load glLineWidth
            SetLineWidth = (SetLineWidthDelegate) Marshal.GetDelegateForFunctionPointer(GetProcAddress("glLineWidth"), typeof(SetLineWidthDelegate));
        }

        #region Line

        public void DrawLine(Vector2 p1, Vector2 p2, Color color, int lineWidth = 1)
        {
            var di = DrawInfo.ForLine(BasicEffect, _blankTexture, lineWidth);
            CheckFlush(di);

            var v1 = AddVertex(p1, color);
            var v2 = AddVertex(p2, color);
            AddIndex(v1);
            AddIndex(v2);
        }

        public void DrawLines(IEnumerable<Vector2> points, Color color, int lineWidth = 1)
        {
            var di = DrawInfo.ForLine(BasicEffect, _blankTexture, lineWidth);
            CheckFlush(di);

            var en = points.GetEnumerator();
            if (!en.MoveNext())
                throw new Exception("No points.");

            var v1 = AddVertex(en.Current, color);
            while (en.MoveNext())
            {
                AddIndex(v1);
                var v2 = AddVertex(en.Current, color);
                AddIndex(v2);
                v1 = v2;
            }
            en.Dispose();
        }

        #endregion

        #region Rectangle

        public void DrawRect(Rectangle rect, Color color, int lineWidth = 1)
        {
            var di = DrawInfo.ForLine(BasicEffect, _blankTexture, lineWidth);
            CheckFlush(di);

            var v1 = AddVertex(new Vector2(rect.Left, rect.Top), color);
            var v2 = AddVertex(new Vector2(rect.Right, rect.Top), color);
            var v3 = AddVertex(new Vector2(rect.Right, rect.Bottom), color);
            var v4 = AddVertex(new Vector2(rect.Left, rect.Bottom), color);
            AddIndex(v1);
            AddIndex(v2);
            AddIndex(v2);
            AddIndex(v3);
            AddIndex(v3);
            AddIndex(v4);
            AddIndex(v4);
            AddIndex(v1);
        }

        public void FillRect(Rectangle rect, Color color)
        {
            var di = DrawInfo.ForFill(BasicEffect, _blankTexture);
            CheckFlush(di);

            var v1 = AddVertex(new Vector2(rect.Left, rect.Top), color);
            var v2 = AddVertex(new Vector2(rect.Right, rect.Top), color);
            var v3 = AddVertex(new Vector2(rect.Right, rect.Bottom), color);
            var v4 = AddVertex(new Vector2(rect.Left, rect.Bottom), color);
            AddIndex(v1);
            AddIndex(v2);
            AddIndex(v4);
            AddIndex(v4);
            AddIndex(v2);
            AddIndex(v3);
        }

        public void FillRect(Rectangle rect, Texture2D tex)
        {
            var di = DrawInfo.ForFill(BasicEffect, tex);
            CheckFlush(di);

            var v1 = AddVertex(new Vector2(rect.Left, rect.Top), Vector2.Zero);
            var v2 = AddVertex(new Vector2(rect.Right, rect.Top), Vector2.UnitX);
            var v3 = AddVertex(new Vector2(rect.Right, rect.Bottom), Vector2.One);
            var v4 = AddVertex(new Vector2(rect.Left, rect.Bottom), Vector2.UnitY);

            AddIndex(v1);
            AddIndex(v2);
            AddIndex(v4);
            AddIndex(v4);
            AddIndex(v2);
            AddIndex(v3);   
        }

        public void DrawRoundedRect(Rectangle rect, Color color, int lineWidth = 1)
        {
            var di = DrawInfo.ForLine(BasicEffect, _blankTexture, lineWidth);
            CheckFlush(di);

            var v1 = AddVertex(new Vector2(rect.Left, rect.Top), color);
            var v2 = AddVertex(new Vector2(rect.Right, rect.Top), color);
            var v3 = AddVertex(new Vector2(rect.Right, rect.Bottom), color);
            var v4 = AddVertex(new Vector2(rect.Left, rect.Bottom), color);
            AddIndex(v1);
            AddIndex(v2);
            AddIndex(v2);
            AddIndex(v3);
            AddIndex(v3);
            AddIndex(v4);
            AddIndex(v4);
            AddIndex(v1);
        }

        /*
        public void FillRoundedRect(int width, int height, int xRadius, int yRadius, int segments, Color color)
        {
            var di = DrawInfo.ForFill(BasicEffect, _blankTexture);
            CheckFlush(di);

            for (var i = 0; i < 4; i++)
            {
                var en = ps.GetEnumerator();
                en.MoveNext();
                var v0 = AddVertex(en.Current, color);
                var v1 = v0;
                var vCenter = AddVertex(center, color);
                while (en.MoveNext())
                {
                    var v2 = AddVertex(p, color);
                    AddIndex(v1);
                    AddIndex(v2);
                    AddIndex(vCenter);
                    v1 = v2;
                }
                en.Dispose();

                AddIndex(v1);
                AddIndex(v0);
                AddIndex(vCenter);
            }
        }

        public void FillRoundedRect(Rectangle rect, Texture2D tex)
        {
            var di = DrawInfo.ForFill(BasicEffect, tex);
            CheckFlush(di);

            var v1 = AddVertex(new Vector2(rect.Left, rect.Top), Vector2.Zero);
            var v2 = AddVertex(new Vector2(rect.Right, rect.Top), Vector2.UnitX);
            var v3 = AddVertex(new Vector2(rect.Right, rect.Bottom), Vector2.One);
            var v4 = AddVertex(new Vector2(rect.Left, rect.Bottom), Vector2.UnitY);

            AddIndex(v1);
            AddIndex(v2);
            AddIndex(v4);
            AddIndex(v4);
            AddIndex(v2);
            AddIndex(v3);   
        }
        */

        #endregion

        public void DrawCircle(Vector2 center, float radius, Color color, int sides, int lineWidth = 1)
        {
            var ps = ExtendedUtil.CreateCircle(center, radius, sides);
            DrawLines(ps, color, lineWidth);
        }

        public void FillCircle(Vector2 center, float radius, Color color, int sides)
        {
            var di = new DrawInfo(BasicEffect, PrimitiveType.TriangleList, _blankTexture, null, 0);
            CheckFlush(di);

            var ps = ExtendedUtil.CreateCircle(center, radius, sides);

            var en = ps.GetEnumerator();
            en.MoveNext();
            var v0 = AddVertex(en.Current, color);
            var v1 = v0;
            var vCenter = AddVertex(center, color);
            while (en.MoveNext())
            {
                var v2 = AddVertex(en.Current, color);
                AddIndex(v1);
                AddIndex(v2);
                AddIndex(vCenter);
                v1 = v2;
            }
            AddIndex(v1);
            AddIndex(v0);
            AddIndex(vCenter);
            en.Dispose();
        }

        public void Flush()
        {
            _gd.BlendState = BlendState.Opaque;
            _gd.DepthStencilState = DepthStencilState.None;
            _gd.SamplerStates[0] = SamplerState.LinearWrap;

            // register last batch
            RegisterFlush();
            foreach (var b in _batches)
            {
                BasicEffect.Texture = b.DrawInfo.T1;

                if (b.DrawInfo.PrimitiveType == PrimitiveType.LineList && b.DrawInfo.LineWidth != _setLineWidth)
                {
                    _setLineWidth = b.DrawInfo.LineWidth;
                    SetLineWidth(_setLineWidth);
                }

                _gd.Textures[0] = b.DrawInfo.T1;
                _gd.Textures[1] = b.DrawInfo.T2;

                foreach (var p in b.DrawInfo.Effect.CurrentTechnique.Passes)
                {
                    p.Apply();
                    var primCount = b.GetPrimitiveCount();
                    _gd.DrawUserIndexedPrimitives(b.DrawInfo.PrimitiveType, _vb, 0, _verticesSubmitted, _ib, b.Startindex, primCount);
                }
            }

            _batches.Clear();
            _nextToDraw = 0;
            _indicesInBatch = 0;
            _verticesSubmitted = 0;
            _lastDrawInfo = null;
        }

        private void CheckFlush(DrawInfo di)
        {
            if (_lastDrawInfo != null && !_lastDrawInfo.Equals(di))
                RegisterFlush();

            _lastDrawInfo = di;
        }

        private void RegisterFlush()
        {
            // if nothing to flush
            if (_indicesInBatch == 0)
                return;

            var bi = new BatchInfo(_lastDrawInfo, _nextToDraw, _indicesInBatch);
            _batches.Add(bi);

            _nextToDraw = _nextToDraw + _indicesInBatch;
            _indicesInBatch = 0;
        }

        private int AddVertex(Vector2 position, Vector2 uv)
        {
            return AddVertex(position, Color.White, uv);
        }

        private int AddVertex(Vector2 position, Color color, Vector2? uv = null)
        {
            var m = Matrices.Get() * CameraMatrix;
            position = Vector2.Transform(position, m);
            var vertex = new VertexPositionColorTexture(new Vector3(position, 0f), color, uv ?? Vector2.Zero);
            var i = _verticesSubmitted;
            _vb[i] = vertex;
            _verticesSubmitted++;
            return i;
        }

        private void AddIndex(int index)
        {
            var i = _nextToDraw + _indicesInBatch;
            _ib[i] = index;
            _indicesInBatch++;
        }
    }
}
