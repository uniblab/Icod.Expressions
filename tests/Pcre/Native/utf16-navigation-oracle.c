/* Development-only native16 consumption preflight. No production dependency. */
#define PCRE2_CODE_UNIT_WIDTH 16
#include <pcre2.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct { pcre2_code *code; pcre2_match_data *data; } program;
static program compile(const char *ascii, int utf) {
    uint16_t pattern[64]; size_t n = strlen(ascii);
    if (n > 64) { exit(2); }
    for (size_t i = 0; i < n; i++) { pattern[i] = (uint16_t)ascii[i]; }
    int error; PCRE2_SIZE offset;
    program p = { pcre2_compile(pattern, n, utf ? PCRE2_UTF : 0, &error, &offset, NULL), NULL };
    if (!p.code || pcre2_jit_compile(p.code, PCRE2_JIT_COMPLETE) != 0) { exit(2); }
    PCRE2_SIZE size = 0;
    if (pcre2_pattern_info(p.code, PCRE2_INFO_JITSIZE, &size) != 0 || size == 0) { exit(2); }
    p.data = pcre2_match_data_create_from_pattern(p.code, NULL);
    if (!p.data) { exit(2); }
    return p;
}
static int check(program p, const uint16_t *subject, size_t length, int reverse, int width, int error) {
    size_t offset = reverse ? length : 0;
    int rc = pcre2_match(p.code, subject, length, offset, 0, p.data, NULL);
    if (error != 0) { if (rc != error) { exit(3); } return rc; }
    if (rc != (reverse ? 2 : 1)) { exit(3); }
    PCRE2_SIZE *v = pcre2_get_ovector_pointer(p.data);
    if (reverse) {
        if (v[0] != length || v[1] != length || v[2] != length - (size_t)width || v[3] != length) { exit(3); }
    } else if (v[0] != 0 || v[1] != (size_t)width) { exit(3); }
    return rc;
}
int main(void) {
    program forward[2], reverse[2];
    for (int utf = 0; utf < 2; utf++) {
        forward[utf] = compile("(?s)\\G.", utf);
        reverse[utf] = compile("(?s)(?<=(.))\\G", utf);
    }
    for (uint32_t value = 0; value <= 0x10FFFF; value++) {
        uint16_t subject[2]; size_t length;
        if (value <= 0xFFFF) { subject[0] = (uint16_t)value; length = 1; }
        else {
            subject[0] = (uint16_t)(0xD800 + ((value - 0x10000) >> 10));
            subject[1] = (uint16_t)(0xDC00 + ((value - 0x10000) & 1023)); length = 2;
        }
        printf("%06X", (unsigned)value);
        for (int utf = 0; utf < 2; utf++) {
            int error = utf && value >= 0xD800 && value <= 0xDFFF
                ? (value <= 0xDBFF ? PCRE2_ERROR_UTF16_ERR1 : PCRE2_ERROR_UTF16_ERR3) : 0;
            int width = utf ? (int)length : 1;
            int first = check(forward[utf], subject, length, 0, width, error);
            int last = check(reverse[utf], subject, length, 1, width, error);
            printf("|%d|%d", first, last);
        }
        putchar('\n');
    }
    for (int utf = 0; utf < 2; utf++) {
        pcre2_match_data_free(forward[utf].data); pcre2_code_free(forward[utf].code);
        pcre2_match_data_free(reverse[utf].data); pcre2_code_free(reverse[utf].code);
    }
    return ferror(stdout) ? 4 : 0;
}
