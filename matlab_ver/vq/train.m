function code = train(traindir, n)

k = 16;                         % number of centroids required
for i = 1:n                     % 对数据库中的代码形成码本
    file = sprintf('%s%d.wav', traindir, i);           
    disp(file);
    [s, fs] = wavread(file);
    v = mfcc(s, fs);            % 计算 MFCC's 提取特征特征，返回值是Mel倒谱系数，是一个log的dct得到的
    code{i} = vqlbg(v, k);      % 训练VQ码本  通过矢量量化，得到原说话人的VQ码本
end
