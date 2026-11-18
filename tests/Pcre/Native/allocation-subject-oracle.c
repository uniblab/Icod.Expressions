/* Original bounded subject-setup probe, independent of managed allocation code. */
#define PCRE2_CODE_UNIT_WIDTH 8
#include <pcre2.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct { const char *name, *pattern; unsigned mode; unsigned char first, second; } profile;
int main(void) {
    const profile profiles[] = {
        {"raw-code-unit", "\\A(\\C)", 0, 0xff, 0x00},
        {"utf8-dot", "\\A(.)", 1, 0xce, 0xbb},
        {"ucp-letter", "\\A(\\p{L})", 3, 0xce, 0xbb},
        {"utf8-no-match", "\\A(Z)", 1, 0xce, 0xbb}
    };
    const size_t lengths[] = {1024, 65536};
    for (size_t p = 0; p < sizeof(profiles)/sizeof(profiles[0]); ++p) {
        const profile *r = &profiles[p];
        const uint32_t flags = PCRE2_ANCHORED | ((r->mode & 1) ? PCRE2_UTF : 0) | ((r->mode & 2) ? PCRE2_UCP : 0);
        int error; PCRE2_SIZE error_offset;
        pcre2_code *code = pcre2_compile((PCRE2_SPTR)r->pattern, strlen(r->pattern), flags, &error, &error_offset, NULL);
        if (!code) { fprintf(stderr, "compile:%d:%zu\n", error, (size_t)error_offset); return 2; }
        const int jit = pcre2_jit_compile(code, PCRE2_JIT_COMPLETE);
        PCRE2_SIZE jit_size = 0;
        if (pcre2_pattern_info(code, PCRE2_INFO_JITSIZE, &jit_size) != 0 || jit != 0 || jit_size == 0) { return 3; }
        pcre2_match_data *data = pcre2_match_data_create_from_pattern(code, NULL);
        if (!data) { return 4; }
        for (size_t n = 0; n < sizeof(lengths)/sizeof(lengths[0]); ++n) {
            const size_t length = lengths[n];
            unsigned char *subject = malloc(length);
            if (!subject) { return 5; }
            for (size_t i = 0; i < length; i += 2) { subject[i] = r->first; subject[i+1] = r->second; }
            const int rc = pcre2_match(code, subject, length, 0, 0, data, NULL);
            if (rc <= 0 && rc != PCRE2_ERROR_NOMATCH) { fprintf(stderr, "match:%d\n", rc); return 6; }
            printf("%s|%zu|%s|", r->name, length, rc > 0 ? "M" : "N");
            if (rc > 0) {
                const PCRE2_SIZE *v = pcre2_get_ovector_pointer(data);
                const uint32_t count = pcre2_get_ovector_count(data);
                for (uint32_t i = 0; i < count; ++i) {
                    if (i) { putchar(','); }
                    if (v[2*i] == PCRE2_UNSET) { putchar('-'); }
                    else { printf("%zu:%lld", (size_t)v[2*i], (long long)v[2*i+1] - (long long)v[2*i]); }
                }
            }
            printf("|%d|%zu\n", jit, (size_t)jit_size);
            free(subject);
        }
        pcre2_match_data_free(data);
        pcre2_code_free(code);
    }
    return 0;
}
