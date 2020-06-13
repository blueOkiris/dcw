# The executable name
OBJNAME= dcw

# Build directives
$(OBJNAME) : $(wildcard *.cs)
	export PublishSingleFile=true && \
	dotnet publish -c Release -r linux-x64
	mv ./bin/Release/netcoreapp3.1/linux-x64/publish/dcw ./
	rm -rf /var/tmp/.net

.PHONY : clean
clean :
	rm -rf dcw
	rm -rf bin
	rm -rf obj
