function demo(name1, name2)
disp('正在计算参考模板的参数...')
model = train(name1, 5);
for i=1:5
    ref(i).mfcc=model(i).mfcc;
end

disp('正在计算测试模板的参数...')
for i=1:5
    fname=sprintf('./%s/%d.wav',name2, i);
    [x fs]=wavread(fname);
    %[x1 x2]=vad(x,fs);
    [x1 x2]=vad(x);
    m=mfcc(x,fs);
    m=m(x1-2:x2-2,:);
    test(i).mfcc=m;
end

disp('正在进行模板匹配...')
dist=zeros(5,5);
for i=1:5
    for j=1:5
        dist(i,j)=dtw(test(i).mfcc,ref(j).mfcc);
    end
end
 
disp('正在计算匹配结果...')
for i=1:5
    %d接受最小值，j为最小值下标
    %[d,j]=min(dist(i,:));
    d=mean(dist(i,:))
    fprintf('测试模板%d的识别距离为：%d\n',i,d);
end