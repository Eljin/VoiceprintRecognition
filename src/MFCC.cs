using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

namespace PwdManagement.Voice
{
    class MFCC
    {

        public static float SP_EMPHASIS_FACTOR = 0.97f;


        struct TWavHeader
        {
            public int rId;    //标志符（RIFF）
            public int rLen;   //数据大小,包括数据头的大小和音频文件的大小
            public int wId;    //格式类型（"WAVE"）
            public int fId;    //"fmt"
            public int fLen;   //Sizeof(WAVEFORMATEX)

            public short wFormatTag;       //编码格式，包括WAVE_FORMAT_PCM，WAVEFORMAT_ADPCM等
            public short nChannels;        //声道数，单声道为1，双声道为2
            public int nSamplesPerSec;   //采样频率
            public int nAvgBytesPerSec;  //每秒的数据量
            public short nBlockAlign;      //块对齐
            public short wBitsPerSample;   //WAVE文件的采样大小
            public int dId;              //"data"
            public int wSampleLength;    //音频数据的大小
        };

        public static int FS = 16;
        /*修改帧长*/
        public static long FrmLen = 1024;   //可修改帧长
        public static ulong FFTLen = 512;    //参与FFT运算的512个数据
        public static double PI = 3.1415926536;
        public static int FiltNum = 40;      //滤波器组数，一共40组
        public static int PCEP = 13;         //最后得到的关于的13个MFCC的系数     
        //const int PCEP=24;         //最后得到的关于的24个MFCC的系数     

        public static double[] Hamming = new double[FrmLen];
        public static int temp_1;  //计算次数的

        public static List<double> MFCCcoefficient = new List<double>();

        static double last = 0;  //一窗数据最后一个点的值，此点用于预加重


        public static void getMfcc(String[] infilename, String[] outfilename)
        {
            //	TWavHeader waveheader;
            //FILE *sourcefile, *MFCCFile;
            short[] buffer = new short[FrmLen];
            double[] dBuff = new double[FrmLen];
            double[] result = new double[FrmLen];  //预加重结果
            double[] data = new double[FrmLen];    //加窗后得到的数据

            double[] FiltCoe1 = new double[FFTLen / 2 + 1];  //左系数
            double[] FiltCoe2 = new double[FFTLen / 2 + 1];  //右系数
            int[] Num = new int[FFTLen / 2 + 1];     //一般而言，每个点会包含在相邻的两个滤波器中，这里是与该点相关的第二个滤波器
            double[] En = new double[FiltNum + 1];         //频带能量

            double[] Cep = new double[PCEP];//MFCC结果


            //对定义的变量赋初值
            temp_1 = 0;
            int i = 0;
            for (i = 0; i < FrmLen; i++)
            {
                buffer[i] = 0;
                dBuff[i] = result[i] = data[i] = 0.0f;
            }

            for (i = 0; i < (int)FFTLen / 2 + 1; i++)
            {
                FiltCoe1[i] = FiltCoe2[i] = 0.0f;
                Num[i] = 0;
            }

            for (i = 0; i < FiltNum + 1; i++)
            {
                En[i] = 0.0f;
            }
            List<complex> vecList = new List<complex>();//FFT计算之后的数据

            int filenum = infilename.Length;

            for (int k = 0; k < filenum; k++)
            {
                //String infilename ="";
                //Console.Write("请输入音频文件路径(需要加入后缀名*.wav)\n");
                //infilename = Console.ReadLine();
                //String outfilename = "";
                //Console.Write("请输入MFCC输出文件路径(需要加入后缀名*.txt)\n");
                //outfilename = Console.ReadLine();
                InitHamming();//初始化汉明窗
                InitFilt(FiltCoe1, FiltCoe2, Num); //初始化MEL滤波系数
                FileStream fs = new FileStream(infilename[k], FileMode.Open);
                //FileStream output=new FileStream(outfilename,FileMode.OpenOrCreate);
                StreamWriter sw = new StreamWriter(outfilename[k]);
                BinaryReader br = new BinaryReader(fs, Encoding.Default);

                int counter = 0;
                short temp;
                try
                {
                    while (true)
                    {
                        temp = br.ReadInt16();
                        if (counter < 1024)
                        {
                            buffer[counter] = temp;
                            dBuff[counter] = (double)buffer[counter];
                            counter++;
                        }
                        else
                        {

                            counter = 0;
                            preemphasis(dBuff, result, (short)FrmLen);//预加重结果存在result里面
                            HammingWindow(result, data); //给一帧数据加窗,存在data里面
                            compute_fft(data, vecList);
                            CFilt(data, FiltCoe1, FiltCoe2, Num, En, vecList);
                            mfcc(En, Cep);

                            for (int j = 0; j < PCEP - 1; j++)
                            {
                                if (j == 1)
                                {
                                    Cep[j] = Math.Round(Cep[j], 1);
                                    String str = Cep[j].ToString() + ',';
                                    sw.Write(str);
                                    temp_1++;
                                }
                                //if ( j == PCEP -1)
                                //	{fprintf(MFCCFile, "%f,", Cep[j]);
                                //temp_1++;}
                            }

                            vecList.Clear();
                            fs.Seek(-FrmLen / 2, SeekOrigin.Current);

                            //output.Close();
                        }

                    }
                }
                catch (Exception e)
                {
                    //Console.Write(e.Message);
                }

                fs.Close();
                sw.Close();
            }
        }


