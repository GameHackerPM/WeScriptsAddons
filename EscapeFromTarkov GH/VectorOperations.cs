using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeFromTarkov
{
	public static class VectorOperations
	{
		/* ==== Bitwise operations ==== */

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static unsafe Vector4f AndNot(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* a = (int*)&v1;
			int* b = (int*)&v2;
			int* c = (int*)&res;
			*c++ = ~*a++ & *b++;
			*c++ = ~*a++ & *b++;
			*c++ = ~*a++ & *b++;
			*c = ~*a & *b;
			return res;
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static Vector4f Sqrt(this Vector4f v1)
		{
			return new Vector4f((float)System.Math.Sqrt((float)v1.x),
								(float)System.Math.Sqrt((float)v1.y),
								(float)System.Math.Sqrt((float)v1.z),
								(float)System.Math.Sqrt((float)v1.w));
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static Vector4f InvSqrt(this Vector4f v1)
		{
			return new Vector4f((float)(1.0 / System.Math.Sqrt((float)v1.x)),
								(float)(1.0 / System.Math.Sqrt((float)v1.y)),
								(float)(1.0 / System.Math.Sqrt((float)v1.z)),
								(float)(1.0 / System.Math.Sqrt((float)v1.w)));
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static Vector4f Reciprocal(this Vector4f v1)
		{
			return new Vector4f(1.0f / v1.x, 1.0f / v1.y, 1.0f / v1.z, 1.0f / v1.w);
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static Vector4f Max(this Vector4f v1, Vector4f v2)
		{
			return new Vector4f(System.Math.Max(v1.x, v2.x),
								System.Math.Max(v1.y, v2.y),
								System.Math.Max(v1.z, v2.z),
								System.Math.Max(v1.w, v2.w));
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static Vector4f Min(this Vector4f v1, Vector4f v2)
		{
			return new Vector4f(System.Math.Min(v1.x, v2.x),
								System.Math.Min(v1.y, v2.y),
								System.Math.Min(v1.z, v2.z),
								System.Math.Min(v1.w, v2.w));
		}

		/* ==== Horizontal operations ==== */

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE3)]
		public static Vector4f HorizontalAdd(this Vector4f v1, Vector4f v2)
		{
			return new Vector4f(v1.x + v1.y, v1.z + v1.w, v2.x + v2.y, v2.z + v2.w);
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE3)]
		public static Vector4f HorizontalSub(this Vector4f v1, Vector4f v2)
		{
			return new Vector4f(v1.x - v1.y, v1.z - v1.w, v2.x - v2.y, v2.z - v2.w);
		}


		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE3)]
		public static Vector4f AddSub(this Vector4f v1, Vector4f v2)
		{
			return new Vector4f(v1.x - v2.x, v1.y + v2.y, v1.z - v2.z, v1.w + v2.w);
		}


		/* ==== Compare methods ==== */

		/*Same as a == b. */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareEqual(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = v1.x == v2.x ? -1 : 0;
			*c++ = v1.y == v2.y ? -1 : 0;
			*c++ = v1.z == v2.z ? -1 : 0;
			*c = v1.w == v2.w ? -1 : 0;
			return res;
		}
		/*Same as a < b. */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareLessThan(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = v1.x < v2.x ? -1 : 0;
			*c++ = v1.y < v2.y ? -1 : 0;
			*c++ = v1.z < v2.z ? -1 : 0;
			*c = v1.w < v2.w ? -1 : 0;
			return res;
		}

		/*Same as a <= b. */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareLessEqual(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = v1.x <= v2.x ? -1 : 0;
			*c++ = v1.y <= v2.y ? -1 : 0;
			*c++ = v1.z <= v2.z ? -1 : 0;
			*c = v1.w <= v2.w ? -1 : 0;
			return res;
		}


		/*Same float.IsNaN (a) || float.IsNaN (b). */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareUnordered(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = float.IsNaN(v1.x) || float.IsNaN(v2.x) ? -1 : 0;
			*c++ = float.IsNaN(v1.y) || float.IsNaN(v2.y) ? -1 : 0;
			*c++ = float.IsNaN(v1.z) || float.IsNaN(v2.z) ? -1 : 0;
			*c = float.IsNaN(v1.w) || float.IsNaN(v2.w) ? -1 : 0;
			return res;
		}


		/*Same as a != b. */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareNotEqual(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = v1.x != v2.x ? -1 : 0;
			*c++ = v1.y != v2.y ? -1 : 0;
			*c++ = v1.z != v2.z ? -1 : 0;
			*c = v1.w != v2.w ? -1 : 0;
			return res;
		}

		/*Same as !(a < b). */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareNotLessThan(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = v1.x < v2.x ? 0 : -1;
			*c++ = v1.y < v2.y ? 0 : -1;
			*c++ = v1.z < v2.z ? 0 : -1;
			*c = v1.w < v2.w ? 0 : -1;
			return res;
		}


		/*Same as !(a <= b). */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareNotLessEqual(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = v1.x <= v2.x ? 0 : -1;
			*c++ = v1.y <= v2.y ? 0 : -1;
			*c++ = v1.z <= v2.z ? 0 : -1;
			*c = v1.w <= v2.w ? 0 : -1;
			return res;
		}

		/*Same !float.IsNaN (a) && !float.IsNaN (b). */
		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public unsafe static Vector4f CompareOrdered(this Vector4f v1, Vector4f v2)
		{
			Vector4f res = new Vector4f();
			int* c = (int*)&res;
			*c++ = !float.IsNaN(v1.x) && !float.IsNaN(v2.x) ? -1 : 0;
			*c++ = !float.IsNaN(v1.y) && !float.IsNaN(v2.y) ? -1 : 0;
			*c++ = !float.IsNaN(v1.z) && !float.IsNaN(v2.z) ? -1 : 0;
			*c = !float.IsNaN(v1.w) && !float.IsNaN(v2.w) ? -1 : 0;
			return res;
		}

		/* ==== Data shuffling ==== */

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static Vector4f InterleaveHigh(this Vector4f v1, Vector4f v2)
		{
			return new Vector4f(v1.z, v2.z, v1.w, v2.w);
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE1)]
		public static Vector4f InterleaveLow(this Vector4f v1, Vector4f v2)
		{
			return new Vector4f(v1.x, v2.x, v1.y, v2.y);
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE3)]
		public static Vector4f DuplicateLow(this Vector4f v1)
		{
			return new Vector4f(v1.x, v1.x, v1.z, v1.z);
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE3)]
		public static Vector4f DuplicateHigh(this Vector4f v1)
		{
			return new Vector4f(v1.y, v1.y, v1.w, v1.w);
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE2)]
		public static unsafe Vector4f Shuffle(this Vector4f v1, Vector4f v2, ShuffleSel sel)
		{
			float* p1 = (float*)&v1;
			float* p2 = (float*)&v2;
			int idx = (int)sel;
			return new Vector4f(*(p1 + ((idx >> 0) & 0x3)), *(p1 + ((idx >> 2) & 0x3)), *(p2 + ((idx >> 4) & 0x3)), *(p2 + ((idx >> 6) & 0x3)));
		}

		[Vector4f.AccelerationAttribute(Vector4f.AccelMode.SSE2)]
		public static unsafe Vector4f Shuffle(this Vector4f v1, ShuffleSel sel)
		{
			float* ptr = (float*)&v1;
			int idx = (int)sel;
			return new Vector4f(*(ptr + ((idx >> 0) & 0x3)), *(ptr + ((idx >> 2) & 0x3)), *(ptr + ((idx >> 4) & 0x3)), *(ptr + ((idx >> 6) & 0x3)));
		}
	}
}
