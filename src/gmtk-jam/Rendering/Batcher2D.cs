using System;
using System.Collections.Generic;
using System.Linq;
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
                TextureEnabled = true,
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

        public void DrawLineStrip(IList<Vector2> points, Color color, int lineWidth = 1)
        {
            var di = DrawInfo.ForLine(BasicEffect, _blankTexture, lineWidth);
            CheckFlush(di);

            if (points.Count < 2)
                throw new Exception("Need at least two points.");

            var v1 = AddVertex(points[0], color);
            for (var i = 1; i < points.Count; i++)
            {
                var p = points[i];
                AddIndex(v1);
                var v2 = AddVertex(p, color);
                AddIndex(v2);
                v1 = v2;
            }
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
            FillRect(rect, color, color, color, color);
        }

        public void FillRect(Rectangle rect, Color c1, Color c2, Color c3, Color c4)
        {
            var di = DrawInfo.ForFill(BasicEffect, _blankTexture);
            CheckFlush(di);

            var v1 = AddVertex(new Vector2(rect.Left, rect.Top), c1);
            var v2 = AddVertex(new Vector2(rect.Right, rect.Top), c2);
            var v3 = AddVertex(new Vector2(rect.Right, rect.Bottom), c3);
            var v4 = AddVertex(new Vector2(rect.Left, rect.Bottom), c4);
            AddIndex(v1);
            AddIndex(v2);
            AddIndex(v4);
            AddIndex(v4);
            AddIndex(v2);
            AddIndex(v3);
        }

        public void FillRect(Rectangle rect, Sprite sprite)
        {
            var di = DrawInfo.ForFill(BasicEffect, sprite.Texture);
            CheckFlush(di);

            var v1 = AddVertex(new Vector2(rect.Left, rect.Top), sprite.UV1);
            var v2 = AddVertex(new Vector2(rect.Right, rect.Top), sprite.UV2);
            var v3 = AddVertex(new Vector2(rect.Right, rect.Bottom), sprite.UV3);
            var v4 = AddVertex(new Vector2(rect.Left, rect.Bottom), sprite.UV4);

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

        public void FillRoundedRect(Rectangle rectangle, float radius, int segments, Color color)
        {
            if (radius > rectangle.Width / 2f || radius > rectangle.Height / 2f)
                throw new Exception("Radius too large");

            if (radius == 0)
            {
                FillRect(rectangle, color);
                return;
            }

            var outerRect = rectangle;
            var innerRect = rectangle;
            innerRect.Inflate(-radius, -radius);

            FillRect(new Rectangle(innerRect.Left, outerRect.Top, innerRect.Width, outerRect.Height), color);
            FillRect(new Rectangle(outerRect.Left, innerRect.Top, outerRect.Width, innerRect.Height), color);
            var leftAngle = MathHelper.Pi;
            var topAngle = 3 * MathHelper.PiOver2;
            var rightAngle = 0;
            var botAngle = MathHelper.PiOver2;
            var tl = new Vector2(innerRect.Left, innerRect.Top);
            var tr = new Vector2(innerRect.Right, innerRect.Top);
            var bl = new Vector2(innerRect.Left, innerRect.Bottom);
            var br = new Vector2(innerRect.Right, innerRect.Bottom);
            FillCircleSegment(tl, radius, leftAngle, topAngle, color, segments);
            FillCircleSegment(tr, radius, topAngle, rightAngle, color, segments);
            FillCircleSegment(br, radius, rightAngle, botAngle, color, segments);
            FillCircleSegment(bl, radius, botAngle, leftAngle, color, segments);
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

        #endregion

        #region Circle

        public void DrawCircle(Vector2 center, float radius, Color color, int sides, int lineWidth = 1)
        {
            DrawCircleSegment(center, radius, 0, MathHelper.TwoPi, color, lineWidth);
        }

        public void DrawCircleSegment(Vector2 center, float radius, float start, float end, Color color, int sides, int lineWidth = 1)
        {
            var ps = ExtendedUtil.CreateCircle(center, radius, sides, start, end);
            DrawLineStrip(ps, color, lineWidth);
        }

        public void FillCircle(Vector2 center, float radius, Color color, int sides)
        {
            FillCircleSegment(center, radius, 0, MathHelper.TwoPi, color, sides);
        }

        public void FillCircleSegment(Vector2 center, float radius, float start, float end, Color color, int sides)
        {
            var ps = ExtendedUtil.CreateCircle(center, radius, sides, start, end);
            FillTriangleFan(center, ps, color);
        }

        #endregion

        public void Flush()
        {
            _gd.BlendState = BlendState.AlphaBlend;
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

        #region Low level

        public void FillTriangleFan(Vector2 center, IList<Vector2> ps, Color color)
        {
            var di = DrawInfo.ForFill(BasicEffect, _blankTexture);
            CheckFlush(di);

            if (ps.Count < 3)
                throw new Exception("Triangle fan needs at least 3 points.");

            var centerIndex = AddVertex(center, color);
            var v0 =  AddVertex(ps[0], color);
            var v1 = AddVertex(ps[0], color);
            for (var i = 1; i < ps.Count; i++)
            {
                var v2 = AddVertex(ps[i], color);
                AddIndex(v1);
                AddIndex(v2);
                AddIndex(centerIndex);
                v1 = v2;
            }
            AddIndex(v1);
            AddIndex(v0);
            AddIndex(centerIndex);
        }

        #endregion

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

