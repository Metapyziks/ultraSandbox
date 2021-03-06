﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Collections;
using Jitter.Dynamics;
using OpenTkProject.Drawables.Models;
using System.Xml;
using OpenTkProject.Drawables;

namespace OpenTkProject
{
    public abstract class GameObject
    {
        protected Scene scene;
        public OpenTkProjectWindow gameWindow;

        public bool wasUpdated = true;

        public string name = "";

        //public bool savable = false;

        private GameObject parent;
        protected int childId;

        protected List<GameObject> childs = new List<GameObject> { };
        protected Hashtable childNames = new Hashtable();

        protected Vector3 position;

        protected Vector3 pointingDirection;

        protected bool forceupdate = false; //selected = false;

        protected Vector3 size = new Vector3(1, 1, 1);

        protected Vector4 color = new Vector4(1, 1, 1, 1);
        protected Vector3 colorRgb = new Vector3(1, 1, 1);

        protected GameObject() { }

        public virtual RigidBody Body { get; set; }

        public virtual Scene Scene { get { return scene; } set { scene = value; } }

        public bool Forceupdate { get { return forceupdate; } set { forceupdate = value; } }

        public virtual Vector3 PointingDirection { get { return pointingDirection; } set { pointingDirection = value; } }

        public virtual Vector3 Position { get { return position; } set { position = value; } }

        public virtual Vector4 Color { get { return color; } set { color = value; colorRgb = value.Xyz; } }

        public virtual Vector3 Size { get { return size; } set { size = value; } }

        #region constructor

        protected GameObject(GameObject parent)
        {
            Parent = parent;
        }

        protected GameObject(OpenTkProjectWindow gameWindow)
        {
            this.gameWindow = gameWindow;
        }

        #endregion constructor

        #region child management

        public virtual int AddChild(GameObject newChild)
        {
            childs.Add(newChild);
            return childs.Count - 1;
        }

        public virtual void removeChild(GameObject reChild)
        {
            childs.Remove(reChild);
        }

        #endregion childmanagement

        #region parent management

        public virtual GameObject Parent { 
            get { return parent; } 
            set {

                if (parent != null)
                {
                    parent.removeChild(this);
                }

                if (value != null)
                {
                    gameWindow = value.gameWindow;

                    if (value.Scene != null)
                    {
                        Scene = value.Scene;
                    }

                    parent = value;

                    childId = parent.AddChild(this);

                    if (name != "")
                        parent.childNames.Add(name, childId);
                }
            }
        }

        #endregion parent management

        public virtual void save(ref StringBuilder sb, int level)
        {
            saveChilds(ref sb, level);
        }

        public void saveChilds(ref StringBuilder sb, int level)
        {
            level++;
            foreach (var child in childs)
            {
                child.save(ref sb, level);
            }
        }

        public virtual void forceUpdate()
        {
            updateChilds();
        }

        public virtual void kill()
        {
            Parent = null;

            parent.childNames.Remove(this);

            killChilds();
        }

        protected void killChilds()
        {
            while(childs.Count > 0)
            {
                childs[0].kill();
            }
        }

        public virtual void update()
        {
            updateChilds();
        }

        protected void updateChilds()
        {
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].update();
            }
        }

        internal void renameChild(GameObject curChild, string newName)
        {
            childNames.Remove(curChild.name);
            childNames.Add(newName, curChild.childId);
        }

        internal void resetUpdateState()
        {
            wasUpdated = false;
            resetUpdateStateChilds();
        }

        private void resetUpdateStateChilds()
        {
            foreach (var child in childs)
            {
                child.resetUpdateState();
            }
        }

        public virtual Matrix4 Orientation { get; set; }

        protected virtual void load(ref XmlTextReader reader, string type)
        {
            while (reader.Read() && reader.Name != type)
            {
                genericLoad(ref reader, type);

                specialLoad(ref reader, type);
            }
        }

        protected virtual void specialLoad(ref XmlTextReader reader, string type)
        {
        }

        protected void genericLoad(ref XmlTextReader reader, string type)
        {
            if (reader.Name == "pmodel")
            {
                string childname = scene.getUniqueName();

                while (reader.MoveToNextAttribute())
                {
                    if (reader.Name == "name")
                        childname = reader.Value;
                }

                GameObject child = new PhysModel(this);
                child.name = childname;
                child.load(ref reader, "pmodel");
            }

            if (reader.Name == "metamodel")
            {
                string childname = scene.getUniqueName();

                while (reader.MoveToNextAttribute())
                {
                    if (reader.Name == "name")
                        childname = reader.Value;
                }

                GameObject child = new MetaModel(this);
                child.name = childname;
                child.load(ref reader, "metamodel");
            }

            if (reader.Name == "lamp")
            {
                string childname = scene.getUniqueName();

                while (reader.MoveToNextAttribute())
                {
                    if (reader.Name == "name")
                        childname = reader.Value;
                }

                GameObject child = new LightSpot(this);
                child.name = childname;
                child.load(ref reader, "lamp");
            }

            if (reader.Name == "position" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                Position = GenericMethods.VectorFromString(reader.Value);
            }

            if (reader.Name == "direction" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                PointingDirection = GenericMethods.VectorFromString(reader.Value);
            }

            if (reader.Name == "color" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                Color = new Vector4(GenericMethods.VectorFromString(reader.Value),1);
            }

            if (reader.Name == "rotation" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                Orientation = GenericMethods.ToOpenTKMatrix(GenericMethods.JMatrixFromString(reader.Value));
            }
        }
    }
}
