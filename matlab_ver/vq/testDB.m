function testmsg = testDB(testdir, n, code)
nameList={'1','2','3','4','5','6','7','8','9' };                        %这个是我们要识别的9个数
for k = 1:n                     % 数据库中每一个说话人的特征
    file = sprintf('%ss%d.wav', testdir, k);	%找出文件的路径
    [s, fs] = wavread(file);      
        
    v = mfcc(s, fs);            % 对找到的文件取mfcc变换
    distmin = inf;
    k1 = 0;
   
    for l = 1:length(code)   
        d = disteu(v, code{l}); 
        dist = sum(min(d,[],2)) / size(d,1);
      
        if dist < distmin
            distmin = dist;%%这里和test函数里面一样  但多了一个具体语者的识别
            k1 = l;
        end      
    end
    msg=nameList{k1}
    msgbox(msg);
end