        public static void getMfcc(String infilename, String outfilename)
        {
            //	TWavHeader waveheader;
            //FILE *sourcefile, *MFCCFile;
            short[] buffer = new short[FrmLen];
            double[] dBuff = new double[FrmLen];
            double[] result = new double[FrmLen];  //预加重结果
            double[] data = new double[FrmLen];    //加窗后得到的数据

            double[] FiltCoe1 = new double[FFTLen / 2 + 1];  //左系数
            double[] FiltCoe2 = new double[FFTLen / 2 + 1];  //右系数
            int[] Num = new int[FFTLen / 2 + 1];     //一般而言，每个点会包含在相邻的两个滤波器中，这里是与该点相关的第二个滤波器
            double[] En = new double[FiltNum + 1];         //频带能量

            double[] Cep = new double[PCEP];//MFCC结果


            //对定义的变量赋初值
            temp_1 = 0;
            int i = 0;
            for (i = 0; i < FrmLen; i++)
            {
                buffer[i] = 0;
                dBuff[i] = result[i] = data[i] = 0.0f;
            }

            for (i = 0; i < (int)FFTLen / 2 + 1; i++)
            {
                FiltCoe1[i] = FiltCoe2[i] = 0.0f;
                Num[i] = 0;
            }

            for (i = 0; i < FiltNum + 1; i++)
            {
                En[i] = 0.0f;
            }
            List<complex> vecList = new List<complex>();//FFT计算之后的数据


            //String infilename ="";
            //Console.Write("请输入音频文件路径(需要加入后缀名*.wav)\n");
            //infilename = Console.ReadLine();
            //String outfilename = "";
            //Console.Write("请输入MFCC输出文件路径(需要加入后缀名*.txt)\n");
            //outfilename = Console.ReadLine();
            InitHamming();//初始化汉明窗
            InitFilt(FiltCoe1, FiltCoe2, Num); //初始化MEL滤波系数
            FileStream fs = new FileStream(infilename, FileMode.Open);
            //FileStream output=new FileStream(outfilename,FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(outfilename);
            BinaryReader br = new BinaryReader(fs, Encoding.Default);

            int counter = 0;
            short temp;
            try
            {
                while (true)
                {
                    temp = br.ReadInt16();
                    if (counter < 1024)
                    {
                        buffer[counter] = temp;
                        dBuff[counter] = (double)buffer[counter];
                        counter++;
                    }
                    else
                    {

                        counter = 0;
                        preemphasis(dBuff, result, (short)FrmLen);//预加重结果存在result里面
                        HammingWindow(result, data); //给一帧数据加窗,存在data里面
                        compute_fft(data, vecList);
                        CFilt(data, FiltCoe1, FiltCoe2, Num, En, vecList);
                        mfcc(En, Cep);

                        for (int j = 0; j < PCEP - 1; j++)
                        {
                            if (j == 1)
                            {
                                Cep[j] = Math.Round(Cep[j], 1);
                                String str = Cep[j].ToString() + ',';
                                sw.Write(str);
                                temp_1++;
                            }
                            //if ( j == PCEP -1)
                            //	{fprintf(MFCCFile, "%f,", Cep[j]);
                            //temp_1++;}
                        }

                        vecList.Clear();
                        fs.Seek(-FrmLen / 2, SeekOrigin.Current);

                        //output.Close();
                    }

                }
            }
            catch (Exception e)
            {
                //Console.Write(e.Message);
            }

            fs.Close();
            sw.Close();
        }

        //预加重
        public static void preemphasis(double[] buf, double[] result, short FrmLen)
        {
            int i;
            result[0] = buf[0] - SP_EMPHASIS_FACTOR * last;
            for (i = 1; i < FrmLen; i++)
            {
                result[i] = buf[i] - SP_EMPHASIS_FACTOR * buf[i - 1];
            }
            last = buf[(FrmLen - 1) / 2];   //假设每次移半帧

        }

        public static void InitHamming()//汉明窗初始化 
        {
            double twopi;
            int i;
            twopi = 2 * PI;
            for (i = 0; i < FrmLen; i++)
            {
                Hamming[i] = (double)(0.54 - 0.46 * Math.Cos(i * twopi / (double)(FrmLen - 1)));
            }
        }

