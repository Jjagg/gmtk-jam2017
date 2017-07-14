using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace GameSandbox
{
    public class MatrixChain
    {
        private readonly Stack<Matrix> _matrices;
        private readonly int _minCount;

        public MatrixChain(int minCount = 0)
        {
            _matrices = new Stack<Matrix>();
            _minCount = minCount;
        }

        public Matrix Push(ref Matrix m)
        {
            var absMatrix = m * Get();
            _matrices.Push(absMatrix);
            return absMatrix;
        }

        public void Pop()
        {
            if (_matrices.Count > _minCount)
                _matrices.Pop();
        }

        public Matrix Get()
        {
            return _matrices.Any() ? _matrices.Peek() : Matrix.Identity;
        }
    }
}