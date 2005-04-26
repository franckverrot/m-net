CC=mcs
SOURCES=		MediaNET.cs \
                UI/MediaNETInterface.cs \
                UI/SettingsScreen.cs \
                UI/SplashScreen.cs \
                UI/AboutScreen.cs \
                UI/CrashScreen.cs \
                Settings/Conf.cs  \
                Settings/CSettings.cs  \
                DB/CMediaCollection.cs 
ASSEMBLIES= -pkg:gtk-sharp -pkg:glade-sharp -r:Plugin.dll -r:Mono.Posix
RESOURCES=  -resource:./UI/medianet.glade,medianet.glade \
			-resource:./UI/Franck.jpg,Franck.jpg
TARGET=MediaNET.exe
OPTIONS= -target:exe
all: $(TARGET)

components:
	cd Components && make clean && make && make plugins && cd ..
	
MediaNET.exe: $(SOURCES) ./UI/medianet.glade
	$(CC) $(OPTIONS) -o $(TARGET) $(ASSEMBLIES) $(SOURCES) $(RESOURCES)

clean:
	rm -f *.exe *.dll
run:
	mono $(TARGET)

