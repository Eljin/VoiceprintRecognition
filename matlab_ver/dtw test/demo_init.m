disp('正在计算参考模板的参数...')
fname=sprintf('./mom/%d.wav',1);
[x fs]=wavread(fname);
[x1 x2]=vad(x);
m=mfcc(x,fs);
m=m(x1-2:x2-2,:);
mr=m;
    

disp('正在计算测试模板的参数...')
fname=sprintf('./mom/%d.wav',2);
[x fs]=wavread(fname);
[x1 x2]=vad(x);
m=mfcc(x,fs);
m=m(x1-2:x2-2,:);
mt=m;
    
disp('正在进行模板匹配...')
dist=zeros(5);
for j=1:5
    dist(j)=dtw(mt,mr);
end
 
disp('正在计算匹配结果...')
[d,j]=min(dist(:,1));
fprintf('测试模板的识别结果为：%d\n',min(mean(dist)));
