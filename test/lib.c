module(lib, lib_print, lib_struct_s, lib_struct_t, g_lib_str, def1);

import(stdio);

void lib_print(char *str1, char *str2) {
    printf("%s, %s!\n", str1, str2);

    if(1) { }
}

void lib_print_unexported() {

}

struct lib_struct_s {
    char *str;
};

struct lib_struct_unexported_s {
    char *str;
};

typedef(
    struct lib_struct_t1_s {
        char *str;
    },
    lib_struct_t
);

typedef(
    struct lib_struct_t2_s {
        char *str;
    },
    lib_struct_unexported_t
);

char *g_lib_str = "Hello, world!\n";
char *g_lib_str_unexported = "foo";

macro(
    #define LIB_DEFINE "Hello, world!\n",
    def1
);

#define LIB_DEF_UNEXPORTED 69420