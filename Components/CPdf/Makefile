CC=mcs
LIBDIR=-L /usr/lib/mono/gtk-sharp/ -L /usr/share/dotnet/mono/2.0 

all:
	$(CC) /target:library -resource:cpdf.glade /r:../../Plugin.dll -o ../../CPdf.dll -recurse:'*.cs' -pkg:glade-sharp $(LIBDIR)

clean:
	rm *.dll

