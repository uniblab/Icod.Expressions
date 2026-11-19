/* Original bounded native8 metadata/status/capture preflight. */
#define PCRE2_CODE_UNIT_WIDTH 8
#include <pcre2.h>
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
static unsigned number(const char *s) {
    char *end; errno=0; unsigned long value=strtoul(s,&end,10);
    if (!*s || *end || *s=='-' || errno || value>UINT32_MAX) { exit(2); }
    return (unsigned)value;
}
static unsigned char *bytes(const char *hex, size_t *length) {
    size_t n=strcmp(hex,"-")==0 ? 0 : strlen(hex);
    if (n%2) { exit(2); }
    *length=n/2; unsigned char *result=calloc(*length+1,1);
    if (!result) { exit(2); }
    for (size_t i=0;i<*length;i++) { unsigned value; if (sscanf(hex+2*i,"%2x",&value)!=1) { exit(2); } result[i]=(unsigned char)value; }
    return result;
}
int main(void) {
    static char line[65536];
    while (fgets(line,sizeof(line),stdin)) {
        size_t n=strlen(line); if (!n || line[n-1]!='\n') { return 2; } line[n-1]=0;
        char *v[4],*p=line;
        for (unsigned i=0;i<4;i++) { v[i]=p; char *sep=strchr(p,'|'); if(i<3) { if(!sep){return 2;} *sep=0;p=sep+1; } else if(sep){return 2;} }
        unsigned id=number(v[0]),mode=number(v[1]); size_t plen,slen;
        unsigned char *pattern=bytes(v[2],&plen),*subject=bytes(v[3],&slen);
        uint32_t flags=(mode&1?PCRE2_UTF:0)|(mode&2?PCRE2_UCP:0)|(mode&4?PCRE2_LITERAL:0)|(mode&8?PCRE2_NO_AUTO_CAPTURE:0)|(mode&16?PCRE2_DUPNAMES:0);
        pcre2_compile_context *context=pcre2_compile_context_create(NULL); if(!context){return 2;}
        pcre2_set_newline(context,PCRE2_NEWLINE_LF);pcre2_set_bsr(context,PCRE2_BSR_UNICODE);pcre2_set_parens_nest_limit(context,512);
        int error;PCRE2_SIZE offset;
        pcre2_code *code=pcre2_compile(pattern,plen,flags,&error,&offset,context);pcre2_compile_context_free(context);
        if(!code){printf("%u|E|%d|%zu\n",id,error,(size_t)offset);free(pattern);free(subject);continue;}
        int jit=pcre2_jit_compile(code,PCRE2_JIT_COMPLETE);PCRE2_SIZE jit_size=0;uint32_t captures=0,names=0,entry_size=0;PCRE2_SPTR table=NULL;
        if(pcre2_pattern_info(code,PCRE2_INFO_JITSIZE,&jit_size)||pcre2_pattern_info(code,PCRE2_INFO_CAPTURECOUNT,&captures)||pcre2_pattern_info(code,PCRE2_INFO_NAMECOUNT,&names)||pcre2_pattern_info(code,PCRE2_INFO_NAMEENTRYSIZE,&entry_size)||pcre2_pattern_info(code,PCRE2_INFO_NAMETABLE,&table)){return 3;}
        printf("%u|%u|",id,captures);
        for(uint32_t i=0;i<names;i++){
            PCRE2_SPTR entry=table+i*entry_size;if(i){putchar(';');}
            for(PCRE2_SPTR name=entry+2;*name;name++){printf("%02X",*name);}printf("=%u",(unsigned)entry[0]*256u+entry[1]);
        }
        pcre2_match_data *data=pcre2_match_data_create_from_pattern(code,NULL);if(!data){return 2;}
        int rc=pcre2_match(code,subject,slen,0,0,data,NULL);
        printf("|%s|",rc>0?"M":rc==PCRE2_ERROR_NOMATCH?"N":"X");
        if(rc>0){PCRE2_SIZE *ov=pcre2_get_ovector_pointer(data);for(uint32_t i=0;i<=captures;i++){if(i){putchar(',');}if(ov[2*i]==PCRE2_UNSET){putchar('-');}else{printf("%zu:%lld",(size_t)ov[2*i],(long long)ov[2*i+1]-(long long)ov[2*i]);}}}
        else if(rc!=PCRE2_ERROR_NOMATCH){printf("%d",rc);}
        printf("|%d|%zu\n",jit,(size_t)jit_size);
        pcre2_match_data_free(data);pcre2_code_free(code);free(pattern);free(subject);
    }
    return ferror(stdin)?2:0;
}
