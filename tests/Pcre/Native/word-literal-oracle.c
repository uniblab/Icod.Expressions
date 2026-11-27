/* Development-only width-16 policy oracle; code-unit hex is host independent. */
#define PCRE2_CODE_UNIT_WIDTH 16
#include <pcre2.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#ifndef ORACLE_LITERAL
#define ORACLE_LITERAL 1
#endif
#ifndef ORACLE_MARKS
#define ORACLE_MARKS 0
#endif
#ifndef ORACLE_INTERPRETER_FALLBACK
#define ORACLE_INTERPRETER_FALLBACK 0
#endif
static unsigned number(const char *s) {
    char *end; errno = 0; unsigned long n = strtoul(s, &end, 10);
    if (!*s || *s == '-' || *end || errno || n > UINT32_MAX) { exit(2); }
    return (unsigned)n;
}
static int nibble(char c) {
    if (c >= '0' && c <= '9') { return c - '0'; }
    if (c >= 'A' && c <= 'F') { return c - 'A' + 10; }
    if (c >= 'a' && c <= 'f') { return c - 'a' + 10; }
    exit(2);
}
static uint16_t *units(const char *hex, size_t *length) {
    size_t n = strcmp(hex, "-") == 0 ? 0 : strlen(hex);
    if (n % 4) { exit(2); }
    *length = n / 4; uint16_t *out = calloc(*length + 1, sizeof(uint16_t));
    if (!out) { exit(2); }
    for (size_t i = 0; i < *length; i++) {
        unsigned value = 0;
        for (unsigned j = 0; j < 4; j++) { value = value * 16u + (unsigned)nibble(hex[i * 4 + j]); }
        out[i] = (uint16_t)value;
    }
    return out;
}
static uint32_t policy(unsigned n) {
    return (n & 1 ? PCRE2_ANCHORED : 0) | (n & 2 ? PCRE2_ENDANCHORED : 0)
        | (n & 4 ? PCRE2_NOTEMPTY : 0) | (n & 8 ? PCRE2_NOTEMPTY_ATSTART : 0)
        | (n & 16 ? PCRE2_NOTBOL : 0) | (n & 32 ? PCRE2_NOTEOL : 0);
}
int main(void) {
    char line[65536];
    while (fgets(line, sizeof(line), stdin)) {
        size_t n = strlen(line); if (!n || line[n - 1] != '\n') { return 2; } line[n - 1] = 0;
        char *v[7], *p = line;
        for (unsigned i = 0; i < 7; i++) {
            v[i] = p; char *sep = strchr(p, '|');
            if (i < 6) { if (!sep) { return 2; } *sep = 0; p = sep + 1; }
            else if (sep) { return 2; }
        }
        unsigned id = number(v[0]), mode = number(v[1]), defaults = number(v[2]), mask = number(v[3]), start = number(v[6]);
        if (mode > 3 || defaults > 63 || mask > 63) { return 2; }
        size_t plen, slen; uint16_t *pattern = units(v[4], &plen), *subject = units(v[5], &slen);
        uint32_t flags = (ORACLE_LITERAL ? PCRE2_LITERAL : 0) | (mode & 1 ? PCRE2_UTF : 0)
            | (mode & 2 ? PCRE2_UCP : 0) | policy(defaults & 3);
        int error; PCRE2_SIZE offset; pcre2_code *code = pcre2_compile(pattern, plen, flags, &error, &offset, NULL);
        printf("%u|%u|%u|", id, defaults, mask);
        if (!code) { printf("E|%d|%zu\n", error, (size_t)offset); free(pattern); free(subject); continue; }
        int jit = pcre2_jit_compile(code, PCRE2_JIT_COMPLETE); PCRE2_SIZE size = 0;
        if (pcre2_pattern_info(code, PCRE2_INFO_JITSIZE, &size)) { return 3; }
        if (jit || !size) {
            /* ACCEPT in a lexical non-atomic assertion disables native JIT. */
            if (!ORACLE_INTERPRETER_FALLBACK || jit != PCRE2_ERROR_JIT_UNSUPPORTED || size) { return 3; }
        }
        pcre2_match_data *data = pcre2_match_data_create_from_pattern(code, NULL); if (!data) { return 2; }
        int rc = pcre2_match(code, subject, slen, start, policy(mask | (defaults & ~3u)), data, NULL);
        if (rc == -1) { puts("N|"); }
        else if (rc < 0) { printf("X|%d\n", rc); }
        else {
            uint32_t captures = 0;
            if (!rc || pcre2_pattern_info(code, PCRE2_INFO_CAPTURECOUNT, &captures)) { return 3; }
            PCRE2_SIZE *ov = pcre2_get_ovector_pointer(data); printf("M|");
            for (uint32_t group = 0; group <= captures; group++) {
                if (group) { putchar(','); }
                if (ov[group * 2] == PCRE2_UNSET) { putchar('-'); }
                else { printf("%zu:%zu", (size_t)ov[group * 2], (size_t)(ov[group * 2 + 1] - ov[group * 2])); }
            }
            if (ORACLE_MARKS) {
                PCRE2_SPTR mark = pcre2_get_mark(data); putchar('|');
                if (mark) { while (*mark) { printf("%04X", (unsigned)*mark++); } }
            }
            putchar('\n');
        }
        pcre2_match_data_free(data); pcre2_code_free(code); free(pattern); free(subject);
    }
    return ferror(stdin) || ferror(stdout) ? 2 : 0;
}
