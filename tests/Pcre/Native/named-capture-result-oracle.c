/* Original native named-substring API/coordinate preflight. */
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
        char *v[5],*p=line;
        for (unsigned i=0;i<5;i++) { v[i]=p; char *sep=strchr(p,'|'); if(i<4) { if(!sep){return 2;} *sep=0;p=sep+1; } else if(sep){return 2;} }
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
        printf("%u|",id);
        pcre2_match_data *data=pcre2_match_data_create_from_pattern(code,NULL);if(!data){return 2;}
        int rc=pcre2_match(code,subject,slen,0,0,data,NULL);
        if(rc>0){PCRE2_SIZE *ov=pcre2_get_ovector_pointer(data);for(uint32_t i=0;i<=captures;i++){if(i){putchar(',');}if(ov[2*i]==PCRE2_UNSET){putchar('-');}else{printf("%zu:%lld",(size_t)ov[2*i],(long long)ov[2*i+1]-(long long)ov[2*i]);}}}
        size_t qlen;unsigned char *query=bytes(v[4],&qlen);(void)qlen;
        int lookup=0,selected=-1;unsigned char *copy=NULL;PCRE2_SIZE copy_length=0;
        if(rc>0){
            lookup=pcre2_substring_get_byname(data,query,&copy,&copy_length);
            PCRE2_SIZE *ov=pcre2_get_ovector_pointer(data);
            for(uint32_t i=0;i<names;i++){
                PCRE2_SPTR entry=table+i*entry_size;
                if(strcmp((const char*)entry+2,(const char*)query)==0){unsigned slot=(unsigned)entry[0]*256u+entry[1];if(ov[2*slot]!=PCRE2_UNSET){selected=(int)slot;break;}}
            }
            if(lookup==0){if(selected<0||copy_length!=ov[2*selected+1]-ov[2*selected]||memcmp(copy,subject+ov[2*selected],copy_length)!=0){return 4;}}
            else if(selected>=0|| (lookup!=PCRE2_ERROR_NOSUBSTRING&&lookup!=PCRE2_ERROR_UNSET)){return 5;}
        }else if(rc!=PCRE2_ERROR_NOMATCH){return 6;}
        const char *kind=rc<0?"N":lookup==0?"M":lookup==PCRE2_ERROR_UNSET?"U":"A";
        printf("|%s|",kind);
        if(selected>=0){PCRE2_SIZE *ov=pcre2_get_ovector_pointer(data);printf("%zu:%lld",(size_t)ov[2*selected],(long long)ov[2*selected+1]-(long long)ov[2*selected]);}
        putchar('|');if(copy){for(PCRE2_SIZE i=0;i<copy_length;i++){printf("%02X",copy[i]);}pcre2_substring_free(copy);}
        printf("|%d|%d|%zu\n",lookup,jit,(size_t)jit_size);free(query);
        pcre2_match_data_free(data);pcre2_code_free(code);free(pattern);free(subject);
    }
    return ferror(stdin)?2:0;
}
