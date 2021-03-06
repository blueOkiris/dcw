module(main);

import(lib);
import(stdio);

int main(int argc, char **args) {
    // Test functions
    lib_print("Hello", "world");

    // Test structs and typedefs
    struct lib_struct_s strct1;
    strct1.str = "Hello";

    lib_struct_t strct2;
    strct2.str = "world";

    printf("%s, %s!\n", strct1.str, strct2.str);

    // Test exported variables
    printf("%s", g_lib_str);

    // Test exported defines
    printf("%s", LIB_DEFINE);

    return 0;
}