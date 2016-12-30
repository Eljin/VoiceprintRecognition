function d = disteu(x, y)

[M, N] = size(x);  %音频x赋值给【M，N】
[M2, P] = size(y); %音频y赋值给【M2，P】
if (M ~= M2)
    error('不匹配！')  %两个音频时间长度不相等
end
d = zeros(N, P);
if (N < P)%在两个音频时间长度相等的前提下
    copies = zeros(1,P);
    for n = 1:N
        d(n,:) = sum((x(:, n+copies) - y) .^2, 1);
    end
else
    copies = zeros(1,N);
    for p = 1:P
        d(:,p) = sum((x - y(:, p+copies)) .^2, 1)';
    end%%成对欧氏距离的两个矩阵的列之间的距离
end
d = d.^0.5;
