function f=enframe1(x,win,inc)
%[F,T]=(X,WIN,INC)
nx=length(x(:));

nwin=length(win);
if nwin == 1
    lw = win;
    w = ones(1,lw);
else
    lw = nwin;
    w = win(:)';
end
if (nargin < 3) || isempty(inc)
    inc = lw;
end
nli=nx-lw+inc;
nf = fix((nli)/inc);
f=zeros(nf,lw);
indf= inc*(0:(nf-1)).';
inds = (1:lw);
f(:) = x(indf(:,ones(1,lw))+inds(ones(nf,1),:));

if (nwin > 1)   % if we have a non-unity window
    f = f .* w(ones(nf,1),:);
end


