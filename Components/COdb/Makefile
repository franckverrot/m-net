CC=mcs
LIBDIR=-L /usr/lib/mono/gtk-sharp/ -L /usr/share/dotnet/mono/2.0 

all:
	$(CC) /target:library -resource:codb.glade /r:../../Plugin.dll -o ../../COdb.dll -recurse:'*.cs' -pkg:glade-sharp $(LIBDIR)

clean:
	rm *.dll

