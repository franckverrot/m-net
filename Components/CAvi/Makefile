CC=mcs
LIBDIR=-L /usr/lib/mono/gtk-sharp/ -L /usr/share/dotnet/mono/2.0 

all:
	$(CC) /target:library -resource:cavi.glade /r:../../Plugin.dll /r:../../CMp3.dll -o ../../CAvi.dll -recurse:'*.cs' -r gtk-sharp -r:glade-sharp $(LIBDIR)

clean:
	rm *.dll

