using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PwdManagement.Voice
{
    public class Register
    {
        //输入声音文件序列和输出文件序列，对应生成txt文件（保存到数据库最好）
        static public void register(String[] infilename, String[] outfilename)
        {
            MFCC.getMfcc(infilename, outfilename);
        }
    }
}
