CC=mcs
LIBDIR=-L /usr/lib/mono/gtk-sharp/ -L /usr/share/dotnet/mono/2.0 

all:
	$(CC) /target:library -resource:cmp3.glade -resource:gnome-mime-audio-x-mp3.png /r:../../Plugin.dll -o ../../CMp3.dll -recurse:'*.cs' -pkg:glade-sharp $(LIBDIR)

clean:
	rm *.dll

