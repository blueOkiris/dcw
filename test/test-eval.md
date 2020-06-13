Goal of these tests is to verify:

1. Exported functions

 - Should get primatives in a header file

 - Regular kept in c file

2. Exported structs

 - Should get fully moved to header

3. Exported typedefs

 - Should get fully moved to header

4. Exported variables

 - `extern <varname>` in header

 - Regular kept in c file

5. Defines

 - Moved to header

So I need a project that exports a struct, typedef (I'll just do a typedef struct), an external variable, a function, and a def.