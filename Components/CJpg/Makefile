CC=mcs
LIBDIR=-L /usr/lib/mono/gtk-sharp/ -L /usr/share/dotnet/mono/2.0 

all:
	$(CC) /target:library -resource:cjpg.glade /r:../../Plugin.dll -o ../../CJpg.dll -recurse:'*.cs' -pkg:glade-sharp $(LIBDIR)

clean:
	rm *.dll

