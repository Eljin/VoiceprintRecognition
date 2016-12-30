%mfcc
function  mfc=mfcc(x,fs)
%%%%%%%%%%%%%%%%%%%%%%%%%
%对输入的语音序列x进行mfcc参数提取，返回mfcc参数和一阶差分mfcc参数，mel滤波器的阶数为23
%fft变换长度为256，采样频率为8000HZ,对x 256点分为一帧
%%%%%%%%%%%%%%%%%%%%%%%%%%%%

bank=melbankm(24,256,fs,0,0.5,'m');
%归一化mel滤波器组参数
bank=full(bank);
bank=bank/max(bank(:));

%DCT系数，12*24
for k=1:12
    n=0:23;
    %dctcoef(:,k)=cos((2*n+1)*k*pi/(2*24));
    dctcoef(k,:)=cos((2*n+1)*k*pi/(2*24));
end

%归一化倒谱提升窗口
w=1+6*sin(pi*[1:12]./12);
w=w/max(w);

%预加重滤波器
xx=double(x);
xx=filter([1-0.9375],1,xx);

%语音信号分帧
xx=enframe(xx,256,80);
%xx=enframe(xx,256,128);

%计算每帧的mfcc参数
for i=1:size(xx,1) %
    y=xx(i,:);
    s=y'.*hamming(256);
    t=abs(fft(s));
    t=t.^2;%计算能量
    c1=dctcoef*log(bank*t(1:129));%dctcoef为dct系数，bank归一化mel滤波器组系数
    c2=c1.*w';%w为归一化倒谱提升窗口
    m(i,:)=c2';
end

%差分系数
dtm=zeros(size(m));
for i=3:size(m,1)-2;
    dtm(i,:)=-2*m(i-2,:)-m(i-1,:)+m(i+1,:)+2*m(i+2,:);
end
dtm=dtm/3;

%合并mfcc参数和一阶差分mfcc参数
mfc=[m dtm];
%去除首尾两帧，因为这两帧的一阶差分参数为0
mfc=mfc(3:size(m,1)-2,:);

mfcc波形图
subplot(211)
mfc_1=mfc(:,1);
plot(mfc_1);title('MFCC');