using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PwdManagement.Voice
{
    public class Test
    {
        //matchfile 是数据库里取出的，注册声音的输出结果（txt文件）
        //inputfile是测试时输入的声音，outputfile是输出文件
        //这个函数算出来的是【单个】测试声音和数据库里保存声音【处理后的】文件的距离的平均值，然后算出的匹配度。
        public static double matchingDegree(String[] matchfile, String inputfile, String outputfile)
        {
            //这个是getMfcc的重载函数，Register.cs里用的是字符串数组为参数，这里是单个字符串为参数。
            MFCC.getMfcc(inputfile, outputfile);
            int len = matchfile.Length;
            double distance = Dtw.getDtw(matchfile[0], outputfile);
            for (int i = 1; i < len; i++)
            {
                distance = distance + Dtw.getDtw(matchfile[i], outputfile);
            }
            distance = distance / len / 1.0;
            var credit = ((50 - distance) / 50.0) * 100;
            if (credit < 80)
                credit = 0;
            else if (credit > 100)
                credit = 100 ;
            else
                credit = Math.Sqrt((credit - 80) / 20);
            credit *= 100;
            return credit ;
        }
    }
}
