# This simply builds the other two makefiles
.PHONY : all
all :
	cd app; make
	cd test; make
	