﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperTiled2Unity
{
    [Serializable]
    public class CollisionObject
    {
        public int m_ObjectId;

        public string m_ObjectName;

        public string m_ObjectType;

        public Vector2 m_Position;

        public Vector2 m_Size;

        public float m_Rotation;

        public List<CustomProperty> m_CustomProperties;

        public string m_PhysicsLayer;

        public bool m_IsTrigger;

        [SerializeField]
        private Vector2[] m_Points;
        public Vector2[] Points { get { return m_Points; } }

        [SerializeField]
        private bool m_IsClosed;
        public bool IsClosed { get { return m_IsClosed; } }

        [SerializeField]
        private CollisionShapeType m_CollisionShapeType;
        public CollisionShapeType CollisionShapeType { get { return m_CollisionShapeType; } }

        public void MakePointsFromRectangle()
        {
            m_CollisionShapeType = CollisionShapeType.Rectangle;
            m_IsClosed = true;

            // Make the points the give us a rectangle shape
            // Note: points are counter-clockwise
            m_Points = new Vector2[4];
            m_Points[0] = Vector2.zero;
            m_Points[1] = new Vector2(0, m_Size.y);
            m_Points[2] = new Vector2(m_Size.x, m_Size.y);
            m_Points[3] = new Vector2(m_Size.x, 0);
        }

        public void MakePointsFromEllipse(int numEdges)
        {
            m_CollisionShapeType = CollisionShapeType.Ellipse;
            m_IsClosed = true;

            // Estimate the ellipse with a polygon
            float theta = ((float)Math.PI * 2.0f) / numEdges;
            float half_x = m_Size.x * 0.5f;
            float half_y = m_Size.y * 0.5f;

            m_Points = new Vector2[numEdges];
            for (int i = 0; i < numEdges; i++)
            {
                m_Points[i].x = half_x + half_x * Mathf.Cos(theta * i);
                m_Points[i].y = half_y + half_y * Mathf.Sin(theta * i);
            }
        }

        public void MakePointsFromPolygon(Vector2[] points)
        {
            m_CollisionShapeType = CollisionShapeType.Polygon;
            m_IsClosed = true;
            m_Points = points;
        }

        public void MakePointsFromPolyline(Vector2[] points)
        {
            m_CollisionShapeType = CollisionShapeType.Polyline;
            m_IsClosed = false;
            m_Points = points;
        }

        // This must be called in order for rotation and position offset to by applied
        public void RenderPoints(SuperTile tile, GridOrientation orientation, Vector2 gridSize)
        {
            if (orientation == GridOrientation.Isometric) // fixit - this is busted now? (Or always has been?)
            {
                m_Position = IsometricTransform(m_Position, tile, gridSize);
                m_Position.x += gridSize.x * 0.5f;
                m_Position.y += tile.m_Height - gridSize.y; // fixit - do we have to do this? Can't we put all positions into space of bottom-left corner?

                for (int i = 0; i < m_Points.Length; i++)
                {
                    m_Points[i] = IsometricTransform(m_Points[i], tile, gridSize);
                }

                // Also, we are forced to use polygon colliders for isometric projection
                if (m_CollisionShapeType == CollisionShapeType.Ellipse || m_CollisionShapeType == CollisionShapeType.Rectangle)
                {
                    m_CollisionShapeType = CollisionShapeType.Polygon;
                }
            }

            ApplyRotationToPoints();
            m_Points = m_Points.Select(p => p + m_Position).ToArray();
        }

        private Vector2 IsometricTransform(Vector2 pt, SuperTile tile,Vector2 gridSize)
        {
            float cx = pt.x / tile.m_Height;
            float cy = pt.y / tile.m_Height;

            float x = (cx - cy) * gridSize.x * 0.5f;
            float y = (cx + cy) * gridSize.y * 0.5f;

            return new Vector2(x, y);
        }

        private void ApplyRotationToPoints()
        {
            if (m_Rotation != 0)
            {
                var rads = m_Rotation * Mathf.Deg2Rad;
                var cos = Mathf.Cos(rads);
                var sin = Mathf.Sin(rads);

                var rotate = MatrixUtils.Rotate2d(cos, -sin, sin, cos);

                m_Points = m_Points.Select(p => rotate.MultiplyPoint(p)).Select(v3 => (Vector2)v3).ToArray();
            }
        }
    }
}
