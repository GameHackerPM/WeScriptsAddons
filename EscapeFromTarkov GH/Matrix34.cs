using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace EscapeFromTarkov
{
    public struct Matrix33
    {
        #region Fields

        public float M00;
        public float M01;
        public float M02;
        public float M10;
        public float M11;
        public float M12;
        public float M20;
        public float M21;
        public float M22;

        #endregion Fields

        #region Constructors

        public Matrix33(Matrix34 m)
        {
            M00 = m.M00;
            M01 = m.M01;
            M02 = m.M02;

            M10 = m.M10;
            M11 = m.M11;
            M12 = m.M12;

            M20 = m.M20;
            M21 = m.M21;
            M22 = m.M22;
        }

        #endregion Constructors

        #region Properties

        public Vector3 Angles
        {
            get
            {
                var angles = new Vector3();

                angles.Y = (float)Math.Asin(Math.Max(-1.0, Math.Min(1.0, -M20)));
                if (Math.Abs(Math.Abs(angles.Y) - (Math.PI * 0.5)) < 0.01)
                {
                    angles.X = 0;
                    angles.Z = (float)Math.Atan2(-M01, M11);
                }
                else
                {
                    angles.X = (float)Math.Atan2(M21, M22);
                    angles.Z = (float)Math.Atan2(M10, M00);
                }

                return angles;
            }
        }

        #endregion Properties

        #region Methods

        public static Matrix33 operator *(Matrix33 left, float op)
        {
            var m33 = left;
            m33.M00 *= op; m33.M01 *= op; m33.M02 *= op;
            m33.M10 *= op; m33.M11 *= op; m33.M12 *= op;
            m33.M20 *= op; m33.M21 *= op; m33.M22 *= op;
            return m33;
        }

        public static Matrix33 operator *(Matrix33 left, Matrix33 right)
        {
            var m = new Matrix33();
            m.M00 = left.M00 * right.M00 + left.M01 * right.M10 + left.M02 * right.M20;
            m.M01 = left.M00 * right.M01 + left.M01 * right.M11 + left.M02 * right.M21;
            m.M02 = left.M00 * right.M02 + left.M01 * right.M12 + left.M02 * right.M22;
            m.M10 = left.M10 * right.M00 + left.M11 * right.M10 + left.M12 * right.M20;
            m.M11 = left.M10 * right.M01 + left.M11 * right.M11 + left.M12 * right.M21;
            m.M12 = left.M10 * right.M02 + left.M11 * right.M12 + left.M12 * right.M22;
            m.M20 = left.M20 * right.M00 + left.M21 * right.M10 + left.M22 * right.M20;
            m.M21 = left.M20 * right.M01 + left.M21 * right.M11 + left.M22 * right.M21;
            m.M22 = left.M20 * right.M02 + left.M21 * right.M12 + left.M22 * right.M22;
            return m;
        }

        public static Vector3 operator *(Matrix33 left, Vector3 right)
        {
            return new Vector3(right.X * left.M00 + right.Y * left.M01 + right.Z * left.M02,
                right.X * left.M10 + right.Y * left.M11 + right.Z * left.M12,
                right.X * left.M20 + right.Y * left.M21 + right.Z * left.M22);
        }

        public static Matrix33 operator /(Matrix33 left, float op)
        {
            var m33 = left;
            var iop = 1.0f / op;
            m33.M00 *= iop; m33.M01 *= iop; m33.M02 *= iop;
            m33.M10 *= iop; m33.M11 *= iop; m33.M12 *= iop;
            m33.M20 *= iop; m33.M21 *= iop; m33.M22 *= iop;
            return m33;
        }

        public static Matrix33 CreateFromVectors(Vector3 vx, Vector3 vy, Vector3 vz)
        {
            var matrix = new Matrix33();
            matrix.SetFromVectors(vx, vy, vz);

            return matrix;
        }

        public static Matrix33 CreateIdentity()
        {
            var matrix = new Matrix33();
            matrix.SetIdentity();

            return matrix;
        }

        public static Matrix33 CreateRotationAA(float c, float s, Vector3 axis)
        {
            var matrix = new Matrix33();
            matrix.SetRotationAA(c, s, axis);

            return matrix;
        }

        public static Matrix33 CreateScale(Vector3 s)
        {
            var matrix = new Matrix33();
            matrix.SetScale(s);

            return matrix;
        }

        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                int hash = 17;

                hash = hash * 29 + M00.GetHashCode();
                hash = hash * 29 + M01.GetHashCode();
                hash = hash * 29 + M02.GetHashCode();

                hash = hash * 29 + M10.GetHashCode();
                hash = hash * 29 + M11.GetHashCode();
                hash = hash * 29 + M12.GetHashCode();

                hash = hash * 29 + M20.GetHashCode();
                hash = hash * 29 + M21.GetHashCode();
                hash = hash * 29 + M22.GetHashCode();

                return hash;
            }
        }

        public void SetFromVectors(Vector3 vx, Vector3 vy, Vector3 vz)
        {
            M00 = vx.X; M01 = vy.X; M02 = vz.X;
            M10 = vx.Y; M11 = vy.Y; M12 = vz.Y;
            M20 = vx.Z; M21 = vy.Z; M22 = vz.Z;
        }

        public void SetIdentity()
        {
            M00 = 1;
            M01 = 0;
            M02 = 0;

            M10 = 0;
            M11 = 1;
            M12 = 0;

            M20 = 0;
            M21 = 0;
            M22 = 1;
        }
        public void SetRotationAA(float c, float s, Vector3 axis)
        {
            float mc = 1 - c;
            M00 = mc * axis.X * axis.X + c; M01 = mc * axis.X * axis.Y - axis.Z * s; M02 = mc * axis.X * axis.Z + axis.Y * s;
            M10 = mc * axis.Y * axis.X + axis.Z * s; M11 = mc * axis.Y * axis.Y + c; M12 = mc * axis.Y * axis.Z - axis.X * s;
            M20 = mc * axis.Z * axis.X - axis.Y * s; M21 = mc * axis.Z * axis.Y + axis.X * s; M22 = mc * axis.Z * axis.Z + c;
        }

        public void SetScale(Vector3 s)
        {
            M00 = s.X; M01 = 0; M02 = 0;
            M10 = 0; M11 = s.Y; M12 = 0;
            M20 = 0; M21 = 0; M22 = s.Z;
        }

        #endregion Methods
    }

    public struct Matrix34
    {

        public Vector4f vec0;
        public Vector4f vec1;
        public Vector4f vec2;

        #region Constructors

        public Matrix34(float v00, float v01, float v02, float v03, float v10, float v11, float v12, float v13, float v20, float v21, float v22, float v23)
            : this()
        {
            M00 = v00; M01 = v01; M02 = v02; M03 = v03;
            M10 = v10; M11 = v11; M12 = v12; M13 = v13;
            M20 = v20; M21 = v21; M22 = v22; M23 = v23;
        }
        public Matrix34(Matrix33 m33)
            : this()
        {
        }

        #endregion Constructors

        #region Properties

        public Vector3 Angles
        {
            get
            {
                var angles = new Vector3();

                angles.Y = (float)Math.Asin(Math.Max(-1.0, Math.Min(1.0, -M20)));
                if (Math.Abs(Math.Abs(angles.Y) - (Math.PI * 0.5)) < 0.01)
                {
                    angles.X = 0;
                    angles.Z = (float)Math.Atan2(-M01, M11);
                }
                else
                {
                    angles.X = (float)Math.Atan2(M21, M22);
                    angles.Z = (float)Math.Atan2(M10, M00);
                }

                return angles;
            }
        }

        public Vector3 Column0
        {
            get { return new Vector3(M00, M10, M20); }
        }

        public Vector3 Column1
        {
            get { return new Vector3(M01, M11, M21); }
        }

        public Vector3 Column2
        {
            get { return new Vector3(M02, M12, M22); }
        }

        public Vector3 Column3
        {
            get { return new Vector3(M03, M13, M23); }
        }

        public float M00
        {
            get; set;
        }

        public float M01
        {
            get; set;
        }

        public float M02
        {
            get; set;
        }

        public float M03
        {
            get; set;
        }

        public float M10
        {
            get; set;
        }

        public float M11
        {
            get; set;
        }

        public float M12
        {
            get; set;
        }

        public float M13
        {
            get; set;
        }

        public float M20
        {
            get; set;
        }

        public float M21
        {
            get; set;
        }

        public float M22
        {
            get; set;
        }

        public float M23
        {
            get; set;
        }

        public Vector3 Translation
        {
            get { return new Vector3(M03, M13, M23); }
        }

        Matrix34 Inverted
        {
            get
            {
                var dst = this;

                dst.Invert();

                return dst;
            }
        }

        Matrix34 InvertedFast
        {
            get
            {
                var dst = new Matrix34();
                dst.M00 = M00; dst.M01 = M10; dst.M02 = M20; dst.M03 = -M03 * M00 - M13 * M10 - M23 * M20;
                dst.M10 = M01; dst.M11 = M11; dst.M12 = M21; dst.M13 = -M03 * M01 - M13 * M11 - M23 * M21;
                dst.M20 = M02; dst.M21 = M12; dst.M22 = M22; dst.M23 = -M03 * M02 - M13 * M12 - M23 * M22;
                return dst;
            }
        }

        #endregion Properties

        #region Methods

        public static explicit operator Matrix33(Matrix34 m)
        {
            return new Matrix33(m);
        }

        public static Matrix34 operator *(Matrix34 l, Matrix34 r)
        {
            var m = new Matrix34();
            m.M00 = l.M00 * r.M00 + l.M01 * r.M10 + l.M02 * r.M20;
            m.M10 = l.M10 * r.M00 + l.M11 * r.M10 + l.M12 * r.M20;
            m.M20 = l.M20 * r.M00 + l.M21 * r.M10 + l.M22 * r.M20;
            m.M01 = l.M00 * r.M01 + l.M01 * r.M11 + l.M02 * r.M21;
            m.M11 = l.M10 * r.M01 + l.M11 * r.M11 + l.M12 * r.M21;
            m.M21 = l.M20 * r.M01 + l.M21 * r.M11 + l.M22 * r.M21;
            m.M02 = l.M00 * r.M02 + l.M01 * r.M12 + l.M02 * r.M22;
            m.M12 = l.M10 * r.M02 + l.M11 * r.M12 + l.M12 * r.M22;
            m.M22 = l.M20 * r.M02 + l.M21 * r.M12 + l.M22 * r.M22;
            m.M03 = l.M00 * r.M03 + l.M01 * r.M13 + l.M02 * r.M23 + l.M03;
            m.M13 = l.M10 * r.M03 + l.M11 * r.M13 + l.M12 * r.M23 + l.M13;
            m.M23 = l.M20 * r.M03 + l.M21 * r.M13 + l.M22 * r.M23 + l.M23;

            return m;
        }
        public static Matrix34 CreateFromVectors(Vector3 vx, Vector3 vy, Vector3 vz, Vector3 pos)
        {
            var matrix = new Matrix34();
            matrix.SetFromVectors(vx, vy, vz, pos);

            return matrix;
        }

        public static Matrix34 CreateIdentity()
        {
            var matrix = new Matrix34();

            matrix.SetIdentity();

            return matrix;
        }
        public static Matrix34 CreateRotationAA(float c, float s, Vector3 axis, Vector3 t = default(Vector3))
        {
            var matrix = new Matrix34();
            matrix.SetRotationAA(c, s, axis, t);

            return matrix;
        }

        public static Matrix34 CreateScale(Vector3 s, Vector3 t = default(Vector3))
        {
            var matrix = new Matrix34();
            matrix.SetScale(s, t);

            return matrix;
        }
        public static Matrix34 CreateTranslationMat(Vector3 v)
        {
            var matrix = new Matrix34();
            matrix.SetTranslationMat(v);

            return matrix;
        }

        public Matrix34 AddTranslation(Vector3 t)
        {
            M03 += t.X;
            M13 += t.Y;
            M23 += t.Z;

            return this;
        }

        /// <summary>
        /// determinant is ambiguous: only the upper-left-submatrix's determinant is calculated
        /// </summary>
        /// <returns></returns>
        public float Determinant()
        {
            return (M00 * M11 * M22) + (M01 * M12 * M20) + (M02 * M10 * M21) - (M02 * M11 * M20) - (M00 * M12 * M21) - (M01 * M10 * M22);
        }

        public override int GetHashCode()
        {
            // Overflow is fine, just wrap
            unchecked
            {
                int hash = 17;

                hash = hash * 29 + M00.GetHashCode();
                hash = hash * 29 + M01.GetHashCode();
                hash = hash * 29 + M02.GetHashCode();
                hash = hash * 29 + M03.GetHashCode();

                hash = hash * 29 + M10.GetHashCode();
                hash = hash * 29 + M11.GetHashCode();
                hash = hash * 29 + M12.GetHashCode();
                hash = hash * 29 + M13.GetHashCode();

                hash = hash * 29 + M20.GetHashCode();
                hash = hash * 29 + M21.GetHashCode();
                hash = hash * 29 + M22.GetHashCode();
                hash = hash * 29 + M23.GetHashCode();

                return hash;
            }
        }

        public void Invert()
        {
            // rescue members
            var m = this;

            // calculate 12 cofactors
            M00 = m.M22 * m.M11 - m.M12 * m.M21;
            M10 = m.M12 * m.M20 - m.M22 * m.M10;
            M20 = m.M10 * m.M21 - m.M20 * m.M11;
            M01 = m.M02 * m.M21 - m.M22 * m.M01;
            M11 = m.M22 * m.M00 - m.M02 * m.M20;
            M21 = m.M20 * m.M01 - m.M00 * m.M21;
            M02 = m.M12 * m.M01 - m.M02 * m.M11;
            M12 = m.M02 * m.M10 - m.M12 * m.M00;
            M22 = m.M00 * m.M11 - m.M10 * m.M01;
            M03 = (m.M22 * m.M13 * m.M01 + m.M02 * m.M23 * m.M11 + m.M12 * m.M03 * m.M21) - (m.M12 * m.M23 * m.M01 + m.M22 * m.M03 * m.M11 + m.M02 * m.M13 * m.M21);
            M13 = (m.M12 * m.M23 * m.M00 + m.M22 * m.M03 * m.M10 + m.M02 * m.M13 * m.M20) - (m.M22 * m.M13 * m.M00 + m.M02 * m.M23 * m.M10 + m.M12 * m.M03 * m.M20);
            M23 = (m.M20 * m.M11 * m.M03 + m.M00 * m.M21 * m.M13 + m.M10 * m.M01 * m.M23) - (m.M10 * m.M21 * m.M03 + m.M20 * m.M01 * m.M13 + m.M00 * m.M11 * m.M23);

            // calculate determinant
            float det = 1.0f / (m.M00 * M00 + m.M10 * M01 + m.M20 * M02);

            // calculate matrix inverse/
            M00 *= det; M01 *= det; M02 *= det; M03 *= det;
            M10 *= det; M11 *= det; M12 *= det; M13 *= det;
            M20 *= det; M21 *= det; M22 *= det; M23 *= det;
        }

        public void InvertFast()
        {
            var v = new Vector3(M03, M13, M23);
            float t = M01; M01 = M10; M10 = t; M03 = -v.X * M00 - v.Y * M01 - v.Z * M20;
            t = M02; M02 = M20; M20 = t; M13 = -v.X * M10 - v.Y * M11 - v.Z * M21;
            t = M12; M12 = M21; M21 = t; M23 = -v.X * M20 - v.Y * M21 - v.Z * M22;
        }

        public bool IsEquivalent(Matrix34 m, float e = 0.05f)
        {
            return ((Math.Abs(M00 - m.M00) <= e) && (Math.Abs(M01 - m.M01) <= e) && (Math.Abs(M02 - m.M02) <= e) && (Math.Abs(M03 - m.M03) <= e) &&
            (Math.Abs(M10 - m.M10) <= e) && (Math.Abs(M11 - m.M11) <= e) && (Math.Abs(M12 - m.M12) <= e) && (Math.Abs(M13 - m.M13) <= e) &&
            (Math.Abs(M20 - m.M20) <= e) && (Math.Abs(M21 - m.M21) <= e) && (Math.Abs(M22 - m.M22) <= e) && (Math.Abs(M23 - m.M23) <= e));
        }


        public void ScaleTranslation(float s)
        {
            M03 *= s;
            M13 *= s;
            M23 *= s;
        }

        public void SetFromVectors(Vector3 vx, Vector3 vy, Vector3 vz, Vector3 pos)
        {
            M00 = vx.X; M01 = vy.X; M02 = vz.X; M03 = pos.X;
            M10 = vx.Y; M11 = vy.Y; M12 = vz.Y; M13 = pos.Y;
            M20 = vx.Z; M21 = vy.Z; M22 = vz.Z; M23 = pos.Z;
        }

        public void SetIdentity()
        {
            M00 = 1.0f; M01 = 0.0f; M02 = 0.0f; M03 = 0.0f;
            M10 = 0.0f; M11 = 1.0f; M12 = 0.0f; M13 = 0.0f;
            M20 = 0.0f; M21 = 0.0f; M22 = 1.0f; M23 = 0.0f;
        }

        public void SetRotation33(Matrix33 m33)
        {
            M00 = m33.M00; M01 = m33.M01; M02 = m33.M02;
            M10 = m33.M10; M11 = m33.M11; M12 = m33.M12;
            M20 = m33.M20; M21 = m33.M21; M22 = m33.M22;
        }

        /*!
        *  Create a rotation matrix around an arbitrary axis (Eulers Theorem).
        *  The axis is specified as an normalized Vector3. The angle is assumed to be in radians.
        *  This function also assumes a translation-vector and stores it in the right column.
        *
        *  Example:
        *        Matrix34 m34;
        *        Vector3 axis=GetNormalized( Vector3(-1.0f,-0.3f,0.0f) );
        *        m34.SetRotationAA( 3.14314f, axis, Vector3(5,5,5) );
        */

        public void SetRotationAA(float c, float s, Vector3 axis, Vector3 t = default(Vector3))
        {
            this = new Matrix34(Matrix33.CreateRotationAA(c, s, axis));
            M03 = t.X; M13 = t.Y; M23 = t.Z;
        }


        public void SetScale(Vector3 s, Vector3 t = default(Vector3))
        {
            this = new Matrix34(Matrix33.CreateScale(s));

            SetTranslation(t);
        }

        public void SetTranslation(Vector3 t)
        {
            M03 = t.X;
            M13 = t.Y;
            M23 = t.Z;
        }

        public void SetTranslationMat(Vector3 v)
        {
            M00 = 1.0f; M01 = 0.0f; M02 = 0.0f; M03 = v.X;
            M10 = 0.0f; M11 = 1.0f; M12 = 0.0f; M13 = v.Y;
            M20 = 0.0f; M21 = 0.0f; M22 = 1.0f; M23 = v.Z;
        }

        /// <summary>
        /// transforms a point and add translation vector
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 p)
        {
            return new Vector3(M00 * p.X + M01 * p.Y + M02 * p.Z + M03, M10 * p.X + M11 * p.Y + M12 * p.Z + M13, M20 * p.X + M21 * p.Y + M22 * p.Z + M23);
        }

        /// <summary>
        /// transforms a vector. the translation is not beeing considered
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector3 TransformVector(Vector3 p)
        {
            return new Vector3(M00 * p.X + M01 * p.Y + M02 * p.Z + M03, M10 * p.X + M11 * p.Y + M12 * p.Z + M13, M20 * p.X + M21 * p.Y + M22 * p.Z + M23);
        }

        void Scale(Vector3 s)
        {
            M00 *= s.X; M01 *= s.Y; M02 *= s.Z;
            M10 *= s.X; M11 *= s.Y; M12 *= s.Z;
            M20 *= s.X; M21 *= s.Y; M22 *= s.Z;
        }

        /// <summary>
        /// apply scaling to the columns of the matrix.
        /// </summary>
        /// <param name="s"></param>
        void ScaleColumn(Vector3 s)
        {
            M00 *= s.X; M01 *= s.Y; M02 *= s.Z;
            M10 *= s.X; M11 *= s.Y; M12 *= s.Z;
            M20 *= s.X; M21 *= s.Y; M22 *= s.Z;
        }

        #endregion Methods
    }
}
