using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace PwdManagement.Voice
{
    class Dtw
    {
        protected const int DTWMAXNUM = 3000;
        protected static double DTWVERYBIG = 100000000.0;
        protected double[,] distance = new double[DTWMAXNUM, DTWMAXNUM]; /*保存距离*/
        protected double[,] dtwpath = new double[DTWMAXNUM, DTWMAXNUM]; /*保存路径*/

        public static double getDtw(String file1, String file2)
        {
            //定义2个数组a_d
            double[] a_data = new double[]{
                    0.0, 0.2, 0.4,
                    0.3, 0.2, 0.4,
                    0.4, 0.2, 0.4,
                    0.5, 0.2, 0.4,
                    5.0, 5.2, 8.4,
                    6.0, 5.2, 7.4,
                    4.0, 5.2, 4.4,
                    10.3, 10.4, 10.5,
                    10.1, 10.6, 10.7,
                    11.3, 10.2, 10.9
                    };
            double[] b_data = new double[]{
                    0.1, 0.2, 0.4,
                    0.3, 0.2, 0.4,
                    0.4, 0.2, 0.4,
                    0.5, 0.2, 0.4,
                    5.0, 5.2, 8.4,
                    6.0, 5.2, 7.4,
                    4.0, 5.2, 4.4,
                    10.3, 10.4, 10.5,
                    10.1, 10.6, 10.7,
                    11.3, 10.2, 10.9
             };

            Dtw dtw = new Dtw();
            //读mfcc数据文件到数组
            int i = 0, j = 0, r;

            double[] a = null;
            double[] b = null;

            a = dtw.read_file(file1, ref i);
            if (a == null)
            {
                //Console.Write("error!\n");
                return -1;
            }

            b = dtw.read_file(file2, ref j);
            if (b == null)
            {
                //Console.Write("error!\n");
                return -1;
            }

            //Console.Write("i = " + i + "\n");
            //Console.Write("j = " + j + "\n");


            double ret1, ret2;
            r = Math.Min(i, j) / 30;
            //ret=DTWDistanceFun(a_data, 10, b_data, 10, 2);
            ret1 = dtw.DTWDistanceFun(a, i, b, j, r);
            //Console.Write("DTW distance = " + ret1 + "\n");
            // ret2 = dtw.DTWDistanceFun(b, j, a, i, r);
            //Console.Write("DTW distance = " + ret2 + "\n");

            //mvector av, bv;
            //ret=VDTWDistanceFun(a_data, 10, b_data, 10, 2);

            //printf("VDTW distance :%f\n", ret);
            return ret1;

        }


        public double[] read_file(string fname, ref int len)
        {
            ArrayList L = new ArrayList();
            double d;
            double[] p = null;
            string strLine = "";
            string s;
            try
            {
                FileStream aFile = new FileStream(fname, FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                s = sr.ReadLine();
                while (s != null)
                {
                    strLine = strLine + s;
                    s = sr.ReadLine();
                }
                //Console.WriteLine(strLine);
                sr.Close();
            }
            catch (IOException ex)
            {
                //Console.WriteLine("An IOException has been thrown!");
                //Console.WriteLine(ex.ToString());
                //Console.ReadLine();
                return p;
            }

            String[] data = strLine.Split(',');
            len = data.Length;
            p = new double[len];
            for (int i = 0; i < len - 1; i++)
            {
                d = Double.Parse(data[i]);
                p[i] = d;
            }
            return p;
        }

        /*****************************************************************************/
        /* DTWDistance，求两个数组之间的匹配距离
        /* A,B分别为第一第二个数组，I，J为其数组长度，r为匹配窗口的大小

        /* r的大小一般取为数组长度的1/10到1/30
        /* 返回两个数组之间的匹配距离,如果返回－1.0，表明数组长度太大了
        /*****************************************************************************/
        public double DTWDistanceFun(double[] A, int I, double[] B, int J, int r)
        {
            int i, j;
            double dist;
            int istart, imax;
            int r2 = r + Math.Abs(I - J);/*匹配距离*/
            double g1, g2, g3;
            int pathsig = 1;/*路径的标志*/

            /*检查参数的有效性*/
            if (I > DTWMAXNUM || J > DTWMAXNUM)
            {
                //printf("Too big number\n");
                return -1.0;
            }

            /*进行一些必要的初始化*/
            for (i = 0; i < I; i++)
            {
                for (j = 0; j < J; j++)
                {
                    dtwpath[i, j] = 0;
                    distance[i, j] = DTWVERYBIG;
                }
            }

            /*动态规划求最小距离*/
            /*这里我采用的路径是 -------
                                      . |
                                    .   |
                                  .     |
                                .       |
             */
            distance[0, 0] = (double)2 * Math.Abs(A[0] - B[0]);
            for (i = 1; i < r2; i++)
            {
                if (i < I)
                    distance[i, 0] = distance[i - 1, 0] + Math.Abs(A[i] - B[0]);
                else break;
            }
            for (j = 1; j < r2; j++)
            {
                if (j < J)
                    distance[0, j] = distance[0, j - 1] + Math.Abs(A[0] - B[j]);
                else break;
            }

            for (j = 1; j < J; j++)
            {
                istart = j - r2;
                if (j <= r2)
                    istart = 1;
                imax = j + r2;
                if (imax >= I)
                    imax = I - 1;

                for (i = istart; i <= imax; i++)
                {
                    g1 = distance[i - 1, j] + Math.Abs(A[i] - B[j]);
                    g2 = distance[i - 1, j - 1] + 2 * Math.Abs(A[i] - B[j]);
                    g3 = distance[i, j - 1] + Math.Abs(A[i] - B[j]);
                    g2 = (g1 > g2) ? g2 : g1;
                    g3 = (g2 > g3) ? g3 : g2;
                    distance[i, j] = g3;
                }
            }

            dist = distance[I - 1, J - 1] / ((double)(I + J));
            return dist;
        }/*end DTWDistance*/

        /*****************************************************************************/
        /* DTWTemplate，进行建立模板的工作
        /* 其中A为已经建立好的模板，我们在以后加入训练样本的时候，
        /* 以已建立好的模板作为第一个参数，I为模板的长度，在这个模板中不再改变
        /* B为新加入的训练样本，J为B的长度，turn为训练的次数，在第一次
        /* 用两个数组建立模板时，r为1，这是出于权值的考虑
        /* temp保存匹配最新训练后的模板，建议temp[DTWMAXNUM]，函数返回最新训练后模板的长度
        /* 如果函数返回-1，表明训练样本之间距离过大，需要重新选择训练样本，
        /* tt为样本之间距离的阀值，自行定义
        /* rltdistance保存距离，第一次两个数组建立模板时可以随意赋予一个值，
        /* 后面用前一次返回的值赋予该参数
        /*****************************************************************************/
        public int DTWTemplate(double[] A, int I, double[] B, int J, double[] temp, int turn, double tt, ref double rltdistance)
        {
            double dist;
            int i, j;
            int pathsig = 1;
            dist = DTWDistanceFun(A, I, B, J, (int)(I / 30));
            if (dist > tt)
            {
                //Console.Write("\nSample doesn't match!\n");
                return -1;
            }

            if (turn == 1)
                rltdistance = dist;
            else
            {
                rltdistance = ((rltdistance) * (turn - 1) + dist) / turn;
            }
            /*寻找路径,这里我采用了逆向搜索法*/
            i = I - 1;
            j = J - 1;
            while (j >= 1 || i >= 1)
            {
                double m;
                if (i > 0 && j > 0)
                {
                    m = Math.Min(Math.Min(distance[i - 1, j], distance[i - 1, j - 1]), distance[i, j - 1]);
                    if (m == distance[i - 1, j])
                    {
                        dtwpath[i - 1, j] = pathsig;
                        i--;
                    }
                    else if (m == distance[i - 1, j - 1])
                    {
                        dtwpath[i - 1, j - 1] = pathsig;
                        i--;
                        j--;
                    }
                    else
                    {
                        dtwpath[i, j - 1] = pathsig;
                        j--;
                    }
                }
                else if (i == 0)
                {
                    dtwpath[0, j - 1] = pathsig;
                    j--;
                }
                else
                {/*j==0*/
                    dtwpath[i - 1, 0] = pathsig;
                    i--;
                }
            }
            dtwpath[0, 0] = pathsig;
            dtwpath[I - 1, J - 1] = pathsig;

            /*建立模板*/
            for (i = 0; i < I; i++)
            {
                double ftemp = 0.0;
                int ntemp = 0;
                for (j = 0; j < J; j++)
                {
                    if (dtwpath[i, j] == pathsig)
                    {
                        ftemp += B[j];
                        ntemp++;
                    }
                }
                ftemp /= ((double)ntemp);
                temp[i] = (A[i] * turn + ftemp) / ((double)(turn + 1));/*注意这里的权值*/
            }

            return I;/*返回模板的长度*/
        }//end DTWTemplate

    }
}
