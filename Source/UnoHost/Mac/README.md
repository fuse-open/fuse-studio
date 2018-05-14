# UnoHost.OSX

![](http://g.gravizo.com/g? \
digraph G {\
Fusion[label="Fusion.OSX",style=dashed];\
Fusion -> FusionSupport[style=dashed];\
FusionSupport -> Protocol;\
FusionSupport -> Program[style=dashed,label="Spawns subprocess"];\
UnoView -> Protocol;\
Program->UnoView;\
})
