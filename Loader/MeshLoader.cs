﻿using System;
using System.Diagnostics;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace OpenTkProject
{
    public struct Mesh
    {
        public List<Vector3> positionVboDataList;
        public List<Vector3> normalVboDataList;
        public List<Vector2> textureVboDataList;
        public List<Face> FaceList;
        public List<Vertice> FpIndiceList;



        public int identifier;
        public const int TYPE_OBJ = 1;
        public const int TYPE_VOX = 2;

        public int type;
        public string pointer;

        public string name;

        public bool loaded;

        public Vector3[] positionVboData;
        public Vector3[] normalVboData;
        public Vector3[] tangentVboData;
        public Vector2[] textureVboData;
        public int[] indicesVboData;
        public int normalVboHandle;
        public int positionVboHandle;
        public int textureVboHandle;
        public int tangentVboHandle;
        public int eboHandle;
        public bool containsVbo;

        public float boundingSphere;

        /*
        public Mesh(Vector3[] positionVboData, Vector3[] normalVboData, Vector3[] tangentVboData, Vector2[] textureVboData, int[] indicesVboData)
        {
            this.positionVboData = positionVboData;
            this.normalVboData = normalVboData;
            this.tangentVboData = tangentVboData;
            this.textureVboData = textureVboData;
            this.indicesVboData = indicesVboData;
        }

        public Mesh(int type, string pointer)
        {
            this.type = type;
            this.pointer = pointer;
        }
         * */
    }

    public class MeshLoader : GameObject
    {
        public List<Mesh> Meshes = new List<Mesh> { };

        public List<Mesh> scheduledVboGenerations = new List<Mesh> { };

        public Hashtable MeshesNames = new Hashtable();

        System.Globalization.NumberFormatInfo nfi;

        public Mesh fromObj(string pointer)
        {
            nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberGroupSeparator = ",";
            nfi.NumberDecimalSeparator = ".";

            string name = pointer.Replace(gameWindow.modelFolder, "");

            if (!MeshesNames.ContainsKey(name))
            {
 
                Mesh curMesh = new Mesh();

                curMesh.identifier = Mesh.TYPE_OBJ;
                curMesh.pointer = pointer;
                curMesh.identifier = Meshes.Count;
                curMesh.name = name;

                MeshesNames.Add(curMesh.name, curMesh.identifier);

                Meshes.Add(curMesh);

                return curMesh;
            }
            else
            {
                return getMesh(name);
            }

        }

        public Mesh getMesh(string name)
        {
            int id = (int)MeshesNames[name];
            return Meshes[id];
        }

        public void loadMeshes()
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (!Meshes[i].loaded)
                {
                    loadObj(Meshes[i]);
                }
            }
        }

        public float loadSingleMeshes()
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                if (!Meshes[i].loaded)
                {
                    Mesh curMesh = Meshes[i];
                    loadObj(curMesh);
                    return (float)i / (float)Meshes.Count;
                }
            }
            return 1;
        }

        public void generateVBO(ref Mesh target)
        {
            if (!target.containsVbo)
            {
                GL.GenBuffers(1, out target.normalVboHandle);
                GL.GenBuffers(1, out target.positionVboHandle);
                GL.GenBuffers(1, out target.textureVboHandle);
                GL.GenBuffers(1, out target.tangentVboHandle);
                GL.GenBuffers(1, out target.eboHandle);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, target.normalVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(target.normalVboData.Length * Vector3.SizeInBytes),
                target.normalVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create NormlBuffer");


            GL.BindBuffer(BufferTarget.ArrayBuffer, target.positionVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(target.positionVboData.Length * Vector3.SizeInBytes),
                target.positionVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create positionBuffer");

            GL.BindBuffer(BufferTarget.ArrayBuffer, target.textureVboHandle);
            GL.BufferData<Vector2>(BufferTarget.ArrayBuffer,
                new IntPtr(target.textureVboData.Length * Vector2.SizeInBytes),
                target.textureVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create uvBuffer");

            GL.BindBuffer(BufferTarget.ArrayBuffer, target.tangentVboHandle);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(target.tangentVboData.Length * Vector3.SizeInBytes),
                target.tangentVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create tangentBuffer");

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, target.eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                new IntPtr(sizeof(uint) * target.indicesVboData.Length),
                target.indicesVboData, BufferUsageHint.StaticDraw);

            gameWindow.checkGlError("Create indice Buffer");

            // --- causes crash ---
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            target.containsVbo = true;
            if (target.identifier != -1)
            {
                Meshes[target.identifier] = target;
            }
        }

        public void loadObj(Mesh target)
        {
            List<Vector3> positionVboDataList = new List<Vector3> { };
            List<Vector3> normalVboDataList = new List<Vector3> { };
            List<Vector2> textureVboDataList = new List<Vector2> { };
            List<Face> FaceList = new List<Face> { };
            List<Vertice> FpIndiceList = new List<Vertice> { };

            // Read the file and display it line by line.
            string line;
            System.IO.StreamReader file =
               new System.IO.StreamReader(target.pointer);
            while ((line = file.ReadLine()) != null)
            {
                string[] sline = line.Split(new string[]{" "},10,StringSplitOptions.None);

                if (sline[0] == "v")
                {
                    float X = float.Parse(sline[1], nfi);
                    float Y = float.Parse(sline[2], nfi);
                    float Z = float.Parse(sline[3], nfi);
                    positionVboDataList.Add(new Vector3(X, Y, Z));

                }

                if (sline[0] == "vn")
                {
                    float X = float.Parse(sline[1], nfi);
                    float Y = float.Parse(sline[2], nfi);
                    float Z = float.Parse(sline[3], nfi);
                    normalVboDataList.Add(new Vector3(X, Y, Z));

                }

                if (sline[0] == "vt")
                {
                    float X = float.Parse(sline[1], nfi);
                    float Y = 1-float.Parse(sline[2], nfi);
                    textureVboDataList.Add(new Vector2(X, Y));

                }

                if (sline[0] == "f")
                {
                    string[] segment = sline[1].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                    if (segment.Length == 3)
                    {
                        Vertice fp1 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        segment = sline[2].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                        Vertice fp2 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        segment = sline[3].Split(new string[] { "/" }, 10, StringSplitOptions.None);
                        Vertice fp3 = new Vertice(int.Parse(segment[0]) - 1, int.Parse(segment[1]) - 1, int.Parse(segment[2]) - 1);

                        FaceList.Add(new Face(fp1, fp2, fp3));
                    }
                }
            }

            file.Close();

            target.positionVboDataList = positionVboDataList;
            target.normalVboDataList = normalVboDataList;
            target.textureVboDataList = textureVboDataList;
            target.FaceList = FaceList;

            parseFaceList(ref target, false);

            target.loaded = true;
            //Meshes[target.identifier] = target;

            generateVBO(ref target);
        }

        public void parseFaceList(ref Mesh target, bool genNormal)
        {
            List<Vector3> positionVboDataList = target.positionVboDataList;
            List<Vector3> normalVboDataList = target.normalVboDataList;
            List<Vector2> textureVboDataList = target.textureVboDataList;
            List<Face> FaceList = target.FaceList;

            removeTemp(ref FaceList);
            convertToTri(ref FaceList);

            Vector3[] positionVboData = new Vector3[FaceList.Count * 3];
            Vector3[] normalVboData = new Vector3[FaceList.Count * 3];
            Vector3[] tangentVboData = new Vector3[FaceList.Count * 3];
            Vector2[] textureVboData = new Vector2[FaceList.Count * 3];
            int[] indicesVboData = new int[FaceList.Count * 3];

            Vector3[] tmpnormalVboData = new Vector3[normalVboDataList.Count];
            Vector3[] tmptangentVboData = new Vector3[normalVboDataList.Count];
            Vector3[] normalPositionData = new Vector3[normalVboDataList.Count];
            Vector2[] normalUvData = new Vector2[normalVboDataList.Count];

            List<Vector3> normalHelperList = new List<Vector3> { };
            List<Vector3> tangentHelperList = new List<Vector3> { };
            List<Vector2> normalUvHelperList = new List<Vector2> { };
            List<Vector3> positionHelperlist = new List<Vector3> { };

            for (int i = 0; i < FaceList.Count; i++)
            {
                // get all the information from Lists into Facelist
                Vector3[] vposition = new Vector3[3];
                Vector3[] vnormal = new Vector3[3];
                Vector2[] vtexture = new Vector2[3];
                for (int j = 0; j < 3; j++)
                {
                    vposition[j] = positionVboDataList[FaceList[i].Vertice[j].Vi];
                    vnormal[j] = normalVboDataList[FaceList[i].Vertice[j].Ni];
                    vtexture[j] = textureVboDataList[FaceList[i].Vertice[j].Ti];

                    int id = i * 3 + j;

                    //FaceList[i].Vertice[j].position = vposition;
                    //FaceList[i].Vertice[j].normal = vnormal;
                    //FaceList[i].Vertice[j].texture = vtexture;
                }
                // calculating face normal and tangent
                Vector3 v1 = vposition[1] - vposition[0];
                Vector3 v2 = vposition[2] - vposition[0];

                Vector2 vtexture1 = vtexture[1] - vtexture[0];
                Vector2 vtexture2 = vtexture[2] - vtexture[0];

                Vector3 fnormal = Vector3.Cross(v1, v2);

                float s = 1f / (vtexture2.X - vtexture1.X * vtexture2.Y / vtexture1.Y);
                float r = 1f / (vtexture1.X - vtexture2.X * vtexture1.Y / vtexture2.Y);

                Vector3 tangent = Vector3.Normalize(r * v1 + s * v2);

                if(tangent == Vector3.Zero){
                    gameWindow.log("tangent generation ERROR");
                }

                // finding out if normal/tangent can be smoothed
                for (int j = 0; j < 3; j++)
                {
                    // if Normal[Normalindice] has not been assigned a uv coordinate do so and set normal
                    if (normalUvData[FaceList[i].Vertice[j].Ni] == Vector2.Zero)
                    {
                        normalUvData[FaceList[i].Vertice[j].Ni] = vtexture[j];
                        normalPositionData[FaceList[i].Vertice[j].Ni] = vposition[j];

                        tmpnormalVboData[FaceList[i].Vertice[j].Ni] = fnormal;
                        tmptangentVboData[FaceList[i].Vertice[j].Ni] = tangent;
                    }
                    else
                    {
                        // if Normal[Normalindice] is of the same Uv and place simply add
                        if (normalUvData[FaceList[i].Vertice[j].Ni] == vtexture[j] && normalPositionData[FaceList[i].Vertice[j].Ni] == vposition[j])
                        {
                            tmpnormalVboData[FaceList[i].Vertice[j].Ni] += fnormal;
                            tmptangentVboData[FaceList[i].Vertice[j].Ni] += tangent;
                        }
                        else
                        {
                            for (int k = 0; k < normalUvHelperList.Count; k++)
                            {
                                // if Normalhelper[Normalindice] is of the same Uv and position simply add
                                if (normalUvHelperList[k] == vtexture[j] && positionHelperlist[k] == vposition[j])
                                {
                                    tangentHelperList[k] += tangent;
                                    normalHelperList[k] += fnormal;

                                    FaceList[i].Vertice[j].Normalihelper = k;
                                }
                            }
                            // if matching Normalhelper has not been found create new one
                            if (FaceList[i].Vertice[j].Normalihelper == -1)
                            {
                                normalUvHelperList.Add(vtexture[j]);

                                tangentHelperList.Add(tangent);
                                normalHelperList.Add(fnormal);
                                positionHelperlist.Add(vposition[j]);
                                FaceList[i].Vertice[j].Normalihelper = normalUvHelperList.Count - 1;
                            }
                        }
                    }
                }
            }

            // put Faces into Arrays
            for (int i = 0; i < FaceList.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int id = i * 3 + j;
                    positionVboData[id] = positionVboDataList[FaceList[i].Vertice[j].Vi];
                    normalVboData[id] = normalVboDataList[FaceList[i].Vertice[j].Ni];
                    if (FaceList[i].Vertice[j].Normalihelper != -1)
                    {
                        if(genNormal)
                            normalVboData[id] = Vector3.Normalize(normalHelperList[FaceList[i].Vertice[j].Normalihelper]); //-dont use calculated normal
                        tangentVboData[id] = Vector3.Normalize(tangentHelperList[FaceList[i].Vertice[j].Normalihelper]);
                    }
                    else
                    {
                        if (genNormal) 
                            normalVboData[id] = Vector3.Normalize(tmpnormalVboData[FaceList[i].Vertice[j].Ni]); //-dont use calculated normal
                        tangentVboData[id] = Vector3.Normalize(tmptangentVboData[FaceList[i].Vertice[j].Ni]);
                    }
                    textureVboData[id] = textureVboDataList[FaceList[i].Vertice[j].Ti];
                    indicesVboData[id] = id;
                }
            }

            //calculate a bounding Sphere
            float sphere = 0;
            foreach (var vec in positionVboData)
            {
                float length = vec.Length;
                if (length > sphere)
                    sphere = length;
            }

            //returning mesh info ... DONE :D
            target.positionVboData = positionVboData;
            target.normalVboData = normalVboData;
            target.tangentVboData = tangentVboData;
            target.textureVboData = textureVboData;
            target.indicesVboData = indicesVboData;
            target.boundingSphere = sphere;
        }

        private void removeTemp(ref List<Face> FaceList)
        {
            int i = 0;
            while (i < FaceList.Count)
            {
                Face curFace = FaceList[i];
                if (curFace.isTemp)
                    FaceList.Remove(curFace);
                else
                    i++;
            }
        }

        private void convertToTri(ref List<Face> FaceList)
        {
            int faces = FaceList.Count;
            for (int i = 0; i < faces; i++)
            {
                Face curFace = FaceList[i];
                if (curFace.Vertice.Length > 3)
                {
                    FaceList[i] = new Face(curFace.Vertice[2], curFace.Vertice[1], curFace.Vertice[0]);
                    FaceList.Add(new Face(curFace.Vertice[1], curFace.Vertice[2], curFace.Vertice[3]));
                }
            }
        }

        public MeshLoader(OpenTkProjectWindow mGameWindow)
        {
            this.gameWindow = mGameWindow;
        }
    }

    public class Vertice
    {
        public int Vi;
        public int Ti;
        public int Ni;
        public int Normalihelper = -1;

        /*
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texture;
        */
        public Vertice(int chi, int ti, int ni)
        {
            Vi = chi;
            Ti = ti;
            Ni = ni;
            // Log.e("VboCube",Vi+"/"+Ti+"/"+Ni);
        }
    }

    public class Face
    {
        public Vertice[] Vertice;
        public bool isTemp;

        public Face(Vertice ind1, Vertice ind2, Vertice ind3)
        {
            Vertice = new Vertice[3];
            Vertice[0] = ind1;
            Vertice[1] = ind2;
            Vertice[2] = ind3;
            // Log.e("VboCube",Vi+"/"+Ti+"/"+Ni);
        }

        public Face(Vertice ind1, Vertice ind2, Vertice ind3, Vertice ind4)
        {
            Vertice = new Vertice[4];
            Vertice[0] = ind1;
            Vertice[1] = ind2;
            Vertice[2] = ind3;
            Vertice[3] = ind4;
            // Log.e("VboCube",Vi+"/"+Ti+"/"+Ni);
        }
    }
}
