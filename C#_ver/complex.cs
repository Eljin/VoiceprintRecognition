using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PwdManagement.Voice
{

    /// <summary>
    /// 复数类
    /// </summary>
    public class complex
    {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public complex()
            : this(0, 0)
        {
        }

        /// <summary>
        /// 只有实部的构造函数
        /// </summary>
        /// <param name="real">实部</param>
        public complex(double real)
            : this(real, 0) { }

        /// <summary>
        /// 由实部和虚部构造
        /// </summary>
        /// <param name="real">实部</param>
        /// <param name="image">虚部</param>
        public complex(double real, double image)
        {
            this.real = real;
            this.image = image;
        }

        private double real;
        /// <summary>
        /// 复数的实部
        /// </summary>
        public double Real
        {
            get { return real; }
            set { real = value; }
        }

        private double image;
        /// <summary>
        /// 复数的虚部
        /// </summary>
        public double Image
        {
            get { return image; }
            set { image = value; }
        }

        ///重载加法
        public static complex operator +(complex c1, complex c2)
        {
            return new complex(c1.real + c2.real, c1.image + c2.image);
        }

        ///重载减法
        public static complex operator -(complex c1, complex c2)
        {
            return new complex(c1.real - c2.real, c1.image - c2.image);
        }

        ///重载乘法
        public static complex operator *(complex c1, complex c2)
        {
            return new complex(c1.real * c2.real - c1.image * c2.image, c1.image * c2.real + c1.real * c2.image);
        }

        /// <summary>
        /// 求复数的模
        /// </summary>
        /// <returns>模</returns>
        public double ToModul()
        {
            return Math.Sqrt(real * real + image * image);
        }

        /// <summary>
        /// 重载ToString方法
        /// </summary>
        /// <returns>打印字符串</returns>
        public override string ToString()
        {
            if (Real == 0 && Image == 0)
            {
                return string.Format("{0}", 0);
            }
            if (Real == 0 && (Image != 1 && Image != -1))
            {
                return string.Format("{0} i", Image);
            }
            if (Image == 0)
            {
                return string.Format("{0}", Real);
            }
            if (Image == 1)
            {
                return string.Format("i");
            }
            if (Image == -1)
            {
                return string.Format("- i");
            }
            if (Image < 0)
            {
                return string.Format("{0} - {1} i", Real, -Image);
            }
            return string.Format("{0} + {1} i", Real, Image);
        }
    }

}
