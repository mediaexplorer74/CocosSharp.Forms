#if USE_PHYSICS
/****************************************************************************
 Copyright (c) 2013 Chukong Technologies Inc. ported by Jose Medrano (@netonjm)
 
 http://www.cocos2d-x.org
 
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChipmunkSharp;


namespace CocosSharp
{

	public enum PhysicsType
	{
		UNKNOWN = 1,
		CIRCLE = 2,
		BOX = 3,
		POLYGEN = 4,
		EDGESEGMENT = 5,
		EDGEBOX = 6,
		EDGEPOLYGEN = 7,
		EDGECHAIN = 8
	};

	public struct CCPhysicsMaterial
	{

		public const float PHYSICSSHAPE_MATERIAL_DEFAULT_DENSITY = 0.100000001f;

		public static CCPhysicsMaterial PHYSICSSHAPE_MATERIAL_DEFAULT { get { return new CCPhysicsMaterial(PHYSICSSHAPE_MATERIAL_DEFAULT_DENSITY, 0.5f, 0.5f); } }

		public float density;          ///< The density of the object.
		public float restitution;      ///< The bounciness of the physics body.
		public float friction;         ///< The roughness of the surface of a shape.

		public CCPhysicsMaterial(float aDensity, float aRestitution, float aFriction)
		{
			density = aDensity;
			restitution = aRestitution;
			friction = aFriction;
		}

	}


	/**
	 * @brief A shape for body. You do not create PhysicsWorld objects directly, instead, you can view PhysicsBody to see how to create it.
	 */
	public class CCPhysicsShape
	{

		#region PROTECTED PARAMETERS

		public CCPhysicsBody _body;
		internal CCPhysicsShapeInfo _info;

		protected PhysicsType _type;
		internal float _area;
		internal float _mass;
		internal float _moment;

		internal float _scaleX;
		internal float _scaleY;
		internal float _newScaleX;
		internal float _newScaleY;
		internal bool _dirty;

		internal CCPhysicsMaterial _material;
		internal int _tag;

		internal int _categoryBitmask;
		internal int _collisionBitmask;
		internal int _contactTestBitmask;
		internal int _group;

		#endregion

		#region PRIVATE PARAMETERS

		private float radius;
		private CCPhysicsMaterial material;
		private cpVect offset;

		#endregion

		public CCPhysicsShape()
		{
			_body = null;
			_info = null;
			_type = PhysicsType.UNKNOWN;
			_area = 0;
			_mass = CCPhysicsBody.MASS_DEFAULT;
			_moment = CCPhysicsBody.MOMENT_DEFAULT;
			_tag = 0;
			_categoryBitmask = int.MaxValue;
			_collisionBitmask = int.MaxValue;
			_contactTestBitmask = 0;
			_group = 0;

			_info = new CCPhysicsShapeInfo(this);

			_scaleX = 1.0f;
			_scaleY = 1.0f;
			_newScaleX = 1.0f;
			_newScaleY = 1.0f;
			_dirty = false;

		}

		public virtual void Update(float delta)
		{
			if (_dirty)
			{
				_scaleX = _newScaleX;
				_scaleY = _newScaleY;
				_dirty = false;
			}
		}

		public CCPhysicsShape(float radius, CCPhysicsMaterial material, CCPoint offset) :this()
		{

			// TODO: Complete member initialization
			this.radius = radius;
			this.material = material;
            this.offset = PhysicsHelper.CCPointToCpVect(offset);

		}




		#region PUBLIC METHODS

		/** Get the body that this shape attaches */
        public CCPhysicsBody Body 
        { 
            get { return _body; } 
            set {
                // already added
                if (value != null && _body == value)
                {
                    return;
                }

                if (_body != null)
                {
                    _body.RemoveShape(this);
                }

                if (value == null)
                {
                    _info.Body = null;
                    _body = null;
                }
                else
                {
                    
                    _info.Body = value._info.Body;
                    _body = value;
                }
            }

        }
		/** Return the type of this shape */
        public PhysicsType PhysicsType { get { return _type; } }
		/** return the area of this shape */
        public float Area { get { return _area; } }
        /** get moment Set moment, it will change the body's moment this shape attaches*/
		public float Moment 
        { 
            get { return _moment; }

            set {
                if (value < 0)
                {
                    return;
                }

                if (_body != null)
                {
                    _body.AddMoment(-_moment);
                    _body.AddMoment(value);
                }
                ;

                _moment = value;
            }
		}

		public int Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

		/** get mass */
        /** Set mass, it will change the body's mass this shape attaches */
		public float Mass
        {

            get { return _mass; }

            set
            {
                if (value < 0)
                {
                    return;
                }

                if (_body != null)
                {
                    _body.AddMass(-_mass);
                    _body.AddMass(value);
                }
                ;

                _mass = value;
            }
        }

		public float Density 
        { 
            get { return _material.density; }

            set {
                if (value < 0)
                {
                    return;
                }

                _material.density = value;

                if (_material.density == cp.PHYSICS_INFINITY)
                {
                    Mass = cp.PHYSICS_INFINITY;
                }
                else if (_area > 0)
                {
                    //TODO: PhysicsHelper ?�
                    Mass = _material.density * _area;
                }
            }
		}
		public float Restitution
        { 
            get { return _material.restitution; }

            set
            {
                _material.restitution = value;

                foreach (cpShape shape in _info.GetShapes())
                {
                    shape.SetElasticity(value);
                }
            }
        }

		public float Friction
        { 
            get { return _material.friction; }
            set
            {
                _material.friction = value;

                foreach (cpShape shape in _info.GetShapes())
                {
                    shape.SetFriction(value);
                }
            }
        }

		public CCPhysicsMaterial Material
        { 
            get { return _material; }

            set
            {
                Density = value.density;
                Restitution = value.restitution;
                Friction = value.friction;
            }
        }

		public int Group
        { 
            get { return _group; }
            set { _group = value; }
        }

		/** Calculate the default moment value */
		public virtual float CalculateDefaultMoment() { return 0.0f; }
		/** Get offset */
        public virtual CCPoint Offset { get { return CCPoint.Zero; } }
		/** Get center of this shape */
        public virtual CCPoint Center { get { return Offset; } }

		/** Test point is in shape or not */
		public bool ContainsPoint(CCPoint point)
		{


			foreach (var shape in _info.GetShapes())
			{
				cpPointQueryInfo info = null;
                shape.PointQuery(PhysicsHelper.CCPointToCpVect(point), ref info);
				if (info != null)
				{
					return true;
				}
			}
			return false;
		}



		/** move the points to the center */
        public static void RecenterPoints(CCPoint[] points, int count, CCPoint center)
		{
            var cpPoints = PhysicsHelper.CCPointsTocpVects(points);
			cp.RecenterPoly(count, cpPoints);
            points = PhysicsHelper.cpVectsTpCCPoints(cpPoints);
			if (center != CCPoint.Zero)
			{
				for (int i = 0; i < points.Length; ++i)
				{
					points[i] += center;
				}
			}
		}
		/** get center of the polyon points */
        public static CCPoint GetPolygonCenter(CCPoint[] points, int count)
		{
            return PhysicsHelper.cpVectToCCPoint(cp.CentroidForPoly(count, PhysicsHelper.CCPointsTocpVects(points)));
		}

		/**
		 * A mask that defines which categories this physics body belongs to.
		 * Every physics body in a scene can be assigned to up to 32 different categories, each corresponding to a bit in the bit mask. You define the mask values used in your game. In conjunction with the collisionBitMask and contactTestBitMask properties, you define which physics bodies interact with each other and when your game is notified of these interactions.
		 * The default value is 0xFFFFFFFF (all bits set).
		 */
		public int CategoryBitmask 
        { 
            get { return _categoryBitmask; }
            set {_categoryBitmask = value; }

        }

		/**
		 * A mask that defines which categories of bodies cause intersection notifications with this physics body.
		 * When two bodies share the same space, each body�s category mask is tested against the other body�s contact mask by performing a logical AND operation. If either comparison results in a non-zero value, an PhysicsContact object is created and passed to the physics world�s delegate. For best performance, only set bits in the contacts mask for interactions you are interested in.
		 * The default value is 0x00000000 (all bits cleared).
		 */
        public int ContactTestBitmask
        { 
            get { return _contactTestBitmask; }
            set { _contactTestBitmask = value; }
        }

		/**
		 * A mask that defines which categories of physics bodies can collide with this physics body.
		 * When two physics bodies contact each other, a collision may occur. This body�s collision mask is compared to the other body�s category mask by performing a logical AND operation. If the result is a non-zero value, then this body is affected by the collision. Each body independently chooses whether it wants to be affected by the other body. For example, you might use this to avoid collision calculations that would make negligible changes to a body�s velocity.
		 * The default value is 0xFFFFFFFF (all bits set).
		 */
		public int CollisionBitmask
        { 
            get { return _collisionBitmask; }
            set { _collisionBitmask = value; }
        }

		public ulong CollisionType
		{
            set {
                foreach (cpShape shape in _info.GetShapes())
                {
                    shape.SetCollisionType(value);
                }
            }
		}

		public float Elasticity
		{
            set 
            {
                foreach (cpShape shape in _info.GetShapes())
                {
                    shape.SetElasticity(value);
                }
            }
		}

		public CCPoint SurfaceVelocity
		{
            set
            {
                var vel = PhysicsHelper.CCPointToCpVect(value);
                foreach (cpShape shape in _info.GetShapes())
                {
                    shape.SetSurfaceVelocity(vel);
                }
            }
		}



		public bool Sensor
		{
            set
            {
                foreach (cpShape shape in _info.GetShapes())
                {
                    shape.SetSensor(value);
                }
            }
		}



		#endregion

		#region PROTECTED METHODS



		/**
		 * @brief PhysicsShape is PhysicsBody's friend class, but all the subclasses isn't. so this method is use for subclasses to catch the bodyInfo from PhysicsBody.
		 */

		internal CCPhysicsBodyInfo BodyInfo
		{
			get
			{
				if (_body != null)
					return _body._info;
				return null;
			}
		}

		/** calculate the area of this shape */
        protected virtual float CalculateArea() { return 0.0f; }

		#endregion



		internal float Scale
		{
            set
            {
                ScaleX = value;
                ScaleY = value;
            }
		}

		internal void SetScale(float scaleX, float scaleY)
		{
			ScaleX = scaleX;
			ScaleY = scaleY;
		}

		internal float ScaleY
		{
            set
            {
                if (_scaleY == value)
                {
                    return;
                }

                _newScaleY = value;
                _dirty = true;
            }
		}

		internal float ScaleX
		{
            set
            {
                if (_scaleX == value)
                {
                    return;
                }

                _newScaleX = value;
                _dirty = true;
            }
		}






	}

	/** A circle shape */
	public class CCPhysicsShapeCircle : CCPhysicsShape
	{

		public CCPhysicsShapeCircle(float radius, CCPoint offset)
			: this(CCPhysicsMaterial.PHYSICSSHAPE_MATERIAL_DEFAULT, radius, offset)
		{

		}

		public CCPhysicsShapeCircle(CCPhysicsMaterial material, float radius, CCPoint offset)
		{

			_type = PhysicsType.CIRCLE;

            cpShape shape = new cpCircleShape(CCPhysicsShapeInfo.SharedBody, radius, PhysicsHelper.CCPointToCpVect(offset));

			_info.Add(shape);

			_area = CalculateArea();
			_mass = material.density == cp.Infinity ? cp.Infinity : material.density * _area;
			_moment = CalculateDefaultMoment();

			Material = material;
		}


		#region PUBLIC METHODS


		public static float CalculateArea(float radius)
		{
			return cp.AreaForCircle(0, radius);
		}

		public static float CalculateMoment(float mass, float radius, CCPoint offset)
		{
			return mass == cp.Infinity ? cp.Infinity
                    : (cp.MomentForCircle(mass, 0, radius, PhysicsHelper.CCPointToCpVect(offset)));
		}



		public override float CalculateDefaultMoment()
		{
			cpShape shape = _info.GetShapes().FirstOrDefault();

			return _mass == cp.Infinity ? cp.Infinity
			: cp.MomentForCircle(_mass, 0,
			(shape as cpCircleShape).GetRadius(),
			(shape as cpCircleShape).GetOffset()
			);

		}

		public override void Update(float delta)
		{

			if (_dirty)
			{
				float factor = cp.cpfabs(_newScaleX / _scaleX);

				cpCircleShape shape = (cpCircleShape)_info.GetShapes().FirstOrDefault();//->getShapes().front();
                cpVect v = PhysicsHelper.CCPointToCpVect(Offset);// cpCircleShapeGetOffset();
				v = cpVect.cpvmult(v, factor);
				shape.c = v;

				shape.SetRadius(shape.GetRadius() * factor);
			}


			base.Update(delta);
		}

		public float Radius
		{
            get { return (_info.GetShapes().FirstOrDefault() as cpCircleShape).GetRadius(); }
		}

		public override CCPoint Offset
		{
            get { return PhysicsHelper.cpVectToCCPoint((_info.GetShapes().FirstOrDefault() as cpCircleShape).GetOffset()); }
		}

		#endregion

		#region PROTECTED METHODS


        protected override float CalculateArea()
		{
            cpCircleShape circle = (cpCircleShape)_info.GetShapes().FirstOrDefault();
            return cp.AreaForCircle(0, circle.GetRadius());
		}

		#endregion

	}

	/** A box shape */
	public class CCPhysicsShapeBox : CCPhysicsShapePolygon
	{

		#region PROTECTED  PARAMETERS
		//protected cpVect _offset;
		#endregion

		#region PUBLIC METHODS

		public CCPhysicsShapeBox(CCSize size, CCPhysicsMaterial material, float radius)
		{

			cpVect wh = PhysicsHelper.size2cpv(size);

			_type = PhysicsType.BOX;

			cpVect[] vec = {
                             
							new cpVect( -wh.x/2.0f,-wh.y/2.0f),
							new cpVect(   -wh.x/2.0f, wh.y/2.0f),
							new cpVect(   wh.x/2.0f, wh.y/2.0f),
							new cpVect(   wh.x/2.0f, -wh.y/2.0f)

                          };

			cpShape shape = new cpPolyShape(CCPhysicsShapeInfo.SharedBody, 4, vec, radius);

			_info.Add(shape);

			//_offset = offset;
			_area = CalculateArea();
			_mass = material.density == cp.Infinity ? cp.Infinity : material.density * _area;
			_moment = CalculateDefaultMoment();

			Material = material;

		}



		public CCSize GetSize()
		{
			cpPolyShape shape = (_info.GetShapes().FirstOrDefault() as cpPolyShape);//->getShapes().front();
			return PhysicsHelper.cpv2size(
				new cpVect(
					cpVect.cpvdist(shape.GetVert(1), shape.GetVert(2)),
					cpVect.cpvdist(shape.GetVert(0), shape.GetVert(1))));
		}

		#endregion

	}

	/** A polygon shape */
	public class CCPhysicsShapePolygon : CCPhysicsShape
	{

		public CCPhysicsShapePolygon()
		{

		}

		public void Init(CCPoint[] vecs, int count, CCPhysicsMaterial material, float radius)
		{
			_type = PhysicsType.POLYGEN;

            cpShape shape = new cpPolyShape(CCPhysicsShapeInfo.SharedBody, count, PhysicsHelper.CCPointsTocpVects(vecs), radius);


			_info.Add(shape);

			_area = CalculateArea();
			_mass = material.density == cp.Infinity ? cp.Infinity : material.density * _area;
			_moment = CalculateDefaultMoment();

			Material = material;

		}

		public CCPhysicsShapePolygon(CCPoint[] vecs, int count, CCPhysicsMaterial material, float radius)
		{
			Init(vecs, count, material, radius);

		}

		#region PUBLIC METHODS

		public static float CalculateMoment(float mass, CCPoint[] vecs, int count, CCPoint offset)
		{
			float moment = mass == cp.Infinity ? cp.Infinity
                : cp.MomentForPoly(mass, count, PhysicsHelper.CCPointsTocpVects(vecs), PhysicsHelper.CCPointToCpVect(offset), 0.0f);

			return moment;
		}

		public override float CalculateDefaultMoment()
		{
			cpPolyShape shape = (cpPolyShape)_info.GetShapes().FirstOrDefault();
			return _mass == cp.Infinity ? cp.Infinity
			: cp.MomentForPoly(_mass, shape.Count, shape.GetVertices(), cpVect.Zero, 0.0f);
		}

		public CCPoint GetPoint(int i)
		{
            return PhysicsHelper.cpVectToCCPoint(((cpPolyShape)_info.GetShapes().FirstOrDefault()).GetVert(i));
		}

		public void GetPoints(out CCPoint[] outPoints) //cpVect outPoints
		{
			cpShape shape = _info.GetShapes().FirstOrDefault();
            outPoints = PhysicsHelper.cpVectsTpCCPoints(((cpPolyShape)shape).GetVertices());
		}

		public int PointsCount
		{
            get
            {
                return ((cpPolyShape)_info.GetShapes().FirstOrDefault()).Count;
            }
		}

		#endregion

		#region PROTECTED METHODS

        protected override float CalculateArea()
		{
            cpPolyShape shape = (cpPolyShape)_info.GetShapes().FirstOrDefault(); //.front();
            shape.CacheBB();
            return cp.AreaForPolyOld(shape.Count, shape.GetVertices());
		}

		#endregion

	}

	/** A segment shape */
	public class CCPhysicsShapeEdgeSegment : CCPhysicsShape
	{

		public CCPhysicsShapeEdgeSegment(CCPoint a, CCPoint b, float border = 1)
			: this(a, b, CCPhysicsMaterial.PHYSICSSHAPE_MATERIAL_DEFAULT, border)
		{

		}

		public CCPhysicsShapeEdgeSegment(CCPoint a, CCPoint b, CCPhysicsMaterial material, float border = 1)
		{

			cpShape shape = new cpSegmentShape(CCPhysicsShapeInfo.SharedBody,
                PhysicsHelper.CCPointToCpVect(a),
                PhysicsHelper.CCPointToCpVect(b),
										   border);

			_type = PhysicsType.EDGESEGMENT;

            _info.Add(shape);

			_mass = cp.Infinity;
			_moment = cp.Infinity;

			Material = material;
		}

		#region PUBLIC METHODS

		public CCPoint PointA
		{
            get
            {
                return PhysicsHelper.cpVectToCCPoint(((cpSegmentShape)(_info.GetShapes().FirstOrDefault())).ta);
            }
		}
		public CCPoint PointB
		{
            get
            {
                return PhysicsHelper.cpVectToCCPoint(((cpSegmentShape)(_info.GetShapes().FirstOrDefault())).tb);
            }
		}
		#endregion


	}

	/* An edge box shape */
	public class CCPhysicsShapeEdgeBox : CCPhysicsShape
	{

        public CCPhysicsShapeEdgeBox(CCSize size, CCPhysicsMaterial material, CCPoint offset, float border = 1)
		{

			_type = PhysicsType.EDGEBOX;

			List<cpVect> vec = new List<cpVect>() {
                new cpVect(-size.Width/2+offset.X, -size.Height/2+offset.Y),
                new cpVect(+size.Width/2+offset.X, -size.Height/2+offset.Y),
                new cpVect(+size.Width/2+offset.X, +size.Height/2+offset.Y),
                new cpVect(-size.Width/2+offset.X, +size.Height/2+offset.Y)
          };


			int i = 0;
			for (; i < 4; ++i)
			{
				cpShape shape = new cpSegmentShape(CCPhysicsShapeInfo.SharedBody, vec[i], vec[(i + 1) % 4],
												   border);
				_info.Add(shape);
			}

			_offset = offset;
			_mass = CCPhysicsBody.MASS_DEFAULT;
			_moment = CCPhysicsBody.MOMENT_DEFAULT;

			Material = material;
		}

		#region PROTECTED PROPERTIES
		private CCPoint _offset;
		#endregion

		#region PUBLIC PROPERTIES

        public override CCPoint Offset
        { 
            get{ return _offset; }
        }

		public List<CCPoint> Points
		{
            get
            {
                List<CCPoint> outPoints = new List<CCPoint>();
                // int i = 0;
                foreach (var shape in _info.GetShapes())
                {
                    outPoints.Add(PhysicsHelper.cpVectToCCPoint(((cpSegmentShape)shape).a));
                }
                return outPoints;
            }
		}
		public int PointsCount
        { 
            get { return 4; }
        }

		#endregion

	}

	/** An edge polygon shape */
	public class CCPhysicsShapeEdgePolygon : CCPhysicsShape
	{


		public override void Update(float delta)
		{

			if (_dirty)
			{
				float factorX = _newScaleX / _scaleX;
				float factorY = _newScaleY / _scaleY;


				foreach (cpSegmentShape shape in _info.GetShapes())
				{
					cpVect a = shape.GetA();
					a.x *= factorX;
					a.y *= factorY;
					cpVect b = shape.GetB();
					b.x *= factorX;
					b.y *= factorY;
					shape.SetEndpoints(a, b);
				}
			}

			base.Update(delta);
		}


        public CCPhysicsShapeEdgePolygon(CCPoint[] vec, int count, CCPhysicsMaterial material, float border = 1)
		{

			_type = PhysicsType.EDGEPOLYGEN;

			int i = 0;
            var vecs = PhysicsHelper.CCPointsTocpVects(vec);
			for (; i < count; ++i)
			{
				cpShape shape = new cpSegmentShape(CCPhysicsShapeInfo.SharedBody, vecs[i], vecs[(i + 1) % count],
												   border);

				if (shape == null)
					break;

				shape.SetElasticity(1.0f);
				shape.SetFriction(1.0f);

				_info.Add(shape);
			}

			_mass = cp.Infinity;
			_moment = cp.Infinity;

			Material = material;

		}


		#region PUBLIC PROPERTIES

		public override CCPoint Center
		{
            get
            {
                var shapes = _info.GetShapes();
                int count = (int)shapes.Count;
                cpVect[] points = new cpVect[count];
                int i = 0;
                foreach (var shape in shapes)
                {
                    points[i++] = ((cpSegmentShape)shape).a;
                }

                cpVect center = cp.CentroidForPoly(count, points);

                return PhysicsHelper.cpVectToCCPoint(center);
            }
		}



		public CCPoint[] Points
		{
            get
            {
                var shapes = _info.GetShapes();

                cpVect[] outPoints = new cpVect[shapes.Count];
                for (int i = 0; i < shapes.Count; i++)
                {
                    outPoints[i] = new cpVect(((cpSegmentShape)shapes[i]).a);
                }

                return PhysicsHelper.cpVectsTpCCPoints(outPoints);
            }

		}
		public int PointsCount
		{
            get
            {
                return (_info.GetShapes().Count);
            }
		}

		#endregion

	}

	/** An edge polygon shape */
	public class CCPhysicsShapeEdgeChain : CCPhysicsShape
	{

		public CCPhysicsShapeEdgeChain(CCPoint[] vec, int count, CCPhysicsMaterial material, float border = 1)
		{

			_type = PhysicsType.EDGECHAIN;
            var vecs = PhysicsHelper.CCPointsTocpVects(vec);
			int i = 0;
			for (; i < count; ++i)
			{
				cpShape shape = new cpSegmentShape(CCPhysicsShapeInfo.SharedBody, vecs[i], vecs[i + 1],
											  border);
				shape.SetElasticity(1.0f);
				shape.SetFriction(1.0f);

				_info.Add(shape);
			}

			_mass = cp.Infinity;
			_moment = cp.Infinity;

			Material = material;
		}

		#region PUBLIC PROPERTIES

		public List<CCPoint> Points
		{
            get
            {
                List<CCPoint> outPoints = new List<CCPoint>();

                foreach (var shape in _info.GetShapes())
                    outPoints.Add(PhysicsHelper.cpVectToCCPoint(((cpSegmentShape)shape).a));

                outPoints.Add(PhysicsHelper.cpVectToCCPoint(((cpSegmentShape)_info.GetShapes().LastOrDefault()).a));

                return outPoints;
            }
		}

		public int PointsCount
		{
            get
            {
                return (_info.GetShapes().Count + 1);
            }
		}

		#endregion

	}

}
#endif