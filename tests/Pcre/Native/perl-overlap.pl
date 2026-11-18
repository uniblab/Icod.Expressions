#!/usr/bin/env perl
use strict;
use warnings FATAL => 'all';
use Encode qw(decode FB_CROAK);
use Unicode::UCD ();

# Independent development-only overlap oracle. No Perl code is imported into
# the managed engine. Pin: Perl v5.40.1, Unicode 15.0.0, source commit
# f722f11a2b1a34e28743e331bad6dd7da41e2086 (tag v5.40.1).
# Input: id|UTF8-pattern-hex|UTF8-subject-hex|scalar-start-offset
# Output: id|M|scalar-start:length,... or id|N| (valid patterns only).
# Every row gets a fresh subject, including fresh /g position state.
die "Perl v5.40.1 / Unicode 15.0.0 required\n"
    unless $^V eq v5.40.1 && Unicode::UCD::UnicodeVersion() eq '15.0.0';
die "Read fixtures from stdin\n" if @ARGV;
my $rows = 0;
while (my $line = <STDIN>) {
    die "Fixture input limit\n" if ++$rows > 10000 || length($line) > 65535;
    $line =~ s/\r?\n\z//;
    my @fields = split /\|/, $line, -1;
    die "Invalid fixture row\n" unless @fields == 4 && $fields[0] =~ /\A[1-9][0-9]*\z/
        && $fields[3] =~ /\A(?:0|[1-9][0-9]*)\z/;
    my ($id, $pattern, $subject, $offset) = @fields;
    for ($pattern, $subject) {
        die "Invalid UTF-8 hex\n" unless length($_) <= 32768 && /\A(?:[0-9A-F]{2})*\z/;
        $_ = decode('UTF-8', pack('H*', $_), FB_CROAK);
        utf8::upgrade($_);
    }
    die "Fixture unit/start limit\n" if length($pattern) > 4096 || length($subject) > 4096 || $offset > length($subject);
    local $SIG{ALRM} = sub { die "Oracle timeout\n" };
    alarm 5;
    my $regex = qr/$pattern/;
    pos($subject) = $offset;
    if ($subject =~ /$regex/g) {
        my @captures;
        for my $group (0 .. $#+) {
            push @captures, defined($-[$group]) ? $-[$group] . ':' . ($+[$group] - $-[$group]) : '-';
        }
        print $id, '|M|', join(',', @captures), "\n";
    } else {
        print $id, "|N|\n";
    }
    alarm 0;
}
