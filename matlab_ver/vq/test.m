function finalmsg = test(testdir, n, code)

for k = 1:n   % read test sound file of each speaker
  file = sprintf('%s%d.wav', testdir, k);
  [s, fs] = wavread(file);      
        
  v = mfcc(s, fs);  % 得到测试人语音的mel倒谱系数
  distmin = 4;      %阈值设置处
                    % 就判断一次，因为模板里面只有一个文件
  d = disteu(v, code{1});    %计算得到模板和要判断的声音之间的“距离”
  dist = sum(min(d,[],2)) / size(d,1);  %变换得到一个距离的量
        
  %测试阈值数量级
  msgc = sprintf('与模板语音信号的差值为:%10f ', dist);
  disp(msgc); 
  %此人匹配  

  if dist <= distmin  %一个阈值，小于阈值，则就是这个人。
     msg = sprintf('第%d位说话者与模板语音信号匹配,符合要求!\n', k);           
     finalmsg = '此位说话者符合要求!'; %界面显示语句，可随意设定        
     disp(msg);       
  end                 
  %此人不匹配  

  if dist > distmin                          
    msg = sprintf('第%d位说话者与模板语音信号不匹配,不符合要求!\n', k);
    finalmsg = '此位说话者不符合要求!'; %界面显示语句，可随意设定
    disp(msg);      
  end        
end
