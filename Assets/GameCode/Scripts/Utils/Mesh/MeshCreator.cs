using System;
using System.Collections.Generic;
using GameCode.Scripts.Utils.World;
using GameCode.Utils.Mesh;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace GameCode.Scripts.Utils.Mesh
{
    public class MeshCreator
    {
        /////////////////////////////////////////////////////////////////////////
        //
        // PORTIONS OF THIS CODE:
        //
        // The MIT License (MIT)
        //
        // Copyright (c) 2012-2013 Mikola Lysenko
        //
        // Permission is hereby granted, free of charge, to any person obtaining a copy
        // of this software and associated documentation files (the "Software"), to deal
        // in the Software without restriction, including without limitation the rights
        // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        // copies of the Software, and to permit persons to whom the Software is
        // furnished to do so, subject to the following conditions:
        //
        // The above copyright notice and this permission notice shall be included in
        // all copies or substantial portions of the Software.
        //
        // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        // THE SOFTWARE.
        public static MeshData ReduceMesh(Chunk chunk)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color32>();

            const int size = Chunk.SIZE;

            for (var d = 0; d < 3; d++)
            {
                int i, j, k, l, w, h, u = (d + 1) % 3, v = (d + 2) % 3;

                var x = new int[3];
                var q = new int[3];

                var mask = new short[size * size * size];

                q[d] = 1;

                for (x[d] = -1; x[d] < size;)
                {
                    var n = 0;
                    for (x[v] = 0; x[v] < size; ++x[v])
                    {
                        for (x[u] = 0; x[u] < size; ++x[u], ++n)
                        {
                            var a = 0 <= x[d] ? chunk.GetBlock(x[0], x[1], x[2]) : 0;
                            var b = x[d] < size - 1 ? chunk.GetBlock(x[0] + q[0], x[1] + q[1], x[2] + q[2]) : 0;

                            if (a != -1 && b != -1 && a == b)
                                mask[n] = 0;
                            else if (a > 0)
                                mask[n] = (short) a;
                            else
                                mask[n] = (short) -b;
                        }
                    }

                    ++x[d];

                    n = 0;
                    for (j = 0; j < size; ++j)
                    {
                        for (i = 0; i < size;)
                        {
                            var c = mask[n];

                            if (c != 0)
                            {
                                for (w = 1; c == mask[n + w] && i + w < size; ++w)
                                {
                                }

                                var done = false;
                                for (h = 1; j + h < size; ++h)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        if (c == mask[n + k + h * size]) continue;
                                        done = true;
                                        break;
                                    }

                                    if (done) break;
                                }

                                var flip = false;

                                x[u] = i;
                                x[v] = j;
                                var du = new int[3];
                                var dv = new int[3];

                                if (c > -1)
                                {
                                    du[u] = w;
                                    dv[v] = h;
                                }
                                else
                                {
                                    flip = true;
                                    c = (short) -c;
                                    du[u] = w;
                                    dv[v] = h;
                                }


                                var v1 = new Vector3(x[0], x[1], x[2]);
                                var v2 = new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]);
                                var v3 = new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1],
                                    x[2] + du[2] + dv[2]);
                                var v4 = new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]);

                                var floor = v1.y == 0 && v2.y == 0 && v3.y == 0 && v4.y == v1.y;

                                if (c >= 1 && !floor)
                                    AddFace(chunk, v1, v2, v3, v4, c, vertices, triangles, colors);

                                if (flip)
                                {
                                    AddFace(chunk, v4, v3, v2, v1, c, vertices, triangles, colors);
                                }

                                for (l = 0; l < h; ++l)
                                {
                                    for (k = 0; k < w; ++k)
                                        mask[n + k + l * size] = 0;
                                }

                                i += w;
                                n += w;
                            }
                            else
                            {
                                ++i;
                                ++n;
                            }
                        }
                    }
                }
            }

            return new MeshData(vertices.ToArray(), colors.ToArray(), triangles.ToArray());
        }


        private static void AddFace(Chunk chunk, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, short colorType,
            ICollection<Vector3> vertices, [NotNull] List<int> elements, List<Color32> colors)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));
            var index = vertices.Count;

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);

            colors.Add(chunk.GetBlockColor(colorType));
            colors.Add(chunk.GetBlockColor(colorType));
            colors.Add(chunk.GetBlockColor(colorType));
            colors.Add(chunk.GetBlockColor(colorType));

            elements.Add(index);
            elements.Add(index + 1);
            elements.Add(index + 2);
            elements.Add(index + 2);
            elements.Add(index + 3);
            elements.Add(index);
        }
    }
}