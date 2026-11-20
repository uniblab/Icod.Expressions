/* Original per-call policy preflight, built only for development. */
#define PCRE2_CODE_UNIT_WIDTH 8
#include <pcre2.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
static unsigned number(const char *s) {
    char *end; errno=0; unsigned long n=strtoul(s,&end,10);
    if (!*s || *s=='-' || *end || errno || n>UINT32_MAX) { exit(2); }
    return (unsigned)n;
}
static unsigned char *bytes(const char *hex, size_t *length) {
    size_t n=strcmp(hex,"-")==0?0:strlen(hex); if(n%2){exit(2);}
    *length=n/2; unsigned char *out=calloc(*length+1,1); if(!out){exit(2);}
    for(size_t i=0;i<*length;i++){unsigned value;if(sscanf(hex+2*i,"%2x",&value)!=1){exit(2);}out[i]=(unsigned char)value;}
    return out;
}
static uint32_t policy(unsigned n) {
    return (n&1?PCRE2_ANCHORED:0)|(n&2?PCRE2_ENDANCHORED:0)
        |(n&4?PCRE2_NOTEMPTY:0)|(n&8?PCRE2_NOTEMPTY_ATSTART:0)
        |(n&16?PCRE2_NOTBOL:0)|(n&32?PCRE2_NOTEOL:0);
}
int main(void) {
    char line[65536];
    while(fgets(line,sizeof(line),stdin)) {
        size_t n=strlen(line);if(!n||line[n-1]!='\n'){return 2;}line[n-1]=0;
        char *v[7],*p=line;
        for(unsigned i=0;i<7;i++){v[i]=p;char *sep=strchr(p,'|');if(i<6){if(!sep){return 2;}*sep=0;p=sep+1;}else if(sep){return 2;}}
        unsigned id=number(v[0]),mode=number(v[1]),defaults=number(v[2]),call=number(v[3]),start=number(v[6]);
        if(defaults>63||call>63){return 2;}
        size_t plen,slen;unsigned char *pattern=bytes(v[4],&plen),*subject=bytes(v[5],&slen);
        uint32_t flags=(mode&1?PCRE2_UTF:0)|(mode&2?PCRE2_UCP:0)|(mode&4?PCRE2_MATCH_INVALID_UTF:0)
            |(mode&16?PCRE2_CASELESS:0)|(mode&32?PCRE2_LITERAL:0)|(mode&128?PCRE2_DOLLAR_ENDONLY:0)
            |(mode&256?PCRE2_ALT_CIRCUMFLEX:0)|(mode&512?PCRE2_FIRSTLINE:0)
            |(defaults&1?PCRE2_ANCHORED:0)|(defaults&2?PCRE2_ENDANCHORED:0);
        pcre2_compile_context *context=pcre2_compile_context_create(NULL);if(!context){return 2;}
        pcre2_set_newline(context,mode&64?PCRE2_NEWLINE_ANYCRLF:PCRE2_NEWLINE_LF);
        pcre2_set_bsr(context,PCRE2_BSR_UNICODE);pcre2_set_parens_nest_limit(context,512);
        if(mode&8){pcre2_set_compile_extra_options(context,PCRE2_EXTRA_ALLOW_LOOKAROUND_BSK);}
        int error;PCRE2_SIZE offset;pcre2_code *code=pcre2_compile(pattern,plen,flags,&error,&offset,context);pcre2_compile_context_free(context);
        if(!code){printf("%u|%u|E|%d|%zu\n",id,call,error,(size_t)offset);return 3;}
        int jit=pcre2_jit_compile(code,PCRE2_JIT_COMPLETE);PCRE2_SIZE size=0;uint32_t captures=0;
        if(pcre2_pattern_info(code,PCRE2_INFO_JITSIZE,&size)||pcre2_pattern_info(code,PCRE2_INFO_CAPTURECOUNT,&captures)){return 3;}
        pcre2_match_data *data=pcre2_match_data_create_from_pattern(code,NULL);if(!data){return 2;}
        int rc=pcre2_match(code,subject,slen,start,policy((defaults & ~3u)|call),data,NULL);
        if(rc<=0&&rc!=PCRE2_ERROR_NOMATCH){printf("%u|%u|X|%d\n",id,call,rc);return 4;}
        printf("%u|%u|%d|%zu|",id,call,jit,(size_t)size);
        if(rc>0){PCRE2_SIZE *ov=pcre2_get_ovector_pointer(data);for(uint32_t i=0;i<=captures;i++){if(i){putchar(',');}if(ov[2*i]==PCRE2_UNSET){putchar('-');}else{printf("%zu:%lld",(size_t)ov[2*i],(long long)ov[2*i+1]-(long long)ov[2*i]);}}}
        putchar('|');if(rc>0){PCRE2_SPTR mark=pcre2_get_mark(data);if(mark){for(size_t i=0;mark[i];i++){printf("%02X",mark[i]);}}}putchar('\n');
        pcre2_match_data_free(data);pcre2_code_free(code);free(pattern);free(subject);
    }
    return ferror(stdin)?2:0;
}
