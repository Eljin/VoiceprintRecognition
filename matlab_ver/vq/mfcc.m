function r = mfcc(s, fs)
%---
m = 100;
n = 256;
l = length(s);
nbFrame = floor((l - n) / m) + 1;   %沿-∞方向取整 
for i = 1:n
for j = 1:nbFrame
M(i, j) = s(((j - 1) * m) + i);  %对矩阵M赋值
end
end
h = hamming(n);    %加 hamming 窗，以增加音框左端和右端的连续性
M2 = diag(h) * M;
for i = 1:nbFrame
frame(:,i) = fft(M2(:, i));  %对信号进行快速傅里叶变换FFT  
end
t = n / 2;
tmax = l / fs;
m = melfb(20, n, fs); %将上述线性频谱通过Mel 频率滤波器组得到Mel 频谱,下面在将其转化成对数频谱
n2 = 1 + floor(n / 2);
z = m * abs(frame(1:n2, :)).^2;
r = dct(log(z));  %将上述对数频谱，经过离散余弦变换(DCT)变换到倒谱域，即可得到Mel 倒谱系数(MFCC参数)
