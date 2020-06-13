module(lib, lib_print, lib_struct_s, lib_struct_t, g_lib_str, LIB_DEFINE)

import(stdio)

void lib_print() {
    printf("Hello, world!\n");
}

struct lib_struct_s {
    char *str;
};

typedef struct lib_struct_t {
    char *str;
} lib_struct_t;

char *g_lib_str = "Hello, world!\n";

#define LIB_DEFINE "Hello, world!\n"
