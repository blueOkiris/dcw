# Name of executable
OBJNAME=  test

# Source files
SRC=      main.c lib.c

# Lib folder locations
LIBFLDRS= 

# Compiler options
CC=       gcc
CFLAGS=   -Wall
LDFLAGS=  

# Build test
$(OBJNAME) : $(SRC) ../app/dcw
	cd ../app; make
	../app/dcw $(CC) $(SRC) -cf $(CFLAGS)
	$(CC) $(addprefix -L,$(LIBFLDRS)) $(wildcard obj/*.o) $(LDFLAGS)
