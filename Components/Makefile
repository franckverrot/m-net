CC=mcs

all:
	$(CC) /target:library /out:../Plugin.dll -recurse:'Plugin/*.cs' -pkg:glade-sharp

plugins:
	cd CMp3 && make && cd ..
	cd CAvi && make && cd ..
	cd CJpg && make && cd ..
	cd COgg && make && cd ..
	cd CPng && make && cd ..
	cd CBmp && make && cd ..
	cd COdb && make && cd ..

clean:
