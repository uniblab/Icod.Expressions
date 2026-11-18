#define PCRE2_CODE_UNIT_WIDTH 0
#include <pcre2.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

/* Development-only, bounded line protocol. Hex denotes code-unit values,
   never host-endian bytes: width|mode|pattern|subject|start. '-' is empty.
   mode: 1=UTF, 2=UCP, 4=allow assertion K. Both native libraries use JIT. */
static int nibble(char c) {
    if (c >= '0' && c <= '9') { return c - '0'; }
    if (c >= 'A' && c <= 'F') { return c - 'A' + 10; }
    if (c >= 'a' && c <= 'f') { return c - 'a' + 10; }
    return -1;
}
static unsigned number(const char *s) {
    char *end;
    errno = 0;
    unsigned long value = strtoul(s, &end, 10);
    if (!*s || *end || errno || value > UINT32_MAX || *s == '-') { exit(2); }
    return (unsigned)value;
}
#define ORACLE(W, U, DIGITS) \
static U *units##W(const char *hex, size_t *length) { \
    size_t size = strcmp(hex, "-") == 0 ? 0 : strlen(hex); \
    if (size % DIGITS != 0) { exit(2); } \
    *length = size / DIGITS; \
    U *result = calloc(*length + 1, sizeof(U)); \
    if (!result) { exit(2); } \
    for (size_t i = 0; i < *length; ++i) { \
        unsigned value = 0; \
        for (unsigned d = 0; d < DIGITS; ++d) { \
            int next = nibble(hex[i * DIGITS + d]); \
            if (next < 0) { exit(2); } \
            value = value * 16u + (unsigned)next; \
        } \
        result[i] = (U)value; \
    } \
    return result; \
} \
static void hex##W(const U *s) { \
    if (s) { while (*s) { printf("%0*X", DIGITS, (unsigned)*s++); } } \
} \
static void run##W(char **v) { \
    unsigned mode = number(v[1]); \
    uint32_t options = (mode & 1 ? PCRE2_UTF : 0) | (mode & 2 ? PCRE2_UCP : 0); \
    size_t pattern_length, subject_length; \
    U *pattern = units##W(v[2], &pattern_length), *subject = units##W(v[3], &subject_length); \
    pcre2_compile_context_##W *context = pcre2_compile_context_create_##W(NULL); \
    if (!context) { exit(2); } \
    pcre2_set_newline_##W(context, PCRE2_NEWLINE_LF); \
    pcre2_set_bsr_##W(context, PCRE2_BSR_UNICODE); \
    pcre2_set_parens_nest_limit_##W(context, 512); \
    pcre2_set_compile_extra_options_##W(context, mode & 4 ? PCRE2_EXTRA_ALLOW_LOOKAROUND_BSK : 0); \
    int error; PCRE2_SIZE error_offset; \
    pcre2_code_##W *code = pcre2_compile_##W(pattern, pattern_length, options, &error, &error_offset, context); \
    pcre2_compile_context_free_##W(context); \
    if (!code) { printf("E|%d|%zu\n", error, (size_t)error_offset); free(pattern); free(subject); return; } \
    int jit = pcre2_jit_compile_##W(code, PCRE2_JIT_COMPLETE); \
    size_t jit_size = 0; uint32_t captures = 0, names = 0, entry_size = 0; const U *table = NULL; \
    pcre2_pattern_info_##W(code, PCRE2_INFO_JITSIZE, &jit_size); \
    pcre2_pattern_info_##W(code, PCRE2_INFO_CAPTURECOUNT, &captures); \
    pcre2_pattern_info_##W(code, PCRE2_INFO_NAMECOUNT, &names); \
    pcre2_pattern_info_##W(code, PCRE2_INFO_NAMEENTRYSIZE, &entry_size); \
    pcre2_pattern_info_##W(code, PCRE2_INFO_NAMETABLE, &table); \
    pcre2_match_data_##W *data = pcre2_match_data_create_from_pattern_##W(code, NULL); \
    if (!data) { exit(2); } \
    int rc = pcre2_match_##W(code, subject, subject_length, number(v[4]), 0, data, NULL); \
    if (rc < 0 && rc != PCRE2_ERROR_NOMATCH) { printf("X|%d|%d|%zu\n", rc, jit, jit_size); } \
    else { \
        printf("%c|%d|%zu|", rc >= 0 ? 'M' : 'N', jit, jit_size); \
        if (rc >= 0) { \
            PCRE2_SIZE *ov = pcre2_get_ovector_pointer_##W(data); \
            for (uint32_t i = 0; i <= captures; ++i) { \
                if (i) { putchar(','); } \
                if (ov[i * 2] == PCRE2_UNSET) { putchar('-'); } \
                else { printf("%zu:%lld", (size_t)ov[i * 2], (long long)ov[i * 2 + 1] - (long long)ov[i * 2]); } \
            } \
        } \
        putchar('|'); if (rc >= 0) { hex##W(pcre2_get_mark_##W(data)); } putchar('|'); \
        for (uint32_t i = 0; i < names; ++i) { \
            const U *entry = table + i * entry_size; \
            unsigned index = W == 8 ? (unsigned)entry[0] * 256u + entry[1] : entry[0]; \
            if (i) { putchar(';'); } hex##W(entry + (W == 8 ? 2 : 1)); printf("=%u", index); \
        } \
        putchar('\n'); \
    } \
    pcre2_match_data_free_##W(data); pcre2_code_free_##W(code); free(pattern); free(subject); \
}
ORACLE(8, uint8_t, 2)
ORACLE(16, uint16_t, 4)
int main(void) {
    static char line[65536];
    while (fgets(line, sizeof(line), stdin)) {
        size_t n = strlen(line);
        if (!n || line[n - 1] != '\n') { return 2; }
        line[n - 1] = 0;
        if (n > 1 && line[n - 2] == '\r') { line[n - 2] = 0; }
        char *v[5], *p = line;
        for (unsigned i = 0; i < 5; ++i) {
            v[i] = p; char *sep = strchr(p, '|');
            if (i < 4) { if (!sep) { return 2; } *sep = 0; p = sep + 1; }
            else if (sep) { return 2; }
        }
        unsigned width = number(v[0]);
        if (width == 8) { run8(v); } else if (width == 16) { run16(v); } else { return 2; }
    }
    return ferror(stdin) ? 2 : 0;
}