        //给一帧数据加窗
        public static void HammingWindow(double[] result, double[] data)
        {
            int i;
            for (i = 0; i < FrmLen; i++)
            {
                data[i] = result[i] * Hamming[i];
            }

        }


        public static void compute_fft(double[] data, List<complex> vecList)
        {
            for (int i = 0; i < (int)FFTLen; ++i)
            {
                if (i < FrmLen)
                {
                    complex temp = new complex(data[i]);
                    vecList.Add(temp);
                }
                else
                {
                    complex temp = new complex(0);
                    vecList.Add(temp);
                }
            }
            //complex[] List = vecList.ToArray<complex>();
            FFT(512, vecList);
        }


        public static void FFT(uint ulN, List<complex> vecList) //complex[]
        {
            //得到指数，这个指数实际上指出了计算FFT时内部的循环次数   
            uint ulPower = 0; //指数 
            uint ulN1 = ulN - 1;   //ulN1=511
            while (ulN1 > 0)
            {
                ulPower++;
                ulN1 /= 2;
            }

            //反序，因为FFT计算后的结果次序不是顺序的，需要反序来调整。可以在FFT实质部分计算之前先调整，也可以在结果
            //计算出来后再调整。本程序中是先调整，再计算FFT实质部分
            BitArray bsIndex = new BitArray(sizeof(int) * 8);
            uint ulIndex; //反转后的序号 
            uint ulK;
            for (ulong p = 0; p < ulN; p++)
            {
                ulIndex = 0;
                ulK = 1;
                bsIndex = new BitArray(BitConverter.GetBytes((uint)p));
                for (uint j = 0; j < ulPower; j++)
                {
                    ulIndex += bsIndex[(int)(ulPower - j - 1)] ? ulK : 0;
                    ulK *= 2;
                }

                if (ulIndex > p)     //只有大于时，才调整，否则又调整回去了
                {
                    complex c = vecList[(int)p];
                    vecList[(int)p] = vecList[(int)ulIndex];
                    vecList[(int)ulIndex] = c;
                }
            }

            //计算旋转因子 
            List<complex> vecW = new List<complex>();
            for (uint i = 0; i < ulN / 2; i++)
            {
                vecW.Add(new complex((double)(Math.Cos(2 * i * PI / ulN)), (double)(-1 * Math.Sin(2 * i * PI / ulN))));
            }

            //计算FFT 
            uint ulGroupLength = 1; //段的长度 
            uint ulHalfLength = 0; //段长度的一半 
            uint ulGroupCount = 0; //段的数量 
            complex cw; //WH(x) 
            complex c1; //G(x) + WH(x) 
            complex c2; //G(x) - WH(x) 
            complex[] vecW1 = vecW.ToArray<complex>();
            for (uint b = 0; b < ulPower; b++)
            {
                ulHalfLength = ulGroupLength;
                ulGroupLength *= 2;
                for (int j = 0; j < ulN; j += (int)ulGroupLength)
                {
                    for (int k = 0; k < (int)ulHalfLength; k++)
                    {

                        cw = vecW1[k * ulN / ulGroupLength] * vecList[j + k + (int)ulHalfLength];
                        c1 = vecList[j + k] + cw;
                        c2 = vecList[j + k] - cw;
                        vecList[j + k] = c1;
                        vecList[j + k + (int)ulHalfLength] = c2;
                    }
                }
            }
        }




