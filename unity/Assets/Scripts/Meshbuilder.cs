using System;
using Unity.Collections;
using UnityEngine;

namespace ldjam41 {
    public class Meshbuilder : IDisposable {
        private NativeArray<Vector3> _vertices;
        private int _verticesIdx;

        private NativeArray<Vector2> _uvs; // uses vertexIdx
        private NativeArray<Vector2> _uvs2; // uses vertexIdx

        private NativeArray<int> _triangles;
        private int _trianglesIdx;

        private NativeArray<Vector3> _normals;
        private int _normalIdx;

        //private IDictionary<Vector3, int> _existingVerts;

        public bool InvertTriangles;

        /// <summary>
        /// Creates a short lived mesh builder that doesn't even survive one frame.
        /// </summary>
        /// <param name="vertexCount"></param>
        /// <param name="triangleCount"></param>
        public Meshbuilder(int vertexCount, int triangleCount) {
            _vertices = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
            _uvs = new NativeArray<Vector2>(vertexCount, Allocator.Temp);
            _uvs2 = new NativeArray<Vector2>(vertexCount, Allocator.Temp);
            _triangles = new NativeArray<int>(triangleCount, Allocator.Temp);
            _normals = new NativeArray<Vector3>(vertexCount, Allocator.Temp);
        }

        public int AddVertex(Vector3 vertexPosition, Vector2 uv) {
            ensureVertexArrayLength(_verticesIdx + 1);

            var idx = _verticesIdx++;
            _vertices[idx] = vertexPosition;
            _uvs[idx] = uv;

            return idx;
        }

        public void AddTriangle(int idx0, int idx1, int idx2) {
            ensureTrianglesArrayLength(_trianglesIdx + 3);

            if (InvertTriangles) {
                _triangles[_trianglesIdx++] = idx0;
                _triangles[_trianglesIdx++] = idx2;
                _triangles[_trianglesIdx++] = idx1;
            }
            else {
                _triangles[_trianglesIdx++] = idx0;
                _triangles[_trianglesIdx++] = idx1;
                _triangles[_trianglesIdx++] = idx2;
            }

            var a = _vertices[idx1] - _vertices[idx0];
            var b = _vertices[idx2] - _vertices[idx0];

            ensureNormalsArrayLength(Mathf.Max(idx2, Mathf.Max(idx1, idx0)) + 1);
            var temp = Vector3.Cross(a, b);
            _normals[idx0] = temp;
            _normals[idx1] = temp;
            _normals[idx2] = temp;
        }

        public void setUv2(int idx, Vector2 uv) {
            _uvs2[idx] = uv;
        }

        /// <summary>
        /// Adds a quad (two triangles).
        /// </summary>
        /// <param name="idxtl">Top left index</param>
        /// <param name="idxtr">Top right index</param>
        /// <param name="idxbl">Bottom left index</param>
        /// <param name="idxbr">Bottom right index</param>
        public void AddQuad(int idxtl, int idxtr, int idxbl, int idxbr) {
            ensureTrianglesArrayLength(_trianglesIdx + 6);

            AddTriangle(idxtr, idxbl, idxtl);
            AddTriangle(idxtr, idxbr, idxbl);
        }

        private void ensureTrianglesArrayLength(int needed) {
            if (needed <= _triangles.Length) {
                return;
            }

            // ensure the new size is a multiple of 3
            var newSize = Mathf.CeilToInt(Mathf.Round(_triangles.Length * 1.5f / 3.0f) * 3);
            //Debug.LogWarning("Expanded triangle array to <" + newSize + ">.");

            var tempTriangles = _triangles;
            _triangles = new NativeArray<int>(newSize, Allocator.Temp);
            for (var i = 0; i < tempTriangles.Length; i++) {
                _triangles[i] = tempTriangles[i];
            }

            tempTriangles.Dispose();
        }

        private void ensureNormalsArrayLength(int needed) {
            if (needed <= _normals.Length) {
                return;
            }

            // ensure the new size is a multiple of 3
            var newSize = Mathf.CeilToInt(Mathf.Round(_normals.Length * 1.5f / 3.0f) * 3);
            //Debug.LogWarning("Expanded triangle array to <" + newSize + ">.");

            var tempNormals = _normals;
            _normals = new NativeArray<Vector3>(newSize, Allocator.Temp);
            for (var i = 0; i < tempNormals.Length; i++) {
                _normals[i] = tempNormals[i];
            }

            tempNormals.Dispose();
        }

        private void ensureVertexArrayLength(int needed) {
            if (needed <= _vertices.Length) {
                return;
            }

            var tempVertices = _vertices;
            var tempUvs = _uvs;
            var tempUvs2 = _uvs2;

            var newSize = Mathf.CeilToInt(_uvs.Length * 1.5f);
            //Debug.LogWarning("Expanded vertex array to <" + newSize + ">.");

            _vertices = new NativeArray<Vector3>(newSize, Allocator.Temp);
            _uvs = new NativeArray<Vector2>(newSize, Allocator.Temp);
            _uvs2 = new NativeArray<Vector2>(newSize, Allocator.Temp);

            for (var i = 0; i < tempVertices.Length; i++) {
                _vertices[i] = tempVertices[i];
            }

            for (var i = 0; i < tempUvs.Length; i++) {
                _uvs[i] = tempUvs[i];
                _uvs2[i] = tempUvs2[i];
            }

            tempVertices.Dispose();
            tempUvs.Dispose();
            tempUvs2.Dispose();
        }

        public Mesh GenerateMesh() {
            var allVertices = _vertices.ToArray();
            var allUvs = _uvs.ToArray();
            var allUvs2 = _uvs2.ToArray();
            var allTriangles = _triangles.ToArray();
            var allNormals = _normals.ToArray();

            var vertices = new Vector3[_verticesIdx];
            Array.Copy(allVertices, 0, vertices, 0, _verticesIdx);

            var uvs = new Vector2[_verticesIdx];
            Array.Copy(allUvs, 0, uvs, 0, _verticesIdx);

            var uvs2 = new Vector2[_verticesIdx];
            Array.Copy(allUvs2, 0, uvs2, 0, _verticesIdx);

            var triangles = new int[_trianglesIdx];
            Array.Copy(allTriangles, 0, triangles, 0, _trianglesIdx);

            var normals = new Vector3[_normalIdx];
            Array.Copy(allNormals, 0, normals, 0, _normalIdx);

            var m = new Mesh {
                vertices = vertices,
                uv = uvs,
                uv2 = uvs2,
                triangles = triangles,
                // normals = normals
            };

            m.RecalculateBounds();
            m.RecalculateNormals();
            return m;
        }

        public void Dispose() {
            _vertices.Dispose();
            _uvs.Dispose();
            _uvs2.Dispose();
            _triangles.Dispose();
            _normals.Dispose();
        }
    }
}