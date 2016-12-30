%getmodel
function model = train(fold, n) 

for i=1:n
    fname=sprintf('./%s/b%d.wav',fold,i);
    [x fs]=wavread(fname);
    %[x1 x2]=vad(x,fs);
    [x1 x2]=vad(x);
    m=mfcc(x, fs);
    m=m(x1-2:x2-2,:);
    vec(i).mfcc=m;
end
model = vec;