        /*
设置滤波器参数
输入参数：无
输出参数：*FiltCoe1---三角形滤波器左边的系数
          *FiltCoe2---三角形滤波器右边的系数
          *Num     ---决定每个点属于哪一个滤波器
*/
        public static void InitFilt(double[] FiltCoe1, double[] FiltCoe2, int[] Num)
        {
            int i, k;
            double Freq;
            double[] FiltFreq = new double[FiltNum + 2]; //40个滤波器，故有42各滤波器端点。每一个滤波器的左右端点分别是前一个及后一个滤波器的中心频率所在的点
            double[] BW = new double[FiltNum + 1]; //带宽，即每个相邻端点之间的频率跨度

            double low = (double)(400.0 / 3.0);    /* 滤波器组的最低频率，即第一个端点值 */
            short lin = 13;    /* 1000Hz以前的13个滤波器是线性的分布的 */
            double lin_spacing = (double)(200.0 / 3.0);    /* 相邻滤波器中心的距离为66.6Hz */
            short log = 27;     /* 1000Hz以后是27个对数线性分布的滤波器 */
            double log_spacing = 1.0711703f;    /* 相邻滤波器左半边宽度的比值 */

            for (i = 0; i < lin; i++)
            {
                FiltFreq[i] = low + i * lin_spacing;
            }
            for (i = lin; i < lin + log + 2; i++)
            {
                FiltFreq[i] = FiltFreq[lin - 1] * (double)Math.Pow(log_spacing, i - lin + 1);
            }
            for (i = 0; i < FiltNum + 1; i++)
            {
                BW[i] = FiltFreq[i + 1] - FiltFreq[i];
            }

            for (i = 0; i <= (int)FFTLen / 2; i++)
            {
                Num[i] = 0;
            }

            bool bFindFilt = false;
            for (i = 0; i <= (int)FFTLen / 2; i++)
            {
                Freq = FS * 1000.0F * i / (double)(FFTLen);
                bFindFilt = false;

                for (k = 0; k <= FiltNum; k++)
                {
                    if (Freq >= FiltFreq[k] && Freq <= FiltFreq[k + 1])
                    {
                        bFindFilt = true;
                        if (k == FiltNum)
                        {
                            FiltCoe1[i] = 0.0F;
                        }
                        else
                        {
                            FiltCoe1[i] = (Freq - FiltFreq[k]) / (double)(BW[k]) * 2.0f / (BW[k] + BW[k + 1]);
                        }

                        if (k == 0)
                        {
                            FiltCoe2[i] = 0.0F;
                        }
                        else
                        {
                            FiltCoe2[i] = (FiltFreq[k + 1] - Freq) / (double)(BW[k]) * 2.0f / (BW[k] + BW[k - 1]);
                        }

                        Num[i] = k;		//当k==FiltNum时，它为第FiltNum个滤波器，实际上它并不存在。这里只是为了计算方便，假设有第FiltNum个滤波器存在。
                        //但其实这并不影响结果
                        break;

                    }

                }

                if (!bFindFilt)
                {
                    Num[i] = 0;    //这时，该点不属于任何滤波器，因为其左右系数皆为0，所以可以假定它属于某个滤波器，而不会影响结果。这里我
                    //将其设为第一个滤波器。
                    FiltCoe1[i] = 0.0F;
                    FiltCoe2[i] = 0.0F;
                }
            }
        }




        /*
        根据滤波器参数计算频带能量
        输入参数：*spdata  ---预处理之后的一帧语音信号
                  *FiltCoe1---三角形滤波器左边的系数
                  *FiltCoe2---三角形滤波器右边的系数
                  *Num     ---决定每个点属于哪一个滤波器
          
        输出参数：*En      ---输出对数频带能量
        */
        //把属于某一频带的能量全部加起来了
        //CFilt(data, FiltCoe1, FiltCoe2, Num, En,vecList); veclist ： FFT计算出的结果  Num:决定每个点属于哪一个滤波器
        public static void CFilt(double[] spdata, double[] FiltCoe1, double[] FiltCoe2, int[] Num, double[] En, List<complex> vecList)
        {
            double temp = 0;
            int id, id1, id2;

            for (id = 0; id < FiltNum; id++)
            {
                En[id] = 0.0F;
            }
            for (id = 0; id <= (int)FFTLen / 2; id++)
            {
                temp = vecList[id].Real * vecList[id].Real + vecList[id].Image * vecList[id].Image;
                temp = temp / ((FrmLen / 2) * (FrmLen / 2));
                id1 = Num[id];
                if (id1 == 0)
                    En[id1] = En[id1] + FiltCoe1[id] * temp;
                if (id1 == FiltNum)
                    En[id1 - 1] = En[id1 - 1] + FiltCoe2[id] * temp;
                if ((id1 > 0) && (id1 < FiltNum))
                {
                    id2 = id1 - 1;
                    En[id1] = En[id1] + FiltCoe1[id] * temp;
                    En[id2] = En[id2] + FiltCoe2[id] * temp;
                }
            }
            for (id = 0; id < FiltNum; id++)
            {
                if (En[id] != 0)
                    En[id] = (double)Math.Log10(En[id]);
            }
        }


        /*
计算MFCC系数
输入参数：*En ---对数频带能量
*/

        public static void mfcc(double[] En, double[] Cep)
        {
            int idcep, iden;
            //	double Cep[13];

            for (idcep = 0; idcep < PCEP; idcep++)
            {
                Cep[idcep] = 0.0f;

                for (iden = 0; iden < FiltNum; iden++)   //离散余弦变换
                {
                    if (iden == 0)
                        Cep[idcep] = Cep[idcep] + En[iden] * (double)Math.Cos(idcep * (iden + 0.5f) * PI / (FiltNum)) * 10.0f * Math.Sqrt(1 / (double)FiltNum);
                    else
                        Cep[idcep] = Cep[idcep] + En[iden] * (double)Math.Cos(idcep * (iden + 0.5f) * PI / (FiltNum)) * 10.0f * Math.Sqrt(2 / (double)FiltNum);

                }
                MFCCcoefficient.Add(Cep[idcep]);
            }

        }
    }
}
