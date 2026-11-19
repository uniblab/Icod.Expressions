/* Original pinned native semantic preflight; no managed allocation implementation. */
#define PCRE2_CODE_UNIT_WIDTH 8
#include <pcre2.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
static const char *patterns[] = {"(A)","(A?)","()","(?<=A)()","(?<!A)()","(?:A(*SKIP)(*F)|())","(?s:.*)(A)","(?s:.*)(Z)","(?m:^)(A?)","\\G(A?)","(A)\\1","(?s:.*)(?=Z)","(*NO_DOTSTAR_ANCHOR)(?s:.*)(A)","(*NO_START_OPT)(?s:.*)(A)"};
static const char *subjects[] = {"","41","FF","41FF41","FF41FF","41FFFF42","C3A9FF41","F09F9880FF41","41FF0A41","41FF0D0A41","4180FF41","E28241","FFFF41FFFF"};
typedef struct { const char *name, *pattern; int anchored, from_end; size_t start; } profile;
static const profile profiles[] = {{"caller-match","(A)",1,0,0},{"caller-miss","(Z)",1,0,0},{"caller-late","(A)",1,1,0},{"caller-invalid-empty","(A?)",1,0,1},{"dotstar-match","(?s:.*)(A)",0,0,0},{"dotstar-miss","(?s:.*)(Z)",0,0,0}};
static void run(const char *pattern, const unsigned char *subject, size_t length, size_t start, int anchored) {
    int error; PCRE2_SIZE error_offset;
    pcre2_code *code = pcre2_compile((PCRE2_SPTR)pattern, strlen(pattern), PCRE2_UTF | PCRE2_MATCH_INVALID_UTF | (anchored ? PCRE2_ANCHORED : 0), &error, &error_offset, NULL);
    if (!code) { fprintf(stderr,"compile:%d:%zu\n",error,(size_t)error_offset); exit(2); }
    int jit = pcre2_jit_compile(code, PCRE2_JIT_COMPLETE);
    PCRE2_SIZE jit_size = 0;
    if (pcre2_pattern_info(code, PCRE2_INFO_JITSIZE, &jit_size) != 0 || jit != 0 || jit_size == 0) { exit(3); }
    pcre2_match_data *data = pcre2_match_data_create_from_pattern(code,NULL);
    if (!data) { exit(4); }
    int rc = pcre2_match(code,subject,length,start,0,data,NULL);
    if (rc <= 0 && rc != PCRE2_ERROR_NOMATCH) { fprintf(stderr,"match:%d\n",rc); exit(5); }
    printf("%s|",rc > 0 ? "M" : "N");
    if (rc > 0) {
        PCRE2_SIZE *v = pcre2_get_ovector_pointer(data);
        uint32_t count = pcre2_get_ovector_count(data);
        for (uint32_t i=0;i<count;i++) {
            if (i) { putchar(','); }
            if (v[2*i] == PCRE2_UNSET) { putchar('-'); }
            else { printf("%zu:%lld",(size_t)v[2*i],(long long)v[2*i+1]-(long long)v[2*i]); }
        }
    }
    printf("|%d|%zu\n",jit,(size_t)jit_size);
    pcre2_match_data_free(data); pcre2_code_free(code);
}
int main(void) {
    for (size_t p=0;p<sizeof(patterns)/sizeof(patterns[0]);p++) {
        for (size_t s=0;s<sizeof(subjects)/sizeof(subjects[0]);s++) {
            size_t length = strlen(subjects[s])/2;
            unsigned char subject[32] = {0};
            for (size_t i=0;i<length;i++) {
                unsigned value;
                if (sscanf(subjects[s]+2*i,"%2x",&value) != 1) { return 6; }
                subject[i] = (unsigned char)value;
            }
            for (int anchored=0;anchored<=1;anchored++) {
                for (size_t start=0;start<=length;start++) {
                    printf("S|%zu|%zu|%d|%zu|",p,s,anchored,start);
                    run(patterns[p],subject,length,start,anchored);
                }
            }
        }
    }
    const size_t lengths[] = {1024,65536};
    for (size_t p=0;p<sizeof(profiles)/sizeof(profiles[0]);p++) {
        for (size_t n=0;n<sizeof(lengths)/sizeof(lengths[0]);n++) {
            size_t length=lengths[n]; unsigned char *subject=malloc(length);
            if (!subject) { return 7; }
            for (size_t i=0;i<length;i+=2) { subject[i]=0x41; subject[i+1]=0xff; }
            const profile *r=&profiles[p]; size_t start=r->from_end ? length-2 : r->start;
            printf("L|%s|%zu|%zu|",r->name,length,start);
            run(r->pattern,subject,length,start,r->anchored);
            free(subject);
        }
    }
    return 0;
}
